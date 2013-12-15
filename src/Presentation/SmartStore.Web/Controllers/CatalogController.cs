﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.ServiceModel.Syndication;
using System.Web.Mvc;
using SmartStore.Core;
using SmartStore.Core.Caching;
using SmartStore.Core.Domain.Catalog;
using SmartStore.Core.Domain.Customers;
using SmartStore.Core.Domain.Localization;
using SmartStore.Core.Domain.Media;
using SmartStore.Core.Domain.Orders;
using SmartStore.Core.Domain.Directory; 
using SmartStore.Services.Catalog;
using SmartStore.Services.Common;
using SmartStore.Services.Customers;
using SmartStore.Services.Directory;
using SmartStore.Services.Helpers;
using SmartStore.Services.Localization;
using SmartStore.Services.Media;
using SmartStore.Services.Messages;
using SmartStore.Services.Orders;
using SmartStore.Services.Security;
using SmartStore.Services.Seo;
using SmartStore.Services.Tax;
using SmartStore.Web.Extensions;
using SmartStore.Web.Framework;
using SmartStore.Web.Framework.Controllers;
using SmartStore.Web.Framework.Security;
using SmartStore.Web.Framework.UI.Captcha;
using SmartStore.Web.Infrastructure.Cache;
using SmartStore.Web.Models.Catalog;
using SmartStore.Web.Models.Media;
using SmartStore.Services.Logging;
using SmartStore.Services.Stores;
using SmartStore.Collections;
using SmartStore.Core.Domain.Tax;
using SmartStore.Services.Configuration;
using SmartStore.Services.Filter;
using SmartStore.Core.Infrastructure;
using SmartStore.Core.Data;
using System.Data.Entity.Infrastructure;

namespace SmartStore.Web.Controllers
{
    public partial class CatalogController : SmartController
    {
        #region Fields

        private static object s_lock = new object();

        private readonly ICategoryService _categoryService;
        private readonly IManufacturerService _manufacturerService;
        private readonly IProductService _productService;
        private readonly IProductTemplateService _productTemplateService;
        private readonly ICategoryTemplateService _categoryTemplateService;
        private readonly IManufacturerTemplateService _manufacturerTemplateService;
        private readonly IProductAttributeService _productAttributeService;
        private readonly IProductAttributeParser _productAttributeParser;
        private readonly IWorkContext _workContext;
		private readonly IStoreContext _storeContext;
        private readonly ITaxService _taxService;
        private readonly ICurrencyService _currencyService;
        private readonly IPictureService _pictureService;
        private readonly ILocalizationService _localizationService;
        private readonly IPriceCalculationService _priceCalculationService;
        private readonly IPriceFormatter _priceFormatter;
        private readonly IWebHelper _webHelper;
        private readonly ISpecificationAttributeService _specificationAttributeService;
        private readonly ICustomerContentService _customerContentService;
        private readonly IDateTimeHelper _dateTimeHelper;
        private readonly IShoppingCartService _shoppingCartService;
        private readonly IRecentlyViewedProductsService _recentlyViewedProductsService;
        private readonly ICompareProductsService _compareProductsService;
        private readonly IWorkflowMessageService _workflowMessageService;
        private readonly IProductTagService _productTagService;
        private readonly IOrderReportService _orderReportService;
        private readonly IGenericAttributeService _genericAttributeService;
        private readonly IBackInStockSubscriptionService _backInStockSubscriptionService;
        private readonly IAclService _aclService;
		private readonly IStoreMappingService _storeMappingService;
        private readonly IPermissionService _permissionService;
        private readonly IDownloadService _downloadService;
        private readonly ICustomerActivityService _customerActivityService;

        private readonly MediaSettings _mediaSettings;
        private readonly CatalogSettings _catalogSettings;
        private readonly ShoppingCartSettings _shoppingCartSettings;
        private readonly LocalizationSettings _localizationSettings;
        private readonly CustomerSettings _customerSettings;
        private readonly ICacheManager _cacheManager;
        private readonly CaptchaSettings _captchaSettings;

        //codehint: sm-edit begin
        private readonly TaxSettings _taxSettings;
        private readonly IMeasureService _measureService;
        private readonly MeasureSettings _measureSettings;
        private readonly IFilterService _filterService;
        private readonly IDeliveryTimeService _deliveryTimeService;
        private readonly IDbContext _dbContext;
        private readonly ISettingService _settingService;
        //codehint: sm-edit end

        #endregion

        #region Constructors

        public CatalogController(ICategoryService categoryService,
            IManufacturerService manufacturerService, IProductService productService,
            IProductTemplateService productTemplateService,
            ICategoryTemplateService categoryTemplateService,
            IManufacturerTemplateService manufacturerTemplateService,
            IProductAttributeService productAttributeService, IProductAttributeParser productAttributeParser,
            IWorkContext workContext, IStoreContext storeContext,
			ITaxService taxService, ICurrencyService currencyService,
            IPictureService pictureService, ILocalizationService localizationService,
            IPriceCalculationService priceCalculationService, IPriceFormatter priceFormatter,
            IWebHelper webHelper, ISpecificationAttributeService specificationAttributeService,
            ICustomerContentService customerContentService, IDateTimeHelper dateTimeHelper,
            IShoppingCartService shoppingCartService,
            IRecentlyViewedProductsService recentlyViewedProductsService, ICompareProductsService compareProductsService,
            IWorkflowMessageService workflowMessageService, IProductTagService productTagService,
            IOrderReportService orderReportService, IGenericAttributeService genericAttributeService,
            IBackInStockSubscriptionService backInStockSubscriptionService, IAclService aclService,
			IStoreMappingService storeMappingService,
            IPermissionService permissionService, IDownloadService downloadService,
            ICustomerActivityService customerActivityService,
            MediaSettings mediaSettings, CatalogSettings catalogSettings,
            ShoppingCartSettings shoppingCartSettings,
            LocalizationSettings localizationSettings, CustomerSettings customerSettings,
            CaptchaSettings captchaSettings,
            ICacheManager cacheManager,
            IMeasureService measureService, MeasureSettings measureSettings, TaxSettings taxSettings, IFilterService filterService,     /* codehint: sm-add */
            IDeliveryTimeService deliveryTimeService, IDbContext dbContext, ISettingService settingService                              /* codehint: sm-add */
            )
        {
            this._categoryService = categoryService;
            this._manufacturerService = manufacturerService;
            this._productService = productService;
            this._productTemplateService = productTemplateService;
            this._categoryTemplateService = categoryTemplateService;
            this._manufacturerTemplateService = manufacturerTemplateService;
            this._productAttributeService = productAttributeService;
            this._productAttributeParser = productAttributeParser;
            this._workContext = workContext;
			this._storeContext = storeContext;
            this._taxService = taxService;
            this._currencyService = currencyService;
            this._pictureService = pictureService;
            this._localizationService = localizationService;
            this._priceCalculationService = priceCalculationService;
            this._priceFormatter = priceFormatter;
            this._webHelper = webHelper;
            this._specificationAttributeService = specificationAttributeService;
            this._customerContentService = customerContentService;
            this._dateTimeHelper = dateTimeHelper;
            this._shoppingCartService = shoppingCartService;
            this._recentlyViewedProductsService = recentlyViewedProductsService;
            this._compareProductsService = compareProductsService;
            this._workflowMessageService = workflowMessageService;
            this._productTagService = productTagService;
            this._orderReportService = orderReportService;
            this._genericAttributeService = genericAttributeService;
            this._backInStockSubscriptionService = backInStockSubscriptionService;
            this._aclService = aclService;
			this._storeMappingService = storeMappingService;
            this._permissionService = permissionService;
            this._downloadService = downloadService;
            this._customerActivityService = customerActivityService;

            //codehint: sm-edit begin
            this._measureService = measureService;
            this._measureSettings = measureSettings;
            this._taxSettings = taxSettings;
            this._filterService = filterService;
            this._deliveryTimeService = deliveryTimeService;
            this._dbContext = dbContext;
            this._settingService = settingService;
            //codehint: sm-edit end

            this._mediaSettings = mediaSettings;
            this._catalogSettings = catalogSettings;
            this._shoppingCartSettings = shoppingCartSettings;
            this._localizationSettings = localizationSettings;
            this._customerSettings = customerSettings;
            this._captchaSettings = captchaSettings;

            this._cacheManager = cacheManager;
        }

        #endregion

        #region Utilities

        [NonAction]
        protected List<int> GetChildCategoryIds(int parentCategoryId, bool showHidden = false)
        {
            var customerRolesIds = _workContext.CurrentCustomer.CustomerRoles
                .Where(cr => cr.Active).Select(cr => cr.Id).ToList();
			string cacheKey = string.Format(ModelCacheEventConsumer.CATEGORY_CHILD_IDENTIFIERS_MODEL_KEY, 
				parentCategoryId, showHidden, string.Join(",", customerRolesIds), _storeContext.CurrentStore.Id);
            return _cacheManager.Get(cacheKey, () =>
            {
                var categoriesIds = new List<int>();
                var categories = _categoryService.GetAllCategoriesByParentCategoryId(parentCategoryId, showHidden);
                foreach (var category in categories)
                {
                    categoriesIds.Add(category.Id);
                    categoriesIds.AddRange(GetChildCategoryIds(category.Id, showHidden));
                }
                return categoriesIds;
            });
        }

        private IList<int> GetCurrentCategoryPath(int currentCategoryId, int currentProductId)
        {
            string cacheKey = "sm.temp.category.path.{0}-{1}".FormatInvariant(currentCategoryId, currentProductId);

            if (TempData.ContainsKey(cacheKey))
            {
                return TempData[cacheKey] as IList<int>;
            }

            var path = GetCategoryBreadCrumb(currentCategoryId, currentProductId).Select(x => x.Id).ToList();
            TempData[cacheKey] = path;

            return path;
        }

        // codehint: sm-add
        [NonAction]
        protected IList<Category> GetCategoryBreadCrumb(int currentCategoryId, int currentProductId)
        {

            Category currentCategory = null;
            if (currentCategoryId > 0)
                currentCategory = _categoryService.GetCategoryById(currentCategoryId);

            if (currentCategory == null && currentProductId > 0)
            {
                var productCategories = _categoryService.GetProductCategoriesByProductId(currentProductId);
                if (productCategories.Count > 0)
                    currentCategory = productCategories[0].Category;
            }

            if (currentCategory != null)
            {
                return GetCategoryBreadCrumb(currentCategory);
            }

            return new List<Category>();
        }

        [NonAction]
        protected IList<Category> GetCategoryBreadCrumb(Category category)
        {
            if (category == null)
                throw new ArgumentNullException("category");

            var breadCrumb = new List<Category>();

            while (category != null && //category is not null
                !category.Deleted && //category is not deleted
                category.Published && //category is published
				_aclService.Authorize(category) && //ACL
				_storeMappingService.Authorize(category)) //Store mapping
            {
                breadCrumb.Add(category);
                category = _categoryService.GetCategoryById(category.ParentCategoryId);
            }
            breadCrumb.Reverse();
            return breadCrumb;
        }

        [NonAction]
        protected IEnumerable<ProductOverviewModel> PrepareProductOverviewModels(IEnumerable<Product> products,
            bool preparePriceModel = true, bool preparePictureModel = true,
            int? productThumbPictureSize = null, bool prepareSpecificationAttributes = false,
            bool forceRedirectionAfterAddingToCart = false,
            bool prepareColorAttributes = false)
        {
            if (products == null)
                throw new ArgumentNullException("products");

            var models = new List<ProductOverviewModel>();

            var permDisplayPrices = _permissionService.Authorize(StandardPermissionProvider.DisplayPrices);
            var permEnableCart = _permissionService.Authorize(StandardPermissionProvider.EnableShoppingCart);
            var permEnableWishlist = _permissionService.Authorize(StandardPermissionProvider.EnableWishlist);
            var workingCurrency = _workContext.WorkingCurrency;
            var primaryStoreCurrency = _currencyService.GetCurrencyById(EngineContext.Current.Resolve<CurrencySettings>().PrimaryStoreCurrencyId);
            var currentCustomer = _workContext.CurrentCustomer;
            var taxDisplayType = _workContext.GetTaxDisplayTypeFor(currentCustomer, _storeContext.CurrentStore.Id);
            bool includingTax = _workContext.TaxDisplayType == TaxDisplayType.IncludingTax;
            string taxInfo = (taxDisplayType == TaxDisplayType.IncludingTax)
                ? _localizationService.GetResource("Tax.InclVAT")
                : _localizationService.GetResource("Tax.ExclVAT");
            string shippingInfoLink = Url.RouteUrl("Topic", new { SystemName = "shippinginfo" });

            foreach (var product in products)
            {
                decimal? minimalPrice = null;
				string additionalShippingCosts = "";

				var productMinPrice = _priceCalculationService.GetProductWithMinimalPrice(product, _workContext.CurrentCustomer, true, int.MaxValue, out minimalPrice);

                string weight = (productMinPrice.Weight > 0) ? 
					"{0} {1}".FormatCurrent(productMinPrice.Weight.ToString("F2"), _measureService.GetMeasureWeightById(_measureSettings.BaseWeightId).Name) : "";

                var addShippingPrice = _currencyService.ConvertCurrency(productMinPrice.AdditionalShippingCharge, primaryStoreCurrency, workingCurrency);

                if (addShippingPrice > 0)
                {
                    additionalShippingCosts = _localizationService.GetResource("Common.AdditionalShippingSurcharge").FormatWith(_priceFormatter.FormatPrice(addShippingPrice, false, false));
                }

                var model = new ProductOverviewModel()
                {
                    Id = product.Id,
					ProductMinPriceId = productMinPrice.Id,
                    Name = product.GetLocalized(x => x.Name),
                    ShortDescription = product.GetLocalized(x => x.ShortDescription),
                    FullDescription = product.GetLocalized(x => x.FullDescription),
                    SeName = product.GetSeName(),

                    Manufacturers = PrepareManufacturersOverviewModel(_manufacturerService.GetProductManufacturersByProductId(product.Id)),
                    ShowSku = _catalogSettings.ShowProductSku,
                    ShowWeight = _catalogSettings.ShowWeight,
                    ShowDimensions = _catalogSettings.ShowDimensions,
                    Sku = productMinPrice.Sku,
                    Weight = weight,
                    Dimensions = _localizationService.GetResource("Products.DimensionsValue").FormatCurrent(
                        productMinPrice.Width.ToString("F2"),
                        productMinPrice.Height.ToString("F2"),
                        productMinPrice.Length.ToString("F2")
                    ),
                    DimensionMeasureUnit = _measureService.GetMeasureDimensionById(_measureSettings.BaseDimensionId).Name,
                    ThumbDimension = _mediaSettings.ProductThumbPictureSize,
                    ShowLegalInfo = _taxSettings.ShowLegalHintsInProductList,
                    LegalInfo = string.Format(_localizationService.GetResource("Tax.LegalInfoFooter"), taxInfo, shippingInfoLink),
                    TransportSurcharge = additionalShippingCosts,
                    RatingSum = product.ApprovedRatingSum,
                    TotalReviews = product.ApprovedTotalReviews,
                    ShowReviews = _catalogSettings.ShowProductReviewsInProductLists,
                    ShowDeliveryTimes = _catalogSettings.ShowDeliveryTimesInProductLists,
                    DeliveryTime = _deliveryTimeService.GetDeliveryTimeById(productMinPrice.DeliveryTimeId.GetValueOrDefault()),
                    IsShipEnabled = productMinPrice.IsShipEnabled,
                    DisplayDeliveryTimeAccordingToStock = productMinPrice.DisplayDeliveryTimeAccordingToStock(),
                    StockAvailablity = productMinPrice.FormatStockMessage(_localizationService),

                    DisplayBasePrice = _catalogSettings.ShowBasePriceInProductLists,
                    BasePriceInfo = productMinPrice.GetBasePriceInfo(_localizationService, _priceFormatter),
                    CompareEnabled = _catalogSettings.CompareProductsEnabled,
                };


                // IsNew
                if (_catalogSettings.LabelAsNewForMaxDays.HasValue)
                {
                    model.IsNew = (DateTime.UtcNow - product.CreatedOnUtc).Days <= _catalogSettings.LabelAsNewForMaxDays.Value;
                }

                //price
                if (preparePriceModel)
                {
                    #region Prepare product price

                    var priceModel = new ProductOverviewModel.ProductPriceModel();

					var childProducts = new List<Product>();
					childProducts.Add(product);

					if (childProducts.Count == 0)
					{
						priceModel.OldPrice = null;
						priceModel.Price = null;
					}
					else
					{
						if (permDisplayPrices)
						{
							if (!productMinPrice.CustomerEntersPrice)
							{
								if (productMinPrice.CallForPrice)
								{
									priceModel.OldPrice = null;
									priceModel.Price = _localizationService.GetResource("Products.CallForPrice");
								}
								else if (minimalPrice.HasValue)
								{
									//calculate prices
									decimal taxRate = decimal.Zero;
									decimal oldPriceBase = _taxService.GetProductPrice(productMinPrice, productMinPrice.OldPrice, includingTax, currentCustomer, out taxRate);
									decimal finalPriceBase = _taxService.GetProductPrice(productMinPrice, minimalPrice.Value, includingTax, currentCustomer, out taxRate);

									decimal oldPrice = _currencyService.ConvertCurrency(oldPriceBase, primaryStoreCurrency, workingCurrency);
									decimal finalPrice = _currencyService.ConvertCurrency(finalPriceBase, primaryStoreCurrency, workingCurrency);

									//do we have tier prices configured?
									var tierPrices = new List<TierPrice>();
									if (productMinPrice.HasTierPrices)
									{
										tierPrices.AddRange(productMinPrice.TierPrices
											.OrderBy(tp => tp.Quantity)
											.FilterByStore(_storeContext.CurrentStore.Id)
											.FilterForCustomer(currentCustomer)
											.ToList()
											.RemoveDuplicatedQuantities());
									}
									priceModel.ShowDiscountSign = _catalogSettings.ShowDiscountSign;
									priceModel.HasDiscount = finalPriceBase != oldPriceBase && oldPriceBase != decimal.Zero;
									bool displayFromMessage =
										//When there is just one tier (with  qty 1), there are no actual savings in the list.
										(tierPrices.Count > 0 && !(tierPrices.Count == 1 && tierPrices[0].Quantity <= 1)) ||
										//we have more than one variant
										(childProducts.Count > 1);
									if (displayFromMessage)
									{
										priceModel.OldPrice = null;
										priceModel.Price = String.Format(_localizationService.GetResource("Products.PriceRangeFrom"), _priceFormatter.FormatPrice(finalPrice));
									}
									else
									{
										if (priceModel.HasDiscount)
										{
											priceModel.OldPrice = _priceFormatter.FormatPrice(oldPrice);
											priceModel.Price = _priceFormatter.FormatPrice(finalPrice);
										}
										else
										{
											priceModel.OldPrice = null;
											priceModel.Price = _priceFormatter.FormatPrice(finalPrice);
										}
									}
								}
								else
								{
									//Actually it's not possible (we presume that minimalPrice always has a value)
									//We never should get here
									Debug.WriteLine(string.Format("Cannot calculate minPrice for product variant #{0}", productMinPrice.Id));
								}
							}
						}
						else
						{
							//hide prices
							priceModel.OldPrice = null;
							priceModel.Price = null;
						}
					}

                    //'add to cart' button
					if (childProducts.Count == 0)
					{
						priceModel.DisableBuyButton = true;
						priceModel.AvailableForPreOrder = false;
						priceModel.DisableWishListButton = true;
					}
					else if (childProducts.Count == 1)
					{
						priceModel.DisableBuyButton = productMinPrice.DisableBuyButton || !permEnableCart;

						if (!permDisplayPrices)
						{
							priceModel.DisableBuyButton = true;
						}

						priceModel.DisableWishListButton = productMinPrice.DisableWishlistButton || !permEnableWishlist;
						priceModel.AvailableForPreOrder = productMinPrice.AvailableForPreOrder;
					}
					else
					{
						priceModel.DisableBuyButton = true;
						priceModel.DisableWishListButton = true;
						priceModel.AvailableForPreOrder = false;
					}

                    priceModel.ForceRedirectionAfterAddingToCart = forceRedirectionAfterAddingToCart;
                    model.ProductPrice = priceModel;

                    #endregion
                }

                //picture
                if (preparePictureModel)
                {
                    #region Prepare product picture

                    //If a size has been set in the view, we use it in priority
                    int pictureSize = productThumbPictureSize.HasValue ? productThumbPictureSize.Value : _mediaSettings.ProductThumbPictureSize;
                    //prepare picture model
					var defaultProductPictureCacheKey = string.Format(ModelCacheEventConsumer.PRODUCT_DEFAULTPICTURE_MODEL_KEY, product.Id, pictureSize, true, 
						_workContext.WorkingLanguage.Id, _webHelper.IsCurrentConnectionSecured(), _storeContext.CurrentStore.Id);
                    model.DefaultPictureModel = _cacheManager.Get(defaultProductPictureCacheKey, () =>
                    {
                        var picture = product.GetDefaultProductPicture(_pictureService);
                        var pictureModel = new PictureModel()
                        {
                            ImageUrl = _pictureService.GetPictureUrl(picture, pictureSize),
                            FullSizeImageUrl = _pictureService.GetPictureUrl(picture),
                            Title = string.Format(_localizationService.GetResource("Media.Product.ImageLinkTitleFormat"), model.Name),
                            AlternateText = string.Format(_localizationService.GetResource("Media.Product.ImageAlternateTextFormat"), model.Name)
                        };
                        return pictureModel;
                    });

                    #endregion
                }

                //specs
                if (prepareSpecificationAttributes)
                {
                    //specs for comparing
                    model.SpecificationAttributeModels = PrepareProductSpecificationModel(product);
                }

                // available colors
                if (prepareColorAttributes && _catalogSettings.ShowColorSquaresInLists)
                {   
                    // get the FIRST color type attribute
                    var colorAttr = _productAttributeService.GetProductVariantAttributesByProductId(productMinPrice.Id)
                        .FirstOrDefault(x => x.AttributeControlType == AttributeControlType.ColorSquares);

                    if (colorAttr != null)
                    {
                        var colorValues = 
							from a in colorAttr.ProductVariantAttributeValues.Take(50)
                            where (a.ColorSquaresRgb.HasValue() && !a.ColorSquaresRgb.IsCaseInsensitiveEqual("transparent"))
                            select new ProductOverviewModel.ColorAttributeModel()
							{ 
                                Color = a.ColorSquaresRgb,
                                Alias = a.Alias,
                                FriendlyName = a.GetLocalized(l => l.Name)
                            };

                        if (colorValues.Any())
                        {
                            model.ColorAttributes.AddRange(colorValues.Distinct());
                        }
                    }
                }

                models.Add(model);
            }
            return models;
        }

