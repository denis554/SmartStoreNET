﻿using System;
using System.Web;
using System.Web.Mvc;
using SmartStore.Core;
using SmartStore.Core.Infrastructure;
using SmartStore.Services.Affiliates;
using SmartStore.Services.Customers;

namespace SmartStore.Web.Framework.Controllers
{
    public class CheckAffiliateAttribute : ActionFilterAttribute
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

            if (request.QueryString != null && request.QueryString["AffiliateId"] != null)
            {
                var affiliateId = Convert.ToInt32(request.QueryString["AffiliateId"]);

                if (affiliateId > 0)
                {
                    var affiliateService = EngineContext.Current.Resolve<IAffiliateService>();
                    var affiliate = affiliateService.GetAffiliateById(affiliateId);
                    if (affiliate != null && !affiliate.Deleted && affiliate.Active)
                    {
                        var workContext = EngineContext.Current.Resolve<IWorkContext>();
                        if (workContext.CurrentCustomer != null &&
                            workContext.CurrentCustomer.AffiliateId != affiliate.Id)
                        {
                            workContext.CurrentCustomer.AffiliateId = affiliate.Id;
                            var customerService = EngineContext.Current.Resolve<ICustomerService>();
                            customerService.UpdateCustomer(workContext.CurrentCustomer);
                        }
                    }
                }
            }
        }
    }
}
