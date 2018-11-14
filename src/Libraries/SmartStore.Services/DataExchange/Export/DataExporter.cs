﻿using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading;
using System.Web;
using System.Data.Entity;
using SmartStore.Core;
using SmartStore.Core.Data;
using SmartStore.Core.Domain.Catalog;
using SmartStore.Core.Domain.Common;
using SmartStore.Core.Domain.Customers;
using SmartStore.Core.Domain.DataExchange;
using SmartStore.Core.Domain.Discounts;
using SmartStore.Core.Domain.Localization;
using SmartStore.Core.Domain.Media;
using SmartStore.Core.Domain.Messages;
using SmartStore.Core.Domain.Orders;
using SmartStore.Core.Domain.Shipping;
using SmartStore.Core.Domain.Stores;
using SmartStore.Core.Domain.Tax;
using SmartStore.Core.Email;
using SmartStore.Core.Localization;
using SmartStore.Core.Logging;
using SmartStore.Services.Catalog;
using SmartStore.Services.Catalog.Extensions;
using SmartStore.Services.Common;
using SmartStore.Services.Customers;
using SmartStore.Services.DataExchange.Export.Deployment;
using SmartStore.Services.DataExchange.Export.Internal;
using SmartStore.Services.Directory;
using SmartStore.Services.Localization;
using SmartStore.Services.Media;
using SmartStore.Services.Messages;
using SmartStore.Services.Orders;
using SmartStore.Services.Search;
using SmartStore.Services.Security;
using SmartStore.Services.Seo;
using SmartStore.Services.Shipping;
using SmartStore.Services.Tax;
using SmartStore.Utilities;
using SmartStore.Utilities.Threading;
using SmartStore.Collections;
using SmartStore.Core.Domain.Directory;

namespace SmartStore.Services.DataExchange.Export
{
    public partial class DataExporter : IDataExporter
	{
		private static readonly ReaderWriterLockSlim _rwLock = new ReaderWriterLockSlim();

		#region Dependencies

		private readonly ICommonServices _services;
		private readonly IDbContext _dbContext;
		private readonly HttpContextBase _httpContext;
		private readonly Lazy<IPriceFormatter> _priceFormatter;
		private readonly Lazy<IExportProfileService> _exportProfileService;
        private readonly ILocalizedEntityService _localizedEntityService;
		private readonly Lazy<ILanguageService> _languageService;
        private readonly Lazy<IUrlRecordService> _urlRecordService;
		private readonly Lazy<IPictureService> _pictureService;
		private readonly Lazy<IPriceCalculationService> _priceCalculationService;
		private readonly Lazy<ICurrencyService> _currencyService;
		private readonly Lazy<ITaxService> _taxService;
		private readonly Lazy<ICategoryService> _categoryService;
		private readonly Lazy<IProductAttributeParser> _productAttributeParser;
		private readonly Lazy<IProductAttributeService> _productAttributeService;
        private readonly Lazy<ISpecificationAttributeService> _specificationAttributeService;
        private readonly Lazy<IProductTemplateService> _productTemplateService;
		private readonly Lazy<ICategoryTemplateService> _categoryTemplateService;
		private readonly Lazy<IProductService> _productService;
		private readonly Lazy<IOrderService> _orderService;
		private readonly Lazy<IManufacturerService> _manufacturerService;
		private readonly ICustomerService _customerService;
		private readonly Lazy<IAddressService> _addressService;
		private readonly Lazy<ICountryService> _countryService;
        private readonly Lazy<IShipmentService> _shipmentService;
		private readonly Lazy<IGenericAttributeService> _genericAttributeService;
		private readonly Lazy<IEmailAccountService> _emailAccountService;
		private readonly Lazy<IQueuedEmailService> _queuedEmailService;
		private readonly Lazy<IEmailSender> _emailSender;
		private readonly Lazy<IDeliveryTimeService> _deliveryTimeService;
		private readonly Lazy<IQuantityUnitService> _quantityUnitService;
		private readonly Lazy<ICatalogSearchService> _catalogSearchService;
        private readonly Lazy<IDownloadService> _downloadService;
        private readonly Lazy<ProductUrlHelper> _productUrlHelper;

		private readonly Lazy<IRepository<Customer>>_customerRepository;
		private readonly Lazy<IRepository<NewsLetterSubscription>> _subscriptionRepository;
		private readonly Lazy<IRepository<Order>> _orderRepository;
		private readonly Lazy<IRepository<ShoppingCartItem>> _shoppingCartItemRepository;

		private readonly Lazy<MediaSettings> _mediaSettings;
		private readonly Lazy<ContactDataSettings> _contactDataSettings;
		private readonly Lazy<CustomerSettings> _customerSettings;
		private readonly Lazy<CatalogSettings> _catalogSettings;
		private readonly Lazy<LocalizationSettings> _localizationSettings;
		private readonly Lazy<TaxSettings> _taxSettings;

		public DataExporter(
			ICommonServices services,
			IDbContext dbContext,
			HttpContextBase httpContext,
			Lazy<IPriceFormatter> priceFormatter,
			Lazy<IExportProfileService> exportProfileService,
			ILocalizedEntityService localizedEntityService,
			Lazy<ILanguageService> languageService,
			Lazy<IUrlRecordService> urlRecordService,
			Lazy<IPictureService> pictureService,
			Lazy<IPriceCalculationService> priceCalculationService,
			Lazy<ICurrencyService> currencyService,
			Lazy<ITaxService> taxService,
			Lazy<ICategoryService> categoryService,
			Lazy<IProductAttributeParser> productAttributeParser,
			Lazy<IProductAttributeService> productAttributeService,
            Lazy<ISpecificationAttributeService> specificationAttributeService,
            Lazy<IProductTemplateService> productTemplateService,
			Lazy<ICategoryTemplateService> categoryTemplateService,
			Lazy<IProductService> productService,
			Lazy<IOrderService> orderService,
			Lazy<IManufacturerService> manufacturerService,
			ICustomerService customerService,
			Lazy<IAddressService> addressService,
			Lazy<ICountryService> countryService,
			Lazy<IShipmentService> shipmentService,
			Lazy<IGenericAttributeService> genericAttributeService,
			Lazy<IEmailAccountService> emailAccountService,
			Lazy<IQueuedEmailService> queuedEmailService,
            Lazy<IEmailSender> emailSender,
			Lazy<IDeliveryTimeService> deliveryTimeService,
			Lazy<IQuantityUnitService> quantityUnitService,
			Lazy<ICatalogSearchService> catalogSearchService,
            Lazy<IDownloadService> downloadService,
            Lazy<ProductUrlHelper> productUrlHelper,
			Lazy<IRepository<Customer>> customerRepository,
			Lazy<IRepository<NewsLetterSubscription>> subscriptionRepository,
			Lazy<IRepository<Order>> orderRepository,
			Lazy<IRepository<ShoppingCartItem>> shoppingCartItemRepository,
			Lazy<MediaSettings> mediaSettings,
			Lazy<ContactDataSettings> contactDataSettings,
			Lazy<CustomerSettings> customerSettings,
			Lazy<CatalogSettings> catalogSettings,
			Lazy<LocalizationSettings> localizationSettings,
			Lazy<TaxSettings> taxSettings)
		{
			_services = services;
			_dbContext = dbContext;
			_httpContext = httpContext;
			_priceFormatter = priceFormatter;
			_exportProfileService = exportProfileService;
			_localizedEntityService = localizedEntityService;
			_languageService = languageService;
			_urlRecordService = urlRecordService;
			_pictureService = pictureService;
			_priceCalculationService = priceCalculationService;
			_currencyService = currencyService;
			_taxService = taxService;
			_categoryService = categoryService;
			_productAttributeParser = productAttributeParser;
			_productAttributeService = productAttributeService;
            _specificationAttributeService = specificationAttributeService;
			_productTemplateService = productTemplateService;
			_categoryTemplateService = categoryTemplateService;
			_productService = productService;
			_orderService = orderService;
			_manufacturerService = manufacturerService;
			_customerService = customerService;
			_addressService = addressService;
			_countryService = countryService;
			_shipmentService = shipmentService;
			_genericAttributeService = genericAttributeService;
			_emailAccountService = emailAccountService;
			_queuedEmailService = queuedEmailService;
			_emailSender = emailSender;
			_deliveryTimeService = deliveryTimeService;
			_quantityUnitService = quantityUnitService;
			_catalogSearchService = catalogSearchService;
            _downloadService = downloadService;
			_productUrlHelper = productUrlHelper;

			_customerRepository = customerRepository;
			_subscriptionRepository = subscriptionRepository;
			_orderRepository = orderRepository;
			_shoppingCartItemRepository = shoppingCartItemRepository;

			_mediaSettings = mediaSettings;
			_contactDataSettings = contactDataSettings;
			_customerSettings = customerSettings;
			_catalogSettings = catalogSettings;
			_localizationSettings = localizationSettings;
			_taxSettings = taxSettings;

			T = NullLocalizer.Instance;
		}

