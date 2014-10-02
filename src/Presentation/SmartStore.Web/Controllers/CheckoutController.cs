﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using SmartStore.Core;
using SmartStore.Core.Domain.Common;
using SmartStore.Core.Domain.Customers;
using SmartStore.Core.Domain.Discounts;
using SmartStore.Core.Domain.Orders;
using SmartStore.Core.Domain.Payments;
using SmartStore.Core.Domain.Shipping;
using SmartStore.Core.Plugins;
using SmartStore.Services.Catalog;
using SmartStore.Services.Common;
using SmartStore.Services.Customers;
using SmartStore.Services.Directory;
using SmartStore.Services.Localization;
using SmartStore.Core.Logging;
using SmartStore.Services.Orders;
using SmartStore.Services.Payments;
using SmartStore.Services.Shipping;
using SmartStore.Services.Tax;
using SmartStore.Web.Framework.Controllers;
using SmartStore.Web.Framework.Security;
using SmartStore.Web.Models.Checkout;
using SmartStore.Web.Models.Common;
using SmartStore.Services.Configuration;
using SmartStore.Web.Framework.Plugins;

namespace SmartStore.Web.Controllers
{
    [RequireHttpsByConfigAttribute(SslRequirement.Yes)]
    public partial class CheckoutController : PublicControllerBase
    {
		#region Fields

        private readonly IWorkContext _workContext;
		private readonly IStoreContext _storeContext;
        private readonly IShoppingCartService _shoppingCartService;
        private readonly ILocalizationService _localizationService;
        private readonly ITaxService _taxService;
        private readonly ICurrencyService _currencyService;
        private readonly IPriceFormatter _priceFormatter;
        private readonly IOrderProcessingService _orderProcessingService;
        private readonly ICustomerService _customerService;
        private readonly IGenericAttributeService _genericAttributeService;
        private readonly ICountryService _countryService;
        private readonly IStateProvinceService _stateProvinceService;
        private readonly IShippingService _shippingService;
        private readonly IPaymentService _paymentService;
        private readonly IOrderTotalCalculationService _orderTotalCalculationService;
        private readonly IOrderService _orderService;
        private readonly IWebHelper _webHelper;
        private readonly HttpContextBase _httpContext;
        private readonly IMobileDeviceHelper _mobileDeviceHelper;
		private readonly ISettingService _settingService;

        private readonly OrderSettings _orderSettings;
        private readonly RewardPointsSettings _rewardPointsSettings;
        private readonly PaymentSettings _paymentSettings;
        private readonly AddressSettings _addressSettings;
        private readonly ShoppingCartSettings _shoppingCartSettings;
		private readonly PluginMediator _pluginMediator;

        #endregion

		#region Constructors

		public CheckoutController(IWorkContext workContext, IStoreContext storeContext,
            IShoppingCartService shoppingCartService, ILocalizationService localizationService, 
            ITaxService taxService, ICurrencyService currencyService, 
            IPriceFormatter priceFormatter, IOrderProcessingService orderProcessingService,
            ICustomerService customerService,  IGenericAttributeService genericAttributeService,
            ICountryService countryService,
            IStateProvinceService stateProvinceService, IShippingService shippingService,
			IPaymentService paymentService, 
			IOrderTotalCalculationService orderTotalCalculationService,
            IOrderService orderService, IWebHelper webHelper,
            HttpContextBase httpContext, IMobileDeviceHelper mobileDeviceHelper,
            OrderSettings orderSettings, RewardPointsSettings rewardPointsSettings,
            PaymentSettings paymentSettings, AddressSettings addressSettings,
            ShoppingCartSettings shoppingCartSettings,
			ISettingService settingService,
			PluginMediator pluginMediator)
        {
            this._workContext = workContext;
			this._storeContext = storeContext;
            this._shoppingCartService = shoppingCartService;
            this._localizationService = localizationService;
            this._taxService = taxService;
            this._currencyService = currencyService;
            this._priceFormatter = priceFormatter;
            this._orderProcessingService = orderProcessingService;
            this._customerService = customerService;
            this._genericAttributeService = genericAttributeService;
            this._countryService = countryService;
            this._stateProvinceService = stateProvinceService;
            this._shippingService = shippingService;
            this._paymentService = paymentService;
            this._orderTotalCalculationService = orderTotalCalculationService;
            this._orderService = orderService;
            this._webHelper = webHelper;
            this._httpContext = httpContext;
            this._mobileDeviceHelper = mobileDeviceHelper;
			this._settingService = settingService;

            this._orderSettings = orderSettings;
            this._rewardPointsSettings = rewardPointsSettings;
            this._paymentSettings = paymentSettings;
            this._addressSettings = addressSettings;
            this._shoppingCartSettings = shoppingCartSettings;
			this._pluginMediator = pluginMediator;
        }

        #endregion

        #region Utilities

        [NonAction]
		protected bool IsPaymentWorkflowRequired(IList<OrganizedShoppingCartItem> cart, bool ignoreRewardPoints = false)
        {
            bool result = true;

            //check whether order total equals zero
            decimal? shoppingCartTotalBase = _orderTotalCalculationService.GetShoppingCartTotal(cart, ignoreRewardPoints);
            if (shoppingCartTotalBase.HasValue && shoppingCartTotalBase.Value == decimal.Zero)
                result = false;

            //Check whether paymethod needs workflow
            var processPaymentRequest = _httpContext.Session["OrderPaymentInfo"] as ProcessPaymentRequest;
            if (processPaymentRequest != null)
            {
                result = processPaymentRequest.RequiresPaymentWorkflow;
            }

            return result;
        }

        [NonAction]
        protected CheckoutBillingAddressModel PrepareBillingAddressModel(int? selectedCountryId = null)
        {
            var model = new CheckoutBillingAddressModel();
            //existing addresses
            var addresses = _workContext.CurrentCustomer.Addresses.Where(a => a.Country == null || a.Country.AllowsBilling).ToList();
            foreach (var address in addresses)
            {
                var addressModel = new AddressModel();
                addressModel.PrepareModel(address, 
                    false, 
                    _addressSettings);
                model.ExistingAddresses.Add(addressModel);
            }

            //new address
            model.NewAddress.CountryId = selectedCountryId;
            model.NewAddress.PrepareModel(null,
                false,
                _addressSettings,
                _localizationService,
                _stateProvinceService,
                () => _countryService.GetAllCountriesForBilling());
            return model;
        }

        [NonAction]
        protected CheckoutShippingAddressModel PrepareShippingAddressModel(int? selectedCountryId = null)
        {
            var model = new CheckoutShippingAddressModel();
            //existing addresses
            var addresses = _workContext.CurrentCustomer.Addresses.Where(a => a.Country == null || a.Country.AllowsShipping).ToList();
            foreach (var address in addresses)
            {
                var addressModel = new AddressModel();
                addressModel.PrepareModel(address,
                    false,
                    _addressSettings);
                model.ExistingAddresses.Add(addressModel);
            }

            //new address
            model.NewAddress.CountryId = selectedCountryId;
            model.NewAddress.PrepareModel(null,
                false,
                _addressSettings,
                _localizationService,
                _stateProvinceService,
                () => _countryService.GetAllCountriesForShipping());
            return model;
        }

