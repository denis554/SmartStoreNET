﻿using System.Web.Mvc;
using System.Web.Routing;
using SmartStore.Web.Framework.Mvc.Routes;

namespace SmartStore.Plugin.Payments.PayPalDirect
{
    public partial class RouteProvider : IRouteProvider
    {
        public void RegisterRoutes(RouteCollection routes)
        {
            routes.MapRoute("Plugin.Payments.PayPalDirect.Configure",
                 "Plugins/PaymentPayPalDirect/Configure",
                 new { controller = "PaymentPayPalDirect", action = "Configure" },
                 new[] { "SmartStore.Plugin.Payments.PayPalDirect.Controllers" }
            );

            routes.MapRoute("Plugin.Payments.PayPalDirect.PaymentInfo",
                 "Plugins/PaymentPayPalDirect/PaymentInfo",
                 new { controller = "PaymentPayPalDirect", action = "PaymentInfo" },
                 new[] { "SmartStore.Plugin.Payments.PayPalDirect.Controllers" }
            );
            
            //IPN
            routes.MapRoute("Plugin.Payments.PayPalDirect.IPNHandler",
                 "Plugins/PaymentPayPalDirect/IPNHandler",
                 new { controller = "PaymentPayPalDirect", action = "IPNHandler" },
                 new[] { "SmartStore.Plugin.Payments.PayPalDirect.Controllers" }
            );
        }
        public int Priority
        {
            get
            {
                return 0;
            }
        }
    }
}
