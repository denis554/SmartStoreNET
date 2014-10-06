using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Net;
using System.Text;
using System.Linq;
using System.Web;
using System.Web.Routing;
using Autofac;
using SmartStore.Core;
using SmartStore.Core.Domain.Directory;
using SmartStore.Core.Domain.Orders;
using SmartStore.Core.Domain.Payments;
using SmartStore.Core.Domain.Shipping;
using SmartStore.Plugin.Payments.PayPalStandard.Controllers;
using SmartStore.Plugin.Payments.PayPalStandard.Services;
using SmartStore.Services.Configuration;
using SmartStore.Services.Directory;
using SmartStore.Services.Localization;
using SmartStore.Services.Orders;
using SmartStore.Services.Payments;
using SmartStore.Services.Tax;
using SmartStore.Core.Plugins;

namespace SmartStore.Plugin.Payments.PayPalStandard
{
	/// <summary>
	/// PayPalStandard payment processor
	/// </summary>
	public class PayPalStandardPaymentProcessor : PaymentPluginBase, IConfigurable
	{
		#region Fields

		private readonly PayPalStandardPaymentSettings _paypalStandardPaymentSettings;
		private readonly ISettingService _settingService;
		private readonly ICurrencyService _currencyService;
		private readonly CurrencySettings _currencySettings;
		private readonly IWebHelper _webHelper;
		private readonly ICheckoutAttributeParser _checkoutAttributeParser;
		private readonly ITaxService _taxService;
		private readonly IOrderTotalCalculationService _orderTotalCalculationService;
		private readonly HttpContextBase _httpContext;
		private readonly ILocalizationService _localizationService;
		private readonly IPayPalStandardService _payPalStandardService;

		#endregion

		#region Ctor

		public PayPalStandardPaymentProcessor(PayPalStandardPaymentSettings paypalStandardPaymentSettings,
			ISettingService settingService, ICurrencyService currencyService,
			CurrencySettings currencySettings, IWebHelper webHelper,
			ICheckoutAttributeParser checkoutAttributeParser, ITaxService taxService,
			IOrderTotalCalculationService orderTotalCalculationService, HttpContextBase httpContext,
			ILocalizationService localizationService,
			IPayPalStandardService payPalStandardService)
		{
			this._paypalStandardPaymentSettings = paypalStandardPaymentSettings;
			this._settingService = settingService;
			this._currencyService = currencyService;
			this._currencySettings = currencySettings;
			this._webHelper = webHelper;
			this._checkoutAttributeParser = checkoutAttributeParser;
			this._taxService = taxService;
			this._orderTotalCalculationService = orderTotalCalculationService;
			this._httpContext = httpContext;
			this._localizationService = localizationService;
			this._payPalStandardService = payPalStandardService;
		}

		#endregion

		#region Utilities

		/// <summary>
		/// Gets Paypal URL
		/// </summary>
		/// <returns></returns>
		private string GetPaypalUrl()
		{
			//return _paypalStandardPaymentSettings.UseSandbox ? "https://www.sandbox.paypal.com/us/cgi-bin/webscr" :
			//	"https://www.paypal.com/us/cgi-bin/webscr";

			// codehint: sm-edit (url that paypal actually publish)
			return _paypalStandardPaymentSettings.UseSandbox ?
				"https://www.sandbox.paypal.com/cgi-bin/webscr" :
				"https://www.paypal.com/cgi-bin/webscr";
		}

		/// <summary>
		/// Gets PDT details
		/// </summary>
		/// <param name="tx">TX</param>
		/// <param name="values">Values</param>
		/// <param name="response">Response</param>
		/// <returns>Result</returns>
		public bool GetPDTDetails(string tx, out Dictionary<string, string> values, out string response)
		{
			var req = (HttpWebRequest)WebRequest.Create(GetPaypalUrl());
			req.Method = "POST";
			req.ContentType = "application/x-www-form-urlencoded";

			string formContent = string.Format("cmd=_notify-synch&at={0}&tx={1}", _paypalStandardPaymentSettings.PdtToken, tx);
			req.ContentLength = formContent.Length;

			using (var sw = new StreamWriter(req.GetRequestStream(), Encoding.ASCII))
				sw.Write(formContent);

			response = null;
			using (var sr = new StreamReader(req.GetResponse().GetResponseStream()))
				response = HttpUtility.UrlDecode(sr.ReadToEnd());

			values = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
			bool firstLine = true, success = false;
			foreach (string l in response.Split('\n'))
			{
				string line = l.Trim();
				if (firstLine)
				{
					success = line.Equals("SUCCESS", StringComparison.OrdinalIgnoreCase);
					firstLine = false;
				}
				else
				{
					int equalPox = line.IndexOf('=');
					if (equalPox >= 0)
						values.Add(line.Substring(0, equalPox), line.Substring(equalPox + 1));
				}
			}

			return success;
		}