		public Localizer T { get; set; }

        #endregion

        #region Utilities

        private LocalizedPropertyCollection CreateTranslationCollection(string keyGroup, IEnumerable<BaseEntity> entities)
        {
            if (entities == null || !entities.Any())
            {
                return new LocalizedPropertyCollection(keyGroup, null, Enumerable.Empty<LocalizedProperty>());
            }

            var collection = _localizedEntityService.GetLocalizedPropertyCollection(keyGroup, entities.Select(x => x.Id).Distinct().ToArray());
            return collection;
        }

        private void SetProgress(DataExporterContext ctx, int loadedRecords)
		{
			try
			{
				if (!ctx.IsPreview && loadedRecords > 0)
				{
					int totalRecords = ctx.RecordsPerStore.Sum(x => x.Value);

					if (ctx.Request.Profile.Limit > 0 && totalRecords > ctx.Request.Profile.Limit)
						totalRecords = ctx.Request.Profile.Limit;

					ctx.RecordCount = Math.Min(ctx.RecordCount + loadedRecords, totalRecords);
					var msg = ctx.ProgressInfo.FormatInvariant(ctx.RecordCount, totalRecords);
					ctx.Request.ProgressValueSetter.Invoke(ctx.RecordCount, totalRecords, msg);
				}
			}
			catch { }
		}

		private void SetProgress(DataExporterContext ctx, string message)
		{
			try
			{
				if (!ctx.IsPreview && message.HasValue())
				{
					ctx.Request.ProgressValueSetter.Invoke(0, 0, message);
				}
			}
			catch { }
		}

		private bool HasPermission(DataExporterContext ctx)
		{
			if (ctx.Request.HasPermission)
			{
				return true;
			}

			var customer = _services.WorkContext.CurrentCustomer;

			if (customer.SystemName == SystemCustomerNames.BackgroundTask)
			{
				return true;
			}

			switch (ctx.Request.Provider.Value.EntityType)
			{
				case ExportEntityType.Product:
				case ExportEntityType.Category:
				case ExportEntityType.Manufacturer:
					return _services.Permissions.Authorize(StandardPermissionProvider.ManageCatalog, customer);

				case ExportEntityType.Customer:
					return _services.Permissions.Authorize(StandardPermissionProvider.ManageCustomers, customer);

				case ExportEntityType.Order:
				case ExportEntityType.ShoppingCartItem:
					return _services.Permissions.Authorize(StandardPermissionProvider.ManageOrders, customer);

				case ExportEntityType.NewsLetterSubscription:
					return _services.Permissions.Authorize(StandardPermissionProvider.ManageNewsletterSubscribers, customer);
			}

			return true;
		}

        private void StreamToFile(DataExporterContext ctx, Stream stream, string path, Action<Stream> onDisposed)
        {
            if (stream != null)
            {
                try
                {
                    if (ctx.IsFileBasedExport && path.HasValue() && stream.Length > 0)
                    {
                        if (!stream.CanSeek)
                        {
                            ctx.Log.Warn("Data stream seems to be closed!");
                        }

                        stream.Seek(0, SeekOrigin.Begin);

                        using (_rwLock.GetWriteLock())
                        using (var fileStream = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.ReadWrite))
                        {
                            ctx.Log.Info($"Creating file {path}.");
                            stream.CopyTo(fileStream);
                        }
                    }
                }
                catch (Exception ex)
                {
                    ctx.ExecuteContext.Abort = DataExchangeAbortion.Hard;
                    ctx.Log.ErrorFormat(ex, $"Failed to stream file {path}.");
                    ctx.Result.LastError = ex.ToString();
                }
                finally
                {
                    stream.Dispose();
                    onDisposed(stream);
                }
            }

            if (ctx.ExecuteContext.Abort == DataExchangeAbortion.Hard && ctx.IsFileBasedExport && path.HasValue())
            {
                FileSystemHelper.Delete(path);
            }
        }

        private void DetachAllEntitiesAndClear(DataExporterContext ctx)
		{
			try
			{
                ctx.AssociatedProductContext?.Clear();
                ctx.TranslationsPerPage?.Clear();

                if (ctx.ProductExportContext != null)
				{
					_dbContext.DetachEntities(x =>
					{
						return x is Product || x is Discount || x is ProductVariantAttributeCombination || x is ProductVariantAttribute || 
							   x is Picture || x is ProductBundleItem || x is ProductCategory || x is ProductManufacturer ||
							   x is ProductPicture || x is ProductTag || x is ProductSpecificationAttribute || x is TierPrice;
					});

					ctx.ProductExportContext.Clear();
				}

				if (ctx.OrderExportContext != null)
				{
					_dbContext.DetachEntities(x =>
					{
						return x is Order || x is Address || x is GenericAttribute || x is Customer ||
							   x is OrderItem || x is RewardPointsHistory || x is Shipment;
					});

					ctx.OrderExportContext.Clear();
				}

				if (ctx.CategoryExportContext != null)
				{
					_dbContext.DetachEntities(x =>
					{
						return x is Category || x is Picture || x is ProductCategory;
					});

					ctx.CategoryExportContext.Clear();
				}

				if (ctx.ManufacturerExportContext != null)
				{
					_dbContext.DetachEntities(x =>
					{
						return x is Manufacturer || x is Picture || x is ProductManufacturer;
					});

					ctx.ManufacturerExportContext.Clear();
				}

				if (ctx.CustomerExportContext != null)
				{
					_dbContext.DetachEntities(x =>
					{
						return x is Customer || x is GenericAttribute || x is CustomerContent;
					});

					ctx.CustomerExportContext.Clear();
				}

				if (ctx.Request.Provider.Value.EntityType == ExportEntityType.ShoppingCartItem)
				{
					_dbContext.DetachEntities(x =>
					{
						return x is ShoppingCartItem || x is Customer || x is Product;
					});
				}
			}
			catch (Exception ex)
			{
				ctx.Log.Warn(ex, "Detaching entities failed.");
			}
		}