        [NonAction]
        protected IList<ProductSpecificationModel> PrepareProductSpecificationModel(Product product)
        {
            if (product == null)
                throw new ArgumentNullException("product");

            string cacheKey = string.Format(ModelCacheEventConsumer.PRODUCT_SPECS_MODEL_KEY, product.Id, _workContext.WorkingLanguage.Id);
            return _cacheManager.Get(cacheKey, () =>
            {
                var model = _specificationAttributeService.GetProductSpecificationAttributesByProductId(product.Id, null, true)
                   .Select(psa =>
                   {
                       return new ProductSpecificationModel()
                       {
                           SpecificationAttributeId = psa.SpecificationAttributeOption.SpecificationAttributeId,
                           SpecificationAttributeName = psa.SpecificationAttributeOption.SpecificationAttribute.GetLocalized(x => x.Name),
                           SpecificationAttributeOption = psa.SpecificationAttributeOption.GetLocalized(x => x.Name)
                       };
                   }).ToList();
                return model;
            });
        }

        // codehint: sm-add (mc)
        private CategoryNavigationModel GetCategoryNavigationModel(int currentCategoryId, int currentProductId)
        {
            var customerRolesIds = _workContext.CurrentCustomer.CustomerRoles.Where(cr => cr.Active).Select(cr => cr.Id).ToList();
			string cacheKey = string.Format(ModelCacheEventConsumer.CATEGORY_NAVIGATION_MODEL_KEY,
				_workContext.WorkingLanguage.Id, string.Join(",", customerRolesIds), _storeContext.CurrentStore.Id);

            var categories = _cacheManager.Get(cacheKey, () =>
            {
                return PrepareCategoryNavigationModel();
            });

            var breadcrumb = GetCurrentCategoryPath(currentCategoryId, currentProductId);

            // resolve number of products
            if (_catalogSettings.ShowCategoryProductNumber)
            {
                var curId = breadcrumb.LastOrDefault();
                var curNode = curId == 0 ? categories.Root : categories.SelectNode(x => x.Value.Id == curId);

                this.ResolveCategoryProductsCount(curNode);
            }

            var model = new CategoryNavigationModel
            {
                Root = categories,
                Path = breadcrumb,
                CurrentCategoryId = breadcrumb.LastOrDefault()
            };

            return model;
        }

        [NonAction]
        protected void ResolveCategoryProductsCount(TreeNode<CategoryNavigationModel.CategoryModel> curNode)
        {
            // Perf: only resolve counts for categories in the current path.
            while (true)
            {
                if (curNode.Children.Any(x => !x.Value.NumberOfProducts.HasValue))
                {
                    lock (s_lock)
                    {
                        if (curNode.Children.Any(x => !x.Value.NumberOfProducts.HasValue))
                        {
                            foreach (var node in curNode.Children)
                            {
                                var categoryIds = new List<int>();

                                if (_catalogSettings.ShowCategoryProductNumberIncludingSubcategories)
                                {
                                    // include subcategories
                                    node.TraverseTree(x => categoryIds.Add(x.Value.Id));
                                }
                                else
                                {
                                    categoryIds.Add(node.Value.Id);
                                }

                                var ctx = new ProductSearchContext();
                                ctx.CategoryIds = categoryIds;
								ctx.StoreId = _storeContext.CurrentStoreIdIfMultiStoreMode;
                                node.Value.NumberOfProducts = _productService.CountProducts(ctx);
                            }
                        }
                    }
                }

                curNode = curNode.Parent;
                if (curNode == null)
                    break;
            }
        }

        // codehint: sm-add (mc)
        [NonAction]
        protected TreeNode<CategoryNavigationModel.CategoryModel> PrepareCategoryNavigationModel()
        {

            var curParent = new TreeNode<CategoryNavigationModel.CategoryModel>(new CategoryNavigationModel.CategoryModel()
            {
                Id = 0,
                Name = "_ROOT_",
                Level = -1 // important
            });

            Category prevCat = null;
            int level = 0;

            var categories = _categoryService.GetAllCategories();
            foreach (var category in categories)
            {
                var model = new CategoryNavigationModel.CategoryModel()
                {
                    Id = category.Id,
                    Name = category.GetLocalized(x => x.Name),
                    SeName = category.GetSeName()
                };

                // determine parent
                if (prevCat != null)
                {
                    if (category.ParentCategoryId != curParent.Value.Id)
                    {
                        if (category.ParentCategoryId == prevCat.Id)
                        {
                            // level +1
                            curParent = curParent.LastChild;
                            level++;
                        }
                        else
                        {
                            // level -x
                            while (!curParent.IsRoot)
                            {
                                if (curParent.Value.Id == category.ParentCategoryId)
                                {
                                    break;
                                }
                                curParent = curParent.Parent;
                                level--;
                            }
                        }
                    }
                }

                // set level
                model.Level = level;

                // add to parent
                curParent.Append(model);

                prevCat = category;
            }

            var root = curParent.Root;

            return root;
        }

        // codehing: sm-add
        [NonAction]
        protected void PreparePagingFilteringModel(PagingFilteringModel model, PagingFilteringModel command, PageSizeContext pageSizeContext)
        {
            //sorting
            model.AllowProductSorting = _catalogSettings.AllowProductSorting;
            if (model.AllowProductSorting)
            {
                model.OrderBy = command.OrderBy;

                foreach (ProductSortingEnum enumValue in Enum.GetValues(typeof(ProductSortingEnum)))
                {
                    if (enumValue == ProductSortingEnum.CreatedOnAsc)
                    {
                        // TODO: (MC) das von uns eingeführte "CreatedOnAsc" schmeiß ich
                        // jetzt deshalb aus der UI raus, weil wir diese Sortier-Option
                        // auch ins StoredProc (ProductsLoadAllpaged) reinpacken müssten.
                        // Ist eigentlich kein Problem, ABER: Wir müssten immer wenn SmartStore
                        // Änderungen an dieser Proc vornimmt und wir diese Änderungen
                        // übernehmen müssen auch ständig an unseren Mod denken. Lass ma'!
                        continue;
                    }

                    var currentPageUrl = _webHelper.GetThisPageUrl(true);
                    var sortUrl = _webHelper.ModifyQueryString(currentPageUrl, "orderby=" + ((int)enumValue).ToString(), null);

                    var sortValue = enumValue.GetLocalizedEnum(_localizationService, _workContext);
                    model.AvailableSortOptions.Add(new ListOptionItem()
                    {
                        Text = sortValue,
                        Url = sortUrl,
                        Selected = enumValue == (ProductSortingEnum)command.OrderBy
                    });
                }
            }

            //view mode
            model.AllowProductViewModeChanging = _catalogSettings.AllowProductViewModeChanging;
            var viewMode = !string.IsNullOrEmpty(command.ViewMode)
                            ? command.ViewMode
                            : _catalogSettings.DefaultViewMode;

            model.ViewMode = viewMode; // codehint: sm-add

            if (model.AllowProductViewModeChanging)
            {
                var currentPageUrl = _webHelper.GetThisPageUrl(true);
                //grid
                model.AvailableViewModes.Add(new ListOptionItem()
                {
                    Text = _localizationService.GetResource("Categories.ViewMode.Grid"),
                    Url = _webHelper.ModifyQueryString(currentPageUrl, "viewmode=grid", null),
                    Selected = viewMode == "grid",
                    ExtraData = "grid"
                });
                //list
                model.AvailableViewModes.Add(new ListOptionItem()
                {
                    Text = _localizationService.GetResource("Categories.ViewMode.List"),
                    Url = _webHelper.ModifyQueryString(currentPageUrl, "viewmode=list", null),
                    Selected = viewMode == "list",
                    ExtraData = "list"
                });
            }

            //page size
            model.AllowCustomersToSelectPageSize = false;
            if (pageSizeContext.AllowCustomersToSelectPageSize && pageSizeContext.PageSizeOptions.IsEmpty())
            {
                pageSizeContext.PageSizeOptions = _catalogSettings.DefaultPageSizeOptions; // "12, 18, 36, 72, 150";

            }
            if (pageSizeContext.AllowCustomersToSelectPageSize && pageSizeContext.PageSizeOptions.HasValue())
            {
                var pageSizes = pageSizeContext.PageSizeOptions.Split(new char[] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries);

                if (pageSizes.Any())
                {
                    // get the first page size entry to use as the default (category page load) or if customer enters invalid value via query string
                    if (command.PageSize <= 0 || !pageSizes.Contains(command.PageSize.ToString()))
                    {
                        int temp = 0;

                        if (int.TryParse(pageSizes.FirstOrDefault(), out temp))
                        {
                            if (temp > 0)
                            {
                                command.PageSize = temp;
                            }
                        }
                    }

                    var currentPageUrl = _webHelper.GetThisPageUrl(true);
                    var sortUrl = _webHelper.ModifyQueryString(currentPageUrl, "pagesize={0}", null);
                    sortUrl = _webHelper.RemoveQueryString(sortUrl, "pagenumber");

                    foreach (var pageSize in pageSizes)
                    {
                        int temp = 0;
                        if (!int.TryParse(pageSize, out temp))
                        {
                            continue;
                        }
                        if (temp <= 0)
                        {
                            continue;
                        }

                        model.PageSizeOptions.Add(new ListOptionItem()
                        {
                            Text = pageSize,
                            Url = String.Format(sortUrl, pageSize),
                            Selected = pageSize.Equals(command.PageSize.ToString(), StringComparison.InvariantCultureIgnoreCase)
                        });
                    }

                    if (model.PageSizeOptions.Any())
                    {
                        model.PageSizeOptions = model.PageSizeOptions.OrderBy(x => int.Parse(x.Text)).ToList();
                        model.AllowCustomersToSelectPageSize = true;

                        if (command.PageSize <= 0)
                        {
                            command.PageSize = int.Parse(model.PageSizeOptions.FirstOrDefault().Text);
                        }
                    }
                }
            }
            else
            {
                //customer is not allowed to select a page size
                command.PageSize = pageSizeContext.PageSize;
            }

            if (command.PageSize <= 0)
                command.PageSize = pageSizeContext.PageSize;
        }

        /* codehint: sm-add */
        [NonAction]
        protected List<ManufacturerOverviewModel> PrepareManufacturersOverviewModel(ICollection<ProductManufacturer> manufacturers)
        {
            //var manufacturers = _manufacturerService.GetProductManufacturersByProductId(productId);

            var model = new List<ManufacturerOverviewModel>();

            foreach (var manufacturer in manufacturers)
            {
                var item = new ManufacturerOverviewModel
                {
                    Id = manufacturer.Manufacturer.Id,
                    Name = manufacturer.Manufacturer.Name,
                    Description = manufacturer.Manufacturer.Description,
                    SeName = manufacturer.Manufacturer.GetSeName()

                };

                var pic = _pictureService.GetPictureById(manufacturer.Manufacturer.PictureId);
                if (pic != null)
                {
                    item.PictureModel = new PictureModel()
                    {
                        PictureId = pic.Id, // codehint: sm-add
                        Title = string.Format(_localizationService.GetResource("Media.Product.ImageLinkTitleFormat"), manufacturer.Manufacturer.Name),
                        AlternateText = string.Format(_localizationService.GetResource("Media.Product.ImageAlternateTextFormat"), manufacturer.Manufacturer.Name),
                        ImageUrl = _pictureService.GetPictureUrl(manufacturer.Manufacturer.PictureId),
                    };
                }

                model.Add(item);
            }

            return model;
        }

        [NonAction]
        protected ProductDetailsModel PrepareProductDetailsPageModel(Product product, string attributes = null)
        {
            if (product == null)
                throw new ArgumentNullException("product");

            var model = new ProductDetailsModel()
            {
                Id = product.Id,
                Name = product.GetLocalized(x => x.Name),
                ShortDescription = product.GetLocalized(x => x.ShortDescription),
                FullDescription = product.GetLocalized(x => x.FullDescription),
                MetaKeywords = product.GetLocalized(x => x.MetaKeywords),
                MetaDescription = product.GetLocalized(x => x.MetaDescription),
                MetaTitle = product.GetLocalized(x => x.MetaTitle),
                SeName = product.GetSeName(),
                //Manufacturers = _manufacturerService.GetProductManufacturersByProductId(product.Id),  /* codehint: sm-edit */
                Manufacturers = PrepareManufacturersOverviewModel(_manufacturerService.GetProductManufacturersByProductId(product.Id)),
                ReviewCount = product.ApprovedTotalReviews,                     /* codehint: sm-add */
                DisplayAdminLink = _permissionService.Authorize(StandardPermissionProvider.AccessAdminPanel),
                EnableHtmlTextCollapser = Convert.ToBoolean(_settingService.GetSettingByKey<string>("CatalogSettings.EnableHtmlTextCollapser")),
                HtmlTextCollapsedHeight = Convert.ToString(_settingService.GetSettingByKey<string>("CatalogSettings.HtmlTextCollapsedHeight")),
				ShowSku = _catalogSettings.ShowProductSku,
				Sku = product.Sku,
				ShowManufacturerPartNumber = _catalogSettings.ShowManufacturerPartNumber,
				ManufacturerPartNumber = product.ManufacturerPartNumber,
				ShowGtin = _catalogSettings.ShowGtin,
				Gtin = product.Gtin,
				StockAvailability = product.FormatStockMessage(_localizationService),
				HasSampleDownload = product.IsDownload && product.HasSampleDownload,
				IsCurrentCustomerRegistered = _workContext.CurrentCustomer.IsRegistered()
            };

			// Back in stock subscriptions
			if (product.ManageInventoryMethod == ManageInventoryMethod.ManageStock &&
				 product.BackorderMode == BackorderMode.NoBackorders &&
				 product.AllowBackInStockSubscriptions &&
				 product.StockQuantity <= 0)
			{
				//out of stock
				model.DisplayBackInStockSubscription = true;
				model.BackInStockAlreadySubscribed = _backInStockSubscriptionService
					.FindSubscription(_workContext.CurrentCustomer.Id, product.Id, _storeContext.CurrentStore.Id) != null;
			}

            //template
            var templateCacheKey = string.Format(ModelCacheEventConsumer.PRODUCT_TEMPLATE_MODEL_KEY, product.ProductTemplateId);
            model.ProductTemplateViewPath = _cacheManager.Get(templateCacheKey, () =>
            {
                var template = _productTemplateService.GetProductTemplateById(product.ProductTemplateId);
                if (template == null)
                    template = _productTemplateService.GetAllProductTemplates().FirstOrDefault();
                return template.ViewPath;
            });

            // product variant price
            ProductVariantAttributeCombination combination = null;
            var combinationImageIds = new List<int>();
            var queryAttributes = _productAttributeParser.DeserializeQueryData(attributes);
            var selectedAttributes = new FormCollection();

			selectedAttributes.ConvertQueryData(queryAttributes, product.Id);

			model = PrepareProductDetailModel(model, product, selectedAttributes);

			model.Combinations.GetAllCombinationImageIds(combinationImageIds);

			if (combination == null && model.CombinationSelected != null)
				combination = model.CombinationSelected;


            // pictures
            var pictures = _pictureService.GetPicturesByProductId(product.Id);
            PrepareProductDetailsPictureModel(model.DetailsPictureModel, pictures, model.Name, combinationImageIds, combination);

            return model;
        }

