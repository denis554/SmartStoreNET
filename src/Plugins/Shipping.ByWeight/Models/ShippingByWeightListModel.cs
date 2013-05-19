﻿using System.Collections.Generic;
using System.Web.Mvc;
using SmartStore.Web.Framework;
using SmartStore.Web.Framework.Mvc;

namespace SmartStore.Plugin.Shipping.ByWeight.Models
{
    public class ShippingByWeightListModel : ModelBase
    {
        public ShippingByWeightListModel()
        {
            AvailableCountries = new List<SelectListItem>();
            AvailableShippingMethods = new List<SelectListItem>();
            Records = new List<ShippingByWeightModel>();
        }

        [SmartResourceDisplayName("Plugins.Shipping.ByWeight.Fields.Country")]
        public int AddCountryId { get; set; }
        [SmartResourceDisplayName("Plugins.Shipping.ByWeight.Fields.ShippingMethod")]
        public int AddShippingMethodId { get; set; }
        [SmartResourceDisplayName("Plugins.Shipping.ByWeight.Fields.From")]
        public decimal AddFrom { get; set; }
        [SmartResourceDisplayName("Plugins.Shipping.ByWeight.Fields.To")]
        public decimal AddTo { get; set; }
        [SmartResourceDisplayName("Plugins.Shipping.ByWeight.Fields.UsePercentage")]
        public bool AddUsePercentage { get; set; }
        [SmartResourceDisplayName("Plugins.Shipping.ByWeight.Fields.ShippingChargePercentage")]
        public decimal AddShippingChargePercentage { get; set; }
        [SmartResourceDisplayName("Plugins.Shipping.ByWeight.Fields.ShippingChargeAmount")]
        public decimal AddShippingChargeAmount { get; set; }



        [SmartResourceDisplayName("Plugins.Shipping.ByWeight.Fields.LimitMethodsToCreated")]
        public bool LimitMethodsToCreated { get; set; }
        [SmartResourceDisplayName("Plugins.Shipping.ByWeight.Fields.CalculatePerWeightUnit")]
        public bool CalculatePerWeightUnit { get; set; }

        public string PrimaryStoreCurrencyCode { get; set; }
        public string BaseWeightIn { get; set; }


        public IList<SelectListItem> AvailableCountries { get; set; }
        public IList<SelectListItem> AvailableShippingMethods { get; set; }

        public IList<ShippingByWeightModel> Records { get; set; }
        
    }
}