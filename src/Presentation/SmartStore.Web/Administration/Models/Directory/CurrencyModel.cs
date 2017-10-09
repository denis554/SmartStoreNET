﻿using System;
using System.Collections.Generic;
using System.Web.Mvc;
using FluentValidation.Attributes;
using SmartStore.Admin.Models.Stores;
using SmartStore.Admin.Validators.Directory;
using SmartStore.Core.Domain.Directory;
using SmartStore.Web.Framework;
using SmartStore.Web.Framework.Localization;
using SmartStore.Web.Framework.Modelling;

namespace SmartStore.Admin.Models.Directory
{
    [Validator(typeof(CurrencyValidator))]
    public class CurrencyModel : EntityModelBase, ILocalizedModel<CurrencyLocalizedModel>
    {
        public CurrencyModel()
        {
            Locales = new List<CurrencyLocalizedModel>();
            RoundOrderTotalPaymentMethods = new Dictionary<string, string>();
            RoundDuringCalculationDecimals = 2;

            AvailableDomainEndings = new List<SelectListItem>
			{
				new SelectListItem { Text = ".com", Value = ".com" },
				new SelectListItem { Text = ".uk", Value = ".uk" },
				new SelectListItem { Text = ".de", Value = ".de" },
				new SelectListItem { Text = ".ch", Value = ".ch" }
			};
        }
        [SmartResourceDisplayName("Admin.Configuration.Currencies.Fields.Name")]
        [AllowHtml]
        public string Name { get; set; }

        [SmartResourceDisplayName("Admin.Configuration.Currencies.Fields.CurrencyCode")]
        [AllowHtml]
        public string CurrencyCode { get; set; }

        [SmartResourceDisplayName("Admin.Configuration.Currencies.Fields.DisplayLocale")]
        [AllowHtml]
        public string DisplayLocale { get; set; }

        [SmartResourceDisplayName("Admin.Configuration.Currencies.Fields.Rate")]
        public decimal Rate { get; set; }

        [SmartResourceDisplayName("Admin.Configuration.Currencies.Fields.CustomFormatting")]
        [AllowHtml]
        public string CustomFormatting { get; set; }

        [SmartResourceDisplayName("Admin.Configuration.Currencies.Fields.Published")]
        public bool Published { get; set; }

        [SmartResourceDisplayName("Admin.Configuration.Currencies.Fields.DisplayOrder")]
        public int DisplayOrder { get; set; }

        [SmartResourceDisplayName("Admin.Configuration.Currencies.Fields.CreatedOn")]
        public DateTime CreatedOn { get; set; }

		public bool IsPrimaryStoreCurrency { get; set; }
        public bool IsPrimaryExchangeRateCurrency { get; set; }

		[SmartResourceDisplayName("Admin.Configuration.Currencies.Fields.PrimaryStoreCurrencyStores")]
		public IList<SelectListItem> PrimaryStoreCurrencyStores { get; set; }

		[SmartResourceDisplayName("Admin.Configuration.Currencies.Fields.PrimaryExchangeRateCurrencyStores")]
		public IList<SelectListItem> PrimaryExchangeRateCurrencyStores { get; set; }

		[SmartResourceDisplayName("Admin.Configuration.Currencies.Fields.DomainEndings")]
		public string DomainEndings { get; set; }
		public IList<SelectListItem> AvailableDomainEndings { get; set; }

		public IList<CurrencyLocalizedModel> Locales { get; set; }

		[SmartResourceDisplayName("Admin.Common.Store.LimitedTo")]
		public bool LimitedToStores { get; set; }

		[SmartResourceDisplayName("Admin.Common.Store.AvailableFor")]
		public List<StoreModel> AvailableStores { get; set; }
		public int[] SelectedStoreIds { get; set; }

        #region Rounding

        [SmartResourceDisplayName("Admin.Configuration.Currencies.Fields.RoundDuringCalculation")]
        public bool RoundDuringCalculation { get; set; }

        [SmartResourceDisplayName("Admin.Configuration.Currencies.Fields.RoundDuringCalculationDecimals")]
        public int RoundDuringCalculationDecimals { get; set; }

        [SmartResourceDisplayName("Admin.Configuration.Currencies.Fields.RoundOrderTotal")]
        public bool RoundOrderTotal { get; set; }

        [SmartResourceDisplayName("Admin.Configuration.Currencies.Fields.RoundOrderTotalToValue")]
        public decimal RoundOrderTotalToValue { get; set; }

        [SmartResourceDisplayName("Admin.Configuration.Currencies.Fields.RoundOrderTotalRule")]
        public CurrencyRoundingRule RoundOrderTotalRule { get; set; }

        public Dictionary<string, string> RoundOrderTotalPaymentMethods { get; set; }

        #endregion Rounding
    }

    public class CurrencyLocalizedModel : ILocalizedModelLocal
    {
        public int LanguageId { get; set; }

        [SmartResourceDisplayName("Admin.Configuration.Currencies.Fields.Name")]
        [AllowHtml]
        public string Name { get; set; }
    }
}