        [NonAction]
        protected void PrepareProductReviewsModel(ProductReviewsModel model, Product product)
        {
            if (product == null)
                throw new ArgumentNullException("product");

            if (model == null)
                throw new ArgumentNullException("model");

            model.ProductId = product.Id;
            model.ProductName = product.GetLocalized(x => x.Name);
            model.ProductSeName = product.GetSeName();

            var productReviews = product.ProductReviews.Where(pr => pr.IsApproved).OrderBy(pr => pr.CreatedOnUtc);
            foreach (var pr in productReviews)
            {
                model.Items.Add(new ProductReviewModel()
                {
                    Id = pr.Id,
                    CustomerId = pr.CustomerId,
                    CustomerName = pr.Customer.FormatUserName(),
                    AllowViewingProfiles = _customerSettings.AllowViewingProfiles && pr.Customer != null && !pr.Customer.IsGuest(),
                    Title = pr.Title,
                    ReviewText = pr.ReviewText,
                    Rating = pr.Rating,
                    Helpfulness = new ProductReviewHelpfulnessModel()
                    {
                        ProductReviewId = pr.Id,
                        HelpfulYesTotal = pr.HelpfulYesTotal,
                        HelpfulNoTotal = pr.HelpfulNoTotal,
                    },
                    WrittenOnStr = _dateTimeHelper.ConvertToUserTime(pr.CreatedOnUtc, DateTimeKind.Utc).ToString("g"),
                });
            }

            model.AddProductReview.CanCurrentCustomerLeaveReview = _catalogSettings.AllowAnonymousUsersToReviewProduct || !_workContext.CurrentCustomer.IsGuest();
            model.AddProductReview.DisplayCaptcha = _captchaSettings.Enabled && _captchaSettings.ShowOnProductReviewPage;
        }

        // codehing: sm-add
        private PictureModel CreatePictureModel(ProductDetailsPictureModel model, Picture picture)
        {
            var result = new PictureModel()
            {
                PictureId = picture.Id,	// codehint: sm-add
                ThumbImageUrl = _pictureService.GetPictureUrl(picture, _mediaSettings.ProductThumbPictureSizeOnProductDetailsPage),
                ImageUrl = _pictureService.GetPictureUrl(picture, _mediaSettings.ProductDetailsPictureSize),
                FullSizeImageUrl = _pictureService.GetPictureUrl(picture),
                Title = model.Name,	// codehint: sm-add
                AlternateText = model.AlternateText
            };

            return result;
        }

        // codehing: sm-add
        [NonAction]
        protected void PrepareProductDetailsPictureModel(ProductDetailsPictureModel model, IList<Picture> pictures, string name, List<int> allCombinationImageIds,
			ProductVariantAttributeCombination combination = null)
        {
            model.Name = name;
            model.DefaultPictureZoomEnabled = _mediaSettings.DefaultPictureZoomEnabled;
            model.PictureZoomType = _mediaSettings.PictureZoomType;
            model.AlternateText = string.Format(_localizationService.GetResource("Media.Product.ImageAlternateTextFormat"), model.Name);

            Picture defaultPicture = null;
            var combiAssignedImages = (combination == null ? null : combination.GetAssignedPictureIds());

            if (pictures.Count > 0)
            {
                if (pictures.Count <= _catalogSettings.DisplayAllImagesNumber)
                {
                    // show all images
                    foreach (var picture in pictures)
                    {
                        model.PictureModels.Add(CreatePictureModel(model, picture));

                        if (defaultPicture == null && combiAssignedImages != null && combiAssignedImages.Contains(picture.Id))
                        {
                            model.GalleryStartIndex = model.PictureModels.Count - 1;
                            defaultPicture = picture;
                        }
                    }
                }
                else
                {
                    // images not belonging to any combination...
                    foreach (var picture in pictures.Where(p => !allCombinationImageIds.Contains(p.Id)))
                    {
                        model.PictureModels.Add(CreatePictureModel(model, picture));
                    }

                    // plus images belonging to selected combination
                    if (combiAssignedImages != null)
                    {
                        foreach (var picture in pictures.Where(p => combiAssignedImages.Contains(p.Id)))
                        {
                            model.PictureModels.Add(CreatePictureModel(model, picture));

                            if (defaultPicture == null)
                            {
                                model.GalleryStartIndex = model.PictureModels.Count - 1;
                                defaultPicture = picture;
                            }
                        }
                    }
                }

                if (defaultPicture == null)
                {
                    model.GalleryStartIndex = 0;
                    defaultPicture = pictures.First();
                }
            }

            // default picture
            if (defaultPicture == null)
            {
                model.DefaultPictureModel = new PictureModel()
                {
                    ThumbImageUrl = _pictureService.GetDefaultPictureUrl(_mediaSettings.ProductThumbPictureSizeOnProductDetailsPage),
                    ImageUrl = _pictureService.GetDefaultPictureUrl(_mediaSettings.ProductDetailsPictureSize),
                    FullSizeImageUrl = _pictureService.GetDefaultPictureUrl(),
                    Title = string.Format(_localizationService.GetResource("Media.Product.ImageLinkTitleFormat"), model.Name),
                    AlternateText = model.AlternateText
                };
            }
            else
            {
                model.DefaultPictureModel = CreatePictureModel(model, defaultPicture);
            }
        }

        /// <param name="selectedAttributes">Attributes explicitly selected by user or by query string.</param>
        [NonAction]
		protected ProductDetailsModel PrepareProductDetailModel(ProductDetailsModel model, Product product, FormCollection selectedAttributes = null, int selectedQuantity = 1)
        {
            if (product == null)
                throw new ArgumentNullException("product");

            if (model == null)
                throw new ArgumentNullException("model");

            if (selectedAttributes == null)
                selectedAttributes = new FormCollection();

            decimal preSelectedPriceAdjustmentBase = decimal.Zero;
            decimal preSelectedWeightAdjustment = decimal.Zero;
            bool displayPrices = _permissionService.Authorize(StandardPermissionProvider.DisplayPrices);

            bool hasSelectedAttributes = (selectedAttributes.Count > 0);
            List<ProductVariantAttributeValue> selectedAttributeValues = null;
            var variantAttributes = _productAttributeService.GetProductVariantAttributesByProductId(product.Id);

            model.ProductPrice.DynamicPriceUpdate = _catalogSettings.EnableDynamicPriceUpdate;

            if (!model.ProductPrice.DynamicPriceUpdate)
                selectedQuantity = 1;

            #region Product attributes

            foreach (var attribute in variantAttributes)
            {
                var pvaModel = new ProductDetailsModel.ProductVariantAttributeModel()
                {
                    Id = attribute.Id,
                    ProductAttributeId = attribute.ProductAttributeId,
                    Alias = attribute.ProductAttribute.Alias,
                    Name = attribute.ProductAttribute.GetLocalized(x => x.Name),
                    Description = attribute.ProductAttribute.GetLocalized(x => x.Description),
                    TextPrompt = attribute.TextPrompt,
                    IsRequired = attribute.IsRequired,
                    AttributeControlType = attribute.AttributeControlType,
                    AllowedFileExtensions = _catalogSettings.FileUploadAllowedExtensions,
                };

                if (attribute.ShouldHaveValues())
                {
                    var pvaValues = _productAttributeService.GetProductVariantAttributeValues(attribute.Id);

                    foreach (var pvaValue in pvaValues)
                    {
                        var pvaValueModel = new ProductDetailsModel.ProductVariantAttributeValueModel()
                        {
                            Id = pvaValue.Id,
                            Name = pvaValue.GetLocalized(x => x.Name),
                            Alias = pvaValue.Alias,
                            ColorSquaresRgb = pvaValue.ColorSquaresRgb, //used with "Color squares" attribute type
                            IsPreSelected = pvaValue.IsPreSelected,
                        };

                        if (hasSelectedAttributes)
                            pvaValueModel.IsPreSelected = false;	// explicitly selected always discards pre-selected by merchant

                        pvaModel.Values.Add(pvaValueModel);

                        // display price if allowed
                        if (displayPrices)
                        {
                            decimal taxRate = decimal.Zero;
                            decimal priceAdjustmentBase = _taxService.GetProductPrice(product, pvaValue.PriceAdjustment, out taxRate);
                            decimal priceAdjustment = _currencyService.ConvertFromPrimaryStoreCurrency(priceAdjustmentBase, _workContext.WorkingCurrency);

                            if (priceAdjustmentBase > decimal.Zero)
                                pvaValueModel.PriceAdjustment = "+" + _priceFormatter.FormatPrice(priceAdjustment, false, false);
                            else if (priceAdjustmentBase < decimal.Zero)
                                pvaValueModel.PriceAdjustment = "-" + _priceFormatter.FormatPrice(-priceAdjustment, false, false);

                            if (pvaValueModel.IsPreSelected)
                            {
                                preSelectedPriceAdjustmentBase = decimal.Add(preSelectedPriceAdjustmentBase, priceAdjustmentBase);
                                preSelectedWeightAdjustment = decimal.Add(preSelectedWeightAdjustment, pvaValue.WeightAdjustment);
                            }

                            pvaValueModel.PriceAdjustmentValue = priceAdjustment;
                        }
                    }
                }

                // we need selected attributes to get initially displayed combination images
                if (!hasSelectedAttributes)
                {
                    var defaultValue = pvaModel.Values.FirstOrDefault(v => v.IsPreSelected);

                    if (defaultValue == null && pvaModel.Values.Count > 0 && attribute.IsRequired)
                        defaultValue = pvaModel.Values.First();

                    if (defaultValue != null)
                        selectedAttributes.AddProductAttribute(attribute.ProductAttributeId, attribute.Id, defaultValue.Id, product.Id);
                }

                model.ProductVariantAttributes.Add(pvaModel);
            }

            #endregion

            #region Attribute combinations

            model.Combinations = _productAttributeService.GetAllProductVariantAttributeCombinations(product.Id);

            if (selectedAttributes.Count > 0)
            {		// merge with combination data if there's a match
                var warnings = new List<string>();
                string attributeXml = selectedAttributes.CreateSelectedAttributesXml(product.Id, variantAttributes, _productAttributeParser,
                    _localizationService, _downloadService, _catalogSettings, this.Request, warnings);

                selectedAttributeValues = _productAttributeParser.ParseProductVariantAttributeValues(attributeXml).ToList();

                model.CombinationSelected = model.Combinations
                    .FirstOrDefault(x => _productAttributeParser.AreProductAttributesEqual(x.AttributesXml, attributeXml));

                model.IsUnavailable = (model.CombinationSelected != null && model.CombinationSelected.IsActive == false);

                ((IMergedProduct)product).MergeWithCombination(model.CombinationSelected);

                // mark explicitly selected as pre-selected
                foreach (var attribute in model.ProductVariantAttributes)
                {
                    foreach (var value in attribute.Values)
                    {
                        if (selectedAttributeValues.FirstOrDefault(v => v.Id == value.Id) != null)
                            value.IsPreSelected = true;
                    }
                }
            }

            #endregion

            #region Properties

            model.Id = product.Id;
            model.Name = product.GetLocalized(x => x.Name);
            model.ShowSku = _catalogSettings.ShowProductSku;
            model.Sku = product.Sku;
			model.ShortDescription = product.ShortDescription;
            model.FullDescription = product.GetLocalized(x => x.FullDescription);
			model.MetaKeywords = product.GetLocalized(x => x.MetaKeywords);
			model.MetaDescription = product.GetLocalized(x => x.MetaDescription);
			model.MetaTitle = product.GetLocalized(x => x.MetaTitle);
			model.SeName = product.GetSeName();
            model.ShowManufacturerPartNumber = _catalogSettings.ShowManufacturerPartNumber;
            model.ManufacturerPartNumber = product.ManufacturerPartNumber;
            model.ShowDimensions = _catalogSettings.ShowDimensions;
            model.ShowWeight = _catalogSettings.ShowWeight;
            model.ShowGtin = _catalogSettings.ShowGtin;
			model.Gtin = product.Gtin;
            if (model.IsUnavailable)
                model.StockAvailability = _localizationService.GetResource("Products.Availability.OutOfStock");
            else
                model.StockAvailability = product.FormatStockMessage(_localizationService);
            model.HasSampleDownload = product.IsDownload && product.HasSampleDownload;
            model.IsCurrentCustomerRegistered = _workContext.CurrentCustomer.IsRegistered();
            model.IsBasePriceEnabled = product.BasePrice.Enabled;
            model.BasePriceInfo = product.GetBasePriceInfo(_localizationService, _priceFormatter);
            model.ShowLegalInfo = _taxSettings.ShowLegalHintsInProductDetails;

			//model.PictureModel.PictureId = productVariant.PictureId;
			//model.PictureModel.FullSizeImageUrl = _pictureService.GetPictureUrl(productVariant.PictureId, 0, false);
			//model.PictureModel.ImageUrl = _pictureService.GetPictureUrl(productVariant.PictureId, _mediaSettings.ProductVariantPictureSize, false);
			//model.PictureModel.Title = string.Format(_localizationService.GetResource("Media.Product.ImageLinkTitleFormat"), model.Name);
			//model.PictureModel.AlternateText = string.Format(_localizationService.GetResource("Media.Product.ImageAlternateTextFormat"), model.Name);

            //_taxSettings.TaxDisplayType == TaxDisplayType.ExcludingTax;

            string taxInfo = (_workContext.GetTaxDisplayTypeFor(_workContext.CurrentCustomer, _storeContext.CurrentStore.Id) == TaxDisplayType.IncludingTax) 
                ? _localizationService.GetResource("Tax.InclVAT") 
                : _localizationService.GetResource("Tax.ExclVAT");

            string defaultTaxRate = "";
            var taxrate = Convert.ToString(_taxService.GetTaxRate(product, _workContext.CurrentCustomer));
            if (_taxSettings.DisplayTaxRates && !taxrate.Equals("0", StringComparison.InvariantCultureIgnoreCase))
            {
                defaultTaxRate = "({0}%)".FormatWith(taxrate);
            }

            var addShippingPrice = _currencyService.ConvertFromPrimaryStoreCurrency(product.AdditionalShippingCharge, _workContext.WorkingCurrency);
            string additionalShippingCosts = "";
            if (addShippingPrice > 0)
            {
                additionalShippingCosts = _localizationService.GetResource("Common.AdditionalShippingSurcharge").FormatWith(_priceFormatter.FormatPrice(addShippingPrice, false, false)) + ", ";
            }

            string shippingInfoLink = Url.RouteUrl("Topic", new { SystemName = "shippinginfo" });
            model.LegalInfo = string.Format(_localizationService.GetResource("Tax.LegalInfoProductDetail"), taxInfo, defaultTaxRate, additionalShippingCosts, shippingInfoLink);

            string dimension = _measureService.GetMeasureDimensionById(_measureSettings.BaseDimensionId).Name;

            model.WeightValue = product.Weight;
            if (selectedAttributeValues != null)
            {
                foreach (var attributeValue in selectedAttributeValues)
                {
                    model.WeightValue = decimal.Add(model.WeightValue, attributeValue.WeightAdjustment);
                }
            }
            else
            {
                model.WeightValue = decimal.Add(model.WeightValue, preSelectedWeightAdjustment);
            }

            model.Weight = (model.WeightValue > 0) ? "{0} {1}".FormatCurrent(model.WeightValue.ToString("F2"), _measureService.GetMeasureWeightById(_measureSettings.BaseWeightId).Name) : "";
            model.Height = (product.Height > 0) ? "{0} {1}".FormatCurrent(product.Height.ToString("F2"), dimension) : "";
            model.Length = (product.Length > 0) ? "{0} {1}".FormatCurrent(product.Length.ToString("F2"), dimension) : "";
            model.Width = (product.Width > 0) ? "{0} {1}".FormatCurrent(product.Width.ToString("F2"), dimension) : "";

            model.ThumbDimensions = _mediaSettings.ProductVariantPictureSize;
            model.DeliveryTime = _deliveryTimeService.GetDeliveryTimeById(product.DeliveryTimeId.GetValueOrDefault());
            model.DisplayDeliveryTime = _catalogSettings.ShowDeliveryTimesInProductDetail;
            model.IsShipEnabled = product.IsShipEnabled;
            model.DisplayDeliveryTimeAccordingToStock = product.DisplayDeliveryTimeAccordingToStock();

            //back in stock subscriptions)
            if (product.ManageInventoryMethod == ManageInventoryMethod.ManageStock &&
                product.BackorderMode == BackorderMode.NoBackorders &&
                product.AllowBackInStockSubscriptions &&
                product.StockQuantity <= 0)
            {
                //out of stock
                model.DisplayBackInStockSubscription = true;
				model.BackInStockAlreadySubscribed = _backInStockSubscriptionService
					 .FindSubscription(_workContext.CurrentCustomer.Id, product.Id, _storeContext.CurrentStore.Id) != null;
            }

            #endregion

            #region Product variant price
            model.ProductPrice.ProductId = product.Id;

            if (displayPrices)
            {
                model.ProductPrice.HidePrices = false;
                if (product.CustomerEntersPrice)
                {
                    model.ProductPrice.CustomerEntersPrice = true;
                }
                else
                {
                    if (product.CallForPrice)
                    {
                        model.ProductPrice.CallForPrice = true;
                    }
                    else
                    {
                        decimal taxRate = decimal.Zero;
                        decimal oldPriceBase = _taxService.GetProductPrice(product, product.OldPrice, out taxRate);

                        decimal finalPriceWithoutDiscountBase = decimal.Zero;
                        decimal finalPriceWithDiscountBase = decimal.Zero;
                        decimal attributesTotalPriceBase = decimal.Zero;

                        if (model.ProductPrice.DynamicPriceUpdate)
                        {
                            if (selectedAttributeValues != null)
                            {
                                foreach (var attributeValue in selectedAttributeValues)
                                {
                                    taxRate = decimal.Zero;
                                    decimal priceAdjustmentBase = _taxService.GetProductPrice(product, attributeValue.PriceAdjustment, out taxRate);

                                    attributesTotalPriceBase = decimal.Add(attributesTotalPriceBase, priceAdjustmentBase);
                                }
                            }
                            else
                            {
                                attributesTotalPriceBase = preSelectedPriceAdjustmentBase;
                            }
                        }

                        finalPriceWithoutDiscountBase = _priceCalculationService.GetFinalPrice(product, _workContext.CurrentCustomer, attributesTotalPriceBase, false, selectedQuantity);
                        finalPriceWithDiscountBase = _priceCalculationService.GetFinalPrice(product, _workContext.CurrentCustomer, attributesTotalPriceBase, true, selectedQuantity);

                        finalPriceWithoutDiscountBase = _taxService.GetProductPrice(product, finalPriceWithoutDiscountBase, out taxRate);
                        finalPriceWithDiscountBase = _taxService.GetProductPrice(product, finalPriceWithDiscountBase, out taxRate);
                        // codehint: sm-edit (end)

                        decimal oldPrice = _currencyService.ConvertFromPrimaryStoreCurrency(oldPriceBase, _workContext.WorkingCurrency);
                        decimal finalPriceWithoutDiscount = _currencyService.ConvertFromPrimaryStoreCurrency(finalPriceWithoutDiscountBase, _workContext.WorkingCurrency);
                        decimal finalPriceWithDiscount = _currencyService.ConvertFromPrimaryStoreCurrency(finalPriceWithDiscountBase, _workContext.WorkingCurrency);

                        if (finalPriceWithoutDiscountBase != oldPriceBase && oldPriceBase > decimal.Zero)
                            model.ProductPrice.OldPrice = _priceFormatter.FormatPrice(oldPrice);

                        model.ProductPrice.Price = _priceFormatter.FormatPrice(finalPriceWithoutDiscount);

                        if (finalPriceWithoutDiscountBase != finalPriceWithDiscountBase)
                            model.ProductPrice.PriceWithDiscount = _priceFormatter.FormatPrice(finalPriceWithDiscount);

                        model.ProductPrice.PriceValue = finalPriceWithoutDiscount;
                        model.ProductPrice.PriceWithDiscountValue = finalPriceWithDiscount;

						model.BasePriceInfo = product.GetBasePriceInfo(_localizationService, _priceFormatter, attributesTotalPriceBase);
                    }
                }
            }
            else
            {
                model.ProductPrice.HidePrices = true;
                model.ProductPrice.OldPrice = null;
                model.ProductPrice.Price = null;
            }
            #endregion

            #region 'Add to cart' model

            model.AddToCart.ProductId = product.Id;

            //quantity
            model.AddToCart.EnteredQuantity = product.OrderMinimumQuantity;

            //'add to cart', 'add to wishlist' buttons
            model.AddToCart.DisableBuyButton = product.DisableBuyButton || !_permissionService.Authorize(StandardPermissionProvider.EnableShoppingCart);
            model.AddToCart.DisableWishlistButton = product.DisableWishlistButton || !_permissionService.Authorize(StandardPermissionProvider.EnableWishlist);
            if (!displayPrices)
            {
                model.AddToCart.DisableBuyButton = true;
                model.AddToCart.DisableWishlistButton = true;
            }
            //pre-order
            model.AddToCart.AvailableForPreOrder = product.AvailableForPreOrder;

            //customer entered price
            model.AddToCart.CustomerEntersPrice = product.CustomerEntersPrice;
            if (model.AddToCart.CustomerEntersPrice)
            {
                decimal minimumCustomerEnteredPrice = _currencyService.ConvertFromPrimaryStoreCurrency(product.MinimumCustomerEnteredPrice, _workContext.WorkingCurrency);
                decimal maximumCustomerEnteredPrice = _currencyService.ConvertFromPrimaryStoreCurrency(product.MaximumCustomerEnteredPrice, _workContext.WorkingCurrency);

                model.AddToCart.CustomerEnteredPrice = minimumCustomerEnteredPrice;
                model.AddToCart.CustomerEnteredPriceRange = string.Format(_localizationService.GetResource("Products.EnterProductPrice.Range"),
                    _priceFormatter.FormatPrice(minimumCustomerEnteredPrice, false, false),
                    _priceFormatter.FormatPrice(maximumCustomerEnteredPrice, false, false));
            }
            //allowed quantities
            var allowedQuantities = product.ParseAllowedQuatities();
            foreach (var qty in allowedQuantities)
            {
                model.AddToCart.AllowedQuantities.Add(new SelectListItem()
                {
                    Text = qty.ToString(),
                    Value = qty.ToString()
                });
            }

            #endregion

            #region Gift card

            model.GiftCard.IsGiftCard = product.IsGiftCard;
            if (model.GiftCard.IsGiftCard)
            {
                model.GiftCard.GiftCardType = product.GiftCardType;
                model.GiftCard.SenderName = _workContext.CurrentCustomer.GetFullName();
                model.GiftCard.SenderEmail = _workContext.CurrentCustomer.Email;
            }

            #endregion

            // prevent saving merged data to database
            var objectContext = ((IObjectContextAdapter)_dbContext).ObjectContext;
            objectContext.Detach(product);

            return model;
        }

