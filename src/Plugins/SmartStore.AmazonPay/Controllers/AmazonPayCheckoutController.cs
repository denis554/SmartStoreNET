﻿using System.Web;
using System.Web.Mvc;
using SmartStore.AmazonPay.Services;
using SmartStore.Services.Common;

namespace SmartStore.AmazonPay.Controllers
{
	public class AmazonPayCheckoutController : AmazonPayControllerBase
	{
		private readonly HttpContextBase _httpContext;
		private readonly IAmazonPayService _apiService;
		private readonly IGenericAttributeService _genericAttributeService;

		public AmazonPayCheckoutController(
			HttpContextBase httpContext,
			IAmazonPayService apiService,
			IGenericAttributeService genericAttributeService)
		{
			_httpContext = httpContext;
			_apiService = apiService;
			_genericAttributeService = genericAttributeService;
		}

		public ActionResult BillingAddress()
		{
			return RedirectToAction("ShippingAddress", "Checkout", new { area = "" });
		}

		public ActionResult ShippingAddress()
		{
			var model = _apiService.CreateViewModel(AmazonPayRequestType.Address, TempData);

			return GetActionResult(model);
		}

		public ActionResult PaymentMethod()
		{
			var model = _apiService.CreateViewModel(AmazonPayRequestType.PaymentMethod, TempData);

			return GetActionResult(model);
		}

		[HttpPost]
		public ActionResult PaymentMethod(FormCollection form)
		{
			_apiService.GetBillingAddress();

			return RedirectToAction("Confirm", "Checkout", new { area = "" });
		}

		public ActionResult PaymentInfo()
		{
			return RedirectToAction("PaymentMethod", "Checkout", new { area = "" });
		}

		public ActionResult CheckoutCompleted()
		{
			var note = _httpContext.Session["AmazonPayCheckoutCompletedNote"] as string;
			if (note.HasValue())
			{
				return Content(note);
			}

			return new EmptyResult();
		}
	}
}