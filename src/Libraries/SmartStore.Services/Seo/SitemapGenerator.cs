using System;
using System.Linq;
using System.Web.Mvc;
using SmartStore.Core;
using SmartStore.Core.Domain.Catalog;
using SmartStore.Core.Domain.Common;
using SmartStore.Services.Catalog;
using SmartStore.Services.Topics;
using SmartStore.Services.Seo;

namespace SmartStore.Services.Seo
{
    /// <summary>
    /// Represents a sitemap generator
    /// </summary>
    public partial class SitemapGenerator : BaseSitemapGenerator
    {
		private readonly IStoreContext _storeContext;
        private readonly ICategoryService _categoryService;
        private readonly IProductService _productService;
        private readonly IManufacturerService _manufacturerService;
        private readonly ITopicService _topicService;
        private readonly CommonSettings _commonSettings;
        private readonly IWebHelper _webHelper;

		public SitemapGenerator(
			IStoreContext storeContext, 
			ICategoryService categoryService,
            IProductService productService, 
			IManufacturerService manufacturerService,
            ITopicService topicService, 
			CommonSettings commonSettings, 
			IWebHelper webHelper)
        {
			this._storeContext = storeContext;
            this._categoryService = categoryService;
            this._productService = productService;
            this._manufacturerService = manufacturerService;
            this._topicService = topicService;
            this._commonSettings = commonSettings;
            this._webHelper = webHelper;
        }

		protected override void GenerateUrlNodes(UrlHelper urlHelper)
        {
            if (_commonSettings.SitemapIncludeCategories)
            {
				WriteCategories(urlHelper, 0);
            }

            if (_commonSettings.SitemapIncludeManufacturers)
            {
                WriteManufacturers(urlHelper);
            }

            if (_commonSettings.SitemapIncludeProducts)
            {
                WriteProducts(urlHelper);
            }

            if (_commonSettings.SitemapIncludeTopics)
            {
                WriteTopics(urlHelper);
            }
        }

        private void WriteCategories(UrlHelper urlHelper, int parentCategoryId)
        {
            var categories = _categoryService.GetAllCategories(showHidden: false);
            foreach (var category in categories)
            {
				var url = urlHelper.RouteUrl("Category", new { SeName = category.GetSeName() }, "http");
                var updateFrequency = UpdateFrequency.Weekly;
                var updateTime = category.UpdatedOnUtc;
                WriteUrlLocation(url, updateFrequency, updateTime);
            }
        }

		private void WriteManufacturers(UrlHelper urlHelper)
        {
            var manufacturers = _manufacturerService.GetAllManufacturers(false);
            foreach (var manufacturer in manufacturers)
            {
				var url = urlHelper.RouteUrl("Manufacturer", new { SeName = manufacturer.GetSeName() }, "http");
                var updateFrequency = UpdateFrequency.Weekly;
                var updateTime = manufacturer.UpdatedOnUtc;
                WriteUrlLocation(url, updateFrequency, updateTime);
            }
        }

		private void WriteProducts(UrlHelper urlHelper)
        {
            var ctx = new ProductSearchContext()
			{
				OrderBy = ProductSortingEnum.CreatedOn,
				PageSize = int.MaxValue,
				StoreId = _storeContext.CurrentStoreIdIfMultiStoreMode,
				VisibleIndividuallyOnly = true
			};

			var products = _productService.SearchProducts(ctx);
            foreach (var product in products)
            {
				var url = urlHelper.RouteUrl("Product", new { SeName = product.GetSeName() }, "http");
                var updateFrequency = UpdateFrequency.Weekly;
                var updateTime = product.UpdatedOnUtc;
                WriteUrlLocation(url, updateFrequency, updateTime);
            }
        }

		private void WriteTopics(UrlHelper urlHelper)
        {
            var topics = _topicService.GetAllTopics(_storeContext.CurrentStore.Id).ToList().FindAll(t => t.IncludeInSitemap);
            foreach (var topic in topics)
            {
				var url = urlHelper.RouteUrl("Topic", new { SystemName = topic.SystemName }, "http");
                var updateFrequency = UpdateFrequency.Weekly;
                var updateTime = DateTime.UtcNow;
                WriteUrlLocation(url, updateFrequency, updateTime);
            }
        }
    }
}
