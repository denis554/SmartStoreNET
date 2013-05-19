using System;
using System.Collections.Generic;
using System.Linq;
using SmartStore.Core;
using SmartStore.Core.Data;
using SmartStore.Core.Domain.Catalog;
using SmartStore.Core.Domain.Customers;
using SmartStore.Core.Domain.Orders;
using SmartStore.Services.Catalog;
using SmartStore.Services.Customers;
using SmartStore.Services.Directory;
using SmartStore.Services.Events;
using SmartStore.Services.Localization;
using SmartStore.Services.Security;
using SmartStore.Services.Seo;

namespace SmartStore.Services.Orders
{
    /// <summary>
    /// Shopping cart service
    /// </summary>
    public partial class ShoppingCartService : IShoppingCartService
    {
        #region Fields

        private readonly IRepository<ShoppingCartItem> _sciRepository;
        private readonly IWorkContext _workContext;
        private readonly ICurrencyService _currencyService;
        private readonly IProductService _productService;
        private readonly ILocalizationService _localizationService;
        private readonly IProductAttributeParser _productAttributeParser;
        private readonly ICheckoutAttributeService _checkoutAttributeService;
        private readonly ICheckoutAttributeParser _checkoutAttributeParser;
        private readonly IPriceFormatter _priceFormatter;
        private readonly ICustomerService _customerService;
        private readonly ShoppingCartSettings _shoppingCartSettings;
        private readonly IEventPublisher _eventPublisher;
        private readonly IPermissionService _permissionService;
        private readonly IAclService _aclService;
        #endregion