		/// <summary>
		/// Verifies IPN
		/// </summary>
		/// <param name="formString">Form string</param>
		/// <param name="values">Values</param>
		/// <returns>Result</returns>
		public bool VerifyIPN(string formString, out Dictionary<string, string> values)
		{
			var req = (HttpWebRequest)WebRequest.Create(GetPaypalUrl());
			req.Method = "POST";
			req.ContentType = "application/x-www-form-urlencoded";
			req.UserAgent = HttpContext.Current.Request.UserAgent;

			string formContent = string.Format("{0}&cmd=_notify-validate", formString);
			req.ContentLength = formContent.Length;

			using (var sw = new StreamWriter(req.GetRequestStream(), Encoding.ASCII))
			{
				sw.Write(formContent);
			}

			string response = null;
			using (var sr = new StreamReader(req.GetResponse().GetResponseStream()))
			{
				response = HttpUtility.UrlDecode(sr.ReadToEnd());
			}
			bool success = response.Trim().Equals("VERIFIED", StringComparison.OrdinalIgnoreCase);

			values = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
			foreach (string l in formString.Split('&'))
			{
				string line = HttpUtility.UrlDecode(l).Trim();		// codehint: sm-edit
				int equalPox = line.IndexOf('=');
				if (equalPox >= 0)
					values.Add(line.Substring(0, equalPox), line.Substring(equalPox + 1));
			}

			return success;
		}

		#endregion

		#region Methods

		/// <summary>
		/// Process a payment
		/// </summary>
		/// <param name="processPaymentRequest">Payment info required for an order processing</param>
		/// <returns>Process payment result</returns>
		public override ProcessPaymentResult ProcessPayment(ProcessPaymentRequest processPaymentRequest)
		{
			var result = new ProcessPaymentResult();
			result.NewPaymentStatus = PaymentStatus.Pending;

			// codehint: sm-add
			if (_paypalStandardPaymentSettings.BusinessEmail.IsNullOrEmpty() || _paypalStandardPaymentSettings.PdtToken.IsNullOrEmpty())
			{
				result.AddError(_localizationService.GetResource("Plugins.Payments.PayPalStandard.InvalidCredentials"));
			}

			return result;
		}