        [NonAction]
		protected CheckoutShippingMethodModel PrepareShippingMethodModel(IList<OrganizedShoppingCartItem> cart)
        {
            var model = new CheckoutShippingMethodModel();

			var getShippingOptionResponse = _shippingService
				  .GetShippingOptions(cart, _workContext.CurrentCustomer.ShippingAddress,
				  "", _storeContext.CurrentStore.Id);
            if (getShippingOptionResponse.Success)
            {
                //performance optimization. cache returned shipping options.
                //we'll use them later (after a customer has selected an option).
				_genericAttributeService.SaveAttribute(_workContext.CurrentCustomer,
					SystemCustomerAttributeNames.OfferedShippingOptions,
					getShippingOptionResponse.ShippingOptions,
					_storeContext.CurrentStore.Id);

				var shippingMethods = _shippingService.GetAllShippingMethods();
            
                foreach (var shippingOption in getShippingOptionResponse.ShippingOptions)
                {
                    var soModel = new CheckoutShippingMethodModel.ShippingMethodModel()
                    {
                        Name = shippingOption.Name,
                        Description = shippingOption.Description,
                        ShippingRateComputationMethodSystemName = shippingOption.ShippingRateComputationMethodSystemName,
                    };

                    // codehint: sm-add (determine brand image of shipping method)
                    var plugin = PluginManager.ReferencedPlugins.Where(p => p.SystemName == shippingOption.ShippingRateComputationMethodSystemName).FirstOrDefault();
                    if (plugin != null && plugin.BrandImageFileName.HasValue())
                    {
                        soModel.BrandUrl = "~/Plugins/{0}/Content/{1}".FormatInvariant(plugin.SystemName, plugin.BrandImageFileName);
                    }

                    //adjust rate
                    Discount appliedDiscount = null;
                    var shippingTotal = _orderTotalCalculationService.AdjustShippingRate(
						shippingOption.Rate, cart, shippingOption.Name, shippingMethods, out appliedDiscount);

                    decimal rateBase = _taxService.GetShippingPrice(shippingTotal, _workContext.CurrentCustomer);
                    decimal rate = _currencyService.ConvertFromPrimaryStoreCurrency(rateBase, _workContext.WorkingCurrency);
                    soModel.Fee = _priceFormatter.FormatShippingPrice(rate, true);

                    model.ShippingMethods.Add(soModel);
                }

                //find a selected (previously) shipping method
				var selectedShippingOption = _workContext.CurrentCustomer.GetAttribute<ShippingOption>(SystemCustomerAttributeNames.SelectedShippingOption, _storeContext.CurrentStore.Id);
				if (selectedShippingOption != null)
                {
                    var shippingOptionToSelect = model.ShippingMethods.ToList()
						.Find(so => !String.IsNullOrEmpty(so.Name) && so.Name.Equals(selectedShippingOption.Name, StringComparison.InvariantCultureIgnoreCase) &&
						!String.IsNullOrEmpty(so.ShippingRateComputationMethodSystemName) && 
						so.ShippingRateComputationMethodSystemName.Equals(selectedShippingOption.ShippingRateComputationMethodSystemName, StringComparison.InvariantCultureIgnoreCase));
                    if (shippingOptionToSelect != null)
                        shippingOptionToSelect.Selected = true;
                }
                //if no option has been selected, let's do it for the first one
                if (model.ShippingMethods.Where(so => so.Selected).FirstOrDefault() == null)
                {
                    var shippingOptionToSelect = model.ShippingMethods.FirstOrDefault();
                    if (shippingOptionToSelect != null)
                        shippingOptionToSelect.Selected = true;
                }
            }
            else
                foreach (var error in getShippingOptionResponse.Errors)
                    model.Warnings.Add(error);

            return model;
        }

        [NonAction]
		protected CheckoutPaymentMethodModel PreparePaymentMethodModel(IList<OrganizedShoppingCartItem> cart)
        {
            var model = new CheckoutPaymentMethodModel();

            //reward points
            if (_rewardPointsSettings.Enabled && !cart.IsRecurring())
            {
                int rewardPointsBalance = _workContext.CurrentCustomer.GetRewardPointsBalance();
                decimal rewardPointsAmountBase = _orderTotalCalculationService.ConvertRewardPointsToAmount(rewardPointsBalance);
                decimal rewardPointsAmount = _currencyService.ConvertFromPrimaryStoreCurrency(rewardPointsAmountBase, _workContext.WorkingCurrency);
                if (rewardPointsAmount > decimal.Zero)
                {
                    model.DisplayRewardPoints = true;
                    model.RewardPointsAmount = _priceFormatter.FormatPrice(rewardPointsAmount, true, false);
                    model.RewardPointsBalance = rewardPointsBalance;
                }
            }

            var boundPaymentMethods = _paymentService
				.LoadActivePaymentMethods(_workContext.CurrentCustomer.Id, _storeContext.CurrentStore.Id)
				.Where(pm => pm.Value.PaymentMethodType == PaymentMethodType.Standard || pm.Value.PaymentMethodType == PaymentMethodType.Redirection)
                .ToList();
            foreach (var pm in boundPaymentMethods)
            {
				if (cart.IsRecurring() && pm.Value.RecurringPaymentType == RecurringPaymentType.NotSupported)
                    continue;
                
                var pmModel = new CheckoutPaymentMethodModel.PaymentMethodModel()
                {
					Name = _pluginMediator.GetLocalizedFriendlyName(pm.Metadata),
					Description = _pluginMediator.GetLocalizedDescription(pm.Metadata),
                    PaymentMethodSystemName = pm.Metadata.SystemName,
                };
				
				pmModel.BrandUrl = _pluginMediator.GetBrandImageUrl(pm.Metadata);

                //payment method additional fee
				decimal paymentMethodAdditionalFee = _paymentService.GetAdditionalHandlingFee(cart, pm.Metadata.SystemName);
                decimal rateBase = _taxService.GetPaymentMethodAdditionalFee(paymentMethodAdditionalFee, _workContext.CurrentCustomer);
                decimal rate = _currencyService.ConvertFromPrimaryStoreCurrency(rateBase, _workContext.WorkingCurrency);
                if (rate > decimal.Zero)
                    pmModel.Fee = _priceFormatter.FormatPaymentMethodAdditionalFee(rate, true);

                model.PaymentMethods.Add(pmModel);
            }
            
            //find a selected (previously) payment method
			var selectedPaymentMethodSystemName = _workContext.CurrentCustomer.GetAttribute<string>(
				 SystemCustomerAttributeNames.SelectedPaymentMethod,
				 _genericAttributeService, _storeContext.CurrentStore.Id);
			if (!String.IsNullOrEmpty(selectedPaymentMethodSystemName))
            {
                var paymentMethodToSelect = model.PaymentMethods.ToList()
					.Find(pm => pm.PaymentMethodSystemName.Equals(selectedPaymentMethodSystemName, StringComparison.InvariantCultureIgnoreCase));
                if (paymentMethodToSelect != null)
                    paymentMethodToSelect.Selected = true;
            }
            //if no option has been selected, let's do it for the first one
			if (model.PaymentMethods.FirstOrDefault(so => so.Selected) == null)
            {
                var paymentMethodToSelect = model.PaymentMethods.FirstOrDefault();
                if (paymentMethodToSelect != null)
                    paymentMethodToSelect.Selected = true;
            }

            return model;
        }

        [NonAction]
        protected CheckoutPaymentInfoModel PreparePaymentInfoModel(IPaymentMethod paymentMethod)
        {
            var model = new CheckoutPaymentInfoModel();
            string actionName;
            string controllerName;
            RouteValueDictionary routeValues;
            paymentMethod.GetPaymentInfoRoute(out actionName, out controllerName, out routeValues);
            model.PaymentInfoActionName = actionName;
            model.PaymentInfoControllerName = controllerName;
            model.PaymentInfoRouteValues = routeValues;
            model.DisplayOrderTotals = _orderSettings.OnePageCheckoutDisplayOrderTotalsOnPaymentInfoTab;

            return model;
        }

        [NonAction]
		protected CheckoutConfirmModel PrepareConfirmOrderModel(IList<OrganizedShoppingCartItem> cart)
        {
            var model = new CheckoutConfirmModel();
            //min order amount validation
            bool minOrderTotalAmountOk = _orderProcessingService.ValidateMinOrderTotalAmount(cart);
            if (!minOrderTotalAmountOk)
            {
                decimal minOrderTotalAmount = _currencyService.ConvertFromPrimaryStoreCurrency(_orderSettings.MinOrderTotalAmount, _workContext.WorkingCurrency);
                model.MinOrderTotalWarning = string.Format(_localizationService.GetResource("Checkout.MinOrderTotalAmount"), _priceFormatter.FormatPrice(minOrderTotalAmount, true, false));
            }
            //codehint: sm-add
            model.TermsOfServiceEnabled = _orderSettings.TermsOfServiceEnabled;
            model.ShowConfirmOrderLegalHint = _shoppingCartSettings.ShowConfirmOrderLegalHint;
			model.BypassPaymentMethodInfo = _paymentSettings.BypassPaymentMethodInfo;
            return model;
        }

        [NonAction]
        protected bool UseOnePageCheckout()
        {
            bool useMobileDevice = _mobileDeviceHelper.IsMobileDevice()
                && _mobileDeviceHelper.MobileDevicesSupported()
                && !_mobileDeviceHelper.CustomerDontUseMobileVersion();

            //mobile version doesn't support one-page checkout
            if (useMobileDevice)
                return false;

			if (!_orderSettings.OnePageCheckoutEnabled)
				return false;

			var checkoutState = _httpContext.GetCheckoutState();

			if (checkoutState != null && !checkoutState.OnePageCkeckoutEnabled)
				return false;

            return true;
        }