		private IExportDataSegmenterProvider CreateSegmenter(DataExporterContext ctx, int pageIndex = 0)
		{
			var offset = Math.Max(ctx.Request.Profile.Offset, 0) + (pageIndex * PageSize);
			var limit = Math.Max(ctx.Request.Profile.Limit, 0);
			var recordsPerSegment = ctx.IsPreview ? 0 : Math.Max(ctx.Request.Profile.BatchSize, 0);
			var totalCount = Math.Max(ctx.Request.Profile.Offset, 0) + ctx.RecordsPerStore.First(x => x.Key == ctx.Store.Id).Value;
			
			switch (ctx.Request.Provider.Value.EntityType)
			{
				case ExportEntityType.Product:
					ctx.ExecuteContext.DataSegmenter = new ExportDataSegmenter<Product>
					(
						skip => GetProducts(ctx, skip),
						entities =>
						{
                            // Load data behind navigation properties for current queue in one go.
                            ctx.ProductExportContext = CreateProductExportContext(entities, ctx.ContextCustomer, ctx.Store.Id);
                            ctx.AssociatedProductContext = null;

                            var context = ctx.ProductExportContext;
                            if (!ctx.Projection.NoGroupedProducts && entities.Where(x => x.ProductType == ProductType.GroupedProduct).Any())
                            {
                                context.AssociatedProducts.LoadAll();
                                var associatedProducts = context.AssociatedProducts.SelectMany(x => x.Value);
                                ctx.AssociatedProductContext = CreateProductExportContext(associatedProducts, ctx.ContextCustomer, ctx.Store.Id);

                                ctx.Translations[nameof(Product)] = CreateTranslationCollection(nameof(Product), entities.Where(x => x.ProductType != ProductType.GroupedProduct).Concat(associatedProducts));
                            }
                            else
                            {
                                ctx.Translations[nameof(Product)] = CreateTranslationCollection(nameof(Product), entities);
                            }

                            context.ProductTags.LoadAll();
                            context.ProductBundleItems.LoadAll();

                            ctx.TranslationsPerPage[nameof(ProductTag)] = CreateTranslationCollection(nameof(ProductTag), context.ProductTags.SelectMany(x => x.Value));
                            ctx.TranslationsPerPage[nameof(ProductBundleItem)] = CreateTranslationCollection(nameof(ProductBundleItem), context.ProductBundleItems.SelectMany(x => x.Value));

                        },
						entity => Convert(ctx, entity),
						offset, PageSize, limit, recordsPerSegment, totalCount
					);
					break;

				case ExportEntityType.Order:
					ctx.ExecuteContext.DataSegmenter = new ExportDataSegmenter<Order>
					(
						skip => GetOrders(ctx, skip),
						entities =>
						{
							ctx.OrderExportContext = new OrderExportContext(entities,
								x => _customerService.GetCustomersByIds(x),
								x => _genericAttributeService.Value.GetAttributesForEntity(x, "Customer"),
								x => _customerService.GetRewardPointsHistoriesByCustomerIds(x),
								x => _addressService.Value.GetAddressByIds(x),
								x => _orderService.Value.GetOrderItemsByOrderIds(x),
								x => _shipmentService.Value.GetShipmentsByOrderIds(x)
							);
						},
						entity => Convert(ctx, entity),
						offset, PageSize, limit, recordsPerSegment, totalCount
					);
					break;

				case ExportEntityType.Manufacturer:
					ctx.ExecuteContext.DataSegmenter = new ExportDataSegmenter<Manufacturer>
					(
						skip => GetManufacturers(ctx, skip),
						entities =>
						{
							ctx.ManufacturerExportContext = new ManufacturerExportContext(entities,
								x => _manufacturerService.Value.GetProductManufacturersByManufacturerIds(x),
								x => _pictureService.Value.GetPicturesByIds(x)
							);
						},
						entity => Convert(ctx, entity),
						offset, PageSize, limit, recordsPerSegment, totalCount
					);
					break;

				case ExportEntityType.Category:
					ctx.ExecuteContext.DataSegmenter = new ExportDataSegmenter<Category>
					(
						skip => GetCategories(ctx, skip),
						entities =>
						{
							ctx.CategoryExportContext = new CategoryExportContext(entities,
								x => _categoryService.Value.GetProductCategoriesByCategoryIds(x),
								x => _pictureService.Value.GetPicturesByIds(x)
							);
						},
						entity => Convert(ctx, entity),
						offset, PageSize, limit, recordsPerSegment, totalCount
					);
					break;

				case ExportEntityType.Customer:
					ctx.ExecuteContext.DataSegmenter = new ExportDataSegmenter<Customer>
					(
						skip => GetCustomers(ctx, skip),
						entities =>
						{
							ctx.CustomerExportContext = new CustomerExportContext(entities,
								x => _genericAttributeService.Value.GetAttributesForEntity(x, "Customer")
							);
						},
						entity => Convert(ctx, entity),
						offset, PageSize, limit, recordsPerSegment, totalCount
					);
					break;

				case ExportEntityType.NewsLetterSubscription:
					ctx.ExecuteContext.DataSegmenter = new ExportDataSegmenter<NewsLetterSubscription>
					(
						skip => GetNewsLetterSubscriptions(ctx, skip),
						null,
						entity => Convert(ctx, entity),
						offset, PageSize, limit, recordsPerSegment, totalCount
					);
					break;

				case ExportEntityType.ShoppingCartItem:
					ctx.ExecuteContext.DataSegmenter = new ExportDataSegmenter<ShoppingCartItem>
					(
						skip => GetShoppingCartItems(ctx, skip),
						null,
						entity => Convert(ctx, entity),
						offset, PageSize, limit, recordsPerSegment, totalCount
					);
					break;

				default:
					ctx.ExecuteContext.DataSegmenter = null;
					break;
			}

			return ctx.ExecuteContext.DataSegmenter as IExportDataSegmenterProvider;
		}

        private IEnumerable<ExportDataUnit> GetDataUnitsForRelatedEntities(DataExporterContext ctx)
        {
            // Related data is data without own export provider or importer. For a flat formatted export
            // they have to be exported together with metadata to know what to be edited.
            RelatedEntityType[] types = null;

            switch (ctx.Request.Provider.Value.EntityType)
            {
                case ExportEntityType.Product:
                    types = new RelatedEntityType[]
                    {
                        RelatedEntityType.TierPrice,
                        RelatedEntityType.ProductVariantAttributeValue,
                        RelatedEntityType.ProductVariantAttributeCombination
                    };
                    break;
                default:
                    return Enumerable.Empty<ExportDataUnit>();
            }

            var result = new List<ExportDataUnit>();
            var context = ctx.ExecuteContext;
            var fileExtension = Path.GetExtension(context.FileName);

            foreach (var type in types)
            {
                // Convention: Must end with type name because that's how the import identifies the entity.
                // Be careful in case of accidents with file names. They must not be too long.
                var fileName = $"{ctx.Store.Id}-{context.FileIndex.ToString("D4")}-{type.ToString()}";

                if (File.Exists(Path.Combine(context.Folder, fileName + fileExtension)))
                {
                    fileName = $"{CommonHelper.GenerateRandomDigitCode(4)}-{fileName}";
                }

                result.Add(new ExportDataUnit
                {
                    RelatedType = type,
                    DisplayInFileDialog = true,
                    FileName = fileName + fileExtension,
                    DataStream = new MemoryStream()
                });
            }

            return result;
        }

		private bool CallProvider(DataExporterContext ctx, string method, string path)
		{
			if (method != "Execute" && method != "OnExecuted")
			{
				throw new SmartException($"Unknown export method {method.NaIfEmpty()}.");
			}

            var context = ctx.ExecuteContext;

			try
			{
                context.DataStream = new MemoryStream();

                if (method == "Execute")
                {
                    ctx.Request.Provider.Value.Execute(context);
                }
                else if (method == "OnExecuted")
                {
                    ctx.Request.Provider.Value.OnExecuted(context);
                }
            }
            catch (Exception ex)
			{
				context.Abort = DataExchangeAbortion.Hard;
				ctx.Log.ErrorFormat(ex, $"The provider failed at the {method.NaIfEmpty()} method.");
				ctx.Result.LastError = ex.ToString();
			}
			finally
			{
                StreamToFile(ctx, context.DataStream, path, x => context.DataStream = null);

                if (method == "Execute")
                {
                    ctx.Log.Info($"Provider reports {context.RecordsSucceeded.ToString("N0")} successfully exported record(s) of type {ctx.Request.Provider.Value.EntityType.ToString()}.");

                    foreach (var unit in context.ExtraDataUnits.Where(x => x.RelatedType.HasValue))
                    {
                        StreamToFile(ctx, unit.DataStream, Path.Combine(context.Folder, unit.FileName), x => unit.DataStream = null);

                        ctx.Log.Info($"Provider reports {unit.RecordsSucceeded.ToString("N0")} successfully exported record(s) of type {unit.RelatedType.Value.ToString()}.");
                    }
                }
			}

			return context.Abort != DataExchangeAbortion.Hard;
		}

		private bool Deploy(DataExporterContext ctx, string zipPath)
		{
			var allSucceeded = true;
			var deployments = ctx.Request.Profile.Deployments.OrderBy(x => x.DeploymentTypeId).Where(x => x.Enabled);

			if (deployments.Count() == 0)
				return false;

			var context = new ExportDeploymentContext
			{
				T = T,
				Log = ctx.Log,
				FolderContent = ctx.FolderContent,
				ZipPath = zipPath,
				CreateZipArchive = ctx.Request.Profile.CreateZipArchive
			};			

			foreach (var deployment in deployments)
			{
				IFilePublisher publisher = null;

				context.Result = new DataDeploymentResult
				{
					LastExecutionUtc = DateTime.UtcNow
				};

				try
				{
					switch (deployment.DeploymentType)
					{
						case ExportDeploymentType.Email:
							publisher = new EmailFilePublisher(_emailAccountService.Value, _queuedEmailService.Value);
							break;
						case ExportDeploymentType.FileSystem:
							publisher = new FileSystemFilePublisher();
							break;
						case ExportDeploymentType.Ftp:
							publisher = new FtpFilePublisher();
							break;
						case ExportDeploymentType.Http:
							publisher = new HttpFilePublisher();
							break;
						case ExportDeploymentType.PublicFolder:
							publisher = new PublicFolderPublisher();
							break;
					}

					if (publisher != null)
					{
						publisher.Publish(context, deployment);

						if (!context.Result.Succeeded)
							allSucceeded = false;
					}
				}
				catch (Exception ex)
				{
					allSucceeded = false;

					if (context.Result != null)
					{
						context.Result.LastError = ex.ToAllMessages();
					}

					ctx.Log.ErrorFormat(ex, "Deployment \"{0}\" of type {1} failed", deployment.Name, deployment.DeploymentType.ToString());
				}

				deployment.ResultInfo = XmlHelper.Serialize(context.Result);

				_exportProfileService.Value.UpdateExportDeployment(deployment);
			}

			return allSucceeded;
		}