        #endregion

        #region Categories

        [RequireHttpsByConfigAttribute(SslRequirement.No)]
        public ActionResult Category(int categoryId, CatalogPagingFilteringModel command, string filter)
        {
            var category = _categoryService.GetCategoryById(categoryId);
            if (category == null || category.Deleted)
                return RedirectToRoute("HomePage");

            //Check whether the current user has a "Manage catalog" permission
            //It allows him to preview a category before publishing
            if (!category.Published && !_permissionService.Authorize(StandardPermissionProvider.ManageCatalog))
                return RedirectToRoute("HomePage");

            //ACL (access control list)
            if (!_aclService.Authorize(category))
                return RedirectToRoute("HomePage");

			//Store mapping
			if (!_storeMappingService.Authorize(category))
				return RedirectToRoute("HomePage");            

            //'Continue shopping' URL
			_genericAttributeService.SaveAttribute(_workContext.CurrentCustomer,
				SystemCustomerAttributeNames.LastContinueShoppingPage,
				_webHelper.GetThisPageUrl(false),
				_storeContext.CurrentStore.Id);

            if (command.PageNumber <= 0)
                command.PageNumber = 1; // codehint: sm-edit

            var model = category.ToModel();

            // codehing: sm-edit (replaced)
            PreparePagingFilteringModel(model.PagingFilteringContext, command, new PageSizeContext
            {
                AllowCustomersToSelectPageSize = category.AllowCustomersToSelectPageSize,
                PageSize = category.PageSize,
                PageSizeOptions = category.PageSizeOptions
            });

            //price ranges
            model.PagingFilteringContext.PriceRangeFilter.LoadPriceRangeFilters(category.PriceRanges, _webHelper, _priceFormatter);
            var selectedPriceRange = model.PagingFilteringContext.PriceRangeFilter.GetSelectedPriceRange(_webHelper, category.PriceRanges);
            decimal? minPriceConverted = null;
            decimal? maxPriceConverted = null;
            if (selectedPriceRange != null)
            {
                if (selectedPriceRange.From.HasValue)
                    minPriceConverted = _currencyService.ConvertToPrimaryStoreCurrency(selectedPriceRange.From.Value, _workContext.WorkingCurrency);

                if (selectedPriceRange.To.HasValue)
                    maxPriceConverted = _currencyService.ConvertToPrimaryStoreCurrency(selectedPriceRange.To.Value, _workContext.WorkingCurrency);
            }



            //category breadcrumb
            model.DisplayCategoryBreadcrumb = _catalogSettings.CategoryBreadcrumbEnabled;
            if (model.DisplayCategoryBreadcrumb)
            {
                foreach (var catBr in GetCategoryBreadCrumb(category))
                {
                    model.CategoryBreadcrumb.Add(new CategoryModel()
                    {
                        Id = catBr.Id,
                        Name = catBr.GetLocalized(x => x.Name),
                        SeName = catBr.GetSeName()
                    });
                }
            }




            //subcategories
            model.SubCategories = _categoryService
                .GetAllCategoriesByParentCategoryId(categoryId)
                .Select(x =>
                {
                    var subCatName = x.GetLocalized(y => y.Name);
                    var subCatModel = new CategoryModel.SubCategoryModel()
                    {
                        Id = x.Id,
                        Name = subCatName,
                        SeName = x.GetSeName(),
                    };

                    //prepare picture model
                    int pictureSize = _mediaSettings.CategoryThumbPictureSize;
					var categoryPictureCacheKey = string.Format(ModelCacheEventConsumer.CATEGORY_PICTURE_MODEL_KEY, x.Id, pictureSize, true, 
						_workContext.WorkingLanguage.Id, _webHelper.IsCurrentConnectionSecured(), _storeContext.CurrentStore.Id);
                    subCatModel.PictureModel = _cacheManager.Get(categoryPictureCacheKey, () =>
                    {
                        var pictureModel = new PictureModel()
                        {
                            PictureId = x.PictureId, // codehint: sm-add
                            FullSizeImageUrl = _pictureService.GetPictureUrl(x.PictureId),
                            ImageUrl = _pictureService.GetPictureUrl(x.PictureId, pictureSize),
                            Title = string.Format(_localizationService.GetResource("Media.Category.ImageLinkTitleFormat"), subCatName),
                            AlternateText = string.Format(_localizationService.GetResource("Media.Category.ImageAlternateTextFormat"), subCatName)
                        };
                        return pictureModel;
                    });

                    return subCatModel;
                })
                .ToList();




            //featured products
            //Question: should we use '_catalogSettings.ShowProductsFromSubcategories' setting for displaying featured products?
            if (!_catalogSettings.IgnoreFeaturedProducts && _categoryService.GetTotalNumberOfFeaturedProducts(categoryId) > 0)
            {
                var ctx = new ProductSearchContext();

                if (category.Id > 0)
                    ctx.CategoryIds.Add(category.Id);
                ctx.FeaturedProducts = true;
                ctx.LanguageId = _workContext.WorkingLanguage.Id;
                ctx.OrderBy = ProductSortingEnum.Position;
                ctx.PageSize = int.MaxValue;
				ctx.StoreId = _storeContext.CurrentStoreIdIfMultiStoreMode;

                var featuredProducts = _productService.SearchProducts(ctx);

                model.FeaturedProducts = PrepareProductOverviewModels(featuredProducts, prepareColorAttributes: true).ToList();
            }

            //products
            if (filter.HasValue())
            {	// codehint: sm-add (new filter)
                var context = new FilterProductContext
                {
                    ParentCategoryID = category.Id,
                    CategoryIds = new List<int> { category.Id },
                    Criteria = _filterService.Deserialize(filter),
                    OrderBy = command.OrderBy
                };

                if (_catalogSettings.ShowProductsFromSubcategories)
                    context.CategoryIds.AddRange(GetChildCategoryIds(category.Id));

                var filterQuery = _filterService.ProductFilter(context);
                var products = new PagedList<Product>(filterQuery, command.PageIndex, command.PageSize);

                model.Products = PrepareProductOverviewModels(products, prepareColorAttributes: true).ToList();
                model.PagingFilteringContext.LoadPagedList(products);
            }
            else
            {	// use old filter
                IList<int> alreadyFilteredSpecOptionIds = model.PagingFilteringContext.SpecificationFilter.GetAlreadyFilteredSpecOptionIds(_webHelper);

                var ctx2 = new ProductSearchContext();
                if (category.Id > 0)
                {
                    ctx2.CategoryIds.Add(category.Id);
                    if (_catalogSettings.ShowProductsFromSubcategories)
                    {
                        //include subcategories
                        ctx2.CategoryIds.AddRange(GetChildCategoryIds(category.Id));
                    }
                }
                ctx2.FeaturedProducts = _catalogSettings.IncludeFeaturedProductsInNormalLists ? null : (bool?)false;
                ctx2.PriceMin = minPriceConverted;
                ctx2.PriceMax = maxPriceConverted;
                ctx2.LanguageId = _workContext.WorkingLanguage.Id;
                ctx2.FilteredSpecs = alreadyFilteredSpecOptionIds;
                ctx2.OrderBy = (ProductSortingEnum)command.OrderBy; // ProductSortingEnum.Position;
                ctx2.PageIndex = command.PageNumber - 1;
                ctx2.PageSize = command.PageSize;
                ctx2.LoadFilterableSpecificationAttributeOptionIds = true;
				ctx2.StoreId = _storeContext.CurrentStoreIdIfMultiStoreMode;

                var products = _productService.SearchProducts(ctx2);

                model.Products = PrepareProductOverviewModels(products, prepareColorAttributes: true).ToList();

                model.PagingFilteringContext.LoadPagedList(products);
                //model.PagingFilteringContext.ViewMode = viewMode; // codehint: sm-delete

                //specs
                model.PagingFilteringContext.SpecificationFilter.PrepareSpecsFilters(alreadyFilteredSpecOptionIds,
                    ctx2.FilterableSpecificationAttributeOptionIds,
                    _specificationAttributeService, _webHelper, _workContext);
            }

            //template
            var templateCacheKey = string.Format(ModelCacheEventConsumer.CATEGORY_TEMPLATE_MODEL_KEY, category.CategoryTemplateId);
            var templateViewPath = _cacheManager.Get(templateCacheKey, () =>
            {
                var template = _categoryTemplateService.GetCategoryTemplateById(category.CategoryTemplateId);
                if (template == null)
                    template = _categoryTemplateService.GetAllCategoryTemplates().FirstOrDefault();
                return template.ViewPath;
            });

            //activity log
            _customerActivityService.InsertActivity("PublicStore.ViewCategory", _localizationService.GetResource("ActivityLog.PublicStore.ViewCategory"), category.Name);

            return View(templateViewPath, model);
        }

        [ChildActionOnly]
        public ActionResult CategoryNavigation(int currentCategoryId, int currentProductId)
        {
            var model = GetCategoryNavigationModel(currentCategoryId, currentProductId);
            return PartialView(model);
        }

        /// <![CDATA[ codehint: sm-add ]]>
        [ChildActionOnly]
        public ActionResult Megamenu(int currentCategoryId, int currentProductId)
        {
            var model = GetCategoryNavigationModel(currentCategoryId, currentProductId);
            return PartialView(model);
        }

        [ChildActionOnly]
        public ActionResult HomepageCategories()
        {
			var categories = _categoryService.GetAllCategoriesDisplayedOnHomePage()
				.Where(c => _aclService.Authorize(c) && _storeMappingService.Authorize(c))
				.ToList();

            var listModel = categories
                .Select(x =>
                {
                    var catModel = x.ToModel();

                    //prepare picture model
                    int pictureSize = _mediaSettings.CategoryThumbPictureSize;
					var categoryPictureCacheKey = string.Format(ModelCacheEventConsumer.CATEGORY_PICTURE_MODEL_KEY, x.Id, pictureSize, true, 
						_workContext.WorkingLanguage.Id, _webHelper.IsCurrentConnectionSecured(), _storeContext.CurrentStore.Id);
                    catModel.PictureModel = _cacheManager.Get(categoryPictureCacheKey, () =>
                    {
                        var pictureModel = new PictureModel()
                        {
                            PictureId = x.PictureId, // codehint: sm-add
                            FullSizeImageUrl = _pictureService.GetPictureUrl(x.PictureId),
                            ImageUrl = _pictureService.GetPictureUrl(x.PictureId, pictureSize),
                            Title = string.Format(_localizationService.GetResource("Media.Category.ImageLinkTitleFormat"), catModel.Name),
                            AlternateText = string.Format(_localizationService.GetResource("Media.Category.ImageAlternateTextFormat"), catModel.Name)
                        };
                        return pictureModel;
                    });

                    return catModel;
                })
                .ToList();

            return PartialView(listModel);
        }

        #endregion

        #region Manufacturers

        [RequireHttpsByConfigAttribute(SslRequirement.No)]
        public ActionResult Manufacturer(int manufacturerId, CatalogPagingFilteringModel command)
        {
            var manufacturer = _manufacturerService.GetManufacturerById(manufacturerId);
            if (manufacturer == null || manufacturer.Deleted)
                return RedirectToRoute("HomePage");

            //Check whether the current user has a "Manage catalog" permission
            //It allows him to preview a manufacturer before publishing
            if (!manufacturer.Published && !_permissionService.Authorize(StandardPermissionProvider.ManageCatalog))
                return RedirectToRoute("HomePage");

			//Store mapping
			if (!_storeMappingService.Authorize(manufacturer))
				return RedirectToRoute("HomePage");

            //'Continue shopping' URL
			_genericAttributeService.SaveAttribute(_workContext.CurrentCustomer,
				SystemCustomerAttributeNames.LastContinueShoppingPage,
				_webHelper.GetThisPageUrl(false),
				_storeContext.CurrentStore.Id);

            if (command.PageNumber <= 0)
                command.PageNumber = 1; // codehint: sm-edit

            var model = manufacturer.ToModel();

            // prepare picture model
            model.PictureModel = this.PrepareManufacturerPictureModel(manufacturer, model.Name);

            // codehing: sm-edit (replaced)
            PreparePagingFilteringModel(model.PagingFilteringContext, command, new PageSizeContext
            {
                AllowCustomersToSelectPageSize = manufacturer.AllowCustomersToSelectPageSize,
                PageSize = manufacturer.PageSize,
                PageSizeOptions = manufacturer.PageSizeOptions
            });

            //price ranges
            model.PagingFilteringContext.PriceRangeFilter.LoadPriceRangeFilters(manufacturer.PriceRanges, _webHelper, _priceFormatter);
            var selectedPriceRange = model.PagingFilteringContext.PriceRangeFilter.GetSelectedPriceRange(_webHelper, manufacturer.PriceRanges);
            decimal? minPriceConverted = null;
            decimal? maxPriceConverted = null;
            if (selectedPriceRange != null)
            {
                if (selectedPriceRange.From.HasValue)
                    minPriceConverted = _currencyService.ConvertToPrimaryStoreCurrency(selectedPriceRange.From.Value, _workContext.WorkingCurrency);

                if (selectedPriceRange.To.HasValue)
                    maxPriceConverted = _currencyService.ConvertToPrimaryStoreCurrency(selectedPriceRange.To.Value, _workContext.WorkingCurrency);
            }

            //featured products
            if (!_catalogSettings.IgnoreFeaturedProducts && _manufacturerService.GetTotalNumberOfFeaturedProducts(manufacturerId) > 0)
            {
                var ctx = new ProductSearchContext();
                ctx.ManufacturerId = manufacturer.Id;
                ctx.FeaturedProducts = true;
                ctx.LanguageId = _workContext.WorkingLanguage.Id;
                ctx.OrderBy = ProductSortingEnum.Position;
                ctx.PageSize = int.MaxValue;
				ctx.StoreId = _storeContext.CurrentStoreIdIfMultiStoreMode;

                var featuredProducts = _productService.SearchProducts(ctx);

                model.FeaturedProducts = PrepareProductOverviewModels(featuredProducts, prepareColorAttributes: true).ToList();
            }

            //products
            var ctx2 = new ProductSearchContext();
            ctx2.ManufacturerId = manufacturer.Id;
            ctx2.FeaturedProducts = _catalogSettings.IncludeFeaturedProductsInNormalLists ? null : (bool?)false;
            ctx2.PriceMin = minPriceConverted;
            ctx2.PriceMax = maxPriceConverted;
            ctx2.LanguageId = _workContext.WorkingLanguage.Id;
            ctx2.OrderBy = (ProductSortingEnum)command.OrderBy;
            ctx2.PageIndex = command.PageNumber - 1;
            ctx2.PageSize = command.PageSize;
			ctx2.StoreId = _storeContext.CurrentStoreIdIfMultiStoreMode;

            var products = _productService.SearchProducts(ctx2);

            model.Products = PrepareProductOverviewModels(products, prepareColorAttributes: true).ToList();

            model.PagingFilteringContext.LoadPagedList(products);
            //model.PagingFilteringContext.ViewMode = viewMode; // codehint: sm-delete


            //template
            var templateCacheKey = string.Format(ModelCacheEventConsumer.MANUFACTURER_TEMPLATE_MODEL_KEY, manufacturer.ManufacturerTemplateId);
            var templateViewPath = _cacheManager.Get(templateCacheKey, () =>
            {
                var template = _manufacturerTemplateService.GetManufacturerTemplateById(manufacturer.ManufacturerTemplateId);
                if (template == null)
                    template = _manufacturerTemplateService.GetAllManufacturerTemplates().FirstOrDefault();
                return template.ViewPath;
            });

            //activity log
            _customerActivityService.InsertActivity("PublicStore.ViewManufacturer", _localizationService.GetResource("ActivityLog.PublicStore.ViewManufacturer"), manufacturer.Name);

            return View(templateViewPath, model);
        }

