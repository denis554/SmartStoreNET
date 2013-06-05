﻿using System;
using System.Linq;
using System.Web.Mvc;
using SmartStore.Core.Domain.Discounts;
using SmartStore.Plugin.DiscountRules.Store.Models;
using SmartStore.Services.Configuration;
using SmartStore.Services.Discounts;
using SmartStore.Services.Localization;
using SmartStore.Services.Stores;
using SmartStore.Web.Framework.Controllers;

namespace SmartStore.Plugin.DiscountRules.Store.Controllers
{
    public class DiscountRulesStoreController : PluginControllerBase
    {
		private readonly ILocalizationService _localizationService;
		private readonly IDiscountService _discountService;
		private readonly IStoreService _storeService;
		private readonly ISettingService _settingService;

       public DiscountRulesStoreController(ILocalizationService localizationService,
            IDiscountService discountService, IStoreService storeService,
            ISettingService settingService)
        {
            this._localizationService = localizationService;
            this._discountService = discountService;
            this._storeService = storeService;
            this._settingService = settingService;
        }

        public ActionResult Configure(int discountId, int? discountRequirementId)
        {
            var discount = _discountService.GetDiscountById(discountId);
            if (discount == null)
                throw new ArgumentException("Discount could not be loaded");

            DiscountRequirement discountRequirement = null;
            if (discountRequirementId.HasValue)
            {
                discountRequirement = discount.DiscountRequirements.Where(dr => dr.Id == discountRequirementId.Value).FirstOrDefault();
                if (discountRequirement == null)
                    return Content("Failed to load requirement.");
            }

			var storeId = _settingService.GetSettingByKey<int>(string.Format("DiscountRequirement.Store-{0}", discountRequirementId.HasValue ? discountRequirementId.Value : 0));


			var model = new RequirementModel();
			model.RequirementId = discountRequirementId.HasValue ? discountRequirementId.Value : 0;
			model.DiscountId = discountId;
			model.StoreId = storeId;
			//stores
			model.AvailableStores.Add(new SelectListItem() { Text = _localizationService.GetResource("Plugins.DiscountRules.Store.Fields.SelectStore"), Value = "0" });
			foreach (var s in _storeService.GetAllStores())
				model.AvailableStores.Add(new SelectListItem() { Text = s.Name, Value = s.Id.ToString(), Selected = discountRequirement != null && s.Id == storeId });

			//add a prefix
			ViewData.TemplateInfo.HtmlFieldPrefix = string.Format("DiscountRulesStore{0}", discountRequirementId.HasValue ? discountRequirementId.Value.ToString() : "0");

			return View("SmartStore.Plugin.DiscountRules.Store.Views.DiscountRulesStore.Configure", model);
        }

        [HttpPost]
		public ActionResult Configure(int discountId, int? discountRequirementId, int storeId)
        {
			var discount = _discountService.GetDiscountById(discountId);
			if (discount == null)
				throw new ArgumentException("Discount could not be loaded");

			DiscountRequirement discountRequirement = null;
			if (discountRequirementId.HasValue)
				discountRequirement = discount.DiscountRequirements.Where(dr => dr.Id == discountRequirementId.Value).FirstOrDefault();

			if (discountRequirement != null)
			{
				//update existing rule
				_settingService.SetSetting(string.Format("DiscountRequirement.Store-{0}", discountRequirement.Id), storeId);
			}
			else
			{
				//save new rule
				discountRequirement = new DiscountRequirement()
				{
					DiscountRequirementRuleSystemName = "DiscountRequirement.Store"
				};
				discount.DiscountRequirements.Add(discountRequirement);
				_discountService.UpdateDiscount(discount);

				_settingService.SetSetting(string.Format("DiscountRequirement.Store-{0}", discountRequirement.Id), storeId);
			}
			return Json(new { Result = true, NewRequirementId = discountRequirement.Id }, JsonRequestBehavior.AllowGet);
		}
        
    }
}