        [NonAction]
        protected bool IsMinimumOrderPlacementIntervalValid(Customer customer)
        {
            //prevent 2 orders being placed within an X seconds time frame
            if (_orderSettings.MinimumOrderPlacementInterval == 0)
                return true;

			var lastOrder = _orderService.SearchOrders(_storeContext.CurrentStore.Id, _workContext.CurrentCustomer.Id,
				 null, null, null, null, null, null, null, null, 0, 1)
                .FirstOrDefault();
			if (lastOrder == null)
                return true;

			var interval = DateTime.UtcNow - lastOrder.CreatedOnUtc;
            return interval.TotalSeconds > _orderSettings.MinimumOrderPlacementInterval;
        }

		private bool IsValidPaymentForm(IPaymentMethod paymentMethod, FormCollection form)
		{
			var paymentControllerType = paymentMethod.GetControllerType();
			var paymentController = DependencyResolver.Current.GetService(paymentControllerType) as PaymentControllerBase;
			var warnings = paymentController.ValidatePaymentForm(form);
				
			foreach (var warning in warnings)
			{
				ModelState.AddModelError("", warning);
			}

			if (ModelState.IsValid)
			{
				var paymentInfo = paymentController.GetPaymentInfo(form);
				_httpContext.Session["OrderPaymentInfo"] = paymentInfo;

				return true;
			}
			return false;
		}

        #endregion

        #region Methods (multistep checkout)

        public ActionResult Index()
        {
			var cart = _workContext.CurrentCustomer.GetCartItems(ShoppingCartType.ShoppingCart, _storeContext.CurrentStore.Id);

			if (cart.Count == 0)
                return RedirectToRoute("ShoppingCart");

            if ((_workContext.CurrentCustomer.IsGuest() && !_orderSettings.AnonymousCheckoutAllowed))
                return new HttpUnauthorizedResult();

            //reset checkout data
            _customerService.ResetCheckoutData(_workContext.CurrentCustomer, _storeContext.CurrentStore.Id);

            //validation (cart)
			var checkoutAttributesXml = _workContext.CurrentCustomer.GetAttribute<string>(SystemCustomerAttributeNames.CheckoutAttributes, _genericAttributeService);
			var scWarnings = _shoppingCartService.GetShoppingCartWarnings(cart, checkoutAttributesXml, true);
            if (scWarnings.Count > 0)
                return RedirectToRoute("ShoppingCart");

            //validation (each shopping cart item)
            foreach (var sci in cart)
            {
                var sciWarnings = _shoppingCartService.GetShoppingCartItemWarnings(_workContext.CurrentCustomer,
                    sci.Item.ShoppingCartType,
                    sci.Item.Product,
					sci.Item.StoreId,
                    sci.Item.AttributesXml,
                    sci.Item.CustomerEnteredPrice,
                    sci.Item.Quantity,
                    false, childItems: sci.ChildItems);
                if (sciWarnings.Count > 0)
                    return RedirectToRoute("ShoppingCart");
            }

            if (UseOnePageCheckout())
                return RedirectToRoute("CheckoutOnePage");
            else
                return RedirectToAction("BillingAddress");
        }


        public ActionResult BillingAddress()
        {
            //validation
			var cart = _workContext.CurrentCustomer.GetCartItems(ShoppingCartType.ShoppingCart, _storeContext.CurrentStore.Id);

			if (cart.Count == 0)
                return RedirectToRoute("ShoppingCart");

            if (UseOnePageCheckout())
                return RedirectToRoute("CheckoutOnePage");

            if ((_workContext.CurrentCustomer.IsGuest() && !_orderSettings.AnonymousCheckoutAllowed))
                return new HttpUnauthorizedResult();

            //model
            var model = PrepareBillingAddressModel();
            return View(model);
        }
        public ActionResult SelectBillingAddress(int addressId)
        {
            var address = _workContext.CurrentCustomer.Addresses.Where(a => a.Id == addressId).FirstOrDefault();
            if (address == null)
				return RedirectToAction("BillingAddress");

            _workContext.CurrentCustomer.BillingAddress = address;
            _customerService.UpdateCustomer(_workContext.CurrentCustomer);

			return RedirectToAction("ShippingAddress");
        }
        [HttpPost, ActionName("BillingAddress")]
        [FormValueRequired("nextstep")]
        public ActionResult NewBillingAddress(CheckoutBillingAddressModel model)
        {
            //validation
			var cart = _workContext.CurrentCustomer.GetCartItems(ShoppingCartType.ShoppingCart, _storeContext.CurrentStore.Id);

			if (cart.Count == 0)
                return RedirectToRoute("ShoppingCart");

            if (UseOnePageCheckout())
                return RedirectToRoute("CheckoutOnePage");

            if ((_workContext.CurrentCustomer.IsGuest() && !_orderSettings.AnonymousCheckoutAllowed))
                return new HttpUnauthorizedResult();

            if (ModelState.IsValid)
            {
                var address = model.NewAddress.ToEntity();
                address.CreatedOnUtc = DateTime.UtcNow;
                //some validation
                if (address.CountryId == 0)
                    address.CountryId = null;
                if (address.StateProvinceId == 0)
                    address.StateProvinceId = null;
                _workContext.CurrentCustomer.Addresses.Add(address);
                _workContext.CurrentCustomer.BillingAddress = address;
                _customerService.UpdateCustomer(_workContext.CurrentCustomer);

				return RedirectToAction("ShippingAddress");
            }


            //If we got this far, something failed, redisplay form
            model = PrepareBillingAddressModel(model.NewAddress.CountryId);
            return View(model);
        }

        public ActionResult ShippingAddress()
        {
            //validation
			var cart = _workContext.CurrentCustomer.GetCartItems(ShoppingCartType.ShoppingCart, _storeContext.CurrentStore.Id);

			if (cart.Count == 0)
                return RedirectToRoute("ShoppingCart");

            if (UseOnePageCheckout())
                return RedirectToRoute("CheckoutOnePage");

            if ((_workContext.CurrentCustomer.IsGuest() && !_orderSettings.AnonymousCheckoutAllowed))
                return new HttpUnauthorizedResult();

            if (!cart.RequiresShipping())
            {
                _workContext.CurrentCustomer.ShippingAddress = null;
                _customerService.UpdateCustomer(_workContext.CurrentCustomer);
                return RedirectToAction("ShippingMethod");
            }

            //model
            var model = PrepareShippingAddressModel();
            return View(model);
        }
        public ActionResult SelectShippingAddress(int addressId)
        {
            var address = _workContext.CurrentCustomer.Addresses.Where(a => a.Id == addressId).FirstOrDefault();
            if (address == null)
				return RedirectToAction("ShippingAddress");

            _workContext.CurrentCustomer.ShippingAddress = address;
            _customerService.UpdateCustomer(_workContext.CurrentCustomer);

			return RedirectToAction("ShippingMethod");
        }
        [HttpPost, ActionName("ShippingAddress")]
        [FormValueRequired("nextstep")]
        public ActionResult NewShippingAddress(CheckoutShippingAddressModel model)
        {
            //validation
			var cart = _workContext.CurrentCustomer.GetCartItems(ShoppingCartType.ShoppingCart, _storeContext.CurrentStore.Id);

			if (cart.Count == 0)
                return RedirectToRoute("ShoppingCart");

            if (UseOnePageCheckout())
                return RedirectToRoute("CheckoutOnePage");

            if ((_workContext.CurrentCustomer.IsGuest() && !_orderSettings.AnonymousCheckoutAllowed))
                return new HttpUnauthorizedResult();

            if (!cart.RequiresShipping())
            {
                _workContext.CurrentCustomer.ShippingAddress = null;
                _customerService.UpdateCustomer(_workContext.CurrentCustomer);
				return RedirectToAction("ShippingMethod");
            }

            if (ModelState.IsValid)
            {
                var address = model.NewAddress.ToEntity();
                address.CreatedOnUtc = DateTime.UtcNow;
                //some validation
                if (address.CountryId == 0)
                    address.CountryId = null;
                if (address.StateProvinceId == 0)
                    address.StateProvinceId = null;
                _workContext.CurrentCustomer.Addresses.Add(address);
                _workContext.CurrentCustomer.ShippingAddress = address;
                _customerService.UpdateCustomer(_workContext.CurrentCustomer);

				return RedirectToAction("ShippingMethod");
            }


            //If we got this far, something failed, redisplay form
            model = PrepareShippingAddressModel(model.NewAddress.CountryId);
            return View(model);
        }
        

