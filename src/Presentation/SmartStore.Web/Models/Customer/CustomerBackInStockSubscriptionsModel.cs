﻿using System.Collections.Generic;
using SmartStore.Core;
using SmartStore.Web.Models.Common;

namespace SmartStore.Web.Models.Customer
{
    public partial class CustomerBackInStockSubscriptionsModel : PageableBase
    {
        public CustomerBackInStockSubscriptionsModel(IPageable pageable) : base(pageable)
        {
            this.Subscriptions = new List<BackInStockSubscriptionModel>();
        }

        public IList<BackInStockSubscriptionModel> Subscriptions { get; set; }
        public CustomerNavigationModel NavigationModel { get; set; }
    }
}