        #region Ctor

        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="sciRepository">Shopping cart repository</param>
        /// <param name="workContext">Work context</param>
        /// <param name="currencyService">Currency service</param>
        /// <param name="productService">Product settings</param>
        /// <param name="localizationService">Localization service</param>
        /// <param name="productAttributeParser">Product attribute parser</param>
        /// <param name="checkoutAttributeService">Checkout attribute service</param>
        /// <param name="checkoutAttributeParser">Checkout attribute parser</param>
        /// <param name="priceFormatter">Price formatter</param>
        /// <param name="customerService">Customer service</param>
        /// <param name="shoppingCartSettings">Shopping cart settings</param>
        /// <param name="eventPublisher">Event publisher</param>
        /// <param name="permissionService">Permission service</param>
        /// <param name="aclService">ACL service</param>
        public ShoppingCartService(IRepository<ShoppingCartItem> sciRepository,
            IWorkContext workContext, ICurrencyService currencyService,
            IProductService productService, ILocalizationService localizationService,
            IProductAttributeParser productAttributeParser,
            ICheckoutAttributeService checkoutAttributeService,
            ICheckoutAttributeParser checkoutAttributeParser,
            IPriceFormatter priceFormatter,
            ICustomerService customerService,
            ShoppingCartSettings shoppingCartSettings,
            IEventPublisher eventPublisher,
            IPermissionService permissionService, 
            IAclService aclService)
        {
            this._sciRepository = sciRepository;
            this._workContext = workContext;
            this._currencyService = currencyService;
            this._productService = productService;
            this._localizationService = localizationService;
            this._productAttributeParser = productAttributeParser;
            this._checkoutAttributeService = checkoutAttributeService;
            this._checkoutAttributeParser = checkoutAttributeParser;
            this._priceFormatter = priceFormatter;
            this._customerService = customerService;
            this._shoppingCartSettings = shoppingCartSettings;
            this._eventPublisher = eventPublisher;
            this._permissionService = permissionService;
            this._aclService = aclService;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Delete shopping cart item
        /// </summary>
        /// <param name="shoppingCartItem">Shopping cart item</param>
        /// <param name="resetCheckoutData">A value indicating whether to reset checkout data</param>
        /// <param name="ensureOnlyActiveCheckoutAttributes">A value indicating whether to ensure that only active checkout attributes are attached to the current customer</param>
        public virtual void DeleteShoppingCartItem(ShoppingCartItem shoppingCartItem, bool resetCheckoutData = true, 
            bool ensureOnlyActiveCheckoutAttributes = false)
        {
            if (shoppingCartItem == null)
                throw new ArgumentNullException("shoppingCartItem");

            var customer = shoppingCartItem.Customer;

            //reset checkout data
            if (resetCheckoutData)
            {
                _customerService.ResetCheckoutData(shoppingCartItem.Customer);
            }

            //delete item
            _sciRepository.Delete(shoppingCartItem);

            //validate checkout attributes
            if (ensureOnlyActiveCheckoutAttributes &&
                //only for shopping cart items (ignore wishlist)
                shoppingCartItem.ShoppingCartType == ShoppingCartType.ShoppingCart)
            {
                var cart = customer.ShoppingCartItems.Where(x => x.ShoppingCartType == ShoppingCartType.ShoppingCart).ToList();
                customer.CheckoutAttributes = _checkoutAttributeParser.EnsureOnlyActiveAttributes(customer.CheckoutAttributes, cart);
                _customerService.UpdateCustomer(customer);
            }

            //event notification
            _eventPublisher.EntityDeleted(shoppingCartItem);
        }

        /// <summary>
        /// Deletes expired shopping cart items
        /// </summary>
        /// <param name="olderThanUtc">Older than date and time</param>
        /// <returns>Number of deleted items</returns>
        public virtual int DeleteExpiredShoppingCartItems(DateTime olderThanUtc)
        {
            var query = from sci in _sciRepository.Table
                           where sci.UpdatedOnUtc < olderThanUtc
                           select sci;

            var cartItems = query.ToList();
            foreach (var cartItem in cartItems)
                _sciRepository.Delete(cartItem);
            return cartItems.Count;
        }
        
        /// <summary>
        /// Validates required product variants (product variants which require other variant to be added to the cart)
        /// </summary>
        /// <param name="customer">Customer</param>
        /// <param name="shoppingCartType">Shopping cart type</param>
        /// <param name="productVariant">Product variant</param>
        /// <param name="automaticallyAddRequiredProductVariantsIfEnabled">Automatically add required product variants if enabled</param>
        /// <returns>Warnings</returns>
        public virtual IList<string> GetRequiredProductVariantWarnings(Customer customer, ShoppingCartType shoppingCartType,
            ProductVariant productVariant, bool automaticallyAddRequiredProductVariantsIfEnabled)
        {
            if (customer == null)
                throw new ArgumentNullException("customer");

            if (productVariant == null)
                throw new ArgumentNullException("productVariant");

            var cart = customer.ShoppingCartItems.Where(sci => sci.ShoppingCartType == shoppingCartType).ToList();

            var warnings = new List<string>();

            if (productVariant.RequireOtherProducts)
            {
                var requiredProductVariants = new List<ProductVariant>();
                foreach (var id in productVariant.ParseRequiredProductVariantIds())
                {
                    var rpv = _productService.GetProductVariantById(id);
                    if (rpv != null)
                        requiredProductVariants.Add(rpv);
                }
                
                foreach (var rpv in requiredProductVariants)
                {
                    //ensure that product is in the cart
                    bool alreadyInTheCart = false;
                    foreach (var sci in cart)
                    {
                        if (sci.ProductVariantId == rpv.Id)
                        {
                            alreadyInTheCart = true;
                            break;
                        }
                    }
                    //not in the cart
                    if (!alreadyInTheCart)
                    {

                        string fullProductName = "";
                        if (!String.IsNullOrEmpty(rpv.GetLocalized(x => x.Name)))
                            fullProductName = string.Format("{0} ({1})", rpv.Product.GetLocalized(x => x.Name), rpv.GetLocalized(x => x.Name));
                        else
                            fullProductName = rpv.Product.GetLocalized(x => x.Name);

                        if (productVariant.AutomaticallyAddRequiredProductVariants)
                        {
                            //add to cart (if possible)
                            if (automaticallyAddRequiredProductVariantsIfEnabled)
                            {
                                //pass 'false' for 'automaticallyAddRequiredProductVariantsIfEnabled' to prevent circular references
                                var addToCartWarnings = AddToCart(customer, rpv, shoppingCartType, "", decimal.Zero, 1, false);
                                if (addToCartWarnings.Count > 0)
                                {
                                    //a product wasn't atomatically added for some reasons

                                    //don't display specific errors from 'addToCartWarnings' variable
                                    //display only generic error
                                    warnings.Add(string.Format(_localizationService.GetResource("ShoppingCart.RequiredProductWarning"), fullProductName));
                                }
                            }
                            else
                            {
                                warnings.Add(string.Format(_localizationService.GetResource("ShoppingCart.RequiredProductWarning"), fullProductName));
                            }
                        }
                        else
                        {
                            warnings.Add(string.Format(_localizationService.GetResource("ShoppingCart.RequiredProductWarning"), fullProductName));
                        }
                    }
                }
            }

            return warnings;
        }
        
        /// <summary>
        /// Validates a product variant for standard properties
        /// </summary>
        /// <param name="customer">Customer</param>
        /// <param name="shoppingCartType">Shopping cart type</param>
        /// <param name="productVariant">Product variant</param>
        /// <param name="selectedAttributes">Selected attributes</param>
        /// <param name="customerEnteredPrice">Customer entered price</param>
        /// <param name="quantity">Quantity</param>
        /// <returns>Warnings</returns>
        public virtual IList<string> GetStandardWarnings(Customer customer, ShoppingCartType shoppingCartType,
            ProductVariant productVariant, string selectedAttributes, decimal customerEnteredPrice,
            int quantity)
        {
            if (customer == null)
                throw new ArgumentNullException("customer");

            if (productVariant == null)
                throw new ArgumentNullException("productVariant");

            var warnings = new List<string>();

            var product = productVariant.Product;
            if (product == null)
            {
                warnings.Add(string.Format(_localizationService.GetResource("ShoppingCart.CannotLoadProduct"), productVariant.ProductId));
                return warnings;
            }

            //deleted?
            if (product.Deleted || productVariant.Deleted)
            {
                warnings.Add(_localizationService.GetResource("ShoppingCart.ProductDeleted"));
                return warnings;
            }

            //published?
            if (!product.Published || !productVariant.Published)
            {
                warnings.Add(_localizationService.GetResource("ShoppingCart.ProductUnpublished"));
            }
            
            //ACL
            if (!_aclService.Authorize(product, customer))
            {
                warnings.Add(_localizationService.GetResource("ShoppingCart.ProductUnpublished"));
            }

            //disabled "add to cart" button
            if (shoppingCartType == ShoppingCartType.ShoppingCart && productVariant.DisableBuyButton)
            {
                warnings.Add(_localizationService.GetResource("ShoppingCart.BuyingDisabled"));
            }

            //disabled "add to wishlist" button
            if (shoppingCartType == ShoppingCartType.Wishlist && productVariant.DisableWishlistButton)
            {
                warnings.Add(_localizationService.GetResource("ShoppingCart.WishlistDisabled"));
            }

            //call for price
            if (shoppingCartType == ShoppingCartType.ShoppingCart && productVariant.CallForPrice)
            {
                warnings.Add(_localizationService.GetResource("Products.CallForPrice"));
            }

            //customer entered price
            if (productVariant.CustomerEntersPrice)
            {
                if (customerEnteredPrice < productVariant.MinimumCustomerEnteredPrice ||
                    customerEnteredPrice > productVariant.MaximumCustomerEnteredPrice)
                {
                    decimal minimumCustomerEnteredPrice = _currencyService.ConvertFromPrimaryStoreCurrency(productVariant.MinimumCustomerEnteredPrice, _workContext.WorkingCurrency);
                    decimal maximumCustomerEnteredPrice = _currencyService.ConvertFromPrimaryStoreCurrency(productVariant.MaximumCustomerEnteredPrice, _workContext.WorkingCurrency);
                    warnings.Add(string.Format(_localizationService.GetResource("ShoppingCart.CustomerEnteredPrice.RangeError"),
                        _priceFormatter.FormatPrice(minimumCustomerEnteredPrice, false, false),
                        _priceFormatter.FormatPrice(maximumCustomerEnteredPrice, false, false)));
                }
            }

            //quantity validation
            var hasQtyWarnings = false;
            if (quantity < productVariant.OrderMinimumQuantity)
            {
                warnings.Add(string.Format(_localizationService.GetResource("ShoppingCart.MinimumQuantity"), productVariant.OrderMinimumQuantity));
                hasQtyWarnings = true;
            }
            if (quantity > productVariant.OrderMaximumQuantity)
            {
                warnings.Add(string.Format(_localizationService.GetResource("ShoppingCart.MaximumQuantity"), productVariant.OrderMaximumQuantity));
                hasQtyWarnings = true;
            }
            var allowedQuantities = productVariant.ParseAllowedQuatities();
            if (allowedQuantities.Length > 0 && !allowedQuantities.Contains(quantity))
            {
                warnings.Add(string.Format(_localizationService.GetResource("ShoppingCart.AllowedQuantities"), string.Join(", ", allowedQuantities)));
            }

            var validateOutOfStock = shoppingCartType == ShoppingCartType.ShoppingCart || !_shoppingCartSettings.AllowOutOfStockItemsToBeAddedToWishlist;
            if (validateOutOfStock && !hasQtyWarnings)
            {
                switch (productVariant.ManageInventoryMethod)
                {
                    case ManageInventoryMethod.DontManageStock:
                        {
                        }
                        break;
                    case ManageInventoryMethod.ManageStock:
                        {
                            if ((BackorderMode)productVariant.BackorderMode == BackorderMode.NoBackorders)
                            {
                                if (productVariant.StockQuantity < quantity)
                                {
                                    int maximumQuantityCanBeAdded = productVariant.StockQuantity;
                                    if (maximumQuantityCanBeAdded <= 0)
                                        warnings.Add(_localizationService.GetResource("ShoppingCart.OutOfStock"));
                                    else
                                        warnings.Add(string.Format(_localizationService.GetResource("ShoppingCart.QuantityExceedsStock"), maximumQuantityCanBeAdded));
                                }
                            }
                        }
                        break;
                    case ManageInventoryMethod.ManageStockByAttributes:
                        {
                            var combination = productVariant
                                .ProductVariantAttributeCombinations
                                .FirstOrDefault(x => _productAttributeParser.AreProductAttributesEqual(x.AttributesXml, selectedAttributes));
                            if (combination != null &&
                                !combination.AllowOutOfStockOrders &&
                                combination.StockQuantity < quantity)
                            {
                                int maximumQuantityCanBeAdded = combination.StockQuantity;
                                if (maximumQuantityCanBeAdded <= 0)
                                    warnings.Add(_localizationService.GetResource("ShoppingCart.OutOfStock"));
                                else
                                    warnings.Add(string.Format(_localizationService.GetResource("ShoppingCart.QuantityExceedsStock"), maximumQuantityCanBeAdded));
                            }
                        }
                        break;
                    default:
                        break;
                }
            }

            //availability dates
            bool availableStartDateError = false;
            if (productVariant.AvailableStartDateTimeUtc.HasValue)
            {
                DateTime now = DateTime.UtcNow;
                DateTime availableStartDateTime = DateTime.SpecifyKind(productVariant.AvailableStartDateTimeUtc.Value, DateTimeKind.Utc);
                if (availableStartDateTime.CompareTo(now) > 0)
                {
                    warnings.Add(_localizationService.GetResource("ShoppingCart.NotAvailable"));
                    availableStartDateError = true;
                }
            }
            if (productVariant.AvailableEndDateTimeUtc.HasValue && !availableStartDateError)
            {
                DateTime now = DateTime.UtcNow;
                DateTime availableEndDateTime = DateTime.SpecifyKind(productVariant.AvailableEndDateTimeUtc.Value, DateTimeKind.Utc);
                if (availableEndDateTime.CompareTo(now) < 0)
                {
                    warnings.Add(_localizationService.GetResource("ShoppingCart.NotAvailable"));
                }
            }
            return warnings;
        }

        /// <summary>
        /// Validates shopping cart item attributes
        /// </summary>
        /// <param name="shoppingCartType">Shopping cart type</param>
        /// <param name="productVariant">Product variant</param>
        /// <param name="selectedAttributes">Selected attributes</param>
        /// <returns>Warnings</returns>
        public virtual IList<string> GetShoppingCartItemAttributeWarnings(ShoppingCartType shoppingCartType,
            ProductVariant productVariant, string selectedAttributes)
        {
            if (productVariant == null)
                throw new ArgumentNullException("productVariant");

            var warnings = new List<string>();

            //selected attributes
            var pva1Collection = _productAttributeParser.ParseProductVariantAttributes(selectedAttributes);
            foreach (var pva1 in pva1Collection)
            {
                var pv1 = pva1.ProductVariant;
                if (pv1 != null)
                {
                    if (pv1.Id != productVariant.Id)
                    {
                        warnings.Add("Attribute error");
                    }
                }
                else
                {
                    warnings.Add("Attribute error");
                    return warnings;
                }
            }

            //existing product attributes
            var pva2Collection = productVariant.ProductVariantAttributes;
            foreach (var pva2 in pva2Collection)
            {
                if (pva2.IsRequired)
                {
                    bool found = false;
                    //selected product attributes
                    foreach (var pva1 in pva1Collection)
                    {
                        if (pva1.Id == pva2.Id)
                        {
                            var pvaValuesStr = _productAttributeParser.ParseValues(selectedAttributes, pva1.Id);
                            foreach (string str1 in pvaValuesStr)
                            {
                                if (!String.IsNullOrEmpty(str1.Trim()))
                                {
                                    found = true;
                                    break;
                                }
                            }
                        }
                    }

                    //if not found
                    if (!found)
                    {
                        if (!string.IsNullOrEmpty(pva2.TextPrompt))
                        {
                            warnings.Add(pva2.TextPrompt);
                        }
                        else
                        {
                            warnings.Add(string.Format(_localizationService.GetResource("ShoppingCart.SelectAttribute"), pva2.ProductAttribute.GetLocalized(a => a.Name)));
                        }
                    }
                }
            }

            return warnings;
        }

        /// <summary>
        /// Validates shopping cart item (gift card)
        /// </summary>
        /// <param name="shoppingCartType">Shopping cart type</param>
        /// <param name="productVariant">Product variant</param>
        /// <param name="selectedAttributes">Selected attributes</param>
        /// <returns>Warnings</returns>
        public virtual IList<string> GetShoppingCartItemGiftCardWarnings(ShoppingCartType shoppingCartType,
            ProductVariant productVariant, string selectedAttributes)
        {
            if (productVariant == null)
                throw new ArgumentNullException("productVariant");

            var warnings = new List<string>();

            //gift cards
            if (productVariant.IsGiftCard)
            {
                string giftCardRecipientName = string.Empty;
                string giftCardRecipientEmail = string.Empty;
                string giftCardSenderName = string.Empty;
                string giftCardSenderEmail = string.Empty;
                string giftCardMessage = string.Empty;
                _productAttributeParser.GetGiftCardAttribute(selectedAttributes,
                    out giftCardRecipientName, out giftCardRecipientEmail,
                    out giftCardSenderName, out giftCardSenderEmail, out giftCardMessage);

                if (String.IsNullOrEmpty(giftCardRecipientName))
                    warnings.Add(_localizationService.GetResource("ShoppingCart.RecipientNameError"));

                if (productVariant.GiftCardType == GiftCardType.Virtual)
                {
                    //validate for virtual gift cards only
                    if (String.IsNullOrEmpty(giftCardRecipientEmail) || !CommonHelper.IsValidEmail(giftCardRecipientEmail))
                        warnings.Add(_localizationService.GetResource("ShoppingCart.RecipientEmailError"));
                }

                if (String.IsNullOrEmpty(giftCardSenderName))
                    warnings.Add(_localizationService.GetResource("ShoppingCart.SenderNameError"));

                if (productVariant.GiftCardType == GiftCardType.Virtual)
                {
                    //validate for virtual gift cards only
                    if (String.IsNullOrEmpty(giftCardSenderEmail) || !CommonHelper.IsValidEmail(giftCardSenderEmail))
                        warnings.Add(_localizationService.GetResource("ShoppingCart.SenderEmailError"));
                }
            }

            return warnings;
        }
        
        /// <summary>
        /// Validates shopping cart item
        /// </summary>
        /// <param name="customer">Customer</param>
        /// <param name="shoppingCartType">Shopping cart type</param>
        /// <param name="productVariant">Product variant</param>
        /// <param name="selectedAttributes">Selected attributes</param>
        /// <param name="customerEnteredPrice">Customer entered price</param>
        /// <param name="quantity">Quantity</param>
        /// <param name="automaticallyAddRequiredProductVariantsIfEnabled">Automatically add required product variants if enabled</param>
        /// <param name="getStandardWarnings">A value indicating whether we should validate a product variant for standard properties</param>
        /// <param name="getAttributesWarnings">A value indicating whether we should validate product attributes</param>
        /// <param name="getGiftCardWarnings">A value indicating whether we should validate gift card properties</param>
        /// <param name="getRequiredProductVariantWarnings">A value indicating whether we should validate required product variants (product variants which require other variant to be added to the cart)</param>
        /// <returns>Warnings</returns>
        public virtual IList<string> GetShoppingCartItemWarnings(Customer customer, ShoppingCartType shoppingCartType,
            ProductVariant productVariant, string selectedAttributes, decimal customerEnteredPrice,
            int quantity, bool automaticallyAddRequiredProductVariantsIfEnabled,
            bool getStandardWarnings = true, bool getAttributesWarnings = true, 
            bool getGiftCardWarnings = true, bool getRequiredProductVariantWarnings = true)
        {
            if (productVariant == null)
                throw new ArgumentNullException("productVariant");

            var warnings = new List<string>();
            
            //standard properties
            if (getStandardWarnings)
                warnings.AddRange(GetStandardWarnings(customer, shoppingCartType, productVariant, selectedAttributes, customerEnteredPrice, quantity));

            //selected attributes
            if (getAttributesWarnings)
                warnings.AddRange(GetShoppingCartItemAttributeWarnings(shoppingCartType, productVariant, selectedAttributes));

            //gift cards
            if (getGiftCardWarnings)
                warnings.AddRange(GetShoppingCartItemGiftCardWarnings(shoppingCartType, productVariant, selectedAttributes));

            //required product variants
            if (getRequiredProductVariantWarnings)
                warnings.AddRange(GetRequiredProductVariantWarnings(customer, shoppingCartType, productVariant, automaticallyAddRequiredProductVariantsIfEnabled));
            
            return warnings;
        }

        /// <summary>
        /// Validates whether this shopping cart is valid
        /// </summary>
        /// <param name="shoppingCart">Shopping cart</param>
        /// <param name="checkoutAttributes">Checkout attributes</param>
        /// <param name="validateCheckoutAttributes">A value indicating whether to validate checkout attributes</param>
        /// <returns>Warnings</returns>
        public virtual IList<string> GetShoppingCartWarnings(IList<ShoppingCartItem> shoppingCart, 
            string checkoutAttributes, bool validateCheckoutAttributes)
        {
            var warnings = new List<string>();

            bool hasStandartProducts = false;
            bool hasRecurringProducts = false;

            foreach (var sci in shoppingCart)
            {
                var productVariant = sci.ProductVariant;
                if (productVariant == null)
                {
                    warnings.Add(string.Format(_localizationService.GetResource("ShoppingCart.CannotLoadProductVariant"), sci.ProductVariantId));
                    return warnings;
                }

                if (productVariant.IsRecurring)
                    hasRecurringProducts = true;
                else
                    hasStandartProducts = true;
            }

            //don't mix standard and recurring products
            if (hasStandartProducts && hasRecurringProducts)
                warnings.Add(_localizationService.GetResource("ShoppingCart.CannotMixStandardAndAutoshipProducts"));

            //recurring cart validation
            if (hasRecurringProducts)
            {
                int cycleLength = 0;
                RecurringProductCyclePeriod cyclePeriod =  RecurringProductCyclePeriod.Days;
                int totalCycles = 0;
                string cyclesError = shoppingCart.GetRecurringCycleInfo(out cycleLength, out cyclePeriod, out totalCycles);
                if (!string.IsNullOrEmpty(cyclesError))
                {
                    warnings.Add(cyclesError);
                    return warnings;
                }
            }

            //validate checkout attributes
            if (validateCheckoutAttributes)
            {
                //selected attributes
                var ca1Collection = _checkoutAttributeParser.ParseCheckoutAttributes(checkoutAttributes);

                //existing checkout attributes
                var ca2Collection = _checkoutAttributeService.GetAllCheckoutAttributes();
                if (!shoppingCart.RequiresShipping())
                {
                    //remove attributes which require shippable products
                    ca2Collection = ca2Collection.RemoveShippableAttributes();
                }
                foreach (var ca2 in ca2Collection)
                {
                    if (ca2.IsRequired)
                    {
                        bool found = false;
                        //selected checkout attributes
                        foreach (var ca1 in ca1Collection)
                        {
                            if (ca1.Id == ca2.Id)
                            {
                                var caValuesStr = _checkoutAttributeParser.ParseValues(checkoutAttributes, ca1.Id);
                                foreach (string str1 in caValuesStr)
                                    if (!String.IsNullOrEmpty(str1.Trim()))
                                    {
                                        found = true;
                                        break;
                                    }
                            }
                        }

                        //if not found
                        if (!found)
                        {
                            if (!string.IsNullOrEmpty(ca2.GetLocalized(a => a.TextPrompt)))
                                warnings.Add(ca2.GetLocalized(a => a.TextPrompt));
                            else
                                warnings.Add(string.Format(_localizationService.GetResource("ShoppingCart.SelectAttribute"), ca2.GetLocalized(a => a.Name)));
                        }
                    }
                }
            }

            return warnings;
        }

        /// <summary>
        /// Finds a shopping cart item in the cart
        /// </summary>
        /// <param name="shoppingCart">Shopping cart</param>
        /// <param name="shoppingCartType">Shopping cart type</param>
        /// <param name="productVariant">Product variant</param>
        /// <param name="selectedAttributes">Selected attributes</param>
        /// <param name="customerEnteredPrice">Price entered by a customer</param>
        /// <returns>Found shopping cart item</returns>
        public virtual ShoppingCartItem FindShoppingCartItemInTheCart(IList<ShoppingCartItem> shoppingCart,
            ShoppingCartType shoppingCartType,
            ProductVariant productVariant,
            string selectedAttributes = "",
            decimal customerEnteredPrice = decimal.Zero)
        {
            if (shoppingCart == null)
                throw new ArgumentNullException("shoppingCart");

            if (productVariant == null)
                throw new ArgumentNullException("productVariant");

            foreach (var sci in shoppingCart.Where(a => a.ShoppingCartType == shoppingCartType))
            {
                if (sci.ProductVariantId == productVariant.Id)
                {
                    //attributes
                    bool attributesEqual = _productAttributeParser.AreProductAttributesEqual(sci.AttributesXml, selectedAttributes);

                    //gift cards
                    bool giftCardInfoSame = true;
                    if (sci.ProductVariant.IsGiftCard)
                    {
                        string giftCardRecipientName1 = string.Empty;
                        string giftCardRecipientEmail1 = string.Empty;
                        string giftCardSenderName1 = string.Empty;
                        string giftCardSenderEmail1 = string.Empty;
                        string giftCardMessage1 = string.Empty;
                        _productAttributeParser.GetGiftCardAttribute(selectedAttributes,
                            out giftCardRecipientName1, out giftCardRecipientEmail1,
                            out giftCardSenderName1, out giftCardSenderEmail1, out giftCardMessage1);

                        string giftCardRecipientName2 = string.Empty;
                        string giftCardRecipientEmail2 = string.Empty;
                        string giftCardSenderName2 = string.Empty;
                        string giftCardSenderEmail2 = string.Empty;
                        string giftCardMessage2 = string.Empty;
                        _productAttributeParser.GetGiftCardAttribute(sci.AttributesXml,
                            out giftCardRecipientName2, out giftCardRecipientEmail2,
                            out giftCardSenderName2, out giftCardSenderEmail2, out giftCardMessage2);


                        if (giftCardRecipientName1.ToLowerInvariant() != giftCardRecipientName2.ToLowerInvariant() ||
                            giftCardSenderName1.ToLowerInvariant() != giftCardSenderName2.ToLowerInvariant())
                            giftCardInfoSame = false;
                    }

                    //price is the same (for products which require customers to enter a price)
                    bool customerEnteredPricesEqual = true;
                    if (sci.ProductVariant.CustomerEntersPrice)
                        customerEnteredPricesEqual = Math.Round(sci.CustomerEnteredPrice, 2) == Math.Round(customerEnteredPrice, 2);

                    //found?
                    if (attributesEqual && giftCardInfoSame && customerEnteredPricesEqual)
                        return sci;
                }
            }

            return null;
        }

        /// <summary>
        /// Add a product variant to shopping cart
        /// </summary>
        /// <param name="customer">Customer</param>
        /// <param name="productVariant">Product variant</param>
        /// <param name="shoppingCartType">Shopping cart type</param>
        /// <param name="selectedAttributes">Selected attributes</param>
        /// <param name="customerEnteredPrice">The price enter by a customer</param>
        /// <param name="quantity">Quantity</param>
        /// <param name="automaticallyAddRequiredProductVariantsIfEnabled">Automatically add required product variants if enabled</param>
        /// <returns>Warnings</returns>
        public virtual IList<string> AddToCart(Customer customer, ProductVariant productVariant, 
            ShoppingCartType shoppingCartType, string selectedAttributes,
            decimal customerEnteredPrice, int quantity, bool automaticallyAddRequiredProductVariantsIfEnabled)
        {
            if (customer == null)
                throw new ArgumentNullException("customer");

            if (productVariant == null)
                throw new ArgumentNullException("productVariant");

            var warnings = new List<string>();
            if (shoppingCartType == ShoppingCartType.ShoppingCart && !_permissionService.Authorize(StandardPermissionProvider.EnableShoppingCart, customer))
            {
                warnings.Add("Shopping cart is disabled");
                return warnings;
            }
            if (shoppingCartType == ShoppingCartType.Wishlist && !_permissionService.Authorize(StandardPermissionProvider.EnableWishlist, customer))
            {
                warnings.Add("Wishlist is disabled");
                return warnings;
            }

            if (quantity <= 0)
            {
                warnings.Add(_localizationService.GetResource("ShoppingCart.QuantityShouldPositive"));
                return warnings;
            }

            //reset checkout info
            _customerService.ResetCheckoutData(customer);

            var cart = customer.ShoppingCartItems.Where(sci=>sci.ShoppingCartType == shoppingCartType).ToList();

            var shoppingCartItem = FindShoppingCartItemInTheCart(cart,
                shoppingCartType, productVariant, selectedAttributes, customerEnteredPrice);

            if (shoppingCartItem != null)
            {
                //update existing shopping cart item
                int newQuantity = shoppingCartItem.Quantity + quantity;
                warnings.AddRange(GetShoppingCartItemWarnings(customer, shoppingCartType, productVariant,
                    selectedAttributes, customerEnteredPrice, newQuantity, automaticallyAddRequiredProductVariantsIfEnabled));

                if (warnings.Count == 0)
                {
                    shoppingCartItem.AttributesXml = selectedAttributes;
                    shoppingCartItem.Quantity = newQuantity;
                    shoppingCartItem.UpdatedOnUtc = DateTime.UtcNow;
                    _customerService.UpdateCustomer(customer);

                    //event notification
                    _eventPublisher.EntityUpdated(shoppingCartItem);
                }
            }
            else
            {
                //new shopping cart item
                warnings.AddRange(GetShoppingCartItemWarnings(customer, shoppingCartType, productVariant,
                    selectedAttributes, customerEnteredPrice, quantity, automaticallyAddRequiredProductVariantsIfEnabled));
                if (warnings.Count == 0)
                {
                    //maximum items validation
                    switch (shoppingCartType)
                    {
                        case ShoppingCartType.ShoppingCart:
                            {
                                if (cart.Count >= _shoppingCartSettings.MaximumShoppingCartItems)
                                {
                                    warnings.Add(_localizationService.GetResource("ShoppingCart.MaximumShoppingCartItems"));
                                    return warnings;
                                }
                            }
                            break;
                        case ShoppingCartType.Wishlist:
                            {
                                if (cart.Count >= _shoppingCartSettings.MaximumWishlistItems)
                                {
                                    warnings.Add(_localizationService.GetResource("ShoppingCart.MaximumWishlistItems"));
                                    return warnings;
                                }
                            }
                            break;
                        default:
                            break;
                    }

                    DateTime now = DateTime.UtcNow;
                    shoppingCartItem = new ShoppingCartItem()
                    {
                        ShoppingCartType = shoppingCartType,
                        ProductVariant = productVariant,
                        AttributesXml = selectedAttributes,
                        CustomerEnteredPrice = customerEnteredPrice,
                        Quantity = quantity,
                        CreatedOnUtc = now,
                        UpdatedOnUtc = now
                    };
                    customer.ShoppingCartItems.Add(shoppingCartItem);
                    _customerService.UpdateCustomer(customer);

                    //event notification
                    _eventPublisher.EntityInserted(shoppingCartItem);
                }
            }

            return warnings;
        }

        /// <summary>
        /// Updates the shopping cart item
        /// </summary>
        /// <param name="customer">Customer</param>
        /// <param name="shoppingCartItemId">Shopping cart item identifier</param>
        /// <param name="newQuantity">New shopping cart item quantity</param>
        /// <param name="resetCheckoutData">A value indicating whether to reset checkout data</param>
        /// <returns>Warnings</returns>
        public virtual IList<string> UpdateShoppingCartItem(Customer customer, int shoppingCartItemId, 
            int newQuantity, bool resetCheckoutData)
        {
            if (customer == null)
                throw new ArgumentNullException("customer");

            var warnings = new List<string>();

            var shoppingCartItem = customer.ShoppingCartItems.Where(sci => sci.Id == shoppingCartItemId).FirstOrDefault();
            if (shoppingCartItem != null)
            {
                if (resetCheckoutData)
                {
                    //reset checkout data
                    _customerService.ResetCheckoutData(customer);
                }
                if (newQuantity > 0)
                {
                    //check warnings
                    warnings.AddRange(GetShoppingCartItemWarnings(customer, shoppingCartItem.ShoppingCartType,
                        shoppingCartItem.ProductVariant, shoppingCartItem.AttributesXml,
                        shoppingCartItem.CustomerEnteredPrice, newQuantity, false));
                    if (warnings.Count == 0)
                    {
                        //if everything is OK, then update a shopping cart item
                        shoppingCartItem.Quantity = newQuantity;
                        shoppingCartItem.UpdatedOnUtc = DateTime.UtcNow;
                        _customerService.UpdateCustomer(customer);

                        //event notification
                        _eventPublisher.EntityUpdated(shoppingCartItem);
                    }
                }
                else
                {
                    //delete a shopping cart item
                    DeleteShoppingCartItem(shoppingCartItem, resetCheckoutData, true);
                }
            }

            return warnings;
        }
        
        /// <summary>
        /// Migrate shopping cart
        /// </summary>
        /// <param name="fromCustomer">From customer</param>
        /// <param name="toCustomer">To customer</param>
        public virtual void MigrateShoppingCart(Customer fromCustomer, Customer toCustomer)
        {
            if (fromCustomer == null)
                throw new ArgumentNullException("fromCustomer");
            if (toCustomer == null)
                throw new ArgumentNullException("toCustomer");

            if (fromCustomer.Id == toCustomer.Id)
                return; //the same customer

            var fromCart = fromCustomer.ShoppingCartItems.ToList();
            for (int i = 0; i < fromCart.Count; i++)
            {
                var sci = fromCart[i];
                AddToCart(toCustomer, sci.ProductVariant, sci.ShoppingCartType,
                    sci.AttributesXml, sci.CustomerEnteredPrice, sci.Quantity, false);
            }
            for (int i = 0; i < fromCart.Count; i++)
            {
                var sci = fromCart[i];
                DeleteShoppingCartItem(sci);
            }
        }

        #endregion
    }
}