        public ActionResult ShippingMethod()
        {
            //validation
			var cart = _workContext.CurrentCustomer.GetCartItems(ShoppingCartType.ShoppingCart, _storeContext.CurrentStore.Id);

			if (cart.Count == 0)
                return RedirectToRoute("ShoppingCart");

            if (UseOnePageCheckout())
                return RedirectToRoute("CheckoutOnePage");

            if ((_workContext.CurrentCustomer.IsGuest() && !_orderSettings.AnonymousCheckoutAllowed))
                return new HttpUnauthorizedResult();

            if (!cart.RequiresShipping())
            {
				_genericAttributeService.SaveAttribute<ShippingOption>(_workContext.CurrentCustomer, SystemCustomerAttributeNames.SelectedShippingOption, null, _storeContext.CurrentStore.Id);
                return RedirectToAction("PaymentMethod");
            }
            
            
            //model
            var model = PrepareShippingMethodModel(cart);
            return View(model);
        }
        [HttpPost, ActionName("ShippingMethod")]
        [FormValueRequired("nextstep")]
        [ValidateInput(false)]
        public ActionResult SelectShippingMethod(string shippingoption)
        {
            //validation
			var cart = _workContext.CurrentCustomer.GetCartItems(ShoppingCartType.ShoppingCart, _storeContext.CurrentStore.Id);

			if (cart.Count == 0)
                return RedirectToRoute("ShoppingCart");

            if (UseOnePageCheckout())
                return RedirectToRoute("CheckoutOnePage");

            if ((_workContext.CurrentCustomer.IsGuest() && !_orderSettings.AnonymousCheckoutAllowed))
                return new HttpUnauthorizedResult();

            if (!cart.RequiresShipping())
            {
				_genericAttributeService.SaveAttribute<ShippingOption>(_workContext.CurrentCustomer,
					 SystemCustomerAttributeNames.SelectedShippingOption, null, _storeContext.CurrentStore.Id);
				return RedirectToAction("PaymentMethod");
            }

            //parse selected method 
            if (String.IsNullOrEmpty(shippingoption))
                return ShippingMethod();
            var splittedOption = shippingoption.Split(new string[] { "___" }, StringSplitOptions.RemoveEmptyEntries);
            if (splittedOption.Length != 2)
                return ShippingMethod();
            string selectedName = splittedOption[0];
            string shippingRateComputationMethodSystemName = splittedOption[1];
            
            //find it
            //performance optimization. try cache first
			var shippingOptions = _workContext.CurrentCustomer.GetAttribute<List<ShippingOption>>(SystemCustomerAttributeNames.OfferedShippingOptions, _storeContext.CurrentStore.Id);
            if (shippingOptions == null || shippingOptions.Count == 0)
            {
                //not found? let's load them using shipping service
                shippingOptions = _shippingService
					.GetShippingOptions(cart, _workContext.CurrentCustomer.ShippingAddress, shippingRateComputationMethodSystemName, _storeContext.CurrentStore.Id)
                    .ShippingOptions
                    .ToList();
            }
            else
            {
                //loaded cached results. let's filter result by a chosen shipping rate computation method
                shippingOptions = shippingOptions.Where(so => so.ShippingRateComputationMethodSystemName.Equals(shippingRateComputationMethodSystemName, StringComparison.InvariantCultureIgnoreCase))
                    .ToList();
            }

            var shippingOption = shippingOptions
                .Find(so => !String.IsNullOrEmpty(so.Name) && so.Name.Equals(selectedName, StringComparison.InvariantCultureIgnoreCase));
            if (shippingOption == null)
                return ShippingMethod();

            //save
			_genericAttributeService.SaveAttribute(_workContext.CurrentCustomer, SystemCustomerAttributeNames.SelectedShippingOption, shippingOption, _storeContext.CurrentStore.Id);

			return RedirectToAction("PaymentMethod");
        }
        
        
        public ActionResult PaymentMethod()
        {
            //validation
			var cart = _workContext.CurrentCustomer.GetCartItems(ShoppingCartType.ShoppingCart, _storeContext.CurrentStore.Id);

			if (cart.Count == 0)
                return RedirectToRoute("ShoppingCart");

            if (UseOnePageCheckout())
                return RedirectToRoute("CheckoutOnePage");

            if ((_workContext.CurrentCustomer.IsGuest() && !_orderSettings.AnonymousCheckoutAllowed))
                return new HttpUnauthorizedResult();

            //Check whether payment workflow is required
            //we ignore reward points during cart total calculation
            bool isPaymentWorkflowRequired = IsPaymentWorkflowRequired(cart, true);
            if (!isPaymentWorkflowRequired)
            {
                //TODO: get all paymenthods, when there's only one set it to be the selected
				//_genericAttributeService.SaveAttribute<string>(_workContext.CurrentCustomer, SystemCustomerAttributeNames.SelectedPaymentMethod, null, _storeContext.CurrentStore.Id);
                return RedirectToAction("PaymentInfo");
            }

            //model
            var paymentMethodModel = PreparePaymentMethodModel(cart);

            if (_paymentSettings.BypassPaymentMethodSelectionIfOnlyOne &&
                paymentMethodModel.PaymentMethods.Count == 1 && !paymentMethodModel.DisplayRewardPoints)
            {
                //if we have only one payment method and reward points are disabled or the current customer doesn't have any reward points
                //so customer doesn't have to choose a payment method

				_genericAttributeService.SaveAttribute<string>(_workContext.CurrentCustomer,
					SystemCustomerAttributeNames.SelectedPaymentMethod,
					paymentMethodModel.PaymentMethods[0].PaymentMethodSystemName,
					_storeContext.CurrentStore.Id);
				return RedirectToAction("PaymentInfo");
            }

            return View(paymentMethodModel);
        }
        [HttpPost, ActionName("PaymentMethod")]
        [FormValueRequired("nextstep")]
        [ValidateInput(false)]
        public ActionResult SelectPaymentMethod(string paymentmethod, CheckoutPaymentMethodModel model)
        {
            //validation
			var cart = _workContext.CurrentCustomer.GetCartItems(ShoppingCartType.ShoppingCart, _storeContext.CurrentStore.Id);

			if (cart.Count == 0)
                return RedirectToRoute("ShoppingCart");

            if (UseOnePageCheckout())
                return RedirectToRoute("CheckoutOnePage");

            if ((_workContext.CurrentCustomer.IsGuest() && !_orderSettings.AnonymousCheckoutAllowed))
                return new HttpUnauthorizedResult();

            //reward points
			if (_rewardPointsSettings.Enabled)
			{
				_genericAttributeService.SaveAttribute(_workContext.CurrentCustomer,
					SystemCustomerAttributeNames.UseRewardPointsDuringCheckout, model.UseRewardPoints,
					_storeContext.CurrentStore.Id);
			}

            //Check whether payment workflow is required
            bool isPaymentWorkflowRequired = IsPaymentWorkflowRequired(cart);
            if (!isPaymentWorkflowRequired)
            {
				_genericAttributeService.SaveAttribute<string>(_workContext.CurrentCustomer,
					  SystemCustomerAttributeNames.SelectedPaymentMethod, null, _storeContext.CurrentStore.Id);
				return RedirectToAction("PaymentInfo");
            }
            //payment method 
            if (String.IsNullOrEmpty(paymentmethod))
                return PaymentMethod();

			var paymentMethodProvider = _paymentService.LoadPaymentMethodBySystemName(paymentmethod, true, _storeContext.CurrentStore.Id);
			if (paymentMethodProvider == null)
                return PaymentMethod();

            //save
			_genericAttributeService.SaveAttribute<string>(_workContext.CurrentCustomer,
				 SystemCustomerAttributeNames.SelectedPaymentMethod, paymentmethod, _storeContext.CurrentStore.Id);

			return RedirectToAction("PaymentInfo");
        }