        [RequireHttpsByConfigAttribute(SslRequirement.No)]
        public ActionResult ManufacturerAll()
        {
            var model = new List<ManufacturerModel>();
            var manufacturers = _manufacturerService.GetAllManufacturers();
            foreach (var manufacturer in manufacturers)
            {
                var modelMan = manufacturer.ToModel();

                // prepare picture model
                modelMan.PictureModel = this.PrepareManufacturerPictureModel(manufacturer, modelMan.Name);
                model.Add(modelMan);
            }

            return View(model);
        }

        private PictureModel PrepareManufacturerPictureModel(Manufacturer manufacturer, string localizedName)
        {
            var model = new PictureModel();

            int pictureSize = _mediaSettings.ManufacturerThumbPictureSize;
            var manufacturerPictureCacheKey = string.Format(ModelCacheEventConsumer.MANUFACTURER_PICTURE_MODEL_KEY,
                manufacturer.Id,
                pictureSize,
                true,
                _workContext.WorkingLanguage.Id,
                _webHelper.IsCurrentConnectionSecured(),
				_storeContext.CurrentStore.Id);

            model = _cacheManager.Get(manufacturerPictureCacheKey, () =>
            {
                var pictureModel = new PictureModel()
                {
                    PictureId = manufacturer.PictureId, // codehint: sm-add
                    FullSizeImageUrl = _pictureService.GetPictureUrl(manufacturer.PictureId),
                    ImageUrl = _pictureService.GetPictureUrl(manufacturer.PictureId, pictureSize),
                    Title = string.Format(_localizationService.GetResource("Media.Manufacturer.ImageLinkTitleFormat"), localizedName),
                    AlternateText = string.Format(_localizationService.GetResource("Media.Manufacturer.ImageAlternateTextFormat"), localizedName)
                };
                return pictureModel;
            });

            return model;
        }

        [ChildActionOnly]
        public ActionResult ManufacturerNavigation(int currentManufacturerId)
        {
			string cacheKey = string.Format(ModelCacheEventConsumer.MANUFACTURER_NAVIGATION_MODEL_KEY, currentManufacturerId, _workContext.WorkingLanguage.Id, _storeContext.CurrentStore.Id);
            var cacheModel = _cacheManager.Get(cacheKey, () =>
            {
                var currentManufacturer = _manufacturerService.GetManufacturerById(currentManufacturerId);

                var manufacturers = _manufacturerService.GetAllManufacturers();
                var model = new ManufacturerNavigationModel()
                {
                    TotalManufacturers = manufacturers.Count
                };

                foreach (var manufacturer in manufacturers.Take(_catalogSettings.ManufacturersBlockItemsToDisplay))
                {
                    var modelMan = new ManufacturerBriefInfoModel()
                    {
                        Id = manufacturer.Id,
                        Name = manufacturer.GetLocalized(x => x.Name),
                        SeName = manufacturer.GetSeName(),
                        IsActive = currentManufacturer != null && currentManufacturer.Id == manufacturer.Id,
                    };
                    model.Manufacturers.Add(modelMan);
                }
                return model;
            });

            return PartialView(cacheModel);
        }

        #endregion

        #region Products

        //product details page
        [RequireHttpsByConfigAttribute(SslRequirement.No)]
        public ActionResult Product(int productId, string attributes)
        {
            var product = _productService.GetProductById(productId);
            if (product == null || product.Deleted)
                return RedirectToRoute("HomePage");

            //Is published?
            //Check whether the current user has a "Manage catalog" permission
            //It allows him to preview a product before publishing
            if (!product.Published && !_permissionService.Authorize(StandardPermissionProvider.ManageCatalog))
                return RedirectToRoute("HomePage");

            //ACL (access control list)
            if (!_aclService.Authorize(product))
                return RedirectToRoute("HomePage");

			//Store mapping
			if (!_storeMappingService.Authorize(product))
				return RedirectToRoute("HomePage");
            
            //prepare the model
            var model = PrepareProductDetailsPageModel(product, attributes);

            //save as recently viewed
            _recentlyViewedProductsService.AddProductToRecentlyViewedList(product.Id);

            //activity log
            _customerActivityService.InsertActivity("PublicStore.ViewProduct", _localizationService.GetResource("ActivityLog.PublicStore.ViewProduct"), product.Name);

            return View(model.ProductTemplateViewPath, model);
        }

        //add product variant to cart using HTTP POST
        //currently we use this method only for mobile device version
        //desktop version uses AJAX version of this method (ShoppingCartController.AddProductVariantToCart)
		//[HttpPost, ActionName("Product")]
		//[ValidateInput(false)]
		//public ActionResult AddProductVariantToCart(int productId, FormCollection form)
		//{
		//	var product = _productService.GetProductById(productId);
		//	if (product == null || product.Deleted || !product.Published)
		//		return RedirectToRoute("HomePage");

		//	//manually process form
		//	int productVariantId = 0;
		//	ShoppingCartType cartType = ShoppingCartType.ShoppingCart;
		//	foreach (string formKey in form.AllKeys)
		//	{
		//		if (formKey.StartsWith("addtocartbutton-"))
		//		{
		//			productVariantId = Convert.ToInt32(formKey.Substring(("addtocartbutton-").Length));
		//			cartType = ShoppingCartType.ShoppingCart;
		//		}
		//		else if (formKey.StartsWith("addtowishlistbutton-"))
		//		{
		//			productVariantId = Convert.ToInt32(formKey.Substring(("addtowishlistbutton-").Length));
		//			cartType = ShoppingCartType.Wishlist;
		//		}
		//	}

		//	var productVariant = _productService.GetProductVariantById(productVariantId);
		//	if (productVariant == null)
		//		return RedirectToRoute("HomePage");

		//	#region Customer entered price
		//	decimal customerEnteredPrice = decimal.Zero;
		//	decimal customerEnteredPriceConverted = decimal.Zero;
		//	if (productVariant.CustomerEntersPrice)
		//	{
		//		foreach (string formKey in form.AllKeys)
		//			if (formKey.Equals(string.Format("addtocart_{0}.CustomerEnteredPrice", productVariantId), StringComparison.InvariantCultureIgnoreCase))
		//			{
		//				if (decimal.TryParse(form[formKey], out customerEnteredPrice))
		//					customerEnteredPriceConverted = _currencyService.ConvertToPrimaryStoreCurrency(customerEnteredPrice, _workContext.WorkingCurrency);
		//				break;
		//			}
		//	}
		//	#endregion

		//	#region Quantity

		//	int quantity = 1;
		//	foreach (string formKey in form.AllKeys)
		//		if (formKey.Equals(string.Format("addtocart_{0}.AddToCart.EnteredQuantity", productVariantId), StringComparison.InvariantCultureIgnoreCase))
		//		{
		//			int.TryParse(form[formKey], out quantity);
		//			break;
		//		}

		//	#endregion

		//	var addToCartWarnings = new List<string>();
		//	string attributes = "";

		//	#region Product attributes
		//	string selectedAttributes = string.Empty;
		//	var productVariantAttributes = _productAttributeService.GetProductVariantAttributesByProductVariantId(productVariant.Id);
		//	foreach (var attribute in productVariantAttributes)
		//	{
		//		string controlId = string.Format("product_attribute_{0}_{1}_{2}", attribute.ProductVariantId, attribute.ProductAttributeId, attribute.Id);
		//		switch (attribute.AttributeControlType)
		//		{
		//			case AttributeControlType.DropdownList:
		//			case AttributeControlType.RadioList:
		//			case AttributeControlType.ColorSquares:
		//				{
		//					var rblAttributes = form[controlId];
		//					if (!String.IsNullOrEmpty(rblAttributes))
		//					{
		//						int selectedAttributeId = int.Parse(rblAttributes);
		//						if (selectedAttributeId > 0)
		//							selectedAttributes = _productAttributeParser.AddProductAttribute(selectedAttributes,
		//								attribute, selectedAttributeId.ToString());
		//					}
		//				}
		//				break;
		//			case AttributeControlType.Checkboxes:
		//				{
		//					var cblAttributes = form[controlId];
		//					if (!String.IsNullOrEmpty(cblAttributes))
		//					{
		//						foreach (var item in cblAttributes.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
		//						{
		//							int selectedAttributeId = int.Parse(item);
		//							if (selectedAttributeId > 0)
		//								selectedAttributes = _productAttributeParser.AddProductAttribute(selectedAttributes,
		//									attribute, selectedAttributeId.ToString());
		//						}
		//					}
		//				}
		//				break;
		//			case AttributeControlType.TextBox:
		//			case AttributeControlType.MultilineTextbox:
		//				{
		//					var txtAttribute = form[controlId];
		//					if (!String.IsNullOrEmpty(txtAttribute))
		//					{
		//						string enteredText = txtAttribute.Trim();
		//						selectedAttributes = _productAttributeParser.AddProductAttribute(selectedAttributes,
		//							attribute, enteredText);
		//					}
		//				}
		//				break;
		//			case AttributeControlType.Datepicker:
		//				{
		//					var day = form[controlId + "_day"];
		//					var month = form[controlId + "_month"];
		//					var year = form[controlId + "_year"];
		//					DateTime? selectedDate = null;
		//					try
		//					{
		//						selectedDate = new DateTime(Int32.Parse(year), Int32.Parse(month), Int32.Parse(day));
		//					}
		//					catch { }
		//					if (selectedDate.HasValue)
		//					{
		//						selectedAttributes = _productAttributeParser.AddProductAttribute(selectedAttributes,
		//							attribute, selectedDate.Value.ToString("D"));
		//					}
		//				}
		//				break;
		//			case AttributeControlType.FileUpload:
		//				{
		//					var httpPostedFile = this.Request.Files[controlId];
		//					if ((httpPostedFile != null) && (!String.IsNullOrEmpty(httpPostedFile.FileName)))
		//					{
		//						int fileMaxSize = _catalogSettings.FileUploadMaximumSizeBytes;
		//						if (httpPostedFile.ContentLength > fileMaxSize)
		//						{
		//							addToCartWarnings.Add(string.Format(_localizationService.GetResource("ShoppingCart.MaximumUploadedFileSize"), (int)(fileMaxSize / 1024)));
		//						}
		//						else
		//						{
		//							//save an uploaded file
		//							var download = new Download()
		//							{
		//								DownloadGuid = Guid.NewGuid(),
		//								UseDownloadUrl = false,
		//								DownloadUrl = "",
		//								DownloadBinary = httpPostedFile.GetDownloadBits(),
		//								ContentType = httpPostedFile.ContentType,
		//								Filename = System.IO.Path.GetFileNameWithoutExtension(httpPostedFile.FileName),
		//								Extension = System.IO.Path.GetExtension(httpPostedFile.FileName),
		//								IsNew = true
		//							};
		//							_downloadService.InsertDownload(download);
		//							//save attribute
		//							selectedAttributes = _productAttributeParser.AddProductAttribute(selectedAttributes,
		//								attribute, download.DownloadGuid.ToString());
		//						}
		//					}
		//				}
		//				break;
		//			default:
		//				break;
		//		}
		//	}
		//	attributes = selectedAttributes;

		//	#endregion

		//	#region Gift cards

		//	string recipientName = "";
		//	string recipientEmail = "";
		//	string senderName = "";
		//	string senderEmail = "";
		//	string giftCardMessage = "";
		//	if (productVariant.IsGiftCard)
		//	{
		//		foreach (string formKey in form.AllKeys)
		//		{
		//			if (formKey.Equals(string.Format("giftcard_{0}.RecipientName", productVariantId), StringComparison.InvariantCultureIgnoreCase))
		//			{
		//				recipientName = form[formKey];
		//				continue;
		//			}
		//			if (formKey.Equals(string.Format("giftcard_{0}.RecipientEmail", productVariantId), StringComparison.InvariantCultureIgnoreCase))
		//			{
		//				recipientEmail = form[formKey];
		//				continue;
		//			}
		//			if (formKey.Equals(string.Format("giftcard_{0}.SenderName", productVariantId), StringComparison.InvariantCultureIgnoreCase))
		//			{
		//				senderName = form[formKey];
		//				continue;
		//			}
		//			if (formKey.Equals(string.Format("giftcard_{0}.SenderEmail", productVariantId), StringComparison.InvariantCultureIgnoreCase))
		//			{
		//				senderEmail = form[formKey];
		//				continue;
		//			}
		//			if (formKey.Equals(string.Format("giftcard_{0}.Message", productVariantId), StringComparison.InvariantCultureIgnoreCase))
		//			{
		//				giftCardMessage = form[formKey];
		//				continue;
		//			}
		//		}

		//		attributes = _productAttributeParser.AddGiftCardAttribute(attributes,
		//			recipientName, recipientEmail, senderName, senderEmail, giftCardMessage);
		//	}

		//	#endregion

		//	//save item
		//	addToCartWarnings.AddRange(_shoppingCartService.AddToCart(_workContext.CurrentCustomer,
		//		productVariant, cartType, _storeContext.CurrentStore.Id, 
		//		attributes, customerEnteredPriceConverted, quantity, true));

		//	#region Set already entered values

		//	//set already entered values (quantity, customer entered price, gift card attributes, product attributes
		//	//we do it manually because views do not use HTML helpers for rendering controls

		//	Action<ProductDetailsModel> setEnteredValues = (productModel) =>
		//	{
		//		//find product variant model
		//		var productVariantModel = productModel
		//			.ProductVariantModels
		//			.Where(x => x.Id == productVariant.Id)
		//			.FirstOrDefault();
		//		if (productVariantModel == null)
		//			return;

		//		#region 'Add to cart' model

		//		//entered quantity
		//		productVariantModel.AddToCart.EnteredQuantity = quantity;
		//		//allowed quantities
		//		var allowedQuantities = productVariant.ParseAllowedQuatities();
		//		if (allowedQuantities.Length > 0)
		//		{
		//			var allowedQuantitySelectedItem = productVariantModel.AddToCart.AllowedQuantities
		//				.Where(x => x.Text == quantity.ToString())
		//				.FirstOrDefault();
		//			if (allowedQuantitySelectedItem != null)
		//			{
		//				allowedQuantitySelectedItem.Selected = true;
		//			}
		//		}

		//		//customer entered price
		//		if (productVariantModel.AddToCart.CustomerEntersPrice)
		//		{
		//			productVariantModel.AddToCart.CustomerEnteredPrice = customerEnteredPrice;
		//		}

		//		#endregion

		//		#region Gift card attributes

		//		if (productVariant.IsGiftCard)
		//		{
		//			productVariantModel.GiftCard.RecipientName = recipientName;
		//			productVariantModel.GiftCard.RecipientEmail = recipientEmail;
		//			productVariantModel.GiftCard.SenderName = senderName;
		//			productVariantModel.GiftCard.SenderEmail = senderEmail;
		//			productVariantModel.GiftCard.Message = giftCardMessage;
		//		}

		//		#endregion

		//		#region Product attributes
		//		//clear pre-defined values)
		//		foreach (var pvaModel in productVariantModel.ProductVariantAttributes)
		//		{
		//			foreach (var pvavModel in pvaModel.Values)
		//				pvavModel.IsPreSelected = false;
		//		}
		//		//select the previously entered ones
		//		foreach (var attribute in productVariantAttributes)
		//		{
		//			string controlId = string.Format("product_attribute_{0}_{1}_{2}", attribute.ProductVariantId, attribute.ProductAttributeId, attribute.Id);
		//			switch (attribute.AttributeControlType)
		//			{
		//				case AttributeControlType.DropdownList:
		//				case AttributeControlType.RadioList:
		//				case AttributeControlType.ColorSquares:
		//					{
		//						var rblAttributes = form[controlId];
		//						if (!String.IsNullOrEmpty(rblAttributes))
		//						{
		//							int selectedAttributeId = int.Parse(rblAttributes);
		//							if (selectedAttributeId > 0)
		//							{
		//								var pvavModel = productVariantModel.ProductVariantAttributes
		//									.SelectMany(x => x.Values)
		//									.Where(y => y.Id == selectedAttributeId)
		//									.FirstOrDefault();
		//								if (pvavModel != null)
		//									pvavModel.IsPreSelected = true;
		//							}
		//						}
		//					}
		//					break;
		//				case AttributeControlType.Checkboxes:
		//					{
		//						var cblAttributes = form[controlId];
		//						if (!String.IsNullOrEmpty(cblAttributes))
		//						{
		//							foreach (var item in cblAttributes.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
		//							{
		//								int selectedAttributeId = int.Parse(item);
		//								if (selectedAttributeId > 0)
		//								{
		//									var pvavModel = productVariantModel.ProductVariantAttributes
		//									   .SelectMany(x => x.Values)
		//									   .Where(y => y.Id == selectedAttributeId)
		//									   .FirstOrDefault();
		//									if (pvavModel != null)
		//										pvavModel.IsPreSelected = true;
		//								}
		//							}
		//						}
		//					}
		//					break;
		//				case AttributeControlType.TextBox:
		//				case AttributeControlType.MultilineTextbox:
		//					{
		//						var txtAttribute = form[controlId];
		//						if (!String.IsNullOrEmpty(txtAttribute))
		//						{
		//							var pvaModel = productVariantModel
		//								.ProductVariantAttributes
		//								.Select(x => x)
		//								.Where(y => y.Id == attribute.Id)
		//								.FirstOrDefault();