		/// <summary>
		/// Post process payment (used by payment gateways that require redirecting to a third-party URL)
		/// </summary>
		/// <param name="postProcessPaymentRequest">Payment info required for an order processing</param>
		public override void PostProcessPayment(PostProcessPaymentRequest postProcessPaymentRequest)
		{
			if (postProcessPaymentRequest.Order.PaymentStatus == PaymentStatus.Paid)
				return;

			var builder = new StringBuilder();
			builder.Append(GetPaypalUrl());

			string orderNumber = postProcessPaymentRequest.Order.GetOrderNumber();
			string cmd = (_paypalStandardPaymentSettings.PassProductNamesAndTotals ? "_cart" : "_xclick");

			builder.AppendFormat("?cmd={0}&business={1}", cmd, HttpUtility.UrlEncode(_paypalStandardPaymentSettings.BusinessEmail));

			if (_paypalStandardPaymentSettings.PassProductNamesAndTotals)
			{
				builder.AppendFormat("&upload=1");

				int index = 0;
				decimal cartTotal = decimal.Zero;
				//var caValues = _checkoutAttributeParser.ParseCheckoutAttributeValues(postProcessPaymentRequest.Order.CheckoutAttributesXml);

				var lineItems = _payPalStandardService.GetLineItems(postProcessPaymentRequest, out cartTotal);

				_payPalStandardService.AdjustLineItemAmounts(lineItems, postProcessPaymentRequest);

				foreach (var item in lineItems.OrderBy(x => (int)x.Type))
				{
					++index;
					builder.AppendFormat("&item_name_" + index + "={0}", HttpUtility.UrlEncode(item.Name));
					builder.AppendFormat("&amount_" + index + "={0}", item.AmountRounded.ToString("0.00", CultureInfo.InvariantCulture));
					builder.AppendFormat("&quantity_" + index + "={0}", item.Quantity);
				}

				#region old code

				//var cartItems = postProcessPaymentRequest.Order.OrderItems;
				//int x = 1;
				//foreach (var item in cartItems)
				//{
				//	var unitPriceExclTax = item.UnitPriceExclTax;
				//	var priceExclTax = item.PriceExclTax;
				//	//round
				//	var unitPriceExclTaxRounded = Math.Round(unitPriceExclTax, 2);

				//	builder.AppendFormat("&item_name_" + x + "={0}", HttpUtility.UrlEncode(item.Product.Name));
				//	builder.AppendFormat("&amount_" + x + "={0}", unitPriceExclTaxRounded.ToString("0.00", CultureInfo.InvariantCulture));
				//	builder.AppendFormat("&quantity_" + x + "={0}", item.Quantity);
				//	x++;
				//	cartTotal += priceExclTax;
				//}

				////the checkout attributes that have a dollar value and send them to Paypal as items to be paid for
				//foreach (var val in caValues)
				//{
				//	var attPrice = _taxService.GetCheckoutAttributePrice(val, false, postProcessPaymentRequest.Order.Customer);
				//	//round
				//	var attPriceRounded = Math.Round(attPrice, 2);
				//	if (attPrice > decimal.Zero) //if it has a price
				//	{
				//		var ca = val.CheckoutAttribute;
				//		if (ca != null)
				//		{
				//			var attName = ca.Name; //set the name
				//			builder.AppendFormat("&item_name_" + x + "={0}", HttpUtility.UrlEncode(attName)); //name
				//			builder.AppendFormat("&amount_" + x + "={0}", attPriceRounded.ToString("0.00", CultureInfo.InvariantCulture)); //amount
				//			builder.AppendFormat("&quantity_" + x + "={0}", 1); //quantity
				//			x++;
				//			cartTotal += attPrice;
				//		}
				//	}
				//}

				////order totals

				////shipping
				//var orderShippingExclTax = postProcessPaymentRequest.Order.OrderShippingExclTax;
				//var orderShippingExclTaxRounded = Math.Round(orderShippingExclTax, 2);
				//if (orderShippingExclTax > decimal.Zero)
				//{
				//	builder.AppendFormat("&item_name_" + x + "={0}", HttpUtility.UrlEncode(_localizationService.GetResource("Plugins.Payments.PayPalStandard.ShippingFee")));
				//	builder.AppendFormat("&amount_" + x + "={0}", orderShippingExclTaxRounded.ToString("0.00", CultureInfo.InvariantCulture));
				//	builder.AppendFormat("&quantity_" + x + "={0}", 1);
				//	x++;
				//	cartTotal += orderShippingExclTax;
				//}

				////payment method additional fee
				//var paymentMethodAdditionalFeeExclTax = postProcessPaymentRequest.Order.PaymentMethodAdditionalFeeExclTax;
				//var paymentMethodAdditionalFeeExclTaxRounded = Math.Round(paymentMethodAdditionalFeeExclTax, 2);
				//if (paymentMethodAdditionalFeeExclTax > decimal.Zero)
				//{
				//	builder.AppendFormat("&item_name_" + x + "={0}", HttpUtility.UrlEncode(_localizationService.GetResource("Plugins.Payments.PayPalStandard.PaymentMethodFee")));
				//	builder.AppendFormat("&amount_" + x + "={0}", paymentMethodAdditionalFeeExclTaxRounded.ToString("0.00", CultureInfo.InvariantCulture));
				//	builder.AppendFormat("&quantity_" + x + "={0}", 1);
				//	x++;
				//	cartTotal += paymentMethodAdditionalFeeExclTax;
				//}

				////tax
				//var orderTax = postProcessPaymentRequest.Order.OrderTax;
				//var orderTaxRounded = Math.Round(orderTax, 2);
				//if (orderTax > decimal.Zero)
				//{
				//	//builder.AppendFormat("&tax_1={0}", orderTax.ToString("0.00", CultureInfo.InvariantCulture));

				//	//add tax as item
				//	builder.AppendFormat("&item_name_" + x + "={0}", HttpUtility.UrlEncode(_localizationService.GetResource("Plugins.Payments.PayPalStandard.SalesTax")));
				//	builder.AppendFormat("&amount_" + x + "={0}", orderTaxRounded.ToString("0.00", CultureInfo.InvariantCulture)); //amount
				//	builder.AppendFormat("&quantity_" + x + "={0}", 1); //quantity

				//	cartTotal += orderTax;
				//	x++;
				//}

				#endregion

				if (cartTotal > postProcessPaymentRequest.Order.OrderTotal)
				{
					/* Take the difference between what the order total is and what it should be and use that as the "discount".
					 * The difference equals the amount of the gift card and/or reward points used. 
					 */
					decimal discountTotal = cartTotal - postProcessPaymentRequest.Order.OrderTotal;
					discountTotal = Math.Round(discountTotal, 2);
					//gift card or rewared point amount applied to cart in SmartStore.NET - shows in Paypal as "discount"
					builder.AppendFormat("&discount_amount_cart={0}", discountTotal.ToString("0.00", CultureInfo.InvariantCulture));
				}
			}
			else
			{
				//pass order total
				string totalItemName = "{0} {1}".FormatWith(_localizationService.GetResource("Checkout.OrderNumber"), orderNumber);
				builder.AppendFormat("&item_name={0}", HttpUtility.UrlEncode(totalItemName));
				var orderTotal = Math.Round(postProcessPaymentRequest.Order.OrderTotal, 2);
				builder.AppendFormat("&amount={0}", orderTotal.ToString("0.00", CultureInfo.InvariantCulture));
			}

			builder.AppendFormat("&custom={0}", postProcessPaymentRequest.Order.OrderGuid);
			builder.AppendFormat("&charset={0}", "utf-8");
			builder.Append(string.Format("&no_note=1&currency_code={0}", HttpUtility.UrlEncode(_currencyService.GetCurrencyById(_currencySettings.PrimaryStoreCurrencyId).CurrencyCode)));
			builder.AppendFormat("&invoice={0}", HttpUtility.UrlEncode(orderNumber));
			builder.AppendFormat("&rm=2", new object[0]);

			if (postProcessPaymentRequest.Order.ShippingStatus != ShippingStatus.ShippingNotRequired)
				builder.AppendFormat("&no_shipping=2", new object[0]);
			else
				builder.AppendFormat("&no_shipping=1", new object[0]);

			string returnUrl = _webHelper.GetStoreLocation(false) + "Plugins/PaymentPayPalStandard/PDTHandler";
			string cancelReturnUrl = _webHelper.GetStoreLocation(false) + "Plugins/PaymentPayPalStandard/CancelOrder";
			builder.AppendFormat("&return={0}&cancel_return={1}", HttpUtility.UrlEncode(returnUrl), HttpUtility.UrlEncode(cancelReturnUrl));

			//Instant Payment Notification (server to server message)
			if (_paypalStandardPaymentSettings.EnableIpn)
			{
				string ipnUrl;
				if (String.IsNullOrWhiteSpace(_paypalStandardPaymentSettings.IpnUrl))
					ipnUrl = _webHelper.GetStoreLocation(false) + "Plugins/PaymentPayPalStandard/IPNHandler";
				else
					ipnUrl = _paypalStandardPaymentSettings.IpnUrl;
				builder.AppendFormat("&notify_url={0}", ipnUrl);
			}

			//address
			builder.AppendFormat("&address_override=1");
			builder.AppendFormat("&first_name={0}", HttpUtility.UrlEncode(postProcessPaymentRequest.Order.BillingAddress.FirstName));
			builder.AppendFormat("&last_name={0}", HttpUtility.UrlEncode(postProcessPaymentRequest.Order.BillingAddress.LastName));
			builder.AppendFormat("&address1={0}", HttpUtility.UrlEncode(postProcessPaymentRequest.Order.BillingAddress.Address1));
			builder.AppendFormat("&address2={0}", HttpUtility.UrlEncode(postProcessPaymentRequest.Order.BillingAddress.Address2));
			builder.AppendFormat("&city={0}", HttpUtility.UrlEncode(postProcessPaymentRequest.Order.BillingAddress.City));
			//if (!String.IsNullOrEmpty(postProcessPaymentRequest.Order.BillingAddress.PhoneNumber))
			//{
			//    //strip out all non-digit characters from phone number;
			//    string billingPhoneNumber = System.Text.RegularExpressions.Regex.Replace(postProcessPaymentRequest.Order.BillingAddress.PhoneNumber, @"\D", string.Empty);
			//    if (billingPhoneNumber.Length >= 10)
			//    {
			//        builder.AppendFormat("&night_phone_a={0}", HttpUtility.UrlEncode(billingPhoneNumber.Substring(0, 3)));
			//        builder.AppendFormat("&night_phone_b={0}", HttpUtility.UrlEncode(billingPhoneNumber.Substring(3, 3)));
			//        builder.AppendFormat("&night_phone_c={0}", HttpUtility.UrlEncode(billingPhoneNumber.Substring(6, 4)));
			//    }
			//}
			if (postProcessPaymentRequest.Order.BillingAddress.StateProvince != null)
				builder.AppendFormat("&state={0}", HttpUtility.UrlEncode(postProcessPaymentRequest.Order.BillingAddress.StateProvince.Abbreviation));
			else
				builder.AppendFormat("&state={0}", "");

			if (postProcessPaymentRequest.Order.BillingAddress.Country != null)
				builder.AppendFormat("&country={0}", HttpUtility.UrlEncode(postProcessPaymentRequest.Order.BillingAddress.Country.TwoLetterIsoCode));
			else
				builder.AppendFormat("&country={0}", "");

			builder.AppendFormat("&zip={0}", HttpUtility.UrlEncode(postProcessPaymentRequest.Order.BillingAddress.ZipPostalCode));
			builder.AppendFormat("&email={0}", HttpUtility.UrlEncode(postProcessPaymentRequest.Order.BillingAddress.Email));

			_httpContext.Response.Redirect(builder.ToString());
		}

