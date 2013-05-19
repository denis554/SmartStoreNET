﻿using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using System.Web.Routing;
using SmartStore.Admin.Models.Payments;
using SmartStore.Core;
using SmartStore.Core.Domain.Payments;
using SmartStore.Core.Plugins;
using SmartStore.Services.Configuration;
using SmartStore.Services.Payments;
using SmartStore.Services.Security;
using SmartStore.Services.Localization;
using SmartStore.Web.Framework;
using SmartStore.Web.Framework.Controllers;
using Telerik.Web.Mvc;

namespace SmartStore.Admin.Controllers
{
	[AdminAuthorize]
    public partial class PaymentController : AdminControllerBase
	{
		#region Fields

        private readonly IPaymentService _paymentService;
        private readonly PaymentSettings _paymentSettings;
        private readonly ISettingService _settingService;
        private readonly IPermissionService _permissionService;
        private readonly IPluginFinder _pluginFinder;
        private readonly ILocalizationService _localizationService;

		#endregion

		#region Constructors

        public PaymentController(IPaymentService paymentService, PaymentSettings paymentSettings,
            ISettingService settingService, IPermissionService permissionService,
            IPluginFinder pluginFinder, ILocalizationService localizationService)
		{
            this._paymentService = paymentService;
            this._paymentSettings = paymentSettings;
            this._settingService = settingService;
            this._permissionService = permissionService;
            this._pluginFinder = pluginFinder;
            this._localizationService = localizationService;
		}

		#endregion 

        #region Methods

        public ActionResult Methods()
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManagePaymentMethods))
                return AccessDeniedView();

            var paymentMethodsModel = new List<PaymentMethodModel>();
            var paymentMethods = _paymentService.LoadAllPaymentMethods();
            foreach (var paymentMethod in paymentMethods)
            {
                var tmp1 = paymentMethod.ToModel();
                tmp1.IsActive = paymentMethod.IsPaymentMethodActive(_paymentSettings);
                tmp1.RecurringPaymentType = paymentMethod.RecurringPaymentType.GetLocalizedEnum(_localizationService);
                paymentMethodsModel.Add(tmp1);
            }
            var gridModel = new GridModel<PaymentMethodModel>
            {
                Data = paymentMethodsModel,
                Total = paymentMethodsModel.Count()
            };
            return View(gridModel);
        }

        [HttpPost, GridAction(EnableCustomBinding = true)]
        public ActionResult Methods(GridCommand command)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManagePaymentMethods))
                return AccessDeniedView();

            var paymentMethodsModel = new List<PaymentMethodModel>();
            var paymentMethods = _paymentService.LoadAllPaymentMethods();
            foreach (var paymentMethod in paymentMethods)
            {
                var tmp1 = paymentMethod.ToModel();
                tmp1.IsActive = paymentMethod.IsPaymentMethodActive(_paymentSettings);
                paymentMethodsModel.Add(tmp1);
            }
            paymentMethodsModel = paymentMethodsModel.ForCommand(command).ToList();
            var gridModel = new GridModel<PaymentMethodModel>
            {
                Data = paymentMethodsModel,
                Total = paymentMethodsModel.Count()
            };
            return new JsonResult
            {
                Data = gridModel
            };
        }

        [GridAction(EnableCustomBinding = true)]
        public ActionResult MethodUpdate(PaymentMethodModel model, GridCommand command)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManagePaymentMethods))
                return AccessDeniedView();

            var pm = _paymentService.LoadPaymentMethodBySystemName(model.SystemName);
            if (pm.IsPaymentMethodActive(_paymentSettings))
            {
                if (!model.IsActive)
                {
                    //mark as disabled
                    _paymentSettings.ActivePaymentMethodSystemNames.Remove(pm.PluginDescriptor.SystemName);
                    _settingService.SaveSetting(_paymentSettings);
                }
            }
            else
            {
                if (model.IsActive)
                {
                    //mark as active
                    _paymentSettings.ActivePaymentMethodSystemNames.Add(pm.PluginDescriptor.SystemName);
                    _settingService.SaveSetting(_paymentSettings);
                }
            }
            var pluginDescriptor = pm.PluginDescriptor;
            pluginDescriptor.FriendlyName = model.FriendlyName;
            pluginDescriptor.DisplayOrder = model.DisplayOrder;
            PluginFileParser.SavePluginDescriptionFile(pluginDescriptor);
            //reset plugin cache
            _pluginFinder.ReloadPlugins();
            
            return Methods(command);
        }

        public ActionResult ConfigureMethod(string systemName)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManagePaymentMethods))
                return AccessDeniedView();

            var pm = _paymentService.LoadPaymentMethodBySystemName(systemName);
            if (pm == null)
                //No payment method found with the specified id
                return RedirectToAction("Methods");

            var model = pm.ToModel();
            string actionName, controllerName;
            RouteValueDictionary routeValues;
            pm.GetConfigurationRoute(out actionName, out controllerName, out routeValues);
            model.ConfigurationActionName = actionName;
            model.ConfigurationControllerName = controllerName;
            model.ConfigurationRouteValues = routeValues;
            return View(model);
        }

        #endregion
    }
}