		//							if (pvaModel != null)
		//								pvaModel.TextValue = txtAttribute;
		//						}
		//					}
		//					break;
		//				case AttributeControlType.Datepicker:
		//					{
		//						var pvaModel = productVariantModel
		//							.ProductVariantAttributes
		//							.Select(x => x)
		//							.Where(y => y.Id == attribute.Id)
		//							.FirstOrDefault();
		//						if (pvaModel != null)
		//						{
		//							int day, month, year;
		//							if (int.TryParse(form[controlId + "_day"], out day))
		//								pvaModel.SelectedDay = day;
		//							if (int.TryParse(form[controlId + "_month"], out month))
		//								pvaModel.SelectedMonth = month;
		//							if (int.TryParse(form[controlId + "_year"], out year))
		//								pvaModel.SelectedYear = year;
		//						}
		//					}
		//					break;
		//				default:
		//					break;
		//			}
		//		}

		//		#endregion
		//	};

		//	#endregion

		//	#region Return the view

		//	if (addToCartWarnings.Count == 0)
		//	{
		//		switch (cartType)
		//		{
		//			case ShoppingCartType.Wishlist:
		//				{
		//					if (_shoppingCartSettings.DisplayWishlistAfterAddingProduct)
		//					{
		//						//redirect to the wishlist page
		//						return RedirectToRoute("Wishlist");
		//					}
		//					else
		//					{
		//						//redisplay the page with "Product has been added to the wishlist" notification message
		//						var model = PrepareProductDetailsPageModel(product);
		//						this.SuccessNotification(_localizationService.GetResource("Products.ProductHasBeenAddedToTheWishlist"), false);
		//						//set already entered values (quantity, customer entered price, gift card attributes, product attributes)
		//						setEnteredValues(model);

		//						//activity log
		//						_customerActivityService.InsertActivity("PublicStore.AddToWishlist", _localizationService.GetResource("ActivityLog.PublicStore.AddToWishlist"), productVariant.FullProductName);

		//						return View(model.ProductTemplateViewPath, model);
		//					}
		//				}
		//			case ShoppingCartType.ShoppingCart:
		//			default:
		//				{
		//					if (_shoppingCartSettings.DisplayCartAfterAddingProduct)
		//					{
		//						//redirect to the shopping cart page
		//						return RedirectToRoute("ShoppingCart");
		//					}
		//					else
		//					{
		//						//redisplay the page with "Product has been added to the cart" notification message
		//						var model = PrepareProductDetailsPageModel(product);
		//						this.SuccessNotification(_localizationService.GetResource("Products.ProductHasBeenAddedToTheCart"), false);
		//						//set already entered values (quantity, customer entered price, gift card attributes, product attributes)
		//						setEnteredValues(model);

		//						//activity log
		//						_customerActivityService.InsertActivity("PublicStore.AddToShoppingCart", _localizationService.GetResource("ActivityLog.PublicStore.AddToShoppingCart"), productVariant.FullProductName);

		//						return View(model.ProductTemplateViewPath, model);
		//					}
		//				}
		//		}
		//	}
		//	else
		//	{
		//		//Errors
		//		foreach (string error in addToCartWarnings)
		//			ModelState.AddModelError("", error);

		//		//If we got this far, something failed, redisplay form
		//		var model = PrepareProductDetailsPageModel(product);
		//		//set already entered values (quantity, customer entered price, gift card attributes, product attributes
		//		setEnteredValues(model);
		//		return View(model.ProductTemplateViewPath, model);
		//	}

		//	#endregion
		//}

        [ChildActionOnly]
        public ActionResult ProductBreadcrumb(int productId)
        {
            var product = _productService.GetProductById(productId);
            if (product == null)
                throw new ArgumentException("No product found with the specified id");

            if (!_catalogSettings.CategoryBreadcrumbEnabled)
                return Content("");

            var customerRolesIds = _workContext.CurrentCustomer.CustomerRoles
                .Where(cr => cr.Active).Select(cr => cr.Id).ToList();
            var cacheKey = string.Format(ModelCacheEventConsumer.PRODUCT_BREADCRUMB_MODEL_KEY, product.Id, _workContext.WorkingLanguage.Id, string.Join(",", customerRolesIds),
				_storeContext.CurrentStore.Id);
            var cacheModel = _cacheManager.Get(cacheKey, () =>
            {
                var model = new ProductDetailsModel.ProductBreadcrumbModel()
                {
                    ProductId = product.Id,
                    ProductName = product.GetLocalized(x => x.Name),
                    ProductSeName = product.GetSeName()
                };
                var productCategories = _categoryService.GetProductCategoriesByProductId(product.Id);
                if (productCategories.Count > 0)
                {
                    var category = productCategories[0].Category;
                    if (category != null)
                    {
                        foreach (var catBr in GetCategoryBreadCrumb(category))
                        {
                            model.CategoryBreadcrumb.Add(new CategoryModel()
                            {
                                Id = catBr.Id,
                                Name = catBr.GetLocalized(x => x.Name),
                                SeName = catBr.GetSeName()
                            });
                        }
                    }
                }
                return model;
            });

            return PartialView(cacheModel);
        }

        [ChildActionOnly]
        public ActionResult ProductManufacturers(int productId, bool preparePictureModel = false)
        {
			string cacheKey = string.Format(ModelCacheEventConsumer.PRODUCT_MANUFACTURERS_MODEL_KEY, productId, _workContext.WorkingLanguage.Id, _storeContext.CurrentStore.Id);
            var cacheModel = _cacheManager.Get(cacheKey, () =>
            {
                var model = _manufacturerService.GetProductManufacturersByProductId(productId)
                    .Select(x =>
                    {
                        var m = x.Manufacturer.ToModel();
                        if (preparePictureModel)
                        {
                            m.PictureModel.ImageUrl = _pictureService.GetPictureUrl(x.Manufacturer.PictureId);
                            var picture = _pictureService.GetPictureUrl(x.Manufacturer.PictureId);
                            if (picture != null)
                            {
                                m.PictureModel.PictureId = x.Manufacturer.PictureId; // codehint: sm-add
                                m.PictureModel.Title = string.Format(_localizationService.GetResource("Media.Product.ImageLinkTitleFormat"), m.Name);
                                m.PictureModel.AlternateText = string.Format(_localizationService.GetResource("Media.Product.ImageAlternateTextFormat"), m.Name);
                            }
                        }
                        return m;
                    })
                    .ToList();
                return model;
            });

            return PartialView(cacheModel);
        }

        [ChildActionOnly]
        public ActionResult ProductReviewOverview(int productId)
        {
            var product = _productService.GetProductById(productId);
            if (product == null)
                throw new ArgumentException("No product found with the specified id");

            var model = new ProductReviewOverviewModel()
            {
                ProductId = product.Id,
                RatingSum = product.ApprovedRatingSum,
                TotalReviews = product.ApprovedTotalReviews,
                AllowCustomerReviews = product.AllowCustomerReviews
            };
            return PartialView(model);
        }

        [ChildActionOnly]
        //[OutputCache(Duration = 120, VaryByCustom = "WorkingLanguage")]
        public ActionResult ProductSpecifications(int productId)
        {
            var product = _productService.GetProductById(productId);
            if (product == null)
                throw new ArgumentException("No product found with the specified id");

            var model = PrepareProductSpecificationModel(product);
            return PartialView(model);
        }

        [ChildActionOnly]
        public ActionResult ProductDetailReviews(int productId)
        {
            var product = _productService.GetProductById(productId);
            if (product == null || !product.AllowCustomerReviews)
                return Content("");

            var model = new ProductReviewsModel();
            PrepareProductReviewsModel(model, product);

            return PartialView(model);
        }

