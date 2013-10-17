﻿using System;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using SmartStore.Core;
using SmartStore.Core.Data;
using SmartStore.Core.Domain.Localization;
using SmartStore.Core.Infrastructure;
using SmartStore.Web.Framework.Localization;

namespace SmartStore.Web.Framework.Controllers
{
    /// <summary>
    /// Attribute which ensures that store URL contains a language SEO code if "SEO friendly URLs with multiple languages" setting is enabled
    /// </summary>
    public class LanguageSeoCodeAttribute : ActionFilterAttribute
    {

        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            if (filterContext == null || filterContext.HttpContext == null)
                return;

            HttpRequestBase request = filterContext.HttpContext.Request;
            if (request == null)
                return;

            //don't apply filter to child methods
            if (filterContext.IsChildAction)
                return;

            //only GET requests
            if (!String.Equals(filterContext.HttpContext.Request.HttpMethod, "GET", StringComparison.OrdinalIgnoreCase))
                return;

            // ensure that this route is registered and localizable (LocalizedRoute in RouteProvider.cs)
            if (filterContext.RouteData == null || filterContext.RouteData.Route == null || !(filterContext.RouteData.Route is LocalizedRoute))
                return;

            if (!DataSettingsHelper.DatabaseIsInstalled())
                return;

            var localizationSettings = EngineContext.Current.Resolve<LocalizationSettings>();
            if (!localizationSettings.SeoFriendlyUrlsForLanguagesEnabled)
                return;
            
            // process current URL
            var workContext = EngineContext.Current.Resolve<IWorkContext>();
            var workingLanguage = workContext.WorkingLanguage;
            var helper = new LocalizedUrlHelper(filterContext.HttpContext.Request, true);
            string defaultSeoCode = workContext.GetDefaultLanguageSeoCode();

            string seoCode;
            if (helper.IsLocalizedUrl(out seoCode)) 
            {
                if (!workContext.IsPublishedLanguage(seoCode))
                {
                    // language is not defined in system or not assigned to store
                    if (localizationSettings.InvalidLanguageRedirectBehaviour == InvalidLanguageRedirectBehaviour.ReturnHttp404)
                    {
                        filterContext.Result = new RedirectResult("~/404");
                    }
                    else if (localizationSettings.InvalidLanguageRedirectBehaviour == InvalidLanguageRedirectBehaviour.FallbackToWorkingLanguage)
                    {
                        helper.StripSeoCode();
                        filterContext.Result = new RedirectResult(helper.GetAbsolutePath(), true);
                    }
                }
                else
                {
                    // redirect default language (if desired)
                    if (seoCode == defaultSeoCode && localizationSettings.DefaultLanguageRedirectBehaviour == DefaultLanguageRedirectBehaviour.StripSeoCode)
                    {
                        helper.StripSeoCode();
                        filterContext.Result = new RedirectResult(helper.GetAbsolutePath(), true);
                    }
                }

                // already localized URL, skip the rest
                return;
            }

            // keep default language prefixless (if desired)
            if (workingLanguage.UniqueSeoCode == defaultSeoCode && (int)(localizationSettings.DefaultLanguageRedirectBehaviour) > 0)
            {
                return;
            }

            // add language code to URL
            helper.PrependSeoCode(workingLanguage.UniqueSeoCode);
            filterContext.Result = new RedirectResult(helper.GetAbsolutePath());
        }

    }
}