		private void SendCompletionEmail(DataExporterContext ctx, string zipPath)
		{
			var emailAccount = _emailAccountService.Value.GetEmailAccountById(ctx.Request.Profile.EmailAccountId);
			if (emailAccount == null)
			{
				return;
			}

			var downloadUrl = "{0}Admin/Export/DownloadExportFile/{1}?name=".FormatInvariant(_services.WebHelper.GetStoreLocation(ctx.Store.SslEnabled), ctx.Request.Profile.Id);
			var languageId = ctx.Projection.LanguageId ?? 0;
			var smtpContext = new SmtpContext(emailAccount);
			var message = new EmailMessage();

			var storeInfo = "{0} ({1})".FormatInvariant(ctx.Store.Name, ctx.Store.Url);
			var intro =_services.Localization.GetResource("Admin.DataExchange.Export.CompletedEmail.Body", languageId).FormatInvariant(storeInfo);
			var body = new StringBuilder(intro);

			if (ctx.Result.LastError.HasValue())
			{
				body.AppendFormat("<p style=\"color: #B94A48;\">{0}</p>", ctx.Result.LastError);
			}

			if (ctx.IsFileBasedExport && File.Exists(zipPath))
			{
				var fileName = Path.GetFileName(zipPath);
				body.AppendFormat("<p><a href='{0}{1}' download>{2}</a></p>", downloadUrl, HttpUtility.UrlEncode(fileName), fileName);
			}

			if (ctx.IsFileBasedExport && ctx.Result.Files.Any())
			{
				body.Append("<p>");
				foreach (var file in ctx.Result.Files)
				{
					body.AppendFormat("<div><a href='{0}{1}' download>{2}</a></div>", downloadUrl, HttpUtility.UrlEncode(file.FileName), file.FileName);
				}
				body.Append("</p>");
			}

			message.From = new EmailAddress(emailAccount.Email, emailAccount.DisplayName);

			if (ctx.Request.Profile.CompletedEmailAddresses.HasValue())
				message.To.AddRange(ctx.Request.Profile.CompletedEmailAddresses.SplitSafe(",").Where(x => x.IsEmail()).Select(x => new EmailAddress(x)));

			if (message.To.Count == 0 && _contactDataSettings.Value.CompanyEmailAddress.HasValue())
				message.To.Add(new EmailAddress(_contactDataSettings.Value.CompanyEmailAddress));

			if (message.To.Count == 0)
				message.To.Add(new EmailAddress(emailAccount.Email, emailAccount.DisplayName));

			message.Subject = _services.Localization.GetResource("Admin.DataExchange.Export.CompletedEmail.Subject", languageId)
				.FormatInvariant(ctx.Request.Profile.Name);

			message.Body = body.ToString();

			_emailSender.Value.SendEmail(smtpContext, message);

			//_queuedEmailService.Value.InsertQueuedEmail(new QueuedEmail
			//{
			//	From = emailAccount.Email,
			//	FromName = emailAccount.DisplayName,
			//	To = message.To.First().Address,
			//	Subject = message.Subject,
			//	Body = message.Body,
			//	CreatedOnUtc = DateTime.UtcNow,
			//	EmailAccountId = emailAccount.Id,
			//	SendManually = true
			//});
			//_dbContext.SaveChanges();
		}

		#endregion

		#region Getting data

		public virtual ProductExportContext CreateProductExportContext(
			IEnumerable<Product> products = null,
			Customer customer = null,
			int? storeId = null,
			int? maxPicturesPerProduct = null,
			bool showHidden = true)
		{
			if (customer == null)
				customer = _services.WorkContext.CurrentCustomer;

			if (!storeId.HasValue)
				storeId = _services.StoreContext.CurrentStore.Id;

			var context = new ProductExportContext(products,
				x => _productAttributeService.Value.GetProductVariantAttributesByProductIds(x, null),
				x => _productAttributeService.Value.GetProductVariantAttributeCombinations(x),
				x => _specificationAttributeService.Value.GetProductSpecificationAttributesByProductIds(x),
				x => _productService.Value.GetTierPricesByProductIds(x, customer, storeId.GetValueOrDefault()),
				x => _categoryService.Value.GetProductCategoriesByProductIds(x, null, showHidden),
				x => _manufacturerService.Value.GetProductManufacturersByProductIds(x),
				x => _productService.Value.GetAppliedDiscountsByProductIds(x),
				x => _productService.Value.GetBundleItemsByProductIds(x, showHidden),
                x => _productService.Value.GetAssociatedProductsByProductIds(x),
				x => _pictureService.Value.GetPicturesByProductIds(x, maxPicturesPerProduct, true),
				x => _productService.Value.GetProductPicturesByProductIds(x),
				x => _productService.Value.GetProductTagsByProductIds(x),
                x => _downloadService.Value.GetDownloadsByEntityIds(x, nameof(Product))
			);

			return context;
		}

		private IQueryable<Product> GetProductQuery(DataExporterContext ctx, int skip, int take)
		{
			IQueryable<Product> query = null;

			if (ctx.Request.ProductQuery == null)
			{
				var f = ctx.Filter;
				var createdFrom = f.CreatedFrom.HasValue ? (DateTime?)_services.DateTimeHelper.ConvertToUtcTime(f.CreatedFrom.Value, _services.DateTimeHelper.CurrentTimeZone) : null;
				var createdTo = f.CreatedTo.HasValue ? (DateTime?)_services.DateTimeHelper.ConvertToUtcTime(f.CreatedTo.Value, _services.DateTimeHelper.CurrentTimeZone) : null;

				var searchQuery = new CatalogSearchQuery()
					.WithCurrency(ctx.ContextCurrency)
					.WithLanguage(ctx.ContextLanguage)
					.HasStoreId(ctx.Request.Profile.PerStore ? ctx.Store.Id : f.StoreId)
					.VisibleIndividuallyOnly(true)
					.PriceBetween(f.PriceMinimum, f.PriceMaximum)
					.WithStockQuantity(f.AvailabilityMinimum, f.AvailabilityMaximum)
					.CreatedBetween(createdFrom, createdTo);

				if (f.IsPublished.HasValue)
					searchQuery = searchQuery.PublishedOnly(f.IsPublished.Value);

				if (f.ProductType.HasValue)
					searchQuery = searchQuery.IsProductType(f.ProductType.Value);

				if (f.ProductTagId.HasValue)
					searchQuery = searchQuery.WithProductTagIds(f.ProductTagId.Value);

				if (f.WithoutManufacturers.HasValue)
					searchQuery = searchQuery.HasAnyManufacturer(!f.WithoutManufacturers.Value);
				else if (f.ManufacturerId.HasValue)
					searchQuery = searchQuery.WithManufacturerIds(f.FeaturedProducts, f.ManufacturerId.Value);

				if (f.WithoutCategories.HasValue)
					searchQuery = searchQuery.HasAnyCategory(!f.WithoutCategories.Value);
				else if (f.CategoryIds != null && f.CategoryIds.Length > 0)
					searchQuery = searchQuery.WithCategoryIds(f.FeaturedProducts, f.CategoryIds);

				if (ctx.Request.EntitiesToExport.Count > 0)
					searchQuery = searchQuery.WithProductIds(ctx.Request.EntitiesToExport.ToArray());
				else
					searchQuery = searchQuery.WithProductId(f.IdMinimum, f.IdMaximum);

				query = _catalogSearchService.Value.PrepareQuery(searchQuery);
				query = query.OrderByDescending(x => x.CreatedOnUtc);
			}
			else
			{
				query = ctx.Request.ProductQuery;
			}

			if (skip > 0)
				query = query.Skip(() => skip);

			if (take != int.MaxValue)
				query = query.Take(() => take);

			return query;
		}

		private List<Product> GetProducts(DataExporterContext ctx, int skip)
		{
			// We use ctx.EntityIdsPerSegment to avoid exporting products multiple times per segment\file (cause of associated products).
			var result = new List<Product>();
			var products = GetProductQuery(ctx, skip, PageSize).ToList();
            Multimap<int, Product> associatedProducts = null;

            if (ctx.Projection.NoGroupedProducts)
            {
                var groupedProductIds = products.Where(x => x.ProductType == ProductType.GroupedProduct).Select(x => x.Id).ToArray();
                associatedProducts = _productService.Value.GetAssociatedProductsByProductIds(groupedProductIds, true);
            }

			foreach (var product in products)
			{
				if (product.ProductType == ProductType.SimpleProduct || product.ProductType == ProductType.BundledProduct)
				{
					if (!ctx.EntityIdsPerSegment.Contains(product.Id))
					{
						result.Add(product);
						ctx.EntityIdsPerSegment.Add(product.Id);
					}
				}
				else if (product.ProductType == ProductType.GroupedProduct)
				{
					if (ctx.Projection.NoGroupedProducts)
					{
                        if (associatedProducts.ContainsKey(product.Id))
                        {
                            foreach (var associatedProduct in associatedProducts[product.Id])
                            {
                                if (ctx.Projection.OnlyIndividuallyVisibleAssociated && !associatedProduct.VisibleIndividually)
                                {
                                    continue;
                                }
                                if (ctx.Filter.IsPublished.HasValue && ctx.Filter.IsPublished.Value != associatedProduct.Published)
                                {
                                    continue;
                                }

                                if (!ctx.EntityIdsPerSegment.Contains(associatedProduct.Id))
                                {
                                    result.Add(associatedProduct);
                                    ctx.EntityIdsPerSegment.Add(associatedProduct.Id);
                                }
                            }
                        }
					}
					else
					{
						if (!ctx.EntityIdsPerSegment.Contains(product.Id))
						{
							result.Add(product);
							ctx.EntityIdsPerSegment.Add(product.Id);
						}
					}
				}
			}

			SetProgress(ctx, products.Count);

			return result;
		}