        [ChildActionOnly]
        public ActionResult ProductTierPrices(int productId)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.DisplayPrices))
                return Content(""); //hide prices

            var product = _productService.GetProductById(productId);
            if (product == null)
                throw new ArgumentException("No product found with the specified id");

            if (!product.HasTierPrices)
                return Content(""); //no tier prices

            var model = product.TierPrices
                .OrderBy(x => x.Quantity)
				.FilterByStore(_storeContext.CurrentStore.Id)
                .FilterForCustomer(_workContext.CurrentCustomer)
                .ToList()
                .RemoveDuplicatedQuantities()
                .Select(tierPrice =>
                {
                    var m = new ProductDetailsModel.TierPriceModel()
                    {
                        Quantity = tierPrice.Quantity,
                    };
                    decimal taxRate = decimal.Zero;
                    decimal priceBase = _taxService.GetProductPrice(product, _priceCalculationService.GetFinalPrice(product, _workContext.CurrentCustomer, decimal.Zero, _catalogSettings.DisplayTierPricesWithDiscounts, tierPrice.Quantity), out taxRate);
                    //_taxService.GetProductPrice(variant, tierPrice.Price, out taxRate);
                    decimal price = _currencyService.ConvertFromPrimaryStoreCurrency(priceBase, _workContext.WorkingCurrency);
                    m.Price = _priceFormatter.FormatPrice(price, false, false);
                    return m;
                })
                .ToList();

            return PartialView(model);
        }

        [ChildActionOnly]
        public ActionResult RelatedProducts(int productId, int? productThumbPictureSize)
        {
            var products = new List<Product>();
            var relatedProducts = _productService
                .GetRelatedProductsByProductId1(productId);
            foreach (var product in _productService.GetProductsByIds(relatedProducts.Select(x => x.ProductId2).ToArray()))
            {
				//UNDONE revise product-variant logic
                //var variants = _productService.GetProductVariantsByProductId(product.Id);
                //ensure that a product has at least one available variant
				//and has ACL permission and appropriate store mapping
				if (_aclService.Authorize(product) && _storeMappingService.Authorize(product))
                    products.Add(product);
            }
            var model = PrepareProductOverviewModels(products, true, true, productThumbPictureSize).ToList();

            return PartialView(model);
        }

        [ChildActionOnly]
        public ActionResult ProductsAlsoPurchased(int productId, int? productThumbPictureSize)
        {
            if (!_catalogSettings.ProductsAlsoPurchasedEnabled)
                return Content("");

            //load and cache report
			var productIds = _cacheManager.Get(string.Format(ModelCacheEventConsumer.PRODUCTS_ALSO_PURCHASED_IDS_KEY, productId, _storeContext.CurrentStore.Id), () =>
                _orderReportService
				.GetProductsAlsoPurchasedById(_storeContext.CurrentStore.Id, productId, _catalogSettings.ProductsAlsoPurchasedNumber)
                .Select(x => x.Id)
                .ToArray()
                );

            //load products
            var products = _productService.GetProductsByIds(productIds);
			//ACL and store mapping
			products = products.Where(p => _aclService.Authorize(p) && _storeMappingService.Authorize(p)).ToList();
            //prepare model
            var model = PrepareProductOverviewModels(products, true, true, productThumbPictureSize).ToList();

            return PartialView(model);
        }

        [ChildActionOnly]
        public ActionResult ShareButton()
        {
            if (_catalogSettings.ShowShareButton && !String.IsNullOrEmpty(_catalogSettings.PageShareCode))
            {
                var shareCode = _catalogSettings.PageShareCode;
                if (_webHelper.IsCurrentConnectionSecured())
                {
                    //need to change the addthis link to be https linked when the page is, so that the page doesnt ask about mixed mode when viewed in https...
                    shareCode = shareCode.Replace("http://", "https://");
                }

                return PartialView("ShareButton", shareCode);
            }

            return Content("");
        }

        [ChildActionOnly]
        public ActionResult CrossSellProducts(int? productThumbPictureSize)
        {
			var cart = _workContext.CurrentCustomer.ShoppingCartItems
				 .Where(sci => sci.ShoppingCartType == ShoppingCartType.ShoppingCart)
				 .Where(sci => sci.StoreId == _storeContext.CurrentStore.Id)
				 .ToList();

            var products = _productService.GetCrosssellProductsByShoppingCart(cart, _shoppingCartSettings.CrossSellsNumber);
			//ACL and store mapping
			products = products.Where(p => _aclService.Authorize(p) && _storeMappingService.Authorize(p)).ToList();


            //Cross-sell products are dispalyed on the shopping cart page.
            //We know that the entire shopping cart page is not refresh
            //even if "ShoppingCartSettings.DisplayCartAfterAddingProduct" setting  is enabled.
            //That's why we force page refresh (redirect) in this case
            var model = PrepareProductOverviewModels(products,
                productThumbPictureSize: productThumbPictureSize, forceRedirectionAfterAddingToCart: true)
                .ToList();

            return PartialView(model);
        }

        //recently viewed products
        [RequireHttpsByConfigAttribute(SslRequirement.No)]
        public ActionResult RecentlyViewedProducts()
        {
            var model = new List<ProductOverviewModel>();
            if (_catalogSettings.RecentlyViewedProductsEnabled)
            {
                var products = _recentlyViewedProductsService.GetRecentlyViewedProducts(_catalogSettings.RecentlyViewedProductsNumber);
                model.AddRange(PrepareProductOverviewModels(products));
            }
            return View(model);
        }

        [ChildActionOnly]
        public ActionResult RecentlyViewedProductsBlock(int? productThumbPictureSize)
        {
            var model = new List<ProductOverviewModel>();
            if (_catalogSettings.RecentlyViewedProductsEnabled)
            {
                var products = _recentlyViewedProductsService.GetRecentlyViewedProducts(_catalogSettings.RecentlyViewedProductsNumber);
                model.AddRange(PrepareProductOverviewModels(products, false, true, productThumbPictureSize));
            }
            return PartialView(model);
        }

        [RequireHttpsByConfigAttribute(SslRequirement.No)]
        public ActionResult RecentlyAddedProducts(CatalogPagingFilteringModel command)
        {

            //codehint: sm-edit (Änderungen wurden auskommentiert, wegen Schwierigekeiten beim Pagen)
            var model = new RecentlyAddedProductsModel();

            if (_catalogSettings.RecentlyAddedProductsEnabled)
            {
                IList<int> filterableSpecificationAttributeOptionIds = null;

                var ctx = new ProductSearchContext();
                ctx.LanguageId = _workContext.WorkingLanguage.Id;
                //codehint: sm-edit begin 
                //ctx.OrderBy = (ProductSortingEnum)command.OrderBy;
                ctx.OrderBy = ProductSortingEnum.CreatedOn;
                //ctx.PageSize = command.PageSize;
                ctx.PageSize = _catalogSettings.RecentlyAddedProductsNumber;
                //ctx.PageIndex = command.PageNumber - 1;
                //codehint: sm-edit end
                ctx.FilterableSpecificationAttributeOptionIds = filterableSpecificationAttributeOptionIds;
				ctx.StoreId = _storeContext.CurrentStoreIdIfMultiStoreMode;

                var products = _productService.SearchProducts(ctx);

                //var products = _productService.SearchProducts(ctx).Take(_catalogSettings.RecentlyAddedProductsNumber).OrderBy((ProductSortingEnum)command.OrderBy);

                model.Products.AddRange(PrepareProductOverviewModels(products));

                //codehint: sm-add
                //model.PagingFilteringContext.LoadPagedList(products);
            }
            return View(model);
        }

        public ActionResult RecentlyAddedProductsRss()
        {
            var feed = new SyndicationFeed(
									string.Format("{0}: Recently added products", _storeContext.CurrentStore.Name),
                                    "Information about products",
                                    new Uri(_webHelper.GetStoreLocation(false)),
                                    "RecentlyAddedProductsRSS",
                                    DateTime.UtcNow);

            if (!_catalogSettings.RecentlyAddedProductsEnabled)
                return new RssActionResult() { Feed = feed };

            var items = new List<SyndicationItem>();

            var ctx = new ProductSearchContext();
            ctx.LanguageId = _workContext.WorkingLanguage.Id;
            ctx.OrderBy = ProductSortingEnum.CreatedOn;
            ctx.PageSize = _catalogSettings.RecentlyAddedProductsNumber;
			ctx.StoreId = _storeContext.CurrentStoreIdIfMultiStoreMode;

            var products = _productService.SearchProducts(ctx);

            foreach (var product in products)
            {
                string productUrl = Url.RouteUrl("Product", new { SeName = product.GetSeName() }, "http");
                items.Add(new SyndicationItem(product.GetLocalized(x => x.Name), product.GetLocalized(x => x.ShortDescription), new Uri(productUrl), String.Format("RecentlyAddedProduct:{0}", product.Id), product.CreatedOnUtc));
            }
            feed.Items = items;
            return new RssActionResult() { Feed = feed };
        }

        [ChildActionOnly]
        public ActionResult HomepageBestSellers(int? productThumbPictureSize)
        {
            if (!_catalogSettings.ShowBestsellersOnHomepage || _catalogSettings.NumberOfBestsellersOnHomepage == 0)
                return Content("");

            //load and cache report
			var report = _cacheManager.Get(string.Format(ModelCacheEventConsumer.HOMEPAGE_BESTSELLERS_IDS_KEY, _storeContext.CurrentStore.Id), 
                () =>
                    //group by products (not product variants)
                    _orderReportService
					.BestSellersReport(_storeContext.CurrentStore.Id, null, null, null, null, null, 0, _catalogSettings.NumberOfBestsellersOnHomepage));

            //load products
            var products = _productService.GetProductsByIds(report.Select(x => x.EntityId).ToArray());
			//ACL and store mapping
			products = products.Where(p => _aclService.Authorize(p) && _storeMappingService.Authorize(p)).ToList();
			//prepare model
            var model = new HomePageBestsellersModel()
            {
                UseSmallProductBox = _catalogSettings.UseSmallProductBoxOnHomePage,
                Products = PrepareProductOverviewModels(products, true, true, productThumbPictureSize).ToList()
            };

            return PartialView(model);
        }

        [ChildActionOnly]
        public ActionResult HomepageProducts(int? productThumbPictureSize)
        {
            var products = _productService.GetAllProductsDisplayedOnHomePage();
			//ACL and store mapping
			products = products.Where(p => _aclService.Authorize(p) && _storeMappingService.Authorize(p)).ToList();

            var model = new HomePageProductsModel()
            {
                UseSmallProductBox = false, //_catalogSettings.UseSmallProductBoxOnHomePage,
                //codehint: sm-edit
                //Products = PrepareProductOverviewModels(products, 
                //    !_catalogSettings.UseSmallProductBoxOnHomePage, true, productThumbPictureSize)
                //    .ToList()
                Products = PrepareProductOverviewModels(products, true, true, productThumbPictureSize, prepareColorAttributes: true).ToList()
            };

            return PartialView(model);
        }

        public ActionResult BackInStockSubscribePopup(int productId)
        {
            var product = _productService.GetProductById(productId);
            if (product == null || product.Deleted)
                throw new ArgumentException("No product found with the specified id");

            var model = new BackInStockSubscribeModel();
            model.ProductId = product.Id;
            model.ProductName = product.GetLocalized(x => x.Name);
            model.ProductSeName = product.GetSeName();
            model.IsCurrentCustomerRegistered = _workContext.CurrentCustomer.IsRegistered();
            model.MaximumBackInStockSubscriptions = _catalogSettings.MaximumBackInStockSubscriptions;
			model.CurrentNumberOfBackInStockSubscriptions = _backInStockSubscriptionService
				 .GetAllSubscriptionsByCustomerId(_workContext.CurrentCustomer.Id, _storeContext.CurrentStore.Id, 0, 1)
				 .TotalCount;
            if (product.ManageInventoryMethod == ManageInventoryMethod.ManageStock &&
                product.BackorderMode == BackorderMode.NoBackorders &&
                product.AllowBackInStockSubscriptions &&
                product.StockQuantity <= 0)
            {
                //out of stock
                model.SubscriptionAllowed = true;
				model.AlreadySubscribed = _backInStockSubscriptionService
					.FindSubscription(_workContext.CurrentCustomer.Id, product.Id, _storeContext.CurrentStore.Id) != null;
            }
            return View(model);
        }

        [HttpPost, ActionName("BackInStockSubscribePopup")]
        public ActionResult BackInStockSubscribePopupPOST(int productId)
        {
            var product = _productService.GetProductById(productId);
            if (product == null || product.Deleted)
                throw new ArgumentException("No product found with the specified id");

            if (!_workContext.CurrentCustomer.IsRegistered())
                return Content(_localizationService.GetResource("BackInStockSubscriptions.OnlyRegistered"));

            if (product.ManageInventoryMethod == ManageInventoryMethod.ManageStock &&
                product.BackorderMode == BackorderMode.NoBackorders &&
                product.AllowBackInStockSubscriptions &&
                product.StockQuantity <= 0)
            {
                //out of stock
				var subscription = _backInStockSubscriptionService
					.FindSubscription(_workContext.CurrentCustomer.Id, product.Id, _storeContext.CurrentStore.Id);
                if (subscription != null)
                {
                    //unsubscribe
                    _backInStockSubscriptionService.DeleteSubscription(subscription);
                    return Content("Unsubscribed");
                }
                else
                {
					if (_backInStockSubscriptionService
						.GetAllSubscriptionsByCustomerId(_workContext.CurrentCustomer.Id, _storeContext.CurrentStore.Id, 0, 1)
						.TotalCount >= _catalogSettings.MaximumBackInStockSubscriptions)
                        return Content(string.Format(_localizationService.GetResource("BackInStockSubscriptions.MaxSubscriptions"), _catalogSettings.MaximumBackInStockSubscriptions));

                    //subscribe   
                    subscription = new BackInStockSubscription()
                    {
                        Customer = _workContext.CurrentCustomer,
                        Product = product,
						StoreId = _storeContext.CurrentStore.Id,
                        CreatedOnUtc = DateTime.UtcNow
                    };
                    _backInStockSubscriptionService.InsertSubscription(subscription);
                    return Content("Subscribed");
                }

            }
            else
            {
                return Content(_localizationService.GetResource("BackInStockSubscriptions.NotAllowed"));
            }
        }

        [HttpPost]
        public ActionResult UpdateProductDetails(int productId, bool? updateGallery, FormCollection form)
        {
            int quantity = 1;
            int galleryStartIndex = -1;
            string galleryHtml = null;
            var pictureModel = new ProductDetailsPictureModel();
            var m = new ProductDetailsModel();
            var product = _productService.GetProductById(productId);

            // quantity required for tier prices
            string quantityKey = form.AllKeys.FirstOrDefault(k => k.EndsWith("EnteredQuantity"));
            if (quantityKey.HasValue())
                int.TryParse(form[quantityKey], out quantity);

            // get merged model data
            PrepareProductDetailModel(m, product, form, quantity);

            // get updated image gallery
            if (updateGallery ?? true)
            {
                var pictures = _pictureService.GetPicturesByProductId(productId);

                if (pictures.Count <= _catalogSettings.DisplayAllImagesNumber)
                {	// all pictures rendered... only index is required
                    if (m.CombinationSelected != null)
                    {
                        var combiAssignedImages = m.CombinationSelected.GetAssignedPictureIds();
                        if (combiAssignedImages.Length > 0)
                        {
                            var picture = pictures.FirstOrDefault(p => p.Id == combiAssignedImages[0]);
                            if (picture != null)
                                galleryStartIndex = pictures.IndexOf(picture);
                        }
                    }
                }
                else
                {
                    var allCombinationImageIds = new List<int>();

					_productAttributeService
						.GetAllProductVariantAttributeCombinations(product.Id)
						.GetAllCombinationImageIds(allCombinationImageIds);

                    PrepareProductDetailsPictureModel(pictureModel, pictures, product.GetLocalized(x => x.Name), allCombinationImageIds, m.CombinationSelected);

                    galleryHtml = this.RenderPartialViewToString("_ProductDetailsPictures", pictureModel);
                    galleryStartIndex = pictureModel.GalleryStartIndex;
                }
            }

            #region data object
            object data = new
            {
                Delivery = new
                {
                    Id = (m.DeliveryTime == null ? 0 : m.DeliveryTime.Id),
                    Name = (m.DeliveryTime == null ? "" : m.DeliveryTime.Name),
                    Color = (m.DeliveryTime == null ? "" : m.DeliveryTime.ColorHexValue)
                },
                Measure = new
                {
                    Weight = new { Value = m.WeightValue, Text = m.Weight },
                    Height = new { Value = product.Height, Text = m.Height },
                    Width = new { Value = product.Width, Text = m.Width },
                    Length = new { Value = product.Length, Text = m.Length }
                },
                Number = new
                {
                    Sku = new { Value = m.Sku, Show = m.ShowSku },
                    Gtin = new { Value = m.Gtin, Show = m.ShowGtin },
                    Mpn = new { Value = m.ManufacturerPartNumber, Show = m.ShowManufacturerPartNumber }
                },
                Price = new
                {
                    Base = new
                    {
                        Enabled = m.IsBasePriceEnabled,
                        Info = m.BasePriceInfo
                    },
                    Old = new
                    {
                        Value = decimal.Zero,
                        Text = m.ProductPrice.OldPrice
                    },
                    WithoutDiscount = new
                    {
                        Value = m.ProductPrice.PriceValue,
                        Text = m.ProductPrice.Price
                    },
                    WithDiscount = new
                    {
                        Value = m.ProductPrice.PriceWithDiscountValue,
                        Text = m.ProductPrice.PriceWithDiscount
                    }
                },
                Stock = new
                {
                    Quantity = new { Value = product.StockQuantity, Show = product.DisplayStockQuantity },
                    Availability = new { Text = m.StockAvailability, Show = product.DisplayStockAvailability, Unavailable = m.IsUnavailable }
                },

                GalleryHtml = galleryHtml,
                GalleryStartIndex = galleryStartIndex
            };
            #endregion

            return new JsonResult { Data = data };
        }

        #endregion

        #region Product tags

        //Product tags
        [ChildActionOnly]
        public ActionResult ProductTags(int productId)
        {
            var product = _productService.GetProductById(productId);
            if (product == null)
                throw new ArgumentException("No product found with the specified id");

			var cacheKey = string.Format(ModelCacheEventConsumer.PRODUCTTAG_BY_PRODUCT_MODEL_KEY, product.Id, _workContext.WorkingLanguage.Id, _storeContext.CurrentStore.Id);
            var cacheModel = _cacheManager.Get(cacheKey, () =>
                {
                    var model = product.ProductTags
						//filter by store
						.Where(x => _productTagService.GetProductCount(x.Id, _storeContext.CurrentStore.Id) > 0)
                        .Select(x =>
                                    {
                                        var ptModel = new ProductTagModel()
                                        {
                                            Id = x.Id,
                                            Name = x.GetLocalized(y => y.Name),
                                            SeName = x.GetSeName(),
											ProductCount = _productTagService.GetProductCount(x.Id, _storeContext.CurrentStore.Id)
                                        };
                                        return ptModel;
                                    })
                        .ToList();
                    return model;
                });

            return PartialView(cacheModel);
        }

        [ChildActionOnly]
        public ActionResult PopularProductTags()
        {
			var cacheKey = string.Format(ModelCacheEventConsumer.PRODUCTTAG_POPULAR_MODEL_KEY, _workContext.WorkingLanguage.Id, _storeContext.CurrentStore.Id);
            var cacheModel = _cacheManager.Get(cacheKey, () =>
            {
                var model = new PopularProductTagsModel();

                //get all tags
                var allTags = _productTagService
					.GetAllProductTags()
					//filter by current store
					.Where(x => _productTagService.GetProductCount(x.Id, _storeContext.CurrentStore.Id) > 0)
					//order by product count
					.OrderByDescending(x => _productTagService.GetProductCount(x.Id, _storeContext.CurrentStore.Id))
					.ToList();

                var tags = allTags
                    .Take(_catalogSettings.NumberOfProductTags)
                    .ToList();
                //sorting
                tags = tags.OrderBy(x => x.GetLocalized(y => y.Name)).ToList();

                model.TotalTags = allTags.Count;

                foreach (var tag in tags)
                    model.Tags.Add(new ProductTagModel()
                    {
                        Id = tag.Id,
                        Name = tag.GetLocalized(y => y.Name),
                        SeName = tag.GetSeName(),
						ProductCount = _productTagService.GetProductCount(tag.Id, _storeContext.CurrentStore.Id)
                    });
                return model;
            });

            return PartialView(cacheModel);
        }

        [RequireHttpsByConfigAttribute(SslRequirement.No)]
        public ActionResult ProductsByTag(int productTagId, CatalogPagingFilteringModel command)
        {
            var productTag = _productTagService.GetProductTagById(productTagId);
            if (productTag == null)
                return RedirectToRoute("HomePage");

            if (command.PageNumber <= 0)
                command.PageNumber = 1;

            var model = new ProductsByTagModel()
            {
                TagName = productTag.GetLocalized(y => y.Name)
            };

            // codehint: sm-edit (replaced)
            PreparePagingFilteringModel(model.PagingFilteringContext, command, new PageSizeContext
            {
                AllowCustomersToSelectPageSize = _catalogSettings.ProductsByTagAllowCustomersToSelectPageSize,
                PageSize = _catalogSettings.ProductsByTagPageSize,
                PageSizeOptions = _catalogSettings.ProductsByTagPageSizeOptions.IsEmpty()
                    ? _catalogSettings.DefaultPageSizeOptions
                    : _catalogSettings.ProductsByTagPageSizeOptions
            });

            //products

            var ctx = new ProductSearchContext();
            ctx.ProductTagId = productTag.Id;
            ctx.LanguageId = _workContext.WorkingLanguage.Id;
            ctx.OrderBy = (ProductSortingEnum)command.OrderBy;
            ctx.PageIndex = command.PageNumber - 1;
            ctx.PageSize = command.PageSize;
			ctx.StoreId = _storeContext.CurrentStoreIdIfMultiStoreMode;

            var products = _productService.SearchProducts(ctx);

            model.Products = PrepareProductOverviewModels(products, prepareColorAttributes: true).ToList();

            model.PagingFilteringContext.LoadPagedList(products);
            //model.PagingFilteringContext.ViewMode = viewMode; // codehint: sm-delete
            return View(model);
        }

        [RequireHttpsByConfigAttribute(SslRequirement.No)]
        public ActionResult ProductTagsAll()
        {
            var model = new PopularProductTagsModel();
            model.Tags = _productTagService
				.GetAllProductTags()
				//filter by current store
				.Where(x => _productTagService.GetProductCount(x.Id, _storeContext.CurrentStore.Id) > 0)
				//sort by name
				.OrderBy(x => x.GetLocalized(y => y.Name))
				.Select(x =>
							{
								var ptModel = new ProductTagModel()
								{
									Id = x.Id,
									Name = x.GetLocalized(y => y.Name),
									SeName = x.GetSeName(),
									ProductCount = _productTagService.GetProductCount(x.Id, _storeContext.CurrentStore.Id)
								};
								return ptModel;
							})
				.ToList();
            return View(model);
        }

        #endregion

        #region Product reviews

        //products reviews
        [RequireHttpsByConfigAttribute(SslRequirement.No)]
        public ActionResult ProductReviews(int productId)
        {
            var product = _productService.GetProductById(productId);
            if (product == null || product.Deleted || !product.Published || !product.AllowCustomerReviews)
                return RedirectToRoute("HomePage");

            var model = new ProductReviewsModel();
            PrepareProductReviewsModel(model, product);
            //only registered users can leave reviews
            if (_workContext.CurrentCustomer.IsGuest() && !_catalogSettings.AllowAnonymousUsersToReviewProduct)
                ModelState.AddModelError("", _localizationService.GetResource("Reviews.OnlyRegisteredUsersCanWriteReviews"));
            //default value
            model.AddProductReview.Rating = _catalogSettings.DefaultProductRatingValue;
            return View(model);
        }

        [HttpPost, ActionName("ProductReviews")]
        [FormValueRequired("add-review")]
        [CaptchaValidator]
        public ActionResult ProductReviewsAdd(int productId, ProductReviewsModel model, bool captchaValid)
        {
            var product = _productService.GetProductById(productId);
            if (product == null || product.Deleted || !product.Published || !product.AllowCustomerReviews)
                return RedirectToRoute("HomePage");

            //validate CAPTCHA
            if (_captchaSettings.Enabled && _captchaSettings.ShowOnProductReviewPage && !captchaValid)
            {
                ModelState.AddModelError("", _localizationService.GetResource("Common.WrongCaptcha"));
            }

            if (_workContext.CurrentCustomer.IsGuest() && !_catalogSettings.AllowAnonymousUsersToReviewProduct)
            {
                ModelState.AddModelError("", _localizationService.GetResource("Reviews.OnlyRegisteredUsersCanWriteReviews"));
            }

            if (ModelState.IsValid)
            {
                //save review
                int rating = model.AddProductReview.Rating;
                if (rating < 1 || rating > 5)
                    rating = _catalogSettings.DefaultProductRatingValue;
                bool isApproved = !_catalogSettings.ProductReviewsMustBeApproved;

                var productReview = new ProductReview()
                {
                    ProductId = product.Id,
                    CustomerId = _workContext.CurrentCustomer.Id,
                    IpAddress = _webHelper.GetCurrentIpAddress(),
                    Title = model.AddProductReview.Title,
                    ReviewText = model.AddProductReview.ReviewText,
                    Rating = rating,
                    HelpfulYesTotal = 0,
                    HelpfulNoTotal = 0,
                    IsApproved = isApproved,
                    CreatedOnUtc = DateTime.UtcNow,
                    UpdatedOnUtc = DateTime.UtcNow,
                };
                _customerContentService.InsertCustomerContent(productReview);

                //update product totals
                _productService.UpdateProductReviewTotals(product);

                //notify store owner
                if (_catalogSettings.NotifyStoreOwnerAboutNewProductReviews)
                    _workflowMessageService.SendProductReviewNotificationMessage(productReview, _localizationSettings.DefaultAdminLanguageId);

                //activity log
                _customerActivityService.InsertActivity("PublicStore.AddProductReview", _localizationService.GetResource("ActivityLog.PublicStore.AddProductReview"), product.Name);


                PrepareProductReviewsModel(model, product);
                model.AddProductReview.Title = null;
                model.AddProductReview.ReviewText = null;

                model.AddProductReview.SuccessfullyAdded = true;
                if (!isApproved)
                    model.AddProductReview.Result = _localizationService.GetResource("Reviews.SeeAfterApproving");
                else
                    model.AddProductReview.Result = _localizationService.GetResource("Reviews.SuccessfullyAdded");

                return View(model);
            }

            //If we got this far, something failed, redisplay form
            PrepareProductReviewsModel(model, product);
            return View(model);
        }

        [HttpPost]
        public ActionResult SetProductReviewHelpfulness(int productReviewId, bool washelpful)
        {
            var productReview = _customerContentService.GetCustomerContentById(productReviewId) as ProductReview;
            if (productReview == null)
                throw new ArgumentException("No product review found with the specified id");

            if (_workContext.CurrentCustomer.IsGuest() && !_catalogSettings.AllowAnonymousUsersToReviewProduct)
            {
                return Json(new
                {
                    Success = false, // codehint: sm-add
                    Result = _localizationService.GetResource("Reviews.Helpfulness.OnlyRegistered"),
                    TotalYes = productReview.HelpfulYesTotal,
                    TotalNo = productReview.HelpfulNoTotal
                });
            }

            //customers aren't allowed to vote for their own reviews
            if (productReview.CustomerId == _workContext.CurrentCustomer.Id)
            {
                return Json(new
                {
                    Success = false, // codehint: sm-add
                    Result = _localizationService.GetResource("Reviews.Helpfulness.YourOwnReview"),
                    TotalYes = productReview.HelpfulYesTotal,
                    TotalNo = productReview.HelpfulNoTotal
                });
            }

            //delete previous helpfulness
            var oldPrh = (from prh in productReview.ProductReviewHelpfulnessEntries
                          where prh.CustomerId == _workContext.CurrentCustomer.Id
                          select prh).FirstOrDefault();
            if (oldPrh != null)
                _customerContentService.DeleteCustomerContent(oldPrh);

            //insert new helpfulness
            var newPrh = new ProductReviewHelpfulness()
            {
                ProductReviewId = productReview.Id,
                CustomerId = _workContext.CurrentCustomer.Id,
                IpAddress = _webHelper.GetCurrentIpAddress(),
                WasHelpful = washelpful,
                IsApproved = true, //always approved
                CreatedOnUtc = DateTime.UtcNow,
                UpdatedOnUtc = DateTime.UtcNow,
            };
            _customerContentService.InsertCustomerContent(newPrh);

            //new totals
            int helpfulYesTotal = (from prh in productReview.ProductReviewHelpfulnessEntries
                                   where prh.WasHelpful
                                   select prh).Count();
            int helpfulNoTotal = (from prh in productReview.ProductReviewHelpfulnessEntries
                                  where !prh.WasHelpful
                                  select prh).Count();

            productReview.HelpfulYesTotal = helpfulYesTotal;
            productReview.HelpfulNoTotal = helpfulNoTotal;
            _customerContentService.UpdateCustomerContent(productReview);

            return Json(new
            {
                Success = true, // codehint: sm-add
                Result = _localizationService.GetResource("Reviews.Helpfulness.SuccessfullyVoted"),
                TotalYes = productReview.HelpfulYesTotal,
                TotalNo = productReview.HelpfulNoTotal
            });
        }

        #endregion

        #region Ask product question

        [ChildActionOnly]
        public ActionResult ProductAskQuestionButton(int productId)
        {
            if (!_catalogSettings.AskQuestionEnabled)
                return Content("");
            var model = new ProductAskQuestionModel()
            {
                ProductId = productId
            };

            return PartialView("ProductAskQuestionButton", model);
        }

        [RequireHttpsByConfigAttribute(SslRequirement.No)]
        public ActionResult ProductAskQuestion(int productId)
        {
            var product = _productService.GetProductById(productId);
            if (product == null || product.Deleted || !product.Published || !_catalogSettings.AskQuestionEnabled)
                return RedirectToRoute("HomePage");

            var customer = _workContext.CurrentCustomer;

            var model = new ProductAskQuestionModel();
            model.ProductId = product.Id;
            model.ProductName = product.GetLocalized(x => x.Name);
            model.ProductSeName = product.GetSeName();
            model.SenderEmail = customer.Email;
            model.SenderName = customer.GetFullName();
            model.SenderPhone = customer.GetAttribute<string>(SystemCustomerAttributeNames.Phone);
            model.Question = _localizationService.GetResource("Products.AskQuestion.Question.Text").FormatCurrentUI(model.ProductName);
            model.DisplayCaptcha = _captchaSettings.Enabled && _captchaSettings.ShowOnAskQuestionPage;

            return View(model);
        }

        [HttpPost, ActionName("ProductAskQuestion")]
        [CaptchaValidator]
        public ActionResult ProductAskQuestionSend(ProductAskQuestionModel model, bool captchaValid)
        {
            var product = _productService.GetProductById(model.ProductId);
            if (product == null || product.Deleted || !product.Published || !_catalogSettings.AskQuestionEnabled)
                return RedirectToRoute("HomePage");

            // validate CAPTCHA
            if (_captchaSettings.Enabled && _captchaSettings.ShowOnAskQuestionPage && !captchaValid)
            {
                ModelState.AddModelError("", _localizationService.GetResource("Common.WrongCaptcha"));
            }

            if (ModelState.IsValid)
            {
                // email
                var result = _workflowMessageService.SendProductQuestionMessage(
                    _workContext.CurrentCustomer,
                    _workContext.WorkingLanguage.Id,
                    product,
                    model.SenderEmail,
                    model.SenderName,
                    model.SenderPhone,
                    Core.Html.HtmlUtils.FormatText(model.Question, false, true, false, false, false, false));

                if (result > 0)
                {
                    this.SuccessNotification(_localizationService.GetResource("Products.AskQuestion.Sent"), true);
                    return RedirectToRoute("Product", new { SeName = product.GetSeName() });
                }
                else
                {
                    ModelState.AddModelError("", "Fehler beim Versenden der Email. Bitte versuchen Sie es später erneut.");
                }
            }

            // If we got this far, something failed, redisplay form
            var customer = _workContext.CurrentCustomer;
            model.ProductId = product.Id;
            model.ProductName = product.GetLocalized(x => x.Name);
            model.ProductSeName = product.GetSeName();
            model.DisplayCaptcha = _captchaSettings.Enabled && _captchaSettings.ShowOnAskQuestionPage;
            return View(model);
        }

        #endregion

        #region Email a friend

        //products email a friend
        [ChildActionOnly]
        public ActionResult ProductEmailAFriendButton(int productId)
        {
            if (!_catalogSettings.EmailAFriendEnabled)
                return Content("");
            var model = new ProductEmailAFriendModel()
            {
                ProductId = productId
            };

            return PartialView("ProductEmailAFriendButton", model);
        }

        [RequireHttpsByConfigAttribute(SslRequirement.No)]
        public ActionResult ProductEmailAFriend(int productId)
        {
            var product = _productService.GetProductById(productId);
            if (product == null || product.Deleted || !product.Published || !_catalogSettings.EmailAFriendEnabled)
                return RedirectToRoute("HomePage");

            var model = new ProductEmailAFriendModel();
            model.ProductId = product.Id;
            model.ProductName = product.GetLocalized(x => x.Name);
            model.ProductSeName = product.GetSeName();
            model.YourEmailAddress = _workContext.CurrentCustomer.Email;
            model.DisplayCaptcha = _captchaSettings.Enabled && _captchaSettings.ShowOnEmailProductToFriendPage;
            return View(model);
        }

        [HttpPost, ActionName("ProductEmailAFriend")]
        //[FormValueRequired("send-email")]
        [CaptchaValidator]
        public ActionResult ProductEmailAFriendSend(ProductEmailAFriendModel model, bool captchaValid)
        {
            var product = _productService.GetProductById(model.ProductId);
            if (product == null || product.Deleted || !product.Published || !_catalogSettings.EmailAFriendEnabled)
                return RedirectToRoute("HomePage");

            //validate CAPTCHA
            if (_captchaSettings.Enabled && _captchaSettings.ShowOnEmailProductToFriendPage && !captchaValid)
            {
                ModelState.AddModelError("", _localizationService.GetResource("Common.WrongCaptcha"));
            }

            //check whether the current customer is guest and ia allowed to email a friend
            if (_workContext.CurrentCustomer.IsGuest() && !_catalogSettings.AllowAnonymousUsersToEmailAFriend)
            {
                ModelState.AddModelError("", _localizationService.GetResource("Products.EmailAFriend.OnlyRegisteredUsers"));
            }

            if (ModelState.IsValid)
            {
                //email
                _workflowMessageService.SendProductEmailAFriendMessage(_workContext.CurrentCustomer,
                        _workContext.WorkingLanguage.Id, product,
                        model.YourEmailAddress, model.FriendEmail,
                        Core.Html.HtmlUtils.FormatText(model.PersonalMessage, false, true, false, false, false, false));

                model.ProductId = product.Id;
                model.ProductName = product.GetLocalized(x => x.Name);
                model.ProductSeName = product.GetSeName();

                model.SuccessfullySent = true;
                model.Result = _localizationService.GetResource("Products.EmailAFriend.SuccessfullySent");

                return View(model);
            }

            //If we got this far, something failed, redisplay form
            model.ProductId = product.Id;
            model.ProductName = product.GetLocalized(x => x.Name);
            model.ProductSeName = product.GetSeName();
            model.DisplayCaptcha = _captchaSettings.Enabled && _captchaSettings.ShowOnEmailProductToFriendPage;
            return View(model);
        }

        #endregion

        #region Comparing products

        //compare products
        public ActionResult AddProductToCompareList(int productId)
        {
            var product = _productService.GetProductById(productId);
            if (product == null || product.Deleted || !product.Published)
                return RedirectToRoute("HomePage");

            if (!_catalogSettings.CompareProductsEnabled)
                return RedirectToRoute("HomePage");

            _compareProductsService.AddProductToCompareList(productId);

            //activity log
            _customerActivityService.InsertActivity("PublicStore.AddToCompareList", _localizationService.GetResource("ActivityLog.PublicStore.AddToCompareList"), product.Name);

            return RedirectToRoute("CompareProducts");
        }

        // codehint: sm-add
        // ajax
        [HttpPost]
        [ActionName("AddProductToCompareList")]
        public ActionResult AddProductToCompareListAjax(int productId)
        {
            var product = _productService.GetProductById(productId);
            if (product == null || product.Deleted || !product.Published || !_catalogSettings.CompareProductsEnabled)
            {
                return Json(new
                {
                    success = false,
                    message = _localizationService.GetResource("AddProductToCompareList.CouldNotBeAdded")
                });
            }

            _compareProductsService.AddProductToCompareList(productId);

            //activity log
            _customerActivityService.InsertActivity("PublicStore.AddToCompareList", _localizationService.GetResource("ActivityLog.PublicStore.AddToCompareList"), product.Name);

            return Json(new
            {
                success = true,
                message = string.Format(_localizationService.GetResource("AddProductToCompareList.ProductWasAdded"), product.Name)
            });
        }

        public ActionResult RemoveProductFromCompareList(int productId)
        {
            var product = _productService.GetProductById(productId);
            if (product == null)
                return RedirectToRoute("HomePage");

            if (!_catalogSettings.CompareProductsEnabled)
                return RedirectToRoute("HomePage");

            _compareProductsService.RemoveProductFromCompareList(productId);

            return RedirectToRoute("CompareProducts");
        }

        // codehint: sm-add
        // ajax
        [HttpPost]
        [ActionName("RemoveProductFromCompareList")]
        public ActionResult RemoveProductFromCompareListAjax(int productId)
        {
            var product = _productService.GetProductById(productId);
            if (product == null || !_catalogSettings.CompareProductsEnabled)
            {
                return Json(new
                {
                    success = false,
                    message = _localizationService.GetResource("AddProductToCompareList.CouldNotBeRemoved")
                });
            }

            _compareProductsService.RemoveProductFromCompareList(productId);

            return Json(new
            {
                success = true,
                message = string.Format(_localizationService.GetResource("AddProductToCompareList.ProductWasDeleted"), product.Name)
            });
        }

        [RequireHttpsByConfigAttribute(SslRequirement.No)]
        public ActionResult CompareProducts()
        {
            if (!_catalogSettings.CompareProductsEnabled)
                return RedirectToRoute("HomePage");

            var model = new CompareProductsModel()
            {
                IncludeShortDescriptionInCompareProducts = _catalogSettings.IncludeShortDescriptionInCompareProducts,
                IncludeFullDescriptionInCompareProducts = _catalogSettings.IncludeFullDescriptionInCompareProducts,
            };
            var products = _compareProductsService.GetComparedProducts();
            PrepareProductOverviewModels(products, prepareSpecificationAttributes: true)
                .ToList()
                .ForEach(model.Products.Add);
            return View(model);
        }

        public ActionResult ClearCompareList()
        {
            if (!_catalogSettings.CompareProductsEnabled)
                return RedirectToRoute("HomePage");

            _compareProductsService.ClearCompareProducts();

            return RedirectToRoute("CompareProducts");
        }

        [ChildActionOnly]
        public ActionResult CompareProductsButton(int productId)
        {
            if (!_catalogSettings.CompareProductsEnabled)
                return Content("");

            var model = new AddToCompareListModel()
            {
                ProductId = productId
            };

            return PartialView("CompareProductsButton", model);
        }

        // Ajax
        // codehint: sm-add
        [HttpPost]
        public ActionResult CompareSummary()
        {
            return Json(new
            {
                Count = _compareProductsService.GetComparedProducts().Count
            });
        }

        /// <summary>
        /// <remarks>codehint: sm-add</remarks>
        /// </summary>
        /// <returns></returns>
        public ActionResult FlyoutCompare()
        {
            var model = new CompareProductsModel()
            {
                IncludeShortDescriptionInCompareProducts = _catalogSettings.IncludeShortDescriptionInCompareProducts,
                IncludeFullDescriptionInCompareProducts = _catalogSettings.IncludeFullDescriptionInCompareProducts,
            };
            var products = _compareProductsService.GetComparedProducts();
            PrepareProductOverviewModels(products, prepareSpecificationAttributes: true)
                .ToList()
                .ForEach(model.Products.Add);

            return PartialView(model);
        }

        #endregion

        #region Searching

        [RequireHttpsByConfigAttribute(SslRequirement.No)]
        [ValidateInput(false)]
        public ActionResult Search(SearchModel model, SearchPagingFilteringModel command)
        {
            if (model == null)
                model = new SearchModel();

            //'Continue shopping' URL
			_genericAttributeService.SaveAttribute(_workContext.CurrentCustomer,
				SystemCustomerAttributeNames.LastContinueShoppingPage,
				_webHelper.GetThisPageUrl(false),
				_storeContext.CurrentStore.Id);

            if (command.PageSize <= 0)
                command.PageSize = _catalogSettings.SearchPageProductsPerPage; //_catalogSettings.SearchPageProductsPerPage;
            if (command.PageNumber <= 0)
                command.PageNumber = 1;

            // codehint: sm-edit
            PreparePagingFilteringModel(model.PagingFilteringContext, command, new PageSizeContext
            {
                AllowCustomersToSelectPageSize = _catalogSettings.ProductSearchAllowCustomersToSelectPageSize,
                PageSize = _catalogSettings.SearchPageProductsPerPage,
                PageSizeOptions = _catalogSettings.ProductSearchPageSizeOptions
            });
            // codehint: sm-edit

            if (model.Q == null)
                model.Q = "";
            model.Q = model.Q.Trim();

            var categories = _categoryService.GetAllCategories();
            if (categories.Count > 0)
            {
                //first empty entry
                model.AvailableCategories.Add(new SelectListItem()
                {
                    Value = "0",
                    Text = _localizationService.GetResource("Common.All")
                });
                //all other categories
                foreach (var c in categories)
                {
                    //generate full category name (breadcrumb)
                    string fullCategoryBreadcrumbName = "";
                    var breadcrumb = GetCategoryBreadCrumb(c);
                    for (int i = 0; i <= breadcrumb.Count - 1; i++)
                    {
                        fullCategoryBreadcrumbName += breadcrumb[i].GetLocalized(x => x.Name);
                        if (i != breadcrumb.Count - 1)
                            fullCategoryBreadcrumbName += " >> ";
                    }

                    model.AvailableCategories.Add(new SelectListItem()
                    {
                        Value = c.Id.ToString(),
                        Text = fullCategoryBreadcrumbName,
                        Selected = model.Cid == c.Id
                    });
                }
            }

            var manufacturers = _manufacturerService.GetAllManufacturers();
            if (manufacturers.Count > 0)
            {
                model.AvailableManufacturers.Add(new SelectListItem()
                {
                    Value = "0",
                    Text = _localizationService.GetResource("Common.All")
                });
                foreach (var m in manufacturers)
                    model.AvailableManufacturers.Add(new SelectListItem()
                    {
                        Value = m.Id.ToString(),
                        Text = m.GetLocalized(x => x.Name),
                        Selected = model.Mid == m.Id
                    });
            }

            IPagedList<Product> products = new PagedList<Product>(new List<Product>(), 0, 1);
            // only search if query string search keyword is set (used to avoid searching or displaying search term min length error message on /search page load)
            if (Request.Params["Q"] != null)
            {
                if (model.Q.Length < _catalogSettings.ProductSearchTermMinimumLength)
                {
                    model.Warning = string.Format(_localizationService.GetResource("Search.SearchTermMinimumLengthIsNCharacters"), _catalogSettings.ProductSearchTermMinimumLength);
                }
                else
                {
                    var categoryIds = new List<int>();
                    int manufacturerId = 0;
                    decimal? minPriceConverted = null;
                    decimal? maxPriceConverted = null;
                    bool searchInDescriptions = false;
                    if (model.As)
                    {
                        //advanced search
                        var categoryId = model.Cid;
                        if (categoryId > 0)
                        {
                            categoryIds.Add(categoryId);
                            if (model.Isc)
                            {
                                //include subcategories
                                categoryIds.AddRange(GetChildCategoryIds(categoryId));
                            }
                        }


                        manufacturerId = model.Mid;

                        //min price
                        if (!string.IsNullOrEmpty(model.Pf))
                        {
                            decimal minPrice = decimal.Zero;
                            if (decimal.TryParse(model.Pf, out minPrice))
                                minPriceConverted = _currencyService.ConvertToPrimaryStoreCurrency(minPrice, _workContext.WorkingCurrency);
                        }
                        //max price
                        if (!string.IsNullOrEmpty(model.Pt))
                        {
                            decimal maxPrice = decimal.Zero;
                            if (decimal.TryParse(model.Pt, out maxPrice))
                                maxPriceConverted = _currencyService.ConvertToPrimaryStoreCurrency(maxPrice, _workContext.WorkingCurrency);
                        }

                        searchInDescriptions = model.Sid;
                    }

                    //var searchInProductTags = false;
                    var searchInProductTags = searchInDescriptions;

                    //products

                    var ctx = new ProductSearchContext();
                    ctx.CategoryIds = categoryIds;
                    ctx.ManufacturerId = manufacturerId;
                    ctx.PriceMin = minPriceConverted;
                    ctx.PriceMax = maxPriceConverted;
                    ctx.Keywords = model.Q;
                    ctx.SearchDescriptions = searchInDescriptions;
                    ctx.SearchProductTags = searchInProductTags;
                    ctx.LanguageId = _workContext.WorkingLanguage.Id;
                    ctx.OrderBy = (ProductSortingEnum)command.OrderBy; // ProductSortingEnum.Position; // codehint: sm-edit
                    ctx.PageIndex = command.PageNumber - 1;
                    ctx.PageSize = command.PageSize;
					ctx.StoreId = _storeContext.CurrentStoreIdIfMultiStoreMode;

                    products = _productService.SearchProducts(ctx);

                    model.Products = PrepareProductOverviewModels(products, prepareColorAttributes: true).ToList();

                    model.NoResults = !model.Products.Any();
                }
            }

            model.PagingFilteringContext.LoadPagedList(products);
            return View(model);
        }

        [ChildActionOnly]
        public ActionResult SearchBox()
        {
            var model = new SearchBoxModel()
            {
                AutoCompleteEnabled = _catalogSettings.ProductSearchAutoCompleteEnabled,
                ShowProductImagesInSearchAutoComplete = _catalogSettings.ShowProductImagesInSearchAutoComplete,
                SearchTermMinimumLength = _catalogSettings.ProductSearchTermMinimumLength
            };
            return PartialView(model);
        }

        public ActionResult SearchTermAutoComplete(string term)
        {
            if (String.IsNullOrWhiteSpace(term) || term.Length < _catalogSettings.ProductSearchTermMinimumLength)
                return Content("");

            //products
            var productNumber = _catalogSettings.ProductSearchAutoCompleteNumberOfProducts > 0 ?
                _catalogSettings.ProductSearchAutoCompleteNumberOfProducts : 10;

            var ctx = new ProductSearchContext();
            ctx.LanguageId = _workContext.WorkingLanguage.Id;
            ctx.Keywords = term;
            ctx.OrderBy = ProductSortingEnum.Position;
            ctx.PageSize = productNumber;
			ctx.StoreId = _storeContext.CurrentStoreIdIfMultiStoreMode;

            var products = _productService.SearchProducts(ctx);

            var models = PrepareProductOverviewModels(products, false, _catalogSettings.ShowProductImagesInSearchAutoComplete, _mediaSettings.AutoCompleteSearchThumbPictureSize).ToList();
            var result = (from p in models
                          select new
                          {
                              label = p.Name,
                              producturl = Url.RouteUrl("Product", new { SeName = p.SeName }),
                              productpictureurl = p.DefaultPictureModel.ImageUrl
                          })
                          .ToList();
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        #endregion

        #region Nested Classes

        // codehint: sm-add
        public class PageSizeContext
        {
            public bool AllowCustomersToSelectPageSize { get; set; }
            public string PageSizeOptions { get; set; }
            public int PageSize { get; set; }
        }

        #endregion
    }
}