		/// <summary>
		/// Gets additional handling fee
		/// </summary>
		/// <param name="cart">Shoping cart</param>
		/// <returns>Additional handling fee</returns>
		public override decimal GetAdditionalHandlingFee(IList<OrganizedShoppingCartItem> cart)
		{
			var result = this.CalculateAdditionalFee(_orderTotalCalculationService, cart,
				_paypalStandardPaymentSettings.AdditionalFee, _paypalStandardPaymentSettings.AdditionalFeePercentage);
			return result;
		}

		/// <summary>
		/// Gets a value indicating whether customers can complete a payment after order is placed but not completed (for redirection payment methods)
		/// </summary>
		/// <param name="order">Order</param>
		/// <returns>Result</returns>
		public override bool CanRePostProcessPayment(Order order)
		{
			if (order == null)
				throw new ArgumentNullException("order");

			if (order.PaymentStatus == PaymentStatus.Pending && (DateTime.UtcNow - order.CreatedOnUtc).TotalSeconds > 5)
			{
				return true;
			}
			return true;
		}

		/// <summary>
		/// Gets a route for provider configuration
		/// </summary>
		/// <param name="actionName">Action name</param>
		/// <param name="controllerName">Controller name</param>
		/// <param name="routeValues">Route values</param>
		public override void GetConfigurationRoute(out string actionName, out string controllerName, out RouteValueDictionary routeValues)
		{
			actionName = "Configure";
			controllerName = "PaymentPayPalStandard";
			routeValues = new RouteValueDictionary() { { "Namespaces", "SmartStore.Plugin.Payments.PayPalStandard.Controllers" }, { "area", "Payments.PayPalStandard" } };
		}

