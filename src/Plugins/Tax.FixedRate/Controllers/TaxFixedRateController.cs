﻿using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Web.Mvc;
using SmartStore.Plugin.Tax.FixedRate.Models;
using SmartStore.Services.Configuration;
using SmartStore.Services.Tax;
using SmartStore.Web.Framework;
using SmartStore.Web.Framework.Controllers;
using Telerik.Web.Mvc;

namespace SmartStore.Plugin.Tax.FixedRate.Controllers
{

    public class TaxFixedRateController : PluginControllerBase
    {
        private readonly ITaxCategoryService _taxCategoryService;
        private readonly ISettingService _settingService;

        public TaxFixedRateController(ITaxCategoryService taxCategoryService, ISettingService settingService)
        {
            this._taxCategoryService = taxCategoryService;
            this._settingService = settingService;
        }

        protected override void Initialize(System.Web.Routing.RequestContext requestContext)
        {
            //little hack here
            //always set culture to 'en-US' (Telerik Grid has a bug related to editing decimal values in other cultures). Like currently it's done for admin area in Global.asax.cs
            var culture = new CultureInfo("en-US");
            Thread.CurrentThread.CurrentCulture = culture;
            Thread.CurrentThread.CurrentUICulture = culture;

            base.Initialize(requestContext);
        }

        public ActionResult Configure()
        {
            var taxCategories = _taxCategoryService.GetAllTaxCategories();
            if (taxCategories.Count == 0)
                return Content("No tax categories can be loaded");

            var tmp = new List<FixedTaxRateModel>();
            foreach (var taxCategory in taxCategories)
                tmp.Add(new FixedTaxRateModel()
                {
                    TaxCategoryId = taxCategory.Id,
                    TaxCategoryName = taxCategory.Name,
                    Rate = GetTaxRate(taxCategory.Id)
                });

            var gridModel = new GridModel<FixedTaxRateModel>
            {
                Data = tmp,
                Total = tmp.Count
            };

            return View("SmartStore.Plugin.Tax.FixedRate.Views.TaxFixedRate.Configure", gridModel);
        }

        [HttpPost, GridAction(EnableCustomBinding = true)]
        public ActionResult Configure(GridCommand command)
        {
            var tmp = new List<FixedTaxRateModel>();
            foreach (var taxCategory in _taxCategoryService.GetAllTaxCategories())
                tmp.Add(new FixedTaxRateModel()
                {
                    TaxCategoryId = taxCategory.Id,
                    TaxCategoryName = taxCategory.Name,
                    Rate = GetTaxRate(taxCategory.Id)
                });

            var tmp2 = tmp.ForCommand(command);
            var gridModel = new GridModel<FixedTaxRateModel>
            {
                Data = tmp2,
                Total = tmp2.Count()
            };
            return new JsonResult
            {
                Data = gridModel
            };
        }

        [GridAction(EnableCustomBinding = true)]
        public ActionResult TaxRateUpdate(FixedTaxRateModel model, GridCommand command)
        {
            int taxCategoryId = model.TaxCategoryId;
            decimal rate = model.Rate;

            _settingService.SetSetting(string.Format("Tax.TaxProvider.FixedRate.TaxCategoryId{0}", taxCategoryId), rate);

            var tmp = new List<FixedTaxRateModel>();
            foreach (var taxCategory in _taxCategoryService.GetAllTaxCategories())
                tmp.Add(new FixedTaxRateModel()
                {
                    TaxCategoryId = taxCategory.Id,
                    TaxCategoryName = taxCategory.Name,
                    Rate = GetTaxRate(taxCategory.Id)
                });

            var tmp2 = tmp.ForCommand(command);
            var gridModel = new GridModel<FixedTaxRateModel>
            {
                Data = tmp2,
                Total = tmp2.Count()
            };
            return new JsonResult
            {
                Data = gridModel
            };
        }

        [NonAction]
        protected decimal GetTaxRate(int taxCategoryId)
        {
            decimal rate = this._settingService.GetSettingByKey<decimal>(string.Format("Tax.TaxProvider.FixedRate.TaxCategoryId{0}", taxCategoryId));
            return rate;
        }
    }
}
