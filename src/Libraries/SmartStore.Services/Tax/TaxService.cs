using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using SmartStore.Core;
using SmartStore.Core.Domain.Catalog;
using SmartStore.Core.Domain.Common;
using SmartStore.Core.Domain.Customers;
using SmartStore.Core.Domain.Orders;
using SmartStore.Core.Domain.Tax;
using SmartStore.Core.Infrastructure;
using SmartStore.Core.Plugins;
using SmartStore.Services.Common;
using SmartStore.Services.Configuration;

namespace SmartStore.Services.Tax
{
    /// <summary>
    /// Tax service
    /// </summary>
    public partial class TaxService : ITaxService
    {
        #region Fields

        private readonly IAddressService _addressService;
        private readonly IWorkContext _workContext;
        private readonly TaxSettings _taxSettings;
        private readonly IPluginFinder _pluginFinder;
        private readonly IDictionary<string, ITaxProvider> _taxProviders;
        private readonly IDictionary<TaxRateCacheKey, decimal> _cachedTaxRates;

        #endregion

        #region Ctor

        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="addressService">Address service</param>
        /// <param name="workContext">Work context</param>
        /// <param name="taxSettings">Tax settings</param>
        /// <param name="pluginFinder">Plugin finder</param>
        public TaxService(IAddressService addressService,
            IWorkContext workContext,
            TaxSettings taxSettings,
            IPluginFinder pluginFinder)
        {
            _addressService = addressService;
            _workContext = workContext;
            _taxSettings = taxSettings;
            _pluginFinder = pluginFinder;
            _taxProviders = new Dictionary<string, ITaxProvider>();
            _cachedTaxRates = new Dictionary<TaxRateCacheKey, decimal>();
        }

        #endregion

        #region Nested class

        internal class TaxRateCacheKey : Tuple<int, int, int>
        {
            public TaxRateCacheKey(int variantId, int taxCategoryId, int customerId)
                : base(variantId, taxCategoryId, customerId)
            {
            }
        }

        #endregion

        #region Utilities

        internal TaxRateCacheKey CreateTaxRateCacheKey(ProductVariant variant, int taxCategoryId, Customer customer)
        {
            return new TaxRateCacheKey(
                variant == null ? 0 : variant.Id,
                taxCategoryId,
                customer == null ? 0 : customer.Id);
        }

        /// <summary>
        /// Create request for tax calculation
        /// </summary>
        /// <param name="productVariant">Product variant</param>
        /// <param name="taxCategoryId">Tax category identifier</param>
        /// <param name="customer">Customer</param>
        /// <returns>Package for tax calculation</returns>
        protected CalculateTaxRequest CreateCalculateTaxRequest(ProductVariant productVariant,
            int taxCategoryId, Customer customer)
        {
            var calculateTaxRequest = new CalculateTaxRequest();
            calculateTaxRequest.Customer = customer;
            if (taxCategoryId > 0)
            {
                calculateTaxRequest.TaxCategoryId = taxCategoryId;
            }
            else
            {
                if (productVariant != null)
                    calculateTaxRequest.TaxCategoryId = productVariant.TaxCategoryId;
            }

            calculateTaxRequest.Address = this.GetTaxAddress(customer);
            return calculateTaxRequest;
        }

        protected virtual Address GetTaxAddress(Customer customer)
        {
            var basedOn = _taxSettings.TaxBasedOn;

            if (basedOn == TaxBasedOn.BillingAddress)
            {
                if (customer == null || customer.BillingAddress == null)
                {
                    basedOn = TaxBasedOn.DefaultAddress;
                }
            }
            if (basedOn == TaxBasedOn.ShippingAddress)
            {
                if (customer == null || customer.ShippingAddress == null)
                {
                    basedOn = TaxBasedOn.DefaultAddress;
                }
            }

            Address address = null;

            switch (basedOn)
            {
                case TaxBasedOn.BillingAddress:
                    {
                        address = customer.BillingAddress;
                    }
                    break;
                case TaxBasedOn.ShippingAddress:
                    {
                        address = customer.ShippingAddress;
                    }
                    break;
                case TaxBasedOn.DefaultAddress:
                default:
                    {
                        address = _addressService.GetAddressById(_taxSettings.DefaultTaxAddressId);
                    }
                    break;
            }

            return address;
        }