		/// <summary>
		/// Gets a route for payment info
		/// </summary>
		/// <param name="actionName">Action name</param>
		/// <param name="controllerName">Controller name</param>
		/// <param name="routeValues">Route values</param>
		public override void GetPaymentInfoRoute(out string actionName, out string controllerName, out RouteValueDictionary routeValues)
		{
			actionName = "PaymentInfo";
			controllerName = "PaymentPayPalStandard";
			routeValues = new RouteValueDictionary() { { "Namespaces", "SmartStore.Plugin.Payments.PayPalStandard.Controllers" }, { "area", "Payments.PayPalStandard" } };
		}

		public override Type GetControllerType()
		{
			return typeof(PaymentPayPalStandardController);
		}

		public override void Install()
		{
			//settings
			var settings = new PayPalStandardPaymentSettings()
			{
				//UseSandbox = true,
				//BusinessEmail = "test@test.com",
				//PdtToken = "PDT token...",
				PdtValidateOrderTotal = true,
				EnableIpn = true,
			};
			_settingService.SaveSetting(settings);

			_localizationService.ImportPluginResourcesFromXml(this.PluginDescriptor);

			base.Install();
		}

		public override void Uninstall()
		{
			//settings
			_settingService.DeleteSetting<PayPalStandardPaymentSettings>();

			_localizationService.DeleteLocaleStringResources(this.PluginDescriptor.ResourceRootKey);
			_localizationService.DeleteLocaleStringResources("Plugins.FriendlyName.Payments.PayPalStandard", false);

			base.Uninstall();
		}

		#endregion

		#region Properties

		public override PaymentMethodType PaymentMethodType
		{
			get
			{
				return PaymentMethodType.Redirection;
			}
		}

		#endregion
	}
}
