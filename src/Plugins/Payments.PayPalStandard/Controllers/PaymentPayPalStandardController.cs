﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Text;
using System.Linq;
using System.Web.Mvc;
using SmartStore.Core;
using SmartStore.Core.Domain.Orders;
using SmartStore.Core.Domain.Payments;
using SmartStore.Plugin.Payments.PayPalStandard.Models;
using SmartStore.Services.Configuration;
using SmartStore.Core.Logging;
using SmartStore.Services.Orders;
using SmartStore.Services.Payments;
using SmartStore.Web.Framework.Controllers;
using SmartStore.Plugin.Payments.PayPalStandard.Services;
using SmartStore.Services.Localization;
using Autofac;

namespace SmartStore.Plugin.Payments.PayPalStandard.Controllers
{
	public class PaymentPayPalStandardController : PaymentControllerBase
	{
		private readonly ISettingService _settingService;
		private readonly IPaymentService _paymentService;
		private readonly IOrderService _orderService;
		private readonly IOrderProcessingService _orderProcessingService;
		private readonly IStoreContext _storeContext;
		private readonly IWorkContext _workContext;
		private readonly IWebHelper _webHelper;
		private readonly PayPalStandardPaymentSettings _paypalStandardPaymentSettings;
		private readonly PaymentSettings _paymentSettings;
		private readonly ILocalizationService _localizationService;
		private readonly IPayPalStandardService _payPalStandardService;

		public PaymentPayPalStandardController(ISettingService settingService,
			IPaymentService paymentService, IOrderService orderService,
			IOrderProcessingService orderProcessingService,
			IStoreContext storeContext,
			IWorkContext workContext,
			IWebHelper webHelper,
			PayPalStandardPaymentSettings paypalStandardPaymentSettings,
			PaymentSettings paymentSettings,
			ILocalizationService localizationService,
			IPayPalStandardService payPalStandardService)
		{
			this._settingService = settingService;
			this._paymentService = paymentService;
			this._orderService = orderService;
			this._orderProcessingService = orderProcessingService;
			this._storeContext = storeContext;
			this._workContext = workContext;
			this._webHelper = webHelper;
			this._paypalStandardPaymentSettings = paypalStandardPaymentSettings;
			this._paymentSettings = paymentSettings;
			this._localizationService = localizationService;
			this._payPalStandardService = payPalStandardService;
		}

		[AdminAuthorize]
		[ChildActionOnly]
		public ActionResult Configure()
		{
			var model = new ConfigurationModel();
			model.UseSandbox = _paypalStandardPaymentSettings.UseSandbox;
			model.BusinessEmail = _paypalStandardPaymentSettings.BusinessEmail;
			model.PdtToken = _paypalStandardPaymentSettings.PdtToken;
			model.PdtValidateOrderTotal = _paypalStandardPaymentSettings.PdtValidateOrderTotal;
			model.AdditionalFee = _paypalStandardPaymentSettings.AdditionalFee;
			model.AdditionalFeePercentage = _paypalStandardPaymentSettings.AdditionalFeePercentage;
			model.PassProductNamesAndTotals = _paypalStandardPaymentSettings.PassProductNamesAndTotals;
			model.EnableIpn = _paypalStandardPaymentSettings.EnableIpn;
			model.IpnUrl = _paypalStandardPaymentSettings.IpnUrl;

			return View(model);
		}

		[HttpPost]
		[AdminAuthorize]
		[ChildActionOnly]
		public ActionResult Configure(ConfigurationModel model, FormCollection form)
		{
			if (!ModelState.IsValid)
				return Configure();

			//save settings
			_paypalStandardPaymentSettings.UseSandbox = model.UseSandbox;
			_paypalStandardPaymentSettings.BusinessEmail = model.BusinessEmail;
			_paypalStandardPaymentSettings.PdtToken = model.PdtToken;
			_paypalStandardPaymentSettings.PdtValidateOrderTotal = model.PdtValidateOrderTotal;
			_paypalStandardPaymentSettings.AdditionalFee = model.AdditionalFee;
			_paypalStandardPaymentSettings.AdditionalFeePercentage = model.AdditionalFeePercentage;
			_paypalStandardPaymentSettings.PassProductNamesAndTotals = model.PassProductNamesAndTotals;
			_paypalStandardPaymentSettings.EnableIpn = model.EnableIpn;
			_paypalStandardPaymentSettings.IpnUrl = model.IpnUrl;
			_settingService.SaveSetting(_paypalStandardPaymentSettings);

			return View(model);
		}