        public ActionResult PaymentInfo()
        {
            //validation
			var cart = _workContext.CurrentCustomer.GetCartItems(ShoppingCartType.ShoppingCart, _storeContext.CurrentStore.Id);

			if (cart.Count == 0)
                return RedirectToRoute("ShoppingCart");

            if (UseOnePageCheckout())
                return RedirectToRoute("CheckoutOnePage");

            if ((_workContext.CurrentCustomer.IsGuest() && !_orderSettings.AnonymousCheckoutAllowed))
                return new HttpUnauthorizedResult();

            //Check whether payment workflow is required
            bool isPaymentWorkflowRequired = IsPaymentWorkflowRequired(cart);
            if (!isPaymentWorkflowRequired)
            {
                return RedirectToAction("Confirm");
            }

			//load payment method
			var paymentMethodSystemName = _workContext.CurrentCustomer.GetAttribute<string>(
				SystemCustomerAttributeNames.SelectedPaymentMethod,
				_genericAttributeService, _storeContext.CurrentStore.Id);
			var paymentMethod = _paymentService.LoadPaymentMethodBySystemName(paymentMethodSystemName);
            if (paymentMethod == null)
				return RedirectToAction("PaymentMethod");

            RouteInfo routeinfo = paymentMethod.Value.GetPaymentInfoHandlerRoute();
            if (routeinfo != null)
            {
                return new RedirectToRouteResult(routeinfo.RouteValues);
            }

			if (_paymentSettings.BypassPaymentMethodInfo && IsValidPaymentForm(paymentMethod.Value, new FormCollection()))
			{
				return RedirectToAction("Confirm");
			}

			var model = PreparePaymentInfoModel(paymentMethod.Value);

            return View(model);
        }
        [HttpPost, ActionName("PaymentInfo")]
        [FormValueRequired("nextstep")]
        [ValidateInput(false)]
        public ActionResult EnterPaymentInfo(FormCollection form)
        {
            //validation
			var cart = _workContext.CurrentCustomer.GetCartItems(ShoppingCartType.ShoppingCart, _storeContext.CurrentStore.Id);

			if (cart.Count == 0)
                return RedirectToRoute("ShoppingCart");

            if (UseOnePageCheckout())
                return RedirectToRoute("CheckoutOnePage");

            if ((_workContext.CurrentCustomer.IsGuest() && !_orderSettings.AnonymousCheckoutAllowed))
                return new HttpUnauthorizedResult();

            //Check whether payment workflow is required
            bool isPaymentWorkflowRequired = IsPaymentWorkflowRequired(cart);
            if (!isPaymentWorkflowRequired)
            {
				return RedirectToAction("Confirm");
            }

			//load payment method
			var paymentMethodSystemName = _workContext.CurrentCustomer.GetAttribute<string>(
				SystemCustomerAttributeNames.SelectedPaymentMethod,
				_genericAttributeService, _storeContext.CurrentStore.Id);
			var paymentMethod = _paymentService.LoadPaymentMethodBySystemName(paymentMethodSystemName);
            if (paymentMethod == null)
				return RedirectToAction("PaymentMethod");

			if (IsValidPaymentForm(paymentMethod.Value, form))
			{
				return RedirectToAction("Confirm");
			}

            //If we got this far, something failed, redisplay form
            //model
            var model = PreparePaymentInfoModel(paymentMethod.Value);
            return View(model);
        }
        

        public ActionResult Confirm()
        {
            //validation
			var cart = _workContext.CurrentCustomer.GetCartItems(ShoppingCartType.ShoppingCart, _storeContext.CurrentStore.Id);

			if (cart.Count == 0)
                return RedirectToRoute("ShoppingCart");

            if (UseOnePageCheckout())
                return RedirectToRoute("CheckoutOnePage");

            if ((_workContext.CurrentCustomer.IsGuest() && !_orderSettings.AnonymousCheckoutAllowed))
                return new HttpUnauthorizedResult();

            var model = PrepareConfirmOrderModel(cart);

			//if (TempData["ConfirmOrderWarnings"] != null)
			//	model.Warnings.AddRange(TempData["ConfirmOrderWarnings"] as IList<string>);

            return View(model);
        }
        [HttpPost, ActionName("Confirm")]
        [ValidateInput(false)]
        public ActionResult ConfirmOrder()
        {
            //validation
			var cart = _workContext.CurrentCustomer.GetCartItems(ShoppingCartType.ShoppingCart, _storeContext.CurrentStore.Id);

			if (cart.Count == 0)
                return RedirectToRoute("ShoppingCart");

            if (UseOnePageCheckout())
                return RedirectToRoute("CheckoutOnePage");

            if ((_workContext.CurrentCustomer.IsGuest() && !_orderSettings.AnonymousCheckoutAllowed))
                return new HttpUnauthorizedResult();


            //model
            var model = new CheckoutConfirmModel();
            try
            {
                var processPaymentRequest = _httpContext.Session["OrderPaymentInfo"] as ProcessPaymentRequest;
                if (processPaymentRequest == null)
                {
                    //Check whether payment workflow is required
                    if (IsPaymentWorkflowRequired(cart))
						return RedirectToAction("PaymentInfo");
                    else
                        processPaymentRequest = new ProcessPaymentRequest();
                }
                
                //prevent 2 orders being placed within an X seconds time frame
                if (!IsMinimumOrderPlacementIntervalValid(_workContext.CurrentCustomer))
                    throw new Exception(_localizationService.GetResource("Checkout.MinOrderPlacementInterval"));

                //place order
				processPaymentRequest.StoreId = _storeContext.CurrentStore.Id;
                processPaymentRequest.CustomerId = _workContext.CurrentCustomer.Id;
				processPaymentRequest.PaymentMethodSystemName = _workContext.CurrentCustomer.GetAttribute<string>(
					 SystemCustomerAttributeNames.SelectedPaymentMethod, _genericAttributeService, _storeContext.CurrentStore.Id);

                var placeOrderResult = _orderProcessingService.PlaceOrder(processPaymentRequest);

                if (placeOrderResult.Success)
                {
                    _httpContext.Session["OrderPaymentInfo"] = null;
                    var postProcessPaymentRequest = new PostProcessPaymentRequest()
                    {
                        Order = placeOrderResult.PlacedOrder
                    };
                    _paymentService.PostProcessPayment(postProcessPaymentRequest);

					_httpContext.RemoveCheckoutState();

                    if (_webHelper.IsRequestBeingRedirected || _webHelper.IsPostBeingDone)
                    {
                        //redirection or POST has been done in PostProcessPayment
                        return Content("Redirected");
                    }
                    else
                    {
                        //if no redirection has been done (to a third-party payment page)
                        //theoretically it's not possible
                        return RedirectToAction("Completed");
                    }
                }
                else
                {
                    foreach (var error in placeOrderResult.Errors)
                        model.Warnings.Add(error);

					_httpContext.RemoveCheckoutState();
                }
            }
            catch (Exception exc)
            {
				Logger.Warning(exc.Message, exc);
                model.Warnings.Add(exc.Message);
            }

            //If we got this far, something failed, redisplay form

			//if (model.Warnings.Count > 0)
			//	TempData["ConfirmOrderWarnings"] = model.Warnings;

			//return RedirectToRoute("CheckoutConfirm");
            return View(model);
        }


        public ActionResult Completed()
        {
            //validation
            if ((_workContext.CurrentCustomer.IsGuest() && !_orderSettings.AnonymousCheckoutAllowed))
                return new HttpUnauthorizedResult();

            //model
            var model = new CheckoutCompletedModel();

			var order = _orderService.SearchOrders(_storeContext.CurrentStore.Id, _workContext.CurrentCustomer.Id,
				null, null, null, null, null, null, null, null, 0, 1).FirstOrDefault();

			if (order == null || order.Deleted || _workContext.CurrentCustomer.Id != order.CustomerId)
			{
				return HttpNotFound();
			}

			//disable "order completed" page?
			if (_orderSettings.DisableOrderCompletedPage)
			{
				return RedirectToAction("Details", "Order", new { id = order.Id });
			}

			model.OrderId = order.Id;
            model.OrderNumber = order.GetOrderNumber();

            return View(model);
        }
        
        [ChildActionOnly]
        public ActionResult CheckoutProgress(CheckoutProgressStep step)
        {
            var model = new CheckoutProgressModel() {CheckoutProgressStep = step};
            return PartialView(model);
        }
        #endregion

        #region Methods (one page checkout)

        public ActionResult OnePageCheckout()
        {
            //validation
			var cart = _workContext.CurrentCustomer.GetCartItems(ShoppingCartType.ShoppingCart, _storeContext.CurrentStore.Id);

			if (cart.Count == 0)
                return RedirectToRoute("ShoppingCart");

            if (!UseOnePageCheckout())
                return RedirectToAction("Index");

            if ((_workContext.CurrentCustomer.IsGuest() && !_orderSettings.AnonymousCheckoutAllowed))
                return new HttpUnauthorizedResult();

            var model = new OnePageCheckoutModel()
            {
                ShippingRequired = cart.RequiresShipping()
            };
            return View(model);
        }

        [ChildActionOnly]
        public ActionResult OpcBillingForm()
        {
            var billingAddressModel = PrepareBillingAddressModel();
            return PartialView("OpcBillingAddress", billingAddressModel);
        }

