﻿using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using System.Web.Routing;
using SmartStore.Admin.Models.ExternalAuthentication;
using SmartStore.Core.Domain.Customers;
using SmartStore.Services.Authentication.External;
using SmartStore.Services.Configuration;
using SmartStore.Services.Security;
using SmartStore.Web.Framework;
using SmartStore.Web.Framework.Controllers;
using Telerik.Web.Mvc;

namespace SmartStore.Admin.Controllers
{
	[AdminAuthorize]
    public class ExternalAuthenticationController : AdminControllerBase
	{
		#region Fields

        private readonly IOpenAuthenticationService _openAuthenticationService;
        private readonly ExternalAuthenticationSettings _externalAuthenticationSettings;
        private readonly ISettingService _settingService;
        private readonly IPermissionService _permissionService;

		#endregion

		#region Constructors

        public ExternalAuthenticationController(IOpenAuthenticationService openAuthenticationService, 
            ExternalAuthenticationSettings externalAuthenticationSettings,
            ISettingService settingService, IPermissionService permissionService)
		{
            this._openAuthenticationService = openAuthenticationService;
            this._externalAuthenticationSettings = externalAuthenticationSettings;
            this._settingService = settingService;
            this._permissionService = permissionService;
		}

		#endregion 

        #region Methods

        public ActionResult Methods()
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageExternalAuthenticationMethods))
                return AccessDeniedView();

            var methodsModel = new List<AuthenticationMethodModel>();
            var methods = _openAuthenticationService.LoadAllExternalAuthenticationMethods();
            foreach (var method in methods)
            {
                var tmp1 = method.ToModel();
                tmp1.IsActive = method.IsMethodActive(_externalAuthenticationSettings);
                methodsModel.Add(tmp1);
            }
            var gridModel = new GridModel<AuthenticationMethodModel>
            {
                Data = methodsModel,
                Total = methodsModel.Count()
            };
            return View(gridModel);
        }

        [HttpPost, GridAction(EnableCustomBinding = true)]
        public ActionResult Methods(GridCommand command)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageExternalAuthenticationMethods))
                return AccessDeniedView();

            var methodsModel = new List<AuthenticationMethodModel>();
            var methods = _openAuthenticationService.LoadAllExternalAuthenticationMethods();
            foreach (var method in methods)
            {
                var tmp1 = method.ToModel();
                tmp1.IsActive = method.IsMethodActive(_externalAuthenticationSettings);
                methodsModel.Add(tmp1);
            }
            methodsModel = methodsModel.ForCommand(command).ToList();
            var gridModel = new GridModel<AuthenticationMethodModel>
            {
                Data = methodsModel,
                Total = methodsModel.Count()
            };
            return new JsonResult
            {
                Data = gridModel
            };
        }

        [GridAction(EnableCustomBinding = true)]
        public ActionResult MethodUpdate(AuthenticationMethodModel model, GridCommand command)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageExternalAuthenticationMethods))
                return AccessDeniedView();

            var eam = _openAuthenticationService.LoadExternalAuthenticationMethodBySystemName(model.SystemName);
            if (eam.IsMethodActive(_externalAuthenticationSettings))
            {
                if (!model.IsActive)
                {
                    //mark as disabled
                    _externalAuthenticationSettings.ActiveAuthenticationMethodSystemNames.Remove(eam.PluginDescriptor.SystemName);
                    _settingService.SaveSetting(_externalAuthenticationSettings);
                }
            }
            else
            {
                if (model.IsActive)
                {
                    //mark as active
                    _externalAuthenticationSettings.ActiveAuthenticationMethodSystemNames.Add(eam.PluginDescriptor.SystemName);
                    _settingService.SaveSetting(_externalAuthenticationSettings);
                }
            }
            
            return Methods(command);
        }

        public ActionResult ConfigureMethod(string systemName)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageExternalAuthenticationMethods))
                return AccessDeniedView();

            var eam = _openAuthenticationService.LoadExternalAuthenticationMethodBySystemName(systemName);
            if (eam == null)
                //No authentication method found with the specified id
                return RedirectToAction("Methods");

            var model = eam.ToModel();
            string actionName, controllerName;
            RouteValueDictionary routeValues;
            eam.GetConfigurationRoute(out actionName, out controllerName, out routeValues);
            model.ConfigurationActionName = actionName;
            model.ConfigurationControllerName = controllerName;
            model.ConfigurationRouteValues = routeValues;
            return View(model);
        }

        #endregion
    }
}