		[ChildActionOnly]
		public ActionResult PaymentInfo()
		{
			return View();
		}

		[NonAction]
		public override IList<string> ValidatePaymentForm(FormCollection form)
		{
			var warnings = new List<string>();
			return warnings;
		}

		[NonAction]
		public override ProcessPaymentRequest GetPaymentInfo(FormCollection form)
		{
			var paymentInfo = new ProcessPaymentRequest();
			return paymentInfo;
		}

		[ValidateInput(false)]
		public ActionResult PDTHandler(FormCollection form)
		{
			string tx = _webHelper.QueryString<string>("tx");
			Dictionary<string, string> values;
			string response;

			var provider = _paymentService.LoadPaymentMethodBySystemName("Payments.PayPalStandard", true);
			var processor = provider != null ? provider.Value as PayPalStandardPaymentProcessor : null;
			if (processor == null)
				throw new SmartException(_localizationService.GetResource("Plugins.Payments.PayPalStandard.NoModuleLoading"));

			if (processor.GetPDTDetails(tx, out values, out response))
			{
				string orderNumber = string.Empty;
				values.TryGetValue("custom", out orderNumber);
				Guid orderNumberGuid = Guid.Empty;
				try
				{
					orderNumberGuid = new Guid(orderNumber);
				}
				catch { }
				Order order = _orderService.GetOrderByGuid(orderNumberGuid);
				if (order != null)
				{
					decimal total = decimal.Zero;
					try
					{
						total = decimal.Parse(values["mc_gross"], new CultureInfo("en-US"));
					}
					catch (Exception exc)
					{
						Logger.Error(_localizationService.GetResource("Plugins.Payments.PayPalStandard.FailedGetGross"), exc);
					}

					string payer_status = string.Empty;
					values.TryGetValue("payer_status", out payer_status);
					string payment_status = string.Empty;
					values.TryGetValue("payment_status", out payment_status);
					string pending_reason = string.Empty;
					values.TryGetValue("pending_reason", out pending_reason);
					string mc_currency = string.Empty;
					values.TryGetValue("mc_currency", out mc_currency);
					string txn_id = string.Empty;
					values.TryGetValue("txn_id", out txn_id);
					string payment_type = string.Empty;
					values.TryGetValue("payment_type", out payment_type);
					string payer_id = string.Empty;
					values.TryGetValue("payer_id", out payer_id);
					string receiver_id = string.Empty;
					values.TryGetValue("receiver_id", out receiver_id);
					string invoice = string.Empty;
					values.TryGetValue("invoice", out invoice);
					string payment_fee = string.Empty;
					values.TryGetValue("payment_fee", out payment_fee);

					string paymentNote = _localizationService.GetResource("Plugins.Payments.PayPalStandard.PaymentNote").FormatWith(
						total, mc_currency, payer_status, payment_status, pending_reason, txn_id, payment_type,	payer_id, receiver_id, invoice, payment_fee);

					//order note
					order.OrderNotes.Add(new OrderNote()
					{
						Note = paymentNote,
						DisplayToCustomer = false,
						CreatedOnUtc = DateTime.UtcNow
					});
					_orderService.UpdateOrder(order);

					//validate order total
					if (_paypalStandardPaymentSettings.PdtValidateOrderTotal && !Math.Round(total, 2).Equals(Math.Round(order.OrderTotal, 2)))
					{
						Logger.Error(_localizationService.GetResource("Plugins.Payments.PayPalStandard.UnequalTotalOrder").FormatWith(total, order.OrderTotal));

						return RedirectToAction("Index", "Home", new { area = "" });
					}

					//mark order as paid
					if (_orderProcessingService.CanMarkOrderAsPaid(order))
					{
						order.AuthorizationTransactionId = txn_id;
						_orderService.UpdateOrder(order);

						_orderProcessingService.MarkOrderAsPaid(order);
					}
				}

				return RedirectToAction("Completed", "Checkout", new { area = "" });
			}
			else
			{
				string orderNumber = string.Empty;
				values.TryGetValue("custom", out orderNumber);
				Guid orderNumberGuid = Guid.Empty;
				try
				{
					orderNumberGuid = new Guid(orderNumber);
				}
				catch { }
				Order order = _orderService.GetOrderByGuid(orderNumberGuid);
				if (order != null)
				{
					//order note
					order.OrderNotes.Add(new OrderNote()
					{
						Note = "{0} {1}".FormatWith(_localizationService.GetResource("Plugins.Payments.PayPalStandard.PdtFailed"), response),
						DisplayToCustomer = false,
						CreatedOnUtc = DateTime.UtcNow
					});
					_orderService.UpdateOrder(order);
				}
				return RedirectToAction("Index", "Home", new { area = "" });
			}
		}