		private IQueryable<Order> GetOrderQuery(DataExporterContext ctx, int skip, int take)
		{
			var query = _orderService.Value.GetOrders(
				ctx.Request.Profile.PerStore ? ctx.Store.Id : ctx.Filter.StoreId,
				ctx.Projection.CustomerId ?? 0,
				ctx.Filter.CreatedFrom.HasValue ? (DateTime?)_services.DateTimeHelper.ConvertToUtcTime(ctx.Filter.CreatedFrom.Value, _services.DateTimeHelper.CurrentTimeZone) : null,
				ctx.Filter.CreatedTo.HasValue ? (DateTime?)_services.DateTimeHelper.ConvertToUtcTime(ctx.Filter.CreatedTo.Value, _services.DateTimeHelper.CurrentTimeZone) : null,
				ctx.Filter.OrderStatusIds,
				ctx.Filter.PaymentStatusIds,
				ctx.Filter.ShippingStatusIds,
				null,
				null,
				null);

			if (ctx.Request.EntitiesToExport.Any())
				query = query.Where(x => ctx.Request.EntitiesToExport.Contains(x.Id));

			query = query.OrderByDescending(x => x.CreatedOnUtc);

			if (skip > 0)
				query = query.Skip(() => skip);

			if (take != int.MaxValue)
				query = query.Take(() => take);

			return query;
		}

		private List<Order> GetOrders(DataExporterContext ctx, int skip)
		{
			var orders = GetOrderQuery(ctx, skip, PageSize).ToList();

			if (ctx.Projection.OrderStatusChange != ExportOrderStatusChange.None)
			{
				ctx.SetLoadedEntityIds(orders.Select(x => x.Id));
			}

			SetProgress(ctx, orders.Count);

			return orders;
		}

		private IQueryable<Manufacturer> GetManufacturerQuery(DataExporterContext ctx, int skip, int take)
		{
			var storeId = ctx.Request.Profile.PerStore ? ctx.Store.Id : 0;
			var query = _manufacturerService.Value.GetManufacturers(true, storeId);

			if (ctx.Request.EntitiesToExport.Any())
				query = query.Where(x => ctx.Request.EntitiesToExport.Contains(x.Id));

			query = query.OrderBy(x => x.DisplayOrder);

			if (skip > 0)
				query = query.Skip(() => skip);

			if (take != int.MaxValue)
				query = query.Take(() => take);

			return query;
		}

		private List<Manufacturer> GetManufacturers(DataExporterContext ctx, int skip)
		{
			var manus = GetManufacturerQuery(ctx, skip, PageSize).ToList();

			SetProgress(ctx, manus.Count);

			return manus;
		}

		private IQueryable<Category> GetCategoryQuery(DataExporterContext ctx, int skip, int take)
		{
			var storeId = ctx.Request.Profile.PerStore ? ctx.Store.Id : 0;
			var query = _categoryService.Value.BuildCategoriesQuery(null, true, null, storeId);

			if (ctx.Request.EntitiesToExport.Any())
				query = query.Where(x => ctx.Request.EntitiesToExport.Contains(x.Id));

			query = query
				.OrderBy(x => x.ParentCategoryId)
				.ThenBy(x => x.DisplayOrder);

			if (skip > 0)
				query = query.Skip(() => skip);

			if (take != int.MaxValue)
				query = query.Take(() => take);

			return query;
		}

		private List<Category> GetCategories(DataExporterContext ctx, int skip)
		{
			var categories = GetCategoryQuery(ctx, skip, PageSize).ToList();

			SetProgress(ctx, categories.Count);

			return categories;
		}

		private IQueryable<Customer> GetCustomerQuery(DataExporterContext ctx, int skip, int take)
		{
			var query = _customerRepository.Value.TableUntracked
				.Expand(x => x.BillingAddress)
				.Expand(x => x.ShippingAddress)
				.Expand(x => x.Addresses.Select(y => y.Country))
				.Expand(x => x.Addresses.Select(y => y.StateProvince))
				.Expand(x => x.CustomerRoles)
				.Where(x => !x.Deleted);

			if (ctx.Filter.IsActiveCustomer.HasValue)
				query = query.Where(x => x.Active == ctx.Filter.IsActiveCustomer.Value);

			if (ctx.Filter.IsTaxExempt.HasValue)
				query = query.Where(x => x.IsTaxExempt == ctx.Filter.IsTaxExempt.Value);

			if (ctx.Filter.CustomerRoleIds != null && ctx.Filter.CustomerRoleIds.Length > 0)
				query = query.Where(x => x.CustomerRoles.Select(y => y.Id).Intersect(ctx.Filter.CustomerRoleIds).Any());

			if (ctx.Filter.BillingCountryIds != null && ctx.Filter.BillingCountryIds.Length > 0)
				query = query.Where(x => x.BillingAddress != null && ctx.Filter.BillingCountryIds.Contains(x.BillingAddress.Id));

			if (ctx.Filter.ShippingCountryIds != null && ctx.Filter.ShippingCountryIds.Length > 0)
				query = query.Where(x => x.ShippingAddress != null && ctx.Filter.ShippingCountryIds.Contains(x.ShippingAddress.Id));

			if (ctx.Filter.LastActivityFrom.HasValue)
			{
				var activityFrom = _services.DateTimeHelper.ConvertToUtcTime(ctx.Filter.LastActivityFrom.Value, _services.DateTimeHelper.CurrentTimeZone);
				query = query.Where(x => activityFrom <= x.LastActivityDateUtc);
			}

			if (ctx.Filter.LastActivityTo.HasValue)
			{
				var activityTo = _services.DateTimeHelper.ConvertToUtcTime(ctx.Filter.LastActivityTo.Value, _services.DateTimeHelper.CurrentTimeZone);
				query = query.Where(x => activityTo >= x.LastActivityDateUtc);
			}

			if (ctx.Filter.HasSpentAtLeastAmount.HasValue)
			{
				query = query
					.Join(_orderRepository.Value.Table, x => x.Id, y => y.CustomerId, (x, y) => new { Customer = x, Order = y })
					.GroupBy(x => x.Customer.Id)
					.Select(x => new
					{
						Customer = x.FirstOrDefault().Customer,
						OrderTotal = x.Sum(y => y.Order.OrderTotal)
					})
					.Where(x => x.OrderTotal >= ctx.Filter.HasSpentAtLeastAmount.Value)
					.Select(x => x.Customer);
			}

			if (ctx.Filter.HasPlacedAtLeastOrders.HasValue)
			{
				query = query
					.Join(_orderRepository.Value.Table, x => x.Id, y => y.CustomerId, (x, y) => new { Customer = x, Order = y })
					.GroupBy(x => x.Customer.Id)
					.Select(x => new
					{
						Customer = x.FirstOrDefault().Customer,
						OrderCount = x.Count()
					})
					.Where(x => x.OrderCount >= ctx.Filter.HasPlacedAtLeastOrders.Value)
					.Select(x => x.Customer);
			}

			if (ctx.Request.EntitiesToExport.Any())
				query = query.Where(x => ctx.Request.EntitiesToExport.Contains(x.Id));

			query = query.OrderByDescending(x => x.CreatedOnUtc);

			if (skip > 0)
				query = query.Skip(() => skip);

			if (take != int.MaxValue)
				query = query.Take(() => take);

			return query;
		}

		private List<Customer> GetCustomers(DataExporterContext ctx, int skip)
		{
			var customers = GetCustomerQuery(ctx, skip, PageSize).ToList();

			SetProgress(ctx, customers.Count);

			return customers;
		}

		private IQueryable<NewsLetterSubscription> GetNewsLetterSubscriptionQuery(DataExporterContext ctx, int skip, int take)
		{
			var storeId = (ctx.Request.Profile.PerStore ? ctx.Store.Id : ctx.Filter.StoreId);

			var query = _subscriptionRepository.Value.TableUntracked;

			if (storeId > 0)
				query = query.Where(x => x.StoreId == storeId);

			if (ctx.Filter.IsActiveSubscriber.HasValue)
				query = query.Where(x => x.Active == ctx.Filter.IsActiveSubscriber.Value);

            if (ctx.Filter.WorkingLanguageId != null && ctx.Filter.WorkingLanguageId != 0)
            {
                var defaultLanguage = _languageService.Value.GetAllLanguages().FirstOrDefault();
                var isDefaultLanguage = ctx.Filter.WorkingLanguageId == defaultLanguage.Id;

                if (isDefaultLanguage)
                {
                    query = query.Where(x => x.WorkingLanguageId == 0 || x.WorkingLanguageId == ctx.Filter.WorkingLanguageId);
                }
                else
                {
                    query = query.Where(x => x.WorkingLanguageId == ctx.Filter.WorkingLanguageId);
                }
            }

            if (ctx.Filter.CreatedFrom.HasValue)
			{
				var createdFrom = _services.DateTimeHelper.ConvertToUtcTime(ctx.Filter.CreatedFrom.Value, _services.DateTimeHelper.CurrentTimeZone);
				query = query.Where(x => createdFrom <= x.CreatedOnUtc);
			}

			if (ctx.Filter.CreatedTo.HasValue)
			{
				var createdTo = _services.DateTimeHelper.ConvertToUtcTime(ctx.Filter.CreatedTo.Value, _services.DateTimeHelper.CurrentTimeZone);
				query = query.Where(x => createdTo >= x.CreatedOnUtc);
			}

			if (ctx.Request.EntitiesToExport.Any())
				query = query.Where(x => ctx.Request.EntitiesToExport.Contains(x.Id));

			query = query
				.OrderBy(x => x.StoreId)
				.ThenBy(x => x.Email);

			if (skip > 0)
				query = query.Skip(() => skip);

			if (take != int.MaxValue)
				query = query.Take(() => take);

			return query;
		}

		private List<NewsLetterSubscription> GetNewsLetterSubscriptions(DataExporterContext ctx, int skip)
		{
			var subscriptions = GetNewsLetterSubscriptionQuery(ctx, skip, PageSize).ToList();

			SetProgress(ctx, subscriptions.Count);

			return subscriptions;
		}

		private IQueryable<ShoppingCartItem> GetShoppingCartItemQuery(DataExporterContext ctx, int skip, int take)
		{
			var storeId = (ctx.Request.Profile.PerStore ? ctx.Store.Id : ctx.Filter.StoreId);

			var query = _shoppingCartItemRepository.Value.TableUntracked
				.Expand(x => x.Customer)
				.Expand(x => x.Customer.CustomerRoles)
				.Expand(x => x.Product)
				.Where(x => !x.Customer.Deleted);   //  && !x.Product.Deleted

			if (storeId > 0)
				query = query.Where(x => x.StoreId == storeId);

			if (ctx.Request.ActionOrigin.IsCaseInsensitiveEqual("CurrentCarts"))
			{
				query = query.Where(x => x.ShoppingCartTypeId == (int)ShoppingCartType.ShoppingCart);
			}
			else if (ctx.Request.ActionOrigin.IsCaseInsensitiveEqual("CurrentWishlists"))
			{
				query = query.Where(x => x.ShoppingCartTypeId == (int)ShoppingCartType.Wishlist);
			}
			else if (ctx.Filter.ShoppingCartTypeId.HasValue)
			{
				query = query.Where(x => x.ShoppingCartTypeId == ctx.Filter.ShoppingCartTypeId.Value);
			}

			if (ctx.Filter.IsActiveCustomer.HasValue)
				query = query.Where(x => x.Customer.Active == ctx.Filter.IsActiveCustomer.Value);

			if (ctx.Filter.IsTaxExempt.HasValue)
				query = query.Where(x => x.Customer.IsTaxExempt == ctx.Filter.IsTaxExempt.Value);

			if (ctx.Filter.CustomerRoleIds != null && ctx.Filter.CustomerRoleIds.Length > 0)
				query = query.Where(x => x.Customer.CustomerRoles.Select(y => y.Id).Intersect(ctx.Filter.CustomerRoleIds).Any());

			if (ctx.Filter.LastActivityFrom.HasValue)
			{
				var activityFrom = _services.DateTimeHelper.ConvertToUtcTime(ctx.Filter.LastActivityFrom.Value, _services.DateTimeHelper.CurrentTimeZone);
				query = query.Where(x => activityFrom <= x.Customer.LastActivityDateUtc);
			}

			if (ctx.Filter.LastActivityTo.HasValue)
			{
				var activityTo = _services.DateTimeHelper.ConvertToUtcTime(ctx.Filter.LastActivityTo.Value, _services.DateTimeHelper.CurrentTimeZone);
				query = query.Where(x => activityTo >= x.Customer.LastActivityDateUtc);
			}

			if (ctx.Filter.CreatedFrom.HasValue)
			{
				var createdFrom = _services.DateTimeHelper.ConvertToUtcTime(ctx.Filter.CreatedFrom.Value, _services.DateTimeHelper.CurrentTimeZone);
				query = query.Where(x => createdFrom <= x.CreatedOnUtc);
			}

			if (ctx.Filter.CreatedTo.HasValue)
			{
				var createdTo = _services.DateTimeHelper.ConvertToUtcTime(ctx.Filter.CreatedTo.Value, _services.DateTimeHelper.CurrentTimeZone);
				query = query.Where(x => createdTo >= x.CreatedOnUtc);
			}

			if (ctx.Projection.NoBundleProducts)
			{
				query = query.Where(x => x.Product.ProductTypeId != (int)ProductType.BundledProduct);
			}
			else
			{
				query = query.Where(x => x.BundleItemId == null);
			}

			if (ctx.Request.EntitiesToExport.Any())
				query = query.Where(x => ctx.Request.EntitiesToExport.Contains(x.Id));

			query = query
				.OrderBy(x => x.ShoppingCartTypeId)
				.ThenBy(x => x.CustomerId)
				.ThenByDescending(x => x.CreatedOnUtc);

			if (skip > 0)
				query = query.Skip(skip);

			if (take != int.MaxValue)
				query = query.Take(take);

			return query;
		}

		private List<ShoppingCartItem> GetShoppingCartItems(DataExporterContext ctx, int skip)
		{
			var shoppingCartItems = GetShoppingCartItemQuery(ctx, skip, PageSize).ToList();

			SetProgress(ctx, shoppingCartItems.Count);

			return shoppingCartItems;
		}

		#endregion

		private List<Store> Init(DataExporterContext ctx, int? totalRecords = null)
		{
			// Init things that are required for export and for preview.
            // Init things that are only required for export in ExportCoreOuter.
			List<Store> result = null;

			ctx.ContextCurrency = _currencyService.Value.GetCurrencyById(ctx.Projection.CurrencyId ?? 0) ?? _services.WorkContext.WorkingCurrency;
			ctx.ContextCustomer = _customerService.GetCustomerById(ctx.Projection.CustomerId ?? 0) ?? _services.WorkContext.CurrentCustomer;
			ctx.ContextLanguage = _languageService.Value.GetLanguageById(ctx.Projection.LanguageId ?? 0) ?? _services.WorkContext.WorkingLanguage;

			ctx.Stores = _services.StoreService.GetAllStores().ToDictionary(x => x.Id, x => x);
			ctx.Languages = _languageService.Value.GetAllLanguages(true).ToDictionary(x => x.Id, x => x);

            if (ctx.IsPreview)
            {
                ctx.Translations[nameof(Currency)] = new LocalizedPropertyCollection(nameof(Currency), null, Enumerable.Empty<LocalizedProperty>());
                ctx.Translations[nameof(Country)] = new LocalizedPropertyCollection(nameof(Country), null, Enumerable.Empty<LocalizedProperty>());
                ctx.Translations[nameof(StateProvince)] = new LocalizedPropertyCollection(nameof(StateProvince), null, Enumerable.Empty<LocalizedProperty>());
                ctx.Translations[nameof(DeliveryTime)] = new LocalizedPropertyCollection(nameof(DeliveryTime), null, Enumerable.Empty<LocalizedProperty>());
                ctx.Translations[nameof(QuantityUnit)] = new LocalizedPropertyCollection(nameof(QuantityUnit), null, Enumerable.Empty<LocalizedProperty>());
                ctx.Translations[nameof(Manufacturer)] = new LocalizedPropertyCollection(nameof(Manufacturer), null, Enumerable.Empty<LocalizedProperty>());
                ctx.Translations[nameof(Category)] = new LocalizedPropertyCollection(nameof(Category), null, Enumerable.Empty<LocalizedProperty>());
            }
            else
            {
                ctx.Translations[nameof(Currency)] = _localizedEntityService.GetLocalizedPropertyCollection(nameof(Currency), null);
                ctx.Translations[nameof(Country)] = _localizedEntityService.GetLocalizedPropertyCollection(nameof(Country), null);
                ctx.Translations[nameof(StateProvince)] = _localizedEntityService.GetLocalizedPropertyCollection(nameof(StateProvince), null);
                ctx.Translations[nameof(DeliveryTime)] = _localizedEntityService.GetLocalizedPropertyCollection(nameof(DeliveryTime), null);
                ctx.Translations[nameof(QuantityUnit)] = _localizedEntityService.GetLocalizedPropertyCollection(nameof(QuantityUnit), null);
                ctx.Translations[nameof(Manufacturer)] = _localizedEntityService.GetLocalizedPropertyCollection(nameof(Manufacturer), null);
                ctx.Translations[nameof(Category)] = _localizedEntityService.GetLocalizedPropertyCollection(nameof(Category), null);
            }

            if (!ctx.IsPreview && ctx.Request.Profile.PerStore)
			{
				result = new List<Store>(ctx.Stores.Values.Where(x => x.Id == ctx.Filter.StoreId || ctx.Filter.StoreId == 0));
			}
			else
			{
				int? storeId = ctx.Filter.StoreId == 0 ? ctx.Projection.StoreId : ctx.Filter.StoreId;
				ctx.Store = ctx.Stores.Values.FirstOrDefault(x => x.Id == (storeId ?? _services.StoreContext.CurrentStore.Id));

				result = new List<Store> { ctx.Store };
			}

			// Get total records for progress.
			foreach (var store in result)
			{
				ctx.Store = store;

				int totalCount = 0;

				if (totalRecords.HasValue)
				{
                    // Speed up preview by not counting total at each page.
                    totalCount = totalRecords.Value;
				}
				else
				{
					switch (ctx.Request.Provider.Value.EntityType)
					{
						case ExportEntityType.Product:
							totalCount = GetProductQuery(ctx, ctx.Request.Profile.Offset, int.MaxValue).Count();
							break;
						case ExportEntityType.Order:
							totalCount = GetOrderQuery(ctx, ctx.Request.Profile.Offset, int.MaxValue).Count();
							break;
						case ExportEntityType.Manufacturer:
							totalCount = GetManufacturerQuery(ctx, ctx.Request.Profile.Offset, int.MaxValue).Count();
							break;
						case ExportEntityType.Category:
							totalCount = GetCategoryQuery(ctx, ctx.Request.Profile.Offset, int.MaxValue).Count();
							break;
						case ExportEntityType.Customer:
							totalCount = GetCustomerQuery(ctx, ctx.Request.Profile.Offset, int.MaxValue).Count();
							break;
						case ExportEntityType.NewsLetterSubscription:
							totalCount = GetNewsLetterSubscriptionQuery(ctx, ctx.Request.Profile.Offset, int.MaxValue).Count();
							break;
						case ExportEntityType.ShoppingCartItem:
							totalCount = GetShoppingCartItemQuery(ctx, ctx.Request.Profile.Offset, int.MaxValue).Count();
							break;
					}
				}

				ctx.RecordsPerStore.Add(store.Id, totalCount);
			}

			return result;
		}

		private void ExportCoreInner(DataExporterContext ctx, Store store)
		{
            var context = ctx.ExecuteContext;
            var profile = ctx.Request.Profile;
            var provider = ctx.Request.Provider;

            if (context.Abort != DataExchangeAbortion.None)
            {
                return;
            }

			{
				var logHead = new StringBuilder();
				logHead.AppendLine();
				logHead.AppendLine(new string('-', 40));
				logHead.AppendLine("SmartStore.NET: v." + SmartStoreVersion.CurrentFullVersion);
				logHead.Append("Export profile: " + profile.Name);
				logHead.AppendLine(profile.Id == 0 ? " (volatile)" : $" (Id {profile.Id})");

                if (provider.Metadata.FriendlyName.HasValue())
                {
                    logHead.AppendLine($"Export provider: {provider.Metadata.FriendlyName} ({profile.ProviderSystemName})");
                }
                else
                {
                    logHead.AppendLine("Export provider: " + profile.ProviderSystemName);
                }

				var plugin = provider.Metadata.PluginDescriptor;
				logHead.Append("Plugin: ");
				logHead.AppendLine(plugin == null ? "".NaIfEmpty() : $"{plugin.FriendlyName} ({plugin.SystemName}) v.{plugin.Version.ToString()}");
				logHead.AppendLine("Entity: " + provider.Value.EntityType.ToString());

				try
				{
					var uri = new Uri(store.Url);
					logHead.AppendLine($"Store: {uri.DnsSafeHost.NaIfEmpty()} (Id {store.Id})");
				}
				catch {	}

				var customer = _services.WorkContext.CurrentCustomer;
				logHead.Append("Executed by: " + (customer.Email.HasValue() ? customer.Email : customer.SystemName));

				ctx.Log.Info(logHead.ToString());
			}

            var dataExchangeSettings = _services.Settings.LoadSetting<DataExchangeSettings>(store.Id);
            var publicDeployment = profile.Deployments.FirstOrDefault(x => x.DeploymentType == ExportDeploymentType.PublicFolder);

            ctx.Store = store;
            context.FileIndex = 0;
            context.Store = ToDynamic(ctx, ctx.Store);
			context.MaxFileNameLength = dataExchangeSettings.MaxFileNameLength;
			context.HasPublicDeployment = publicDeployment != null;
			context.PublicFolderPath = publicDeployment.GetDeploymentFolder(true);
			context.PublicFolderUrl = publicDeployment.GetPublicFolderUrl(_services, ctx.Store);

			var fileExtension = provider.Value.FileExtension.HasValue() ? provider.Value.FileExtension.ToLower().EnsureStartsWith(".") : "";

			using (var segmenter = CreateSegmenter(ctx))
			{
				if (segmenter == null)
				{
					throw new SmartException($"Unsupported entity type '{provider.Value.EntityType.ToString()}'.");
				}

				if (segmenter.TotalRecords <= 0)
				{
					ctx.Log.Info("There are no records to export.");
				}

				while (context.Abort == DataExchangeAbortion.None && segmenter.HasData)
				{
					segmenter.RecordPerSegmentCount = 0;
					context.RecordsSucceeded = 0;

					string path = null;

					if (ctx.IsFileBasedExport)
					{
                        context.FileIndex = context.FileIndex + 1;
                        context.FileName = profile.ResolveFileNamePattern(ctx.Store, context.FileIndex, context.MaxFileNameLength) + fileExtension;
						path = Path.Combine(context.Folder, context.FileName);

                        if (profile.ExportRelatedData && ctx.Supports(ExportFeatures.UsesRelatedDataUnits))
                        {
                            context.ExtraDataUnits.AddRange(GetDataUnitsForRelatedEntities(ctx));
                        }
					}

					if (CallProvider(ctx, "Execute", path))
					{
						if (ctx.IsFileBasedExport && File.Exists(path))
						{
							ctx.Result.Files.Add(new DataExportResult.ExportFileInfo
							{
								StoreId = ctx.Store.Id,
								FileName = context.FileName
                            });
                        }
					}

                    ctx.EntityIdsPerSegment.Clear();
                    DetachAllEntitiesAndClear(ctx);
                    _localizedEntityService.ClearCache();

                    if (context.IsMaxFailures)
                    {
                        ctx.Log.Warn("Export aborted. The maximum number of failures has been reached.");
                    }
                    if (ctx.CancellationToken.IsCancellationRequested)
                    {
                        ctx.Log.Warn("Export aborted. A cancellation has been requested.");
                    }
				}

				if (context.Abort != DataExchangeAbortion.Hard)
				{
                    var calledExecuted = false;

					context.ExtraDataUnits.ForEach(x =>
					{
                        context.DataStreamId = x.Id;

                        var success = true;
                        var path = x.FileName.HasValue() ? Path.Combine(context.Folder, x.FileName) : null;
                        if (!x.RelatedType.HasValue)
                        {
                            calledExecuted = true;
                            success = CallProvider(ctx, "OnExecuted", path);
                        }

                        if (success && ctx.IsFileBasedExport && x.DisplayInFileDialog && File.Exists(path))
                        {
                            // Save info about extra file.
                            ctx.Result.Files.Add(new DataExportResult.ExportFileInfo
                            {
                                StoreId = ctx.Store.Id,
                                FileName = x.FileName,
                                Label = x.Label,
                                RelatedType = x.RelatedType
                            });
                        }
                    });

                    if (!calledExecuted)
                    {
                        // Always call OnExecuted.
                        CallProvider(ctx, "OnExecuted", null);
                    }
                }

                context.ExtraDataUnits.Clear();
            }
		}

		private void ExportCoreOuter(DataExporterContext ctx)
		{
            var profile = ctx.Request.Profile;
            if (profile == null || !profile.Enabled)
            {
                return;
            }

			var logPath = profile.GetExportLogPath();
			var zipPath = profile.GetExportZipPath();

            FileSystemHelper.Delete(logPath);
			FileSystemHelper.Delete(zipPath);
			FileSystemHelper.ClearDirectory(ctx.FolderContent, false);

			using (var logger = new TraceLogger(logPath))
			{
				try
				{
					ctx.Log = logger;
					ctx.ExecuteContext.Log = logger;
					ctx.ProgressInfo = T("Admin.DataExchange.Export.ProgressInfo");

					if (!ctx.Request.Provider.IsValid())
						throw new SmartException("Export aborted because the export provider is not valid.");

					if (!HasPermission(ctx))
						throw new SmartException("You do not have permission to perform the selected export.");

					foreach (var item in ctx.Request.CustomData)
					{
						ctx.ExecuteContext.CustomProperties.Add(item.Key, item.Value);
					}

					if (profile.ProviderConfigData.HasValue())
					{
						var configInfo = ctx.Request.Provider.Value.ConfigurationInfo;
						if (configInfo != null)
						{
							ctx.ExecuteContext.ConfigurationData = XmlHelper.Deserialize(profile.ProviderConfigData, configInfo.ModelType);
						}
					}

                    // lazyLoading: false, proxyCreation: false impossible due to price calculation.
                    using (var scope = new DbContextScope(_dbContext, autoDetectChanges: false, proxyCreation: true, validateOnSave: false, forceNoTracking: true))
					{
                        // Init things required for export and not required for preview.
						ctx.DeliveryTimes = _deliveryTimeService.Value.GetAllDeliveryTimes().ToDictionary(x => x.Id);
						ctx.QuantityUnits = _quantityUnitService.Value.GetAllQuantityUnits().ToDictionary(x => x.Id);
						ctx.ProductTemplates = _productTemplateService.Value.GetAllProductTemplates().ToDictionary(x => x.Id, x => x.ViewPath);
						ctx.CategoryTemplates = _categoryTemplateService.Value.GetAllCategoryTemplates().ToDictionary(x => x.Id, x => x.ViewPath);

                        if (ctx.Request.Provider.Value.EntityType == ExportEntityType.Product ||
							ctx.Request.Provider.Value.EntityType == ExportEntityType.Order)
						{
							ctx.Countries = _countryService.Value.GetAllCountries(true).ToDictionary(x => x.Id, x => x);
						}

						if (ctx.Request.Provider.Value.EntityType == ExportEntityType.Customer)
						{
							var subscriptionEmails = _subscriptionRepository.Value.TableUntracked
								.Where(x => x.Active)
								.Select(x => x.Email)
								.Distinct()
								.ToList();

							ctx.NewsletterSubscriptions = new HashSet<string>(subscriptionEmails, StringComparer.OrdinalIgnoreCase);
						}

						var stores = Init(ctx);

						ctx.ExecuteContext.Language = ToDynamic(ctx, ctx.ContextLanguage);
						ctx.ExecuteContext.Customer = ToDynamic(ctx, ctx.ContextCustomer);
						ctx.ExecuteContext.Currency = ToDynamic(ctx, ctx.ContextCurrency);
						ctx.ExecuteContext.Profile = ToDynamic(ctx, profile);

						stores.ForEach(x => ExportCoreInner(ctx, x));
					}

					if (!ctx.IsPreview && ctx.ExecuteContext.Abort != DataExchangeAbortion.Hard)
					{
						if (ctx.IsFileBasedExport)
						{
							if (profile.CreateZipArchive)
							{
								ZipFile.CreateFromDirectory(ctx.FolderContent, zipPath, CompressionLevel.Fastest, false);
							}

							if (profile.Deployments.Any(x => x.Enabled))
							{
								SetProgress(ctx, T("Common.Publishing"));

								var allDeploymentsSucceeded = Deploy(ctx, zipPath);

								if (allDeploymentsSucceeded && profile.Cleanup)
								{
									logger.Info("Cleaning up export folder");

									FileSystemHelper.ClearDirectory(ctx.FolderContent, false);
								}
							}
						}

						if (profile.EmailAccountId != 0 && !ctx.Supports(ExportFeatures.CanOmitCompletionMail))
						{
							SendCompletionEmail(ctx, zipPath);
						}
					}
				}
				catch (Exception ex)
				{
					logger.ErrorsAll(ex);
					ctx.Result.LastError = ex.ToString();
				}
				finally
				{
					try
					{
						if (!ctx.IsPreview && profile.Id != 0)
						{
							profile.ResultInfo = XmlHelper.Serialize(ctx.Result);

							_exportProfileService.Value.UpdateExportProfile(profile);
						}
					}
					catch (Exception ex)
					{
						logger.ErrorsAll(ex);
					}

					DetachAllEntitiesAndClear(ctx);
                    _localizedEntityService.ClearCache();

                    try
					{
						ctx.NewsletterSubscriptions.Clear();
						ctx.ProductTemplates.Clear();
						ctx.CategoryTemplates.Clear();
						ctx.Countries.Clear();
						ctx.Languages.Clear();
						ctx.QuantityUnits.Clear();
						ctx.DeliveryTimes.Clear();
						ctx.Stores.Clear();
                        ctx.Translations.Clear();

						ctx.Request.CustomData.Clear();

						ctx.ExecuteContext.CustomProperties.Clear();
						ctx.ExecuteContext.Log = null;
						ctx.Log = null;
					}
					catch (Exception ex)
					{
						logger.ErrorsAll(ex);
					}
				}
			}

            if (ctx.IsPreview || ctx.ExecuteContext.Abort == DataExchangeAbortion.Hard)
            {
                return;
            }

			// Post process order entities.
			if (ctx.EntityIdsLoaded.Any() && ctx.Request.Provider.Value.EntityType == ExportEntityType.Order && ctx.Projection.OrderStatusChange != ExportOrderStatusChange.None)
			{
				using (var logger = new TraceLogger(logPath))
				{
					try
					{
						int? orderStatusId = null;

						if (ctx.Projection.OrderStatusChange == ExportOrderStatusChange.Processing)
							orderStatusId = (int)OrderStatus.Processing;
						else if (ctx.Projection.OrderStatusChange == ExportOrderStatusChange.Complete)
							orderStatusId = (int)OrderStatus.Complete;

						using (var scope = new DbContextScope(_dbContext, false, null, false, false, false, false))
						{
							foreach (var chunk in ctx.EntityIdsLoaded.Slice(128))
							{
								var entities = _orderRepository.Value.Table.Where(x => chunk.Contains(x.Id)).ToList();

								entities.ForEach(x => x.OrderStatusId = (orderStatusId ?? x.OrderStatusId));

								_dbContext.SaveChanges();
							}
						}

						logger.Info($"Updated order status for {ctx.EntityIdsLoaded.Count()} order(s).");
					}
					catch (Exception ex)
					{
						logger.ErrorsAll(ex);
						ctx.Result.LastError = ex.ToString();
					}
				}
			}
		}

		/// <summary>
		/// The name of the public export folder
		/// </summary>
		public static string PublicFolder => "Exchange";

		public static int PageSize => 100;

		public DataExportResult Export(DataExportRequest request, CancellationToken cancellationToken)
		{
			var ctx = new DataExporterContext(request, cancellationToken);

			ExportCoreOuter(ctx);
			cancellationToken.ThrowIfCancellationRequested();

			return ctx.Result;
		}

		public IList<dynamic> Preview(DataExportRequest request, int pageIndex, int? totalRecords = null)
		{
			var result = new List<dynamic>();
			var cancellation = new CancellationTokenSource(TimeSpan.FromMinutes(5.0));
			var ctx = new DataExporterContext(request, cancellation.Token, true);

			var unused = Init(ctx, totalRecords);
			var offset = Math.Max(ctx.Request.Profile.Offset, 0) + (pageIndex * PageSize);

			if (!HasPermission(ctx))
			{
				throw new SmartException(T("Admin.AccessDenied"));
			}

			switch (request.Provider.Value.EntityType)
			{
				case ExportEntityType.Product:
					{
						var items = GetProductQuery(ctx, offset, PageSize).ToList();
						items.Each(x => result.Add(ToDynamic(ctx, x)));
					}
					break;
				case ExportEntityType.Order:
					{
						var items = GetOrderQuery(ctx, offset, PageSize).ToList();
						items.Each(x => result.Add(ToDynamic(ctx, x)));
					}
					break;
				case ExportEntityType.Category:
					{
						var items = GetCategoryQuery(ctx, offset, PageSize).ToList();
						items.Each(x => result.Add(ToDynamic(ctx, x)));
					}
					break;
				case ExportEntityType.Manufacturer:
					{
						var items = GetManufacturerQuery(ctx, offset, PageSize).ToList();
						items.Each(x => result.Add(ToDynamic(ctx, x)));
					}
					break;
				case ExportEntityType.Customer:
					{
						var items = GetCustomerQuery(ctx, offset, PageSize).ToList();
						items.Each(x => result.Add(ToDynamic(ctx, x)));
					}
					break;
				case ExportEntityType.NewsLetterSubscription:
					{
						var items = GetNewsLetterSubscriptionQuery(ctx, offset, PageSize).ToList();
						items.Each(x => result.Add(ToDynamic(ctx, x)));
					}
					break;
				case ExportEntityType.ShoppingCartItem:
					{
						var items = GetShoppingCartItemQuery(ctx, offset, PageSize).ToList();
						items.Each(x => result.Add(ToDynamic(ctx, x)));
					}
					break;
			}

			return result;
		}

		public int GetDataCount(DataExportRequest request)
		{
			var cancellation = new CancellationTokenSource(TimeSpan.FromMinutes(5.0));
			var ctx = new DataExporterContext(request, cancellation.Token, true);
			var unused = Init(ctx);

			var totalCount = ctx.RecordsPerStore.First().Value;
			return totalCount;
		}
	}
}