        [ValidateInput(false)]
        public ActionResult OpcSaveBilling(FormCollection form)
        {
            try
            {
                //validation
				var cart = _workContext.CurrentCustomer.GetCartItems(ShoppingCartType.ShoppingCart, _storeContext.CurrentStore.Id);

				if (cart.Count == 0)
                    throw new Exception("Your cart is empty");

                if (!UseOnePageCheckout())
                    throw new Exception("One page checkout is disabled");

                if ((_workContext.CurrentCustomer.IsGuest() && !_orderSettings.AnonymousCheckoutAllowed))
                    throw new Exception("Anonymous checkout is not allowed");

                int billingAddressId = 0;
                int.TryParse(form["billing_address_id"], out billingAddressId);

                if (billingAddressId > 0)
                {
                    //existing address
                    var address = _workContext.CurrentCustomer.Addresses.Where(a => a.Id == billingAddressId).FirstOrDefault();
                    if (address == null)
                        throw new Exception("Address can't be loaded");

                    _workContext.CurrentCustomer.BillingAddress = address;
                    _customerService.UpdateCustomer(_workContext.CurrentCustomer);
                }
                else
                {
                    //new address
                    var model = new CheckoutBillingAddressModel();
                    TryUpdateModel(model.NewAddress, "BillingNewAddress");
                    //validate model
                    TryValidateModel(model.NewAddress);
                    if (!ModelState.IsValid)
                    {
                        //model is not valid. redisplay the form with errors
                        var billingAddressModel = PrepareBillingAddressModel(model.NewAddress.CountryId);
                        billingAddressModel.NewAddressPreselected = true;
                        return Json(new
                        {
                            update_section = new UpdateSectionJsonModel()
                            {
                                name = "billing",
                                html = this.RenderPartialViewToString("OpcBillingAddress", billingAddressModel)
                            }
                        });
                    }

                    //try to find an address with the same values (don't duplicate records)
                    var address = _workContext.CurrentCustomer.Addresses.ToList().FindAddress(
                        model.NewAddress.FirstName, model.NewAddress.LastName, model.NewAddress.PhoneNumber,
                        model.NewAddress.Email, model.NewAddress.FaxNumber, model.NewAddress.Company,
                        model.NewAddress.Address1, model.NewAddress.Address2, model.NewAddress.City,
                        model.NewAddress.StateProvinceId, model.NewAddress.ZipPostalCode, model.NewAddress.CountryId);
                    if (address == null)
                    {
                        //address is not found. let's create a new one
                        address = model.NewAddress.ToEntity();
                        address.CreatedOnUtc = DateTime.UtcNow;
                        //some validation
                        if (address.CountryId == 0)
                            address.CountryId = null;
                        if (address.StateProvinceId == 0)
                            address.StateProvinceId = null;
						if (address.CountryId.HasValue && address.CountryId.Value > 0)
						{
							address.Country = _countryService.GetCountryById(address.CountryId.Value);
						}
                        _workContext.CurrentCustomer.Addresses.Add(address);
                    }
                    _workContext.CurrentCustomer.BillingAddress = address;
                    _customerService.UpdateCustomer(_workContext.CurrentCustomer);
                }

                if (cart.RequiresShipping())
                {
                    //shipping is required
                    var shippingAddressModel = PrepareShippingAddressModel();
                    return Json(new
                    {
                        update_section = new UpdateSectionJsonModel()
                        {
                            name = "shipping",
                            html = this.RenderPartialViewToString("OpcShippingAddress", shippingAddressModel)
                        },
                        goto_section = "shipping"
                    });
                }
                else
                {
                    //shipping is not required
					_genericAttributeService.SaveAttribute<ShippingOption>(_workContext.CurrentCustomer, SystemCustomerAttributeNames.SelectedShippingOption, null, _storeContext.CurrentStore.Id);


                    //Check whether payment workflow is required
                    //we ignore reward points during cart total calculation
                    bool isPaymentWorkflowRequired = IsPaymentWorkflowRequired(cart, true);
                    if (isPaymentWorkflowRequired)
                    {
                        //payment is required
                        var paymentMethodModel = PreparePaymentMethodModel(cart);

                        if (_paymentSettings.BypassPaymentMethodSelectionIfOnlyOne &&
                            paymentMethodModel.PaymentMethods.Count == 1 && !paymentMethodModel.DisplayRewardPoints)
                        {
                            //if we have only one payment method and reward points are disabled or the current customer doesn't have any reward points
                            //so customer doesn't have to choose a payment method
							var selectedPaymentMethodSystemName = paymentMethodModel.PaymentMethods[0].PaymentMethodSystemName;
							_genericAttributeService.SaveAttribute<string>(_workContext.CurrentCustomer,
								SystemCustomerAttributeNames.SelectedPaymentMethod,
								selectedPaymentMethodSystemName, _storeContext.CurrentStore.Id);

							var paymentMethodInst = _paymentService.LoadPaymentMethodBySystemName(selectedPaymentMethodSystemName, true, _storeContext.CurrentStore.Id);
							if (paymentMethodInst == null)
                                throw new Exception("Selected payment method can't be parsed");

                            var paymenInfoModel = PreparePaymentInfoModel(paymentMethodInst.Value);
                            return Json(new
                            {
                                update_section = new UpdateSectionJsonModel()
                                {
                                    name = "payment-info",
                                    html = this.RenderPartialViewToString("OpcPaymentInfo", paymenInfoModel)
                                },
                                goto_section = "payment_info"
                            });
                        }
                        else
                        {
                            //customer have to choose a payment method
                            return Json(new
                            {
                                update_section = new UpdateSectionJsonModel()
                                {
                                    name = "payment-method",
                                    html = this.RenderPartialViewToString("OpcPaymentMethods", paymentMethodModel)
                                },
                                goto_section = "payment_method"
                            });
                        }
                    }
                    else
                    {
                        //payment is not required
						_genericAttributeService.SaveAttribute<string>(_workContext.CurrentCustomer,
							 SystemCustomerAttributeNames.SelectedPaymentMethod, null, _storeContext.CurrentStore.Id);

                        var confirmOrderModel = PrepareConfirmOrderModel(cart);
                        return Json(new
                        {
                            update_section = new UpdateSectionJsonModel()
                            {
                                name = "confirm-order",
                                html = this.RenderPartialViewToString("OpcConfirmOrder", confirmOrderModel)
                            },
                            goto_section = "confirm_order"
                        });
                    }
                }
            }
            catch (Exception exc)
            {
				Logger.Warning(exc.Message, exc, _workContext.CurrentCustomer);
                return Json(new { error = 1, message = exc.Message });
            }
        }

        [ValidateInput(false)]
        public ActionResult OpcSaveShipping(FormCollection form)
        {
            try
            {
                //validation
				var cart = _workContext.CurrentCustomer.GetCartItems(ShoppingCartType.ShoppingCart, _storeContext.CurrentStore.Id);

				if (cart.Count == 0)
                    throw new Exception("Your cart is empty");

                if (!UseOnePageCheckout())
                    throw new Exception("One page checkout is disabled");

                if ((_workContext.CurrentCustomer.IsGuest() && !_orderSettings.AnonymousCheckoutAllowed))
                    throw new Exception("Anonymous checkout is not allowed");

                if (!cart.RequiresShipping())
                    throw new Exception("Shipping is not required");

                int shippingAddressId = 0;
                int.TryParse(form["shipping_address_id"], out shippingAddressId);

                if (shippingAddressId > 0)
                {
                    //existing address
                    var address = _workContext.CurrentCustomer.Addresses.Where(a => a.Id == shippingAddressId).FirstOrDefault();
                    if (address == null)
                        throw new Exception("Address can't be loaded");

                    _workContext.CurrentCustomer.ShippingAddress = address;
                    _customerService.UpdateCustomer(_workContext.CurrentCustomer);
                }
                else
                {
                    //new address
                    var model = new CheckoutShippingAddressModel();
                    TryUpdateModel(model.NewAddress, "ShippingNewAddress");
                    //validate model
                    TryValidateModel(model.NewAddress);
                    if (!ModelState.IsValid)
                    {
                        //model is not valid. redisplay the form with errors
                        var shippingAddressModel = PrepareShippingAddressModel(model.NewAddress.CountryId);
                        shippingAddressModel.NewAddressPreselected = true;
                        return Json(new
                        {
                            update_section = new UpdateSectionJsonModel()
                            {
                                name = "shipping",
                                html = this.RenderPartialViewToString("OpcShippingAddress", shippingAddressModel)
                            }
                        });
                    }

                    //try to find an address with the same values (don't duplicate records)
                    var address = _workContext.CurrentCustomer.Addresses.ToList().FindAddress(
                        model.NewAddress.FirstName, model.NewAddress.LastName, model.NewAddress.PhoneNumber,
                        model.NewAddress.Email, model.NewAddress.FaxNumber, model.NewAddress.Company,
                        model.NewAddress.Address1, model.NewAddress.Address2, model.NewAddress.City,
                        model.NewAddress.StateProvinceId, model.NewAddress.ZipPostalCode, model.NewAddress.CountryId);
                    if (address == null)
                    {
                        address = model.NewAddress.ToEntity();
                        address.CreatedOnUtc = DateTime.UtcNow;
                        //some validation
                        if (address.CountryId == 0)
                            address.CountryId = null;
                        if (address.StateProvinceId == 0)
                            address.StateProvinceId = null;
                        _workContext.CurrentCustomer.Addresses.Add(address);
                    }
                    _workContext.CurrentCustomer.ShippingAddress = address;
                    _customerService.UpdateCustomer(_workContext.CurrentCustomer);
                }

                var shippingMethodModel = PrepareShippingMethodModel(cart);
                return Json(new
                {
                    update_section = new UpdateSectionJsonModel()
                    {
                        name = "shipping-method",
                        html = this.RenderPartialViewToString("OpcShippingMethods", shippingMethodModel)
                    },
                    goto_section = "shipping_method"
                });
            }
            catch (Exception exc)
            {
				Logger.Warning(exc.Message, exc, _workContext.CurrentCustomer);
                return Json(new { error = 1, message = exc.Message });
            }
        }

        [ValidateInput(false)]
        public ActionResult OpcSaveShippingMethod(FormCollection form)
        {
            try
            {
                //validation
				var cart = _workContext.CurrentCustomer.GetCartItems(ShoppingCartType.ShoppingCart, _storeContext.CurrentStore.Id);

				if (cart.Count == 0)
                    throw new Exception("Your cart is empty");

                if (!UseOnePageCheckout())
                    throw new Exception("One page checkout is disabled");

                if ((_workContext.CurrentCustomer.IsGuest() && !_orderSettings.AnonymousCheckoutAllowed))
                    throw new Exception("Anonymous checkout is not allowed");
                
                if (!cart.RequiresShipping())
                    throw new Exception("Shipping is not required");

                //parse selected method 
                string shippingoption = form["shippingoption"];
                if (String.IsNullOrEmpty(shippingoption))
                    throw new Exception("Selected shipping method can't be parsed");
                var splittedOption = shippingoption.Split(new string[] { "___" }, StringSplitOptions.RemoveEmptyEntries);
                if (splittedOption.Length != 2)
                    throw new Exception("Selected shipping method can't be parsed");
                string selectedName = splittedOption[0];
                string shippingRateComputationMethodSystemName = splittedOption[1];
                
                //find it
                //performance optimization. try cache first
				var shippingOptions = _workContext.CurrentCustomer.GetAttribute<List<ShippingOption>>(SystemCustomerAttributeNames.OfferedShippingOptions, _storeContext.CurrentStore.Id);
                if (shippingOptions == null || shippingOptions.Count == 0)
                {
                    //not found? let's load them using shipping service
                    shippingOptions = _shippingService
						.GetShippingOptions(cart, _workContext.CurrentCustomer.ShippingAddress, shippingRateComputationMethodSystemName, _storeContext.CurrentStore.Id)
                        .ShippingOptions
                        .ToList();
                }
                else
                {
                    //loaded cached results. let's filter result by a chosen shipping rate computation method
                    shippingOptions = shippingOptions.Where(so => so.ShippingRateComputationMethodSystemName.Equals(shippingRateComputationMethodSystemName, StringComparison.InvariantCultureIgnoreCase))
                        .ToList();
                }
                
                var shippingOption = shippingOptions
                    .Find(so => !String.IsNullOrEmpty(so.Name) && so.Name.Equals(selectedName, StringComparison.InvariantCultureIgnoreCase));
                if (shippingOption == null)
                    throw new Exception("Selected shipping method can't be loaded");

                //save
				_genericAttributeService.SaveAttribute(_workContext.CurrentCustomer, SystemCustomerAttributeNames.SelectedShippingOption, shippingOption, _storeContext.CurrentStore.Id);


                //Check whether payment workflow is required
                //we ignore reward points during cart total calculation
                bool isPaymentWorkflowRequired = IsPaymentWorkflowRequired(cart, true);
                if (isPaymentWorkflowRequired)
                {
                    //payment is required
                    var paymentMethodModel = PreparePaymentMethodModel(cart);

                    if (_paymentSettings.BypassPaymentMethodSelectionIfOnlyOne &&
                        paymentMethodModel.PaymentMethods.Count == 1 && !paymentMethodModel.DisplayRewardPoints)
                    {
                        //if we have only one payment method and reward points are disabled or the current customer doesn't have any reward points
                        //so customer doesn't have to choose a payment method
						var selectedPaymentMethodSystemName = paymentMethodModel.PaymentMethods[0].PaymentMethodSystemName;
						_genericAttributeService.SaveAttribute<string>(_workContext.CurrentCustomer,
							SystemCustomerAttributeNames.SelectedPaymentMethod, selectedPaymentMethodSystemName, _storeContext.CurrentStore.Id);

						var paymentMethodInst = _paymentService.LoadPaymentMethodBySystemName(selectedPaymentMethodSystemName, true, _storeContext.CurrentStore.Id);
						if (paymentMethodInst == null)
                            throw new Exception("Selected payment method can't be parsed");

                        var paymenInfoModel = PreparePaymentInfoModel(paymentMethodInst.Value);
                        return Json(new
                        {
                            update_section = new UpdateSectionJsonModel()
                            {
                                name = "payment-info",
                                html = this.RenderPartialViewToString("OpcPaymentInfo", paymenInfoModel)
                            },
                            goto_section = "payment_info"
                        });
                    }
                    else
                    {
                        //customer have to choose a payment method
                        return Json(new
                        {
                            update_section = new UpdateSectionJsonModel()
                            {
                                name = "payment-method",
                                html = this.RenderPartialViewToString("OpcPaymentMethods", paymentMethodModel)
                            },
                            goto_section = "payment_method"
                        });
                    }
                }
                else
                {
                    //payment is not required
					_genericAttributeService.SaveAttribute<string>(_workContext.CurrentCustomer,
						SystemCustomerAttributeNames.SelectedPaymentMethod, null, _storeContext.CurrentStore.Id);

                    var confirmOrderModel = PrepareConfirmOrderModel(cart);
                    return Json(new
                    {
                        update_section = new UpdateSectionJsonModel()
                        {
                            name = "confirm-order",
                            html = this.RenderPartialViewToString("OpcConfirmOrder", confirmOrderModel)
                        },
                        goto_section = "confirm_order"
                    });
                }
            }
            catch (Exception exc)
            {
				Logger.Warning(exc.Message, exc, _workContext.CurrentCustomer);
                return Json(new { error = 1, message = exc.Message });
            }
        }

        [ValidateInput(false)]
        public ActionResult OpcSavePaymentMethod(FormCollection form)
        {
            try
            {
                //validation
				var cart = _workContext.CurrentCustomer.GetCartItems(ShoppingCartType.ShoppingCart, _storeContext.CurrentStore.Id);

				if (cart.Count == 0)
                    throw new Exception("Your cart is empty");

                if (!UseOnePageCheckout())
                    throw new Exception("One page checkout is disabled");

                if ((_workContext.CurrentCustomer.IsGuest() && !_orderSettings.AnonymousCheckoutAllowed))
                    throw new Exception("Anonymous checkout is not allowed");

                string paymentmethod = form["paymentmethod"];
                //payment method 
                if (String.IsNullOrEmpty(paymentmethod))
                    throw new Exception("Selected payment method can't be parsed");


                var model = new CheckoutPaymentMethodModel();
                TryUpdateModel(model);

                //reward points
				if (_rewardPointsSettings.Enabled)
				{
					_genericAttributeService.SaveAttribute(_workContext.CurrentCustomer,
						SystemCustomerAttributeNames.UseRewardPointsDuringCheckout, model.UseRewardPoints,
						_storeContext.CurrentStore.Id);
				}

                //Check whether payment workflow is required
                bool isPaymentWorkflowRequired = IsPaymentWorkflowRequired(cart);
                if (!isPaymentWorkflowRequired)
                {
                    //payment is not required
					_genericAttributeService.SaveAttribute<string>(_workContext.CurrentCustomer,
						 SystemCustomerAttributeNames.SelectedPaymentMethod, null, _storeContext.CurrentStore.Id);

                    var confirmOrderModel = PrepareConfirmOrderModel(cart);
                    return Json(new
                    {
                        update_section = new UpdateSectionJsonModel()
                        {
                            name = "confirm-order",
                            html = this.RenderPartialViewToString("OpcConfirmOrder", confirmOrderModel)
                        },
                        goto_section = "confirm_order"
                    });
                }

				var paymentMethodInst = _paymentService.LoadPaymentMethodBySystemName(paymentmethod, true, _storeContext.CurrentStore.Id);
				if (paymentMethodInst == null)
                    throw new Exception("Selected payment method can't be parsed");

                //save
				_genericAttributeService.SaveAttribute<string>(_workContext.CurrentCustomer,
					 SystemCustomerAttributeNames.SelectedPaymentMethod, paymentmethod, _storeContext.CurrentStore.Id);                

                var paymenInfoModel = PreparePaymentInfoModel(paymentMethodInst.Value);
                return Json(new
                {
                    update_section = new UpdateSectionJsonModel()
                    {
                        name = "payment-info",
                        html = this.RenderPartialViewToString("OpcPaymentInfo", paymenInfoModel)
                    },
                    goto_section = "payment_info"
                });
            }
            catch (Exception exc)
            {
				Logger.Warning(exc.Message, exc, _workContext.CurrentCustomer);
                return Json(new { error = 1, message = exc.Message });
            }
        }

        [ValidateInput(false)]
        public ActionResult OpcSavePaymentInfo(FormCollection form)
        {
            try
            {
                //validation
				var cart = _workContext.CurrentCustomer.GetCartItems(ShoppingCartType.ShoppingCart, _storeContext.CurrentStore.Id);

				if (cart.Count == 0)
                    throw new Exception("Your cart is empty");

                if (!UseOnePageCheckout())
                    throw new Exception("One page checkout is disabled");

                if ((_workContext.CurrentCustomer.IsGuest() && !_orderSettings.AnonymousCheckoutAllowed))
                    throw new Exception("Anonymous checkout is not allowed");

				var paymentMethodSystemName = _workContext.CurrentCustomer.GetAttribute<string>(
					SystemCustomerAttributeNames.SelectedPaymentMethod,
					_genericAttributeService, _storeContext.CurrentStore.Id);
				var paymentMethod = _paymentService.LoadPaymentMethodBySystemName(paymentMethodSystemName);
				if (paymentMethod == null)
                    throw new Exception("Payment method is not selected");

                var paymentControllerType = paymentMethod.Value.GetControllerType();
                var paymentController =
                    DependencyResolver.Current.GetService(paymentControllerType) as PaymentControllerBase;
                var warnings = paymentController.ValidatePaymentForm(form);
                foreach (var warning in warnings)
                    ModelState.AddModelError("", warning);
                if (ModelState.IsValid)
                {
                    //get payment info
                    var paymentInfo = paymentController.GetPaymentInfo(form);
                    //session save
                    _httpContext.Session["OrderPaymentInfo"] = paymentInfo;

                    var confirmOrderModel = PrepareConfirmOrderModel(cart);
                    return Json(new
                    {
                        update_section = new UpdateSectionJsonModel()
                        {
                            name = "confirm-order",
                            html = this.RenderPartialViewToString("OpcConfirmOrder", confirmOrderModel)
                        },
                        goto_section = "confirm_order"
                    });
                }

                //If we got this far, something failed, redisplay form
                var paymenInfoModel = PreparePaymentInfoModel(paymentMethod.Value);
                return Json(new
                {
                    update_section = new UpdateSectionJsonModel()
                    {
                        name = "payment-info",
                        html = this.RenderPartialViewToString("OpcPaymentInfo", paymenInfoModel)
                    }
                });
            }
            catch (Exception exc)
            {
				Logger.Warning(exc.Message, exc, _workContext.CurrentCustomer);
                return Json(new { error = 1, message = exc.Message });
            }
        }

        [ValidateInput(false)]
        public ActionResult OpcConfirmOrder()
        {
            try
            {
                //validation
				var cart = _workContext.CurrentCustomer.GetCartItems(ShoppingCartType.ShoppingCart, _storeContext.CurrentStore.Id);

				if (cart.Count == 0)
                    throw new Exception("Your cart is empty");

                if (!UseOnePageCheckout())
                    throw new Exception("One page checkout is disabled");

                if ((_workContext.CurrentCustomer.IsGuest() && !_orderSettings.AnonymousCheckoutAllowed))
                    throw new Exception("Anonymous checkout is not allowed");

                //prevent 2 orders being placed within an X seconds time frame
                if (!IsMinimumOrderPlacementIntervalValid(_workContext.CurrentCustomer))
                    throw new Exception(_localizationService.GetResource("Checkout.MinOrderPlacementInterval"));

                //place order
                var processPaymentRequest = _httpContext.Session["OrderPaymentInfo"] as ProcessPaymentRequest;
                if (processPaymentRequest == null)
                {
                    //Check whether payment workflow is required
					if (IsPaymentWorkflowRequired(cart))
						throw new Exception("Payment information is not entered");
					else
						processPaymentRequest = new ProcessPaymentRequest();
                }

				processPaymentRequest.StoreId = _storeContext.CurrentStore.Id;
                processPaymentRequest.CustomerId = _workContext.CurrentCustomer.Id;

				processPaymentRequest.PaymentMethodSystemName = _workContext.CurrentCustomer.GetAttribute<string>(
					 SystemCustomerAttributeNames.SelectedPaymentMethod, _genericAttributeService, _storeContext.CurrentStore.Id);

                var placeOrderResult = _orderProcessingService.PlaceOrder(processPaymentRequest);
                if (placeOrderResult.Success)
                {
                    _httpContext.Session["OrderPaymentInfo"] = null;
                    var postProcessPaymentRequest = new PostProcessPaymentRequest()
                    {
                        Order = placeOrderResult.PlacedOrder
                    };


                    var paymentMethod = _paymentService.LoadPaymentMethodBySystemName(placeOrderResult.PlacedOrder.PaymentMethodSystemName);
                    if (paymentMethod != null)
                    {
                        if (paymentMethod.Value.PaymentMethodType == PaymentMethodType.Redirection)
                        {
                            //Redirection will not work because it's AJAX request.
                            //That's why we don't process it here (we redirect a user to another page where he'll be redirected)

                            //redirect
                            return Json(new { redirect = string.Format("{0}checkout/OpcCompleteRedirectionPayment", _webHelper.GetStoreLocation()) });
                        }
                        else
                        {
                            _paymentService.PostProcessPayment(postProcessPaymentRequest);
                            //success
                            return Json(new { success = 1 });
                        }
                    }
                    else
                    {
                        //payment method could be null if order total is 0

                        //success
                        return Json(new { success = 1 });
                    }
                }
                else
                {
                    //error
                    var confirmOrderModel = new CheckoutConfirmModel();
                    foreach (var error in placeOrderResult.Errors)
                        confirmOrderModel.Warnings.Add(error); 
                    
                    return Json(new
                        {
                            update_section = new UpdateSectionJsonModel()
                            {
                                name = "confirm-order",
                                html = this.RenderPartialViewToString("OpcConfirmOrder", confirmOrderModel)
                            },
                            goto_section = "confirm_order"
                        });
                }
            }
            catch (Exception exc)
            {
				Logger.Warning(exc.Message, exc, _workContext.CurrentCustomer);
                return Json(new { error = 1, message = exc.Message });
            }
        }

        public ActionResult OpcCompleteRedirectionPayment()
        {
            try
            {
                //validation
                if (!UseOnePageCheckout())
					return HttpNotFound();

                if ((_workContext.CurrentCustomer.IsGuest() && !_orderSettings.AnonymousCheckoutAllowed))
                    return new HttpUnauthorizedResult();

                //get the order
				var order = _orderService.SearchOrders(_storeContext.CurrentStore.Id, _workContext.CurrentCustomer.Id,
					null, null, null, null, null, null, null, null, 0, 1)
					.FirstOrDefault();
				if (order == null)
					return HttpNotFound();

                var paymentMethod = _paymentService.LoadPaymentMethodBySystemName(order.PaymentMethodSystemName);
                if (paymentMethod == null)
					return HttpNotFound();
                if (paymentMethod.Value.PaymentMethodType != PaymentMethodType.Redirection)
					return HttpNotFound();

                //ensure that order has been just placed
                if ((DateTime.UtcNow - order.CreatedOnUtc).TotalMinutes > 3)
					return HttpNotFound();


                //Redirection will not work on one page checkout page because it's AJAX request.
                //That's why we process it here
                var postProcessPaymentRequest = new PostProcessPaymentRequest()
                {
                    Order = order
                };

                _paymentService.PostProcessPayment(postProcessPaymentRequest);

                if (_webHelper.IsRequestBeingRedirected || _webHelper.IsPostBeingDone)
                {
                    //redirection or POST has been done in PostProcessPayment
                    return Content("Redirected");
                }
                else
                {
                    //if no redirection has been done (to a third-party payment page)
                    //theoretically it's not possible
					return RedirectToAction("Completed");
                }
            }
            catch (Exception exc)
            {
				Logger.Warning(exc.Message, exc, _workContext.CurrentCustomer);
                return Content(exc.Message);
            }
        }

        #endregion
    }
}