		[ValidateInput(false)]
		public ActionResult IPNHandler()
		{
			Debug.WriteLine("PayPal Standard IPN: {0}".FormatWith(Request.ContentLength));

			byte[] param = Request.BinaryRead(Request.ContentLength);
			string strRequest = Encoding.ASCII.GetString(param);
			Dictionary<string, string> values;

			var provider = _paymentService.LoadPaymentMethodBySystemName("Payments.PayPalStandard", true);
			var processor = provider != null ? provider.Value as PayPalStandardPaymentProcessor : null;
			if (processor == null)
				throw new SmartException(_localizationService.GetResource("Plugins.Payments.PayPalStandard.NoModuleLoading"));

			if (processor.VerifyIPN(strRequest, out values))
			{
				#region values
				decimal total = decimal.Zero;
				try
				{
					total = decimal.Parse(values["mc_gross"], new CultureInfo("en-US"));
				}
				catch { }

				string payer_status = string.Empty;
				values.TryGetValue("payer_status", out payer_status);
				string payment_status = string.Empty;
				values.TryGetValue("payment_status", out payment_status);
				string pending_reason = string.Empty;
				values.TryGetValue("pending_reason", out pending_reason);
				string mc_currency = string.Empty;
				values.TryGetValue("mc_currency", out mc_currency);
				string txn_id = string.Empty;
				values.TryGetValue("txn_id", out txn_id);
				string txn_type = string.Empty;
				values.TryGetValue("txn_type", out txn_type);
				string rp_invoice_id = string.Empty;
				values.TryGetValue("rp_invoice_id", out rp_invoice_id);
				string payment_type = string.Empty;
				values.TryGetValue("payment_type", out payment_type);
				string payer_id = string.Empty;
				values.TryGetValue("payer_id", out payer_id);
				string receiver_id = string.Empty;
				values.TryGetValue("receiver_id", out receiver_id);
				string invoice = string.Empty;
				values.TryGetValue("invoice", out invoice);
				string payment_fee = string.Empty;
				values.TryGetValue("payment_fee", out payment_fee);

				#endregion

				var sb = new StringBuilder();
				sb.AppendLine("PayPal IPN:");
				foreach (KeyValuePair<string, string> kvp in values)
				{
					sb.AppendLine(kvp.Key + ": " + kvp.Value);
				}

				var newPaymentStatus = _payPalStandardService.GetPaymentStatus(payment_status, pending_reason);
				sb.AppendLine("{0}: {1}".FormatWith(_localizationService.GetResource("Plugins.Payments.PayPalStandard.NewPaymentStatus"), newPaymentStatus));

				switch (txn_type)
				{
					case "recurring_payment_profile_created":
						//do nothing here
						break;
					case "recurring_payment":
						#region Recurring payment
						{
							Guid orderNumberGuid = Guid.Empty;
							try
							{
								orderNumberGuid = new Guid(rp_invoice_id);
							}
							catch { }

							var initialOrder = _orderService.GetOrderByGuid(orderNumberGuid);
							if (initialOrder != null)
							{
								var recurringPayments = _orderService.SearchRecurringPayments(0, 0, initialOrder.Id, null);
								foreach (var rp in recurringPayments)
								{
									switch (newPaymentStatus)
									{
										case PaymentStatus.Authorized:
										case PaymentStatus.Paid: {
												var recurringPaymentHistory = rp.RecurringPaymentHistory;
												if (recurringPaymentHistory.Count == 0)
												{
													//first payment
													var rph = new RecurringPaymentHistory()
													{
														RecurringPaymentId = rp.Id,
														OrderId = initialOrder.Id,
														CreatedOnUtc = DateTime.UtcNow
													};
													rp.RecurringPaymentHistory.Add(rph);
													_orderService.UpdateRecurringPayment(rp);
												}
												else
												{
													//next payments
													_orderProcessingService.ProcessNextRecurringPayment(rp);
												}
											}
											break;
									}
								}

								//this.OrderService.InsertOrderNote(newOrder.OrderId, sb.ToString(), DateTime.UtcNow);
								Logger.Information(_localizationService.GetResource("Plugins.Payments.PayPalStandard.IpnLogInfo"), new SmartException(sb.ToString()));
							}
							else
							{
								Logger.Error(_localizationService.GetResource("Plugins.Payments.PayPalStandard.IpnOrderNotFound"), new SmartException(sb.ToString()));
							}
						}
						#endregion
						break;
					default:
						#region Standard payment
						{
							string orderNumber = string.Empty;
							values.TryGetValue("custom", out orderNumber);
							Guid orderNumberGuid = Guid.Empty;
							try
							{
								orderNumberGuid = new Guid(orderNumber);
							}
							catch { }

							var order = _orderService.GetOrderByGuid(orderNumberGuid);
							if (order != null)
							{
								//order note
								order.OrderNotes.Add(new OrderNote()
								{
									Note = sb.ToString(),
									DisplayToCustomer = false,
									CreatedOnUtc = DateTime.UtcNow
								});
								_orderService.UpdateOrder(order);

								switch (newPaymentStatus)
								{
									case PaymentStatus.Pending:
										{
										}
										break;
									case PaymentStatus.Authorized: {
											if (_orderProcessingService.CanMarkOrderAsAuthorized(order))
											{
												_orderProcessingService.MarkAsAuthorized(order);
											}
										}
										break;
									case PaymentStatus.Paid:
										{
											if (_orderProcessingService.CanMarkOrderAsPaid(order))
											{

												order.AuthorizationTransactionId = txn_id;
												_orderService.UpdateOrder(order);

												_orderProcessingService.MarkOrderAsPaid(order);
											}
										}
										break;
									case PaymentStatus.Refunded: {
											if (_orderProcessingService.CanRefundOffline(order))
											{
												_orderProcessingService.RefundOffline(order);
											}
										}
										break;
									case PaymentStatus.Voided: {
											if (_orderProcessingService.CanVoidOffline(order))
											{
												_orderProcessingService.VoidOffline(order);
											}
										}
										break;
									default:
										break;
								}
							}
							else
							{
								Logger.Error(_localizationService.GetResource("Plugins.Payments.PayPalStandard.IpnOrderNotFound"), new SmartException(sb.ToString()));
							}
						}
						#endregion
						break;
				}
			}
			else
			{
				Logger.Error(_localizationService.GetResource("Plugins.Payments.PayPalStandard.IpnFailed"), new SmartException(strRequest));
			}

			//nothing should be rendered to visitor
			return Content("");
		}

		public ActionResult CancelOrder(FormCollection form)
		{
			var order = _orderService.SearchOrders(_storeContext.CurrentStore.Id, _workContext.CurrentCustomer.Id, null, null, null, null, null, null, null, null, 0, 1)
				.FirstOrDefault();

			if (order != null)
			{
				return RedirectToAction("Details", "Order", new { id = order.Id });
			}

			return RedirectToAction("Index", "Home", new { area = "" });
		}
	}
}