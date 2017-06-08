﻿using System;
using System.Collections.Generic;
using System.Web.Mvc;
using SmartStore.AmazonPay.Models;
using SmartStore.AmazonPay.Services;
using SmartStore.AmazonPay.Settings;
using SmartStore.Core.Domain.Customers;
using SmartStore.Services.Authentication.External;
using SmartStore.Services.Payments;
using SmartStore.Web.Framework;
using SmartStore.Web.Framework.Controllers;
using SmartStore.Web.Framework.Security;
using SmartStore.Web.Framework.Settings;

namespace SmartStore.AmazonPay.Controllers
{
	public class AmazonPayController : PaymentControllerBase
	{
		private readonly IAmazonPayService _apiService;
		private readonly Lazy<IOpenAuthenticationService> _openAuthenticationService;
		private readonly Lazy<ExternalAuthenticationSettings> _externalAuthenticationSettings;

		public AmazonPayController(
			IAmazonPayService apiService,
			Lazy<IOpenAuthenticationService> openAuthenticationService,
			Lazy<ExternalAuthenticationSettings> externalAuthenticationSettings)
		{
			_apiService = apiService;
			_openAuthenticationService = openAuthenticationService;
			_externalAuthenticationSettings = externalAuthenticationSettings;
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

		[AdminAuthorize]
		public ActionResult Configure()
		{
			var model = new ConfigurationModel();
			int storeScope = this.GetActiveStoreScopeConfiguration(Services.StoreService, Services.WorkContext);
			var settings = Services.Settings.LoadSetting<AmazonPaySettings>(storeScope);

			model.Copy(settings, true);

			_apiService.SetupConfiguration(model);

			var storeDependingSettingHelper = new StoreDependingSettingHelper(ViewData);
			storeDependingSettingHelper.GetOverrideKeys(settings, model, storeScope, Services.Settings);

			return View(model);
		}

		[HttpPost, AdminAuthorize]
		public ActionResult Configure(ConfigurationModel model, FormCollection form)
		{
			if (!ModelState.IsValid)
				return Configure();

			ModelState.Clear();

			var storeDependingSettingHelper = new StoreDependingSettingHelper(ViewData);
			int storeScope = this.GetActiveStoreScopeConfiguration(Services.StoreService, Services.WorkContext);
			var settings = Services.Settings.LoadSetting<AmazonPaySettings>(storeScope);

			model.Copy(settings, false);

			using (Services.Settings.BeginScope())
			{
				storeDependingSettingHelper.UpdateSettings(settings, form, storeScope, Services.Settings);

				Services.Settings.SaveSetting(settings, x => x.DataFetching, 0, false);
				Services.Settings.SaveSetting(settings, x => x.PollingMaxOrderCreationDays, 0, false);
			}

			_apiService.DataPollingTaskUpdate(settings.DataFetching == AmazonPayDataFetchingType.Polling, model.PollingTaskMinutes * 60);

			NotifySuccess(Services.Localization.GetResource("Plugins.Payments.AmazonPay.ConfigSaveNote"));

			return Configure();
		}

		[HttpPost]
		[ValidateInput(false)]
		[RequireHttpsByConfigAttribute(SslRequirement.Yes)]
		public ActionResult IPNHandler()
		{
			_apiService.ProcessIpn(Request);
			return Content("OK");
		}

		// Authentication

		[ChildActionOnly]
		public ActionResult AuthenticationPublicInfo()
		{
			var model = _apiService.CreateViewModel(AmazonPayRequestType.AuthenticationPublicInfo, TempData);
			return View(model);
		}

		public ActionResult AuthenticationButtonHandler(string addressConsentToken, string returnUrl)
		{
			var processor = _openAuthenticationService.Value.LoadExternalAuthenticationMethodBySystemName(AmazonPayPlugin.SystemName, Services.StoreContext.CurrentStore.Id);
			if (processor == null || !processor.IsMethodActive(_externalAuthenticationSettings.Value))
			{
				throw new SmartException(T("Plugins.Payments.AmazonPay.AuthenticationNotActive"));
			}

			return HttpContext.Request.IsAuthenticated ?
				RedirectToReferrer(returnUrl, "~/") :
				new RedirectResult(Url.LogOn(returnUrl));
		}
	}
}