        /// <summary>
        /// Calculated price
        /// </summary>
        /// <param name="price">Price</param>
        /// <param name="percent">Percent</param>
        /// <param name="increase">Increase</param>
        /// <returns>New price</returns>
        protected decimal CalculatePrice(decimal price, decimal percent, bool increase)
        {
            decimal result = decimal.Zero;
            if (percent == decimal.Zero)
                return price;

            if (increase)
            {
                result = price * (1 + percent / 100);
            }
            else
            {
                result = price - (price) / (100 + percent) * percent;
            }
            return result;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Load active tax provider
        /// </summary>
        /// <returns>Active tax provider</returns>
        public virtual ITaxProvider LoadActiveTaxProvider()
        {
            var taxProvider = LoadTaxProviderBySystemName(_taxSettings.ActiveTaxProviderSystemName);
            if (taxProvider == null)
            {
                taxProvider = LoadAllTaxProviders().FirstOrDefault();
                _taxSettings.ActiveTaxProviderSystemName = taxProvider.PluginDescriptor.SystemName;
                EngineContext.Current.Resolve<ISettingService>().SaveSetting(_taxSettings);
            }
            return taxProvider;
        }

        /// <summary>
        /// Load tax provider by system name
        /// </summary>
        /// <param name="systemName">System name</param>
        /// <returns>Found tax provider</returns>
        public virtual ITaxProvider LoadTaxProviderBySystemName(string systemName)
        {
            Guard.ArgumentNotEmpty(() => systemName);

            ITaxProvider provider;
            if (!_taxProviders.TryGetValue(systemName, out provider))
            {
                var descriptor = _pluginFinder.GetPluginDescriptorBySystemName<ITaxProvider>(systemName);
                if (descriptor != null)
                {
                    provider = descriptor.Instance<ITaxProvider>();
                    if (provider != null)
                    {
                        _taxProviders[systemName] = provider;
                    }
                }
            }

            return provider;
        }

        /// <summary>
        /// Load all tax providers
        /// </summary>
        /// <returns>Tax providers</returns>
        public virtual IList<ITaxProvider> LoadAllTaxProviders()
        {
            return _pluginFinder.GetPlugins<ITaxProvider>().ToList();
        }


        private decimal GetOriginTaxRate(ProductVariant productVariant)
        {
            return GetTaxRate(productVariant, 0, null);
        }

        /// <summary>
        /// Gets tax rate
        /// </summary>
        /// <param name="productVariant">Product variant</param>
        /// <param name="customer">Customer</param>
        /// <returns>Tax rate</returns>
        public virtual decimal GetTaxRate(ProductVariant productVariant, Customer customer)
        {
            return GetTaxRate(productVariant, 0, customer);
        }

        /// <summary>
        /// Gets tax rate
        /// </summary>
        /// <param name="taxCategoryId">Tax category identifier</param>
        /// <param name="customer">Customer</param>
        /// <returns>Tax rate</returns>
        public virtual decimal GetTaxRate(int taxCategoryId, Customer customer)
        {
            return GetTaxRate(null, taxCategoryId, customer);
        }

        /// <summary>
        /// Gets tax rate
        /// </summary>
        /// <param name="productVariant">Product variant</param>
        /// <param name="taxCategoryId">Tax category identifier</param>
        /// <param name="customer">Customer</param>
        /// <returns>Tax rate</returns>
        public virtual decimal GetTaxRate(ProductVariant productVariant, int taxCategoryId, Customer customer)
        {
            var cacheKey = this.CreateTaxRateCacheKey(productVariant, taxCategoryId, customer);
            decimal result;
            if (!_cachedTaxRates.TryGetValue(cacheKey, out result))
            {
                result = GetTaxRateCore(productVariant, taxCategoryId, customer);
                _cachedTaxRates[cacheKey] = result;
            }

            return result;
        }

        protected virtual decimal GetTaxRateCore(ProductVariant productVariant, int taxCategoryId, Customer customer)
        {
            ////tax exempt (VATFIX)
            //if (IsTaxExempt(productVariant, customer))
            //{
            //    return decimal.Zero;
            //}

            //tax request
            var calculateTaxRequest = CreateCalculateTaxRequest(productVariant, taxCategoryId, customer);

            ////make EU VAT exempt validation (the European Union Value Added Tax) (VATFIX)
            //if (_taxSettings.EuVatEnabled && IsVatExempt(calculateTaxRequest.Address, calculateTaxRequest.Customer))
            //{
            //    //return zero if VAT is not chargeable
            //    return decimal.Zero;
            //}

            //active tax provider
            var activeTaxProvider = LoadActiveTaxProvider();
            if (activeTaxProvider == null)
            {
                return decimal.Zero;
            }

            //get tax rate
            var calculateTaxResult = activeTaxProvider.GetTaxRate(calculateTaxRequest);
            if (calculateTaxResult.Success)
            {
                // ensure that tax is equal or greater than zero
                return Math.Max(0, calculateTaxResult.TaxRate);
            }
            else
            {
                return decimal.Zero;
            }
        }


        /// <summary>
        /// Gets price
        /// </summary>
        /// <param name="productVariant">Product variant</param>
        /// <param name="price">Price</param>
        /// <param name="taxRate">Tax rate</param>
        /// <returns>Price</returns>
        public virtual decimal GetProductPrice(ProductVariant productVariant, decimal price,
            out decimal taxRate)
        {
            var customer = _workContext.CurrentCustomer;
            return GetProductPrice(productVariant, price, customer, out taxRate);
        }

        /// <summary>
        /// Gets price
        /// </summary>
        /// <param name="productVariant">Product variant</param>
        /// <param name="price">Price</param>
        /// <param name="customer">Customer</param>
        /// <param name="taxRate">Tax rate</param>
        /// <returns>Price</returns>
        public virtual decimal GetProductPrice(ProductVariant productVariant, decimal price,
            Customer customer, out decimal taxRate)
        {
            bool includingTax = _workContext.TaxDisplayType == TaxDisplayType.IncludingTax;
            return GetProductPrice(productVariant, price, includingTax, customer, out taxRate);
        }

        /// <summary>
        /// Gets price
        /// </summary>
        /// <param name="productVariant">Product variant</param>
        /// <param name="price">Price</param>
        /// <param name="includingTax">A value indicating whether calculated price should include tax</param>
        /// <param name="customer">Customer</param>
        /// <param name="taxRate">Tax rate</param>
        /// <returns>Price</returns>
        public virtual decimal GetProductPrice(ProductVariant productVariant, decimal price,
            bool includingTax, Customer customer, out decimal taxRate)
        {
            bool priceIncludesTax = _taxSettings.PricesIncludeTax;
            int taxCategoryId = productVariant.TaxCategoryId; // 0; // (VATFIX)
            return GetProductPrice(productVariant, taxCategoryId, price, includingTax,
                customer, priceIncludesTax, out taxRate);
        }

        /// <summary>
        /// Gets price
        /// </summary>
        /// <param name="productVariant">Product variant</param>
        /// <param name="taxCategoryId">Tax category identifier</param>
        /// <param name="price">Price</param>
        /// <param name="includingTax">A value indicating whether calculated price should include tax</param>
        /// <param name="customer">Customer</param>
        /// <param name="priceIncludesTax">A value indicating whether price already includes tax</param>
        /// <param name="taxRate">Tax rate</param>
        /// <returns>Price</returns>
        public virtual decimal GetProductPrice(ProductVariant productVariant, int taxCategoryId,
            decimal price, bool includingTax, Customer customer,
            bool priceIncludesTax, out decimal taxRate)
        {
            taxRate = GetTaxRate(productVariant, taxCategoryId, customer);

            // Admin: GROSS prices
            if (priceIncludesTax)
            {
                ////  (VATFIX)
                //decimal originTaxRate = GetOriginTaxRate(productVariant);

                //// resolve NET price from GROSS
                //price = CalculatePrice(price, originTaxRate, false);
                //if (includingTax && taxRate > 0)
                //{
                //    // new GROSS: add destination tax to NET price
                //    price = CalculatePrice(price, taxRate, true);
                //}

                if (!includingTax)
                {
                    price = CalculatePrice(price, taxRate, false);
                }
            }
            // Admin: NET prices
            else
            {
                if (includingTax)
                {
                    price = CalculatePrice(price, taxRate, true);
                }
            }

            //allowed to support negative price adjustments
            //if (price < decimal.Zero)
            //    price = decimal.Zero;

            return price;
        }




        /// <summary>
        /// Gets shipping price
        /// </summary>
        /// <param name="price">Price</param>
        /// <param name="customer">Customer</param>
        /// <returns>Price</returns>
        public virtual decimal GetShippingPrice(decimal price, Customer customer)
        {
            bool includingTax = _workContext.TaxDisplayType == TaxDisplayType.IncludingTax;
            return GetShippingPrice(price, includingTax, customer);
        }

        /// <summary>
        /// Gets shipping price
        /// </summary>
        /// <param name="price">Price</param>
        /// <param name="includingTax">A value indicating whether calculated price should include tax</param>
        /// <param name="customer">Customer</param>
        /// <returns>Price</returns>
        public virtual decimal GetShippingPrice(decimal price, bool includingTax, Customer customer)
        {
            decimal taxRate = decimal.Zero;
            return GetShippingPrice(price, includingTax, customer, out taxRate);
        }

        /// <summary>
        /// Gets shipping price
        /// </summary>
        /// <param name="price">Price</param>
        /// <param name="includingTax">A value indicating whether calculated price should include tax</param>
        /// <param name="customer">Customer</param>
        /// <param name="taxRate">Tax rate</param>
        /// <returns>Price</returns>
        public virtual decimal GetShippingPrice(decimal price, bool includingTax, Customer customer, out decimal taxRate)
        {
            taxRate = decimal.Zero;

            if (!_taxSettings.ShippingIsTaxable)
            {
                return price;
            }

            bool priceIncludesTax = _taxSettings.ShippingPriceIncludesTax;
            int taxClassId = _taxSettings.ShippingTaxClassId;
            return GetProductPrice(null, taxClassId, price, includingTax, customer,
                priceIncludesTax, out taxRate);
        }





        /// <summary>
        /// Gets payment method additional handling fee
        /// </summary>
        /// <param name="price">Price</param>
        /// <param name="customer">Customer</param>
        /// <returns>Price</returns>
        public virtual decimal GetPaymentMethodAdditionalFee(decimal price, Customer customer)
        {
            bool includingTax = _workContext.TaxDisplayType == TaxDisplayType.IncludingTax;
            return GetPaymentMethodAdditionalFee(price, includingTax, customer);
        }

        /// <summary>
        /// Gets payment method additional handling fee
        /// </summary>
        /// <param name="price">Price</param>
        /// <param name="includingTax">A value indicating whether calculated price should include tax</param>
        /// <param name="customer">Customer</param>
        /// <returns>Price</returns>
        public virtual decimal GetPaymentMethodAdditionalFee(decimal price, bool includingTax, Customer customer)
        {
            decimal taxRate = decimal.Zero;
            return GetPaymentMethodAdditionalFee(price, includingTax,
                customer, out taxRate);
        }

        /// <summary>
        /// Gets payment method additional handling fee
        /// </summary>
        /// <param name="price">Price</param>
        /// <param name="includingTax">A value indicating whether calculated price should include tax</param>
        /// <param name="customer">Customer</param>
        /// <param name="taxRate">Tax rate</param>
        /// <returns>Price</returns>
        public virtual decimal GetPaymentMethodAdditionalFee(decimal price, bool includingTax, Customer customer, out decimal taxRate)
        {
            taxRate = decimal.Zero;

            if (!_taxSettings.PaymentMethodAdditionalFeeIsTaxable)
            {
                return price;
            }

            bool priceIncludesTax = _taxSettings.PaymentMethodAdditionalFeeIncludesTax;
            int taxClassId = _taxSettings.PaymentMethodAdditionalFeeTaxClassId;
            return GetProductPrice(null, taxClassId, price, includingTax, customer,
                priceIncludesTax, out taxRate);
        }





        /// <summary>
        /// Gets checkout attribute value price
        /// </summary>
        /// <param name="cav">Checkout attribute value</param>
        /// <returns>Price</returns>
        public virtual decimal GetCheckoutAttributePrice(CheckoutAttributeValue cav)
        {
            var customer = _workContext.CurrentCustomer;
            return GetCheckoutAttributePrice(cav, customer);
        }

        /// <summary>
        /// Gets checkout attribute value price
        /// </summary>
        /// <param name="cav">Checkout attribute value</param>
        /// <param name="customer">Customer</param>
        /// <returns>Price</returns>
        public virtual decimal GetCheckoutAttributePrice(CheckoutAttributeValue cav, Customer customer)
        {
            bool includingTax = _workContext.TaxDisplayType == TaxDisplayType.IncludingTax;
            return GetCheckoutAttributePrice(cav, includingTax, customer);
        }

        /// <summary>
        /// Gets checkout attribute value price
        /// </summary>
        /// <param name="cav">Checkout attribute value</param>
        /// <param name="includingTax">A value indicating whether calculated price should include tax</param>
        /// <param name="customer">Customer</param>
        /// <returns>Price</returns>
        public virtual decimal GetCheckoutAttributePrice(CheckoutAttributeValue cav,
            bool includingTax, Customer customer)
        {
            decimal taxRate = decimal.Zero;
            return GetCheckoutAttributePrice(cav, includingTax, customer, out taxRate);
        }

        /// <summary>
        /// Gets checkout attribute value price
        /// </summary>
        /// <param name="cav">Checkout attribute value</param>
        /// <param name="includingTax">A value indicating whether calculated price should include tax</param>
        /// <param name="customer">Customer</param>
        /// <param name="taxRate">Tax rate</param>
        /// <returns>Price</returns>
        public virtual decimal GetCheckoutAttributePrice(CheckoutAttributeValue cav,
            bool includingTax, Customer customer, out decimal taxRate)
        {
            if (cav == null)
                throw new ArgumentNullException("cav");

            taxRate = decimal.Zero;

            bool priceIncludesTax = _taxSettings.PricesIncludeTax;

            decimal price = cav.PriceAdjustment;
            if (cav.CheckoutAttribute.IsTaxExempt)
            {
                return price;
            }

            int taxClassId = cav.CheckoutAttribute.TaxCategoryId;
            return GetProductPrice(null, taxClassId, price, includingTax, customer,
                priceIncludesTax, out taxRate);
        }





        /// <summary>
        /// Gets VAT Number status
        /// </summary>
        /// <param name="fullVatNumber">Two letter ISO code of a country and VAT number (e.g. GB 111 1111 111)</param>
        /// <returns>VAT Number status</returns>
        public virtual VatNumberStatus GetVatNumberStatus(string fullVatNumber)
        {
            string name, address;
            return GetVatNumberStatus(fullVatNumber, out name, out address);
        }

        /// <summary>
        /// Gets VAT Number status
        /// </summary>
        /// <param name="fullVatNumber">Two letter ISO code of a country and VAT number (e.g. GB 111 1111 111)</param>
        /// <param name="name">Name (if received)</param>
        /// <param name="address">Address (if received)</param>
        /// <returns>VAT Number status</returns>
        public virtual VatNumberStatus GetVatNumberStatus(string fullVatNumber,
            out string name, out string address)
        {
            name = string.Empty;
            address = string.Empty;

            if (String.IsNullOrWhiteSpace(fullVatNumber))
                return VatNumberStatus.Empty;
            fullVatNumber = fullVatNumber.Trim();

            //GB 111 1111 111 or GB 1111111111
            //more advanced regex - http://codeigniter.com/wiki/European_Vat_Checker
            var r = new Regex(@"^(\w{2})(.*)");
            var match = r.Match(fullVatNumber);
            if (!match.Success)
                return VatNumberStatus.Invalid;
            var twoLetterIsoCode = match.Groups[1].Value;
            var vatNumber = match.Groups[2].Value;

            return GetVatNumberStatus(twoLetterIsoCode, vatNumber, out name, out address);
        }

        /// <summary>
        /// Gets VAT Number status
        /// </summary>
        /// <param name="twoLetterIsoCode">Two letter ISO code of a country</param>
        /// <param name="vatNumber">VAT number</param>
        /// <returns>VAT Number status</returns>
        public virtual VatNumberStatus GetVatNumberStatus(string twoLetterIsoCode, string vatNumber)
        {
            string name, address;
            return GetVatNumberStatus(twoLetterIsoCode, vatNumber, out name, out address);
        }

        /// <summary>
        /// Gets VAT Number status
        /// </summary>
        /// <param name="twoLetterIsoCode">Two letter ISO code of a country</param>
        /// <param name="vatNumber">VAT number</param>
        /// <param name="name">Name (if received)</param>
        /// <param name="address">Address (if received)</param>
        /// <returns>VAT Number status</returns>
        public virtual VatNumberStatus GetVatNumberStatus(string twoLetterIsoCode, string vatNumber,
            out string name, out string address)
        {
            name = string.Empty;
            address = string.Empty;

            if (String.IsNullOrEmpty(twoLetterIsoCode) || String.IsNullOrEmpty(vatNumber))
                return VatNumberStatus.Empty;

            if (!_taxSettings.EuVatUseWebService)
                return VatNumberStatus.Unknown;

            Exception exception = null;
            return DoVatCheck(twoLetterIsoCode, vatNumber, out name, out address, out exception);
        }

        /// <summary>
        /// Performs a basic check of a VAT number for validity
        /// </summary>
        /// <param name="twoLetterIsoCode">Two letter ISO code of a country</param>
        /// <param name="vatNumber">VAT number</param>
        /// <param name="name">Company name</param>
        /// <param name="address">Address</param>
        /// <param name="exception">Exception</param>
        /// <returns>VAT number status</returns>
        public virtual VatNumberStatus DoVatCheck(string twoLetterIsoCode, string vatNumber,
            out string name, out string address, out Exception exception)
        {
            name = string.Empty;
            address = string.Empty;

            if (vatNumber == null)
                vatNumber = string.Empty;
            vatNumber = vatNumber.Trim().Replace(" ", "");

            if (twoLetterIsoCode == null)
                twoLetterIsoCode = string.Empty;
            if (!String.IsNullOrEmpty(twoLetterIsoCode))
                //The service returns INVALID_INPUT for country codes that are not uppercase.
                twoLetterIsoCode = twoLetterIsoCode.ToUpper();

            EuropaCheckVatService.checkVatService s = null;

            try
            {
                bool valid;

                s = new EuropaCheckVatService.checkVatService();
                s.checkVat(ref twoLetterIsoCode, ref vatNumber, out valid, out name, out address);
                exception = null;
                return valid ? VatNumberStatus.Valid : VatNumberStatus.Invalid;
            }
            catch (Exception ex)
            {
                name = address = string.Empty;
                exception = ex;
                return VatNumberStatus.Unknown;
            }
            finally
            {
                if (name == null)
                    name = string.Empty;

                if (address == null)
                    address = string.Empty;

                if (s != null)
                    s.Dispose();
            }
        }


        /// <summary>
        /// Gets a value indicating whether tax exempt
        /// </summary>
        /// <param name="productVariant">Product variant</param>
        /// <param name="customer">Customer</param>
        /// <returns>A value indicating whether tax exempt</returns>
        public virtual bool IsTaxExempt(ProductVariant productVariant, Customer customer)
        {
            if (customer != null)
            {
                if (customer.IsTaxExempt)
                    return true;

                if (customer.CustomerRoles.Where(cr => cr.Active).Any(cr => cr.TaxExempt))
                    return true;
            }

            if (productVariant == null)
            {
                return false;
            }

            if (productVariant.IsTaxExempt)
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Gets a value indicating whether EU VAT exempt (the European Union Value Added Tax)
        /// </summary>
        /// <param name="address">Address</param>
        /// <param name="customer">Customer</param>
        /// <returns>Result</returns>
        public virtual bool IsVatExempt(Address address, Customer customer)
        {
            if (!_taxSettings.EuVatEnabled)
            {
                return false;
            }

            if (customer == null)
            {
                return false;
            }

            if (address == null)
            {
                address = GetTaxAddress(customer);
            }

            if (address == null || address.Country == null)
            {
                return false;
            }

            if (!address.Country.SubjectToVat)
            {
                // VAT not chargeable if shipping outside VAT zone:
                return true;
            }
            else
            {
                // VAT not chargeable if address, customer and config meet our VAT exemption requirements:
                // returns true if this customer is VAT exempt because they are shipping within the EU but outside our shop country, 
                // they have supplied a validated VAT number, and the shop is configured to allow VAT exemption
                if (address.CountryId == _taxSettings.EuVatShopCountryId)
                    return false;

                var customerVatStatus = (VatNumberStatus)customer.GetAttribute<int>(SystemCustomerAttributeNames.VatNumberStatusId);
                return customerVatStatus == VatNumberStatus.Valid && _taxSettings.EuVatAllowVatExemption;
            }
        }

        #endregion
    }
}
