﻿using FluentValidation;
using FluentValidation.Attributes;
using SmartStore.Web.Framework;
using SmartStore.Web.Framework.Localization;
using SmartStore.Web.Framework.Modelling;
using System.Collections.Generic;
using System.Web.Mvc;

namespace SmartStore.Admin.Models.Shipping
{
    [Validator(typeof(ShippingMethodValidator))]
    public class ShippingMethodModel : TabbableModel, ILocalizedModel<ShippingMethodLocalizedModel>, IStoreSelector
	{
        public ShippingMethodModel()
        {
            Locales = new List<ShippingMethodLocalizedModel>();
			FilterConfigurationUrls = new List<string>();
        }

		public IList<string> FilterConfigurationUrls { get; set; }

        [SmartResourceDisplayName("Admin.Configuration.Shipping.Methods.Fields.Name")]
        [AllowHtml]
        public string Name { get; set; }

        [SmartResourceDisplayName("Admin.Configuration.Shipping.Methods.Fields.Description")]
        [AllowHtml]
        public string Description { get; set; }

        [SmartResourceDisplayName("Common.DisplayOrder")]
        public int DisplayOrder { get; set; }

		[SmartResourceDisplayName("Admin.Configuration.Shipping.Methods.Fields.IgnoreCharges")]
		public bool IgnoreCharges { get; set; }

        public IList<ShippingMethodLocalizedModel> Locales { get; set; }

		// Store mapping
		[SmartResourceDisplayName("Admin.Common.Store.LimitedTo")]
		public bool LimitedToStores { get; set; }
		public IEnumerable<SelectListItem> AvailableStores { get; set; }
		public int[] SelectedStoreIds { get; set; }
	}

	public class ShippingMethodLocalizedModel : ILocalizedModelLocal
    {
        public int LanguageId { get; set; }

        [SmartResourceDisplayName("Admin.Configuration.Shipping.Methods.Fields.Name")]
        [AllowHtml]
        public string Name { get; set; }

        [SmartResourceDisplayName("Admin.Configuration.Shipping.Methods.Fields.Description")]
        [AllowHtml]
        public string Description { get; set; }
    }

    public partial class ShippingMethodValidator : AbstractValidator<ShippingMethodModel>
    {
        public ShippingMethodValidator()
        {
            RuleFor(x => x.Name).NotEmpty();
        }
    }
}