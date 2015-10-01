﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Dynamic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Autofac;
using SmartStore.Collections;
using SmartStore.Core;
using SmartStore.Core.Async;
using SmartStore.Core.Data;
using SmartStore.Core.Domain;
using SmartStore.Core.Domain.Catalog;
using SmartStore.Core.Domain.Common;
using SmartStore.Core.Domain.Customers;
using SmartStore.Core.Domain.DataExchange;
using SmartStore.Core.Domain.Directory;
using SmartStore.Core.Domain.Discounts;
using SmartStore.Core.Domain.Localization;
using SmartStore.Core.Domain.Media;
using SmartStore.Core.Domain.Messages;
using SmartStore.Core.Domain.Orders;
using SmartStore.Core.Domain.Shipping;
using SmartStore.Core.Domain.Stores;
using SmartStore.Core.Email;
using SmartStore.Core.Html;
using SmartStore.Core.IO;
using SmartStore.Core.Logging;
using SmartStore.Core.Plugins;
using SmartStore.Services.Catalog;
using SmartStore.Services.Common;
using SmartStore.Services.Customers;
using SmartStore.Services.Directory;
using SmartStore.Services.Helpers;
using SmartStore.Services.Localization;
using SmartStore.Services.Media;
using SmartStore.Services.Messages;
using SmartStore.Services.Orders;
using SmartStore.Services.Seo;
using SmartStore.Services.Shipping;
using SmartStore.Services.Tasks;
using SmartStore.Services.Tax;
using SmartStore.Utilities;

// note: namespace persisted in ScheduleTask.Type
namespace SmartStore.Services.DataExchange.ExportTask
{
	public class ExportProfileTask : ITask
	{
		#region Dependencies

		private DataExchangeSettings _dataExchangeSettings;
		private ICommonServices _services;
		private IExportService _exportService;
		private IProductService _productService;
		private IPictureService _pictureService;
		private MediaSettings _mediaSettings;
		private IPriceCalculationService _priceCalculationService;
		private ITaxService _taxService;
		private ICurrencyService _currencyService;
		private ICustomerService _customerService;
		private ICategoryService _categoryService;
		private IPriceFormatter _priceFormatter;
		private IDateTimeHelper _dateTimeHelper;
		private IEmailAccountService _emailAccountService;
		private IEmailSender _emailSender;
		private IQueuedEmailService _queuedEmailService;
		private IProductAttributeService _productAttributeService;
		private IProductAttributeParser _productAttributeParser;
		private IDeliveryTimeService _deliveryTimeService;
		private IQuantityUnitService _quantityUnitService;
		private IManufacturerService _manufacturerService;
		private IOrderService _orderService;
		private IAddressService _addressesService;
		private ICountryService _countryService;
		private IRepository<Order> _orderRepository;
		private ILanguageService _languageService;
		private IShipmentService _shipmentService;
		private IProductTemplateService _productTemplateService;
		private ILocalizedEntityService _localizedEntityService;

		private void InitDependencies(TaskExecutionContext context)
		{
			_dataExchangeSettings = context.Resolve<DataExchangeSettings>();
			_services = context.Resolve<ICommonServices>();
			_exportService = context.Resolve<IExportService>();
			_productService = context.Resolve<IProductService>();
			_pictureService = context.Resolve<IPictureService>();
			_mediaSettings = context.Resolve<MediaSettings>();
			_priceCalculationService = context.Resolve<IPriceCalculationService>();
			_taxService = context.Resolve<ITaxService>();
			_currencyService = context.Resolve<ICurrencyService>();
			_customerService = context.Resolve<ICustomerService>();
			_categoryService = context.Resolve<ICategoryService>();
			_priceFormatter = context.Resolve<IPriceFormatter>();
			_dateTimeHelper = context.Resolve<IDateTimeHelper>();
			_emailAccountService = context.Resolve<IEmailAccountService>();
			_emailSender = context.Resolve<IEmailSender>();
			_queuedEmailService = context.Resolve<IQueuedEmailService>();
			_productAttributeService = context.Resolve<IProductAttributeService>();
			_productAttributeParser = context.Resolve<IProductAttributeParser>();
			_deliveryTimeService = context.Resolve<IDeliveryTimeService>();
			_quantityUnitService = context.Resolve<IQuantityUnitService>();
			_manufacturerService = context.Resolve<IManufacturerService>();
			_orderService = context.Resolve<IOrderService>();
			_addressesService = context.Resolve<IAddressService>();
			_countryService = context.Resolve<ICountryService>();
			_orderRepository = context.Resolve<IRepository<Order>>();
			_languageService = context.Resolve<ILanguageService>();
			_shipmentService = context.Resolve<IShipmentService>();
			_productTemplateService = context.Resolve<IProductTemplateService>();
			_localizedEntityService = context.Resolve<ILocalizedEntityService>();
		}

		#endregion

		#region Utilities

		private TPropType GetLocalized<T, TPropType>(Multimap<int, LocalizedProperty> map, T entity, Expression<Func<T, TPropType>> keySelector, int languageId)
			where T : BaseEntity, ILocalizedEntity
		{
			if (languageId > 0 && map.ContainsKey(entity.Id))
			{
				var member = keySelector.Body as MemberExpression;
				var propInfo = member.Member as PropertyInfo;

				var property = map[entity.Id].FirstOrDefault(x => x.LocaleKey == propInfo.Name && x.LanguageId == languageId);
				if (property != null)
				{
					Debug.Assert(property.LocaleKeyGroup.IsCaseInsensitiveEqual(entity.GetType().Name), "Wrong localized map for entity " + entity.GetType().Name);

					return property.LocaleValue.Convert<TPropType>();
				}
			}

			var localizer = keySelector.Compile();
			return localizer(entity);
		}

		private ProductSearchContext GetProductSearchContext(ExportProfileTaskContext ctx, int pageIndex, int pageSize)
		{
			var searchContext = new ProductSearchContext
			{
				OrderBy = ProductSortingEnum.CreatedOn,
				PageIndex = pageIndex,
				PageSize = pageSize,
				ProductIds = ctx.EntityIdsSelected,
				StoreId = (ctx.Profile.PerStore ? ctx.Store.Id : ctx.Filter.StoreId),
				VisibleIndividuallyOnly = true,
				PriceMin = ctx.Filter.PriceMinimum,
				PriceMax = ctx.Filter.PriceMaximum,
				IsPublished = ctx.Filter.IsPublished,
				WithoutCategories = ctx.Filter.WithoutCategories,
				WithoutManufacturers = ctx.Filter.WithoutManufacturers,
				ManufacturerId = ctx.Filter.ManufacturerId ?? 0,
				FeaturedProducts = ctx.Filter.FeaturedProducts,
				ProductType = ctx.Filter.ProductType,
				ProductTagId = ctx.Filter.ProductTagId ?? 0,
				IdMin = ctx.Filter.IdMinimum ?? 0,
				IdMax = ctx.Filter.IdMaximum ?? 0,
				AvailabilityMinimum = ctx.Filter.AvailabilityMinimum,
				AvailabilityMaximum = ctx.Filter.AvailabilityMaximum
			};

			if (!ctx.Filter.IsPublished.HasValue)
				searchContext.ShowHidden = true;

			if (ctx.Filter.CategoryIds != null && ctx.Filter.CategoryIds.Length > 0)
				searchContext.CategoryIds = ctx.Filter.CategoryIds.ToList();

			if (ctx.Filter.CreatedFrom.HasValue)
				searchContext.CreatedFromUtc = _dateTimeHelper.ConvertToUtcTime(ctx.Filter.CreatedFrom.Value, _dateTimeHelper.CurrentTimeZone);

			if (ctx.Filter.CreatedTo.HasValue)
				searchContext.CreatedToUtc = _dateTimeHelper.ConvertToUtcTime(ctx.Filter.CreatedTo.Value, _dateTimeHelper.CurrentTimeZone);

			return searchContext;
		}

		private void PrepareProductDescription(ExportProfileTaskContext ctx, dynamic expando, Product product)
		{
			try
			{
				var languageId = (ctx.Projection.LanguageId ?? 0);
				string description = "";

				// description merging
				if (ctx.Projection.DescriptionMerging != ExportDescriptionMerging.None)
				{
					if (ctx.Projection.DescriptionMerging == ExportDescriptionMerging.ShortDescriptionOrNameIfEmpty)
					{
						description = expando.FullDescription;

						if (description.IsEmpty())
							description = expando.ShortDescription;
						if (description.IsEmpty())
							description = expando.Name;
					}
					else if (ctx.Projection.DescriptionMerging == ExportDescriptionMerging.ShortDescription)
					{
						description = expando.ShortDescription;
					}
					else if (ctx.Projection.DescriptionMerging == ExportDescriptionMerging.Description)
					{
						description = expando.FullDescription;
					}
					else if (ctx.Projection.DescriptionMerging == ExportDescriptionMerging.NameAndShortDescription)
					{
						description = ((string)expando.Name).Grow((string)expando.ShortDescription, " ");
					}
					else if (ctx.Projection.DescriptionMerging == ExportDescriptionMerging.NameAndDescription)
					{
						description = ((string)expando.Name).Grow((string)expando.FullDescription, " ");
					}
					else if (ctx.Projection.DescriptionMerging == ExportDescriptionMerging.ManufacturerAndNameAndShortDescription ||
						ctx.Projection.DescriptionMerging == ExportDescriptionMerging.ManufacturerAndNameAndDescription)
					{
						var productManus = ctx.ProductDataContext.ProductManufacturers.Load(product.Id);

						if (productManus != null && productManus.Any())
							description = productManus.First().Manufacturer.GetLocalized(x => x.Name, languageId, true, false);

						description = description.Grow((string)expando.Name, " ");

						if (ctx.Projection.DescriptionMerging == ExportDescriptionMerging.ManufacturerAndNameAndShortDescription)
							description = description.Grow((string)expando.ShortDescription, " ");
						else
							description = description.Grow((string)expando.FullDescription, " ");
					}
				}
				else
				{
					description = expando.FullDescription;
				}

				// append text
				if (ctx.Projection.AppendDescriptionText.HasValue() && ((string)expando.ShortDescription).IsEmpty() && ((string)expando.FullDescription).IsEmpty())
				{
					string[] appendText = ctx.Projection.AppendDescriptionText.SplitSafe(",");
					if (appendText.Length > 0)
					{
						var rnd = (new Random()).Next(0, appendText.Length - 1);

						description = description.Grow(appendText.SafeGet(rnd), " ");
					}
				}

				// remove critical characters
				if (description.HasValue() && ctx.Projection.RemoveCriticalCharacters)
				{
					foreach (var str in ctx.Projection.CriticalCharacters.SplitSafe(","))
						description = description.Replace(str, "");
				}

				// convert to plain text
				if (ctx.Projection.DescriptionToPlainText)
				{
					//Regex reg = new Regex("<[^>]+>", RegexOptions.IgnoreCase);
					//description = HttpUtility.HtmlDecode(reg.Replace(description, ""));

					description = HtmlUtils.ConvertHtmlToPlainText(description);
					description = HtmlUtils.StripTags(HttpUtility.HtmlDecode(description));
				}

				expando.FullDescription = description;
			}
			catch { }
		}

		private decimal? ConvertPrice(ExportProfileTaskContext ctx, Product product, decimal? price)
		{
			if (price.HasValue)
			{
				if (ctx.Projection.ConvertNetToGrossPrices)
				{
					decimal taxRate;
					price = _taxService.GetProductPrice(product, price.Value, true, ctx.ProjectionCustomer, out taxRate);
				}

				if (price != decimal.Zero)
				{
					price = _currencyService.ConvertFromPrimaryStoreCurrency(price.Value, ctx.ProjectionCurrency, ctx.Store);
				}
			}
			return price;
		}

		private decimal CalculatePrice(ExportProfileTaskContext ctx, Product product, bool forAttributeCombination)
		{
			decimal price = product.Price;

			// price type
			if (ctx.Projection.PriceType.HasValue && !forAttributeCombination)
			{
				var priceCalculationContext = ctx.ProductDataContext as PriceCalculationContext;

				if (ctx.Projection.PriceType.Value == PriceDisplayType.LowestPrice)
				{
					bool displayFromMessage;
					price = _priceCalculationService.GetLowestPrice(product, priceCalculationContext, out displayFromMessage);
				}
				else if (ctx.Projection.PriceType.Value == PriceDisplayType.PreSelectedPrice)
				{
					price = _priceCalculationService.GetPreselectedPrice(product, priceCalculationContext);
				}
				else if (ctx.Projection.PriceType.Value == PriceDisplayType.PriceWithoutDiscountsAndAttributes)
				{
					price = _priceCalculationService.GetFinalPrice(product, null, ctx.ProjectionCustomer, decimal.Zero, false, 1, null, priceCalculationContext);
				}
			}

			return ConvertPrice(ctx, product, price) ?? price;
		}

		private void GetDeliveryTimeAndQuantityUnit(ExportProfileTaskContext ctx, dynamic expando, int? deliveryTimeId, int? quantityUnitId)
		{
			if (deliveryTimeId.HasValue && ctx.DeliveryTimes.ContainsKey(deliveryTimeId.Value))
				expando.DeliveryTime = ToExpando(ctx, ctx.DeliveryTimes[deliveryTimeId.Value]);
			else
				expando.DeliveryTime = null;

			if (quantityUnitId.HasValue && ctx.QuantityUnits.ContainsKey(quantityUnitId.Value))
				expando.QuantityUnit = ToExpando(ctx, ctx.QuantityUnits[quantityUnitId.Value]);
			else
				expando.QuantityUnit = null;
		}

		private void SetProgress(ExportProfileTaskContext ctx, int loadedRecords)
		{
			if (!ctx.IsPreview && loadedRecords > 0 && ctx.TaskContext.ScheduleTask != null)
			{
				int totalRecords = ctx.RecordsPerStore.Sum(x => x.Value);

				ctx.RecordCount = Math.Min(ctx.RecordCount + loadedRecords, totalRecords);

				var msg = ctx.ProgressInfo.FormatInvariant(ctx.RecordCount, totalRecords);

				ctx.TaskContext.SetProgress(ctx.RecordCount, totalRecords, msg, true);
			}
		}

		private void SendCompletionEmail(ExportProfileTaskContext ctx)
		{
			var emailAccount = _emailAccountService.GetEmailAccountById(ctx.Profile.EmailAccountId);
			var smtpContext = new SmtpContext(emailAccount);
			var message = new EmailMessage();

			var storeInfo = "{0} ({1})".FormatInvariant(ctx.Store.Name, ctx.Store.Url);

			message.To.AddRange(ctx.Profile.CompletedEmailAddresses.SplitSafe(",").Where(x => x.IsEmail()).Select(x => new EmailAddress(x)));
			message.From = new EmailAddress(emailAccount.Email, emailAccount.DisplayName);

			message.Subject = _services.Localization.GetResource("Admin.DataExchange.Export.CompletedEmail.Subject", ctx.Projection.LanguageId ?? 0)
				.FormatInvariant(ctx.Profile.Name);

			message.Body = _services.Localization.GetResource("Admin.DataExchange.Export.CompletedEmail.Body", ctx.Projection.LanguageId ?? 0)
				.FormatInvariant(storeInfo);

			_emailSender.SendEmail(smtpContext, message);
		}

		private void DeployFileSystem(ExportProfileTaskContext ctx, ExportDeployment deployment)
		{
			string folderDestination = null;

			if (deployment.IsPublic)
			{
				folderDestination = Path.Combine(HttpRuntime.AppDomainAppPath, PublicFolder);
			}
			else if (deployment.FileSystemPath.IsEmpty())
			{
				return;
			}
			else if (deployment.FileSystemPath.StartsWith("/") || deployment.FileSystemPath.StartsWith("\\") || !Path.IsPathRooted(deployment.FileSystemPath))
			{
				folderDestination = CommonHelper.MapPath(deployment.FileSystemPath);
			}
			else
			{
				folderDestination = deployment.FileSystemPath;
			}

			if (!System.IO.Directory.Exists(folderDestination))
			{
				System.IO.Directory.CreateDirectory(folderDestination);
			}

			if (deployment.CreateZip)
			{
				var path = Path.Combine(folderDestination, ctx.Profile.FolderName + ".zip");

				if (FileSystemHelper.Copy(ctx.ZipPath, path))
					ctx.Log.Information("Copied ZIP archive " + path);
			}
			else
			{
				FileSystemHelper.CopyDirectory(new DirectoryInfo(ctx.FolderContent), new DirectoryInfo(folderDestination));

				ctx.Log.Information("Copied export data files to " + folderDestination);
			}
		}

		private void DeployEmail(ExportProfileTaskContext ctx, ExportDeployment deployment)
		{
			var emailAccount = _emailAccountService.GetEmailAccountById(deployment.EmailAccountId);
			var smtpContext = new SmtpContext(emailAccount);
			var count = 0;

			foreach (var email in deployment.EmailAddresses.SplitSafe(",").Where(x => x.IsEmail()))
			{
				var queuedEmail = new QueuedEmail
				{
					From = emailAccount.Email,
					FromName = emailAccount.DisplayName,
					To = email,
					Subject = deployment.EmailSubject.NaIfEmpty(),
					CreatedOnUtc = DateTime.UtcNow,
					EmailAccountId = deployment.EmailAccountId
				};

				foreach (string path in ctx.GetDeploymentFiles(deployment))
				{
					string name = Path.GetFileName(path);

					queuedEmail.Attachments.Add(new QueuedEmailAttachment
					{
						StorageLocation = EmailAttachmentStorageLocation.Path,
						Path = path,
						Name = name,
						MimeType = MimeTypes.MapNameToMimeType(name)
					});
				}

				_queuedEmailService.InsertQueuedEmail(queuedEmail);
				++count;
			}

			ctx.Log.Information("{0} email(s) created and queued.".FormatInvariant(count));
		}

		private async void DeployHttp(ExportProfileTaskContext ctx, ExportDeployment deployment)
		{
			var succeeded = 0;
			var url = deployment.Url;

			if (!url.StartsWith("http://", StringComparison.InvariantCultureIgnoreCase) && !url.StartsWith("https://", StringComparison.InvariantCultureIgnoreCase))
				url = "http://" + url;

			if (deployment.HttpTransmissionType == ExportHttpTransmissionType.MultipartFormDataPost)
			{
				var count = 0;
				ICredentials credentials = null;

				if (deployment.Username.HasValue())
					credentials = new NetworkCredential(deployment.Username, deployment.Password);

				using (var handler = new HttpClientHandler { Credentials = credentials })
				using (var client = new HttpClient(handler))
				using (var formData = new MultipartFormDataContent())
				{
					foreach (var path in ctx.GetDeploymentFiles(deployment))
					{
						byte[] fileData = File.ReadAllBytes(path);
						formData.Add(new ByteArrayContent(fileData), "file {0}".FormatInvariant(++count), Path.GetFileName(path));
					}

					var response = await client.PostAsync(url, formData);

					if (response.IsSuccessStatusCode)
					{
						succeeded = count;
					}
					else if (response.Content != null)
					{
						var content = await response.Content.ReadAsStringAsync();

						var msg = "Multipart form data upload failed. {0} ({1}). Response: {2}".FormatInvariant(
							response.StatusCode.ToString(), (int)response.StatusCode, content.NaIfEmpty().Truncate(2000, "..."));

						ctx.Log.Error(msg);
					}
				}
			}
			else
			{
				using (var webClient = new WebClient())
				{
					if (deployment.Username.HasValue())
						webClient.Credentials = new NetworkCredential(deployment.Username, deployment.Password);

					foreach (var path in ctx.GetDeploymentFiles(deployment))
					{
						await webClient.UploadFileTaskAsync(url, path);
					}
				}
			}

			ctx.Log.Information("{0} file(s) successfully uploaded via HTTP.".FormatInvariant(succeeded));
		}

		private async void DeployFtp(ExportProfileTaskContext ctx, ExportDeployment deployment)
		{
			var bytesRead = 0;
			var succeededFiles = 0;
			var url = deployment.Url;
			var buffLength = 32768;
			byte[] buff = new byte[buffLength];
			var deploymentFiles = ctx.GetDeploymentFiles(deployment).ToList();
			var lastIndex = (deploymentFiles.Count - 1);

			if (!url.StartsWith("ftp://", StringComparison.InvariantCultureIgnoreCase))
				url = "ftp://" + url;

			foreach (var path in deploymentFiles)
			{
				var fileUrl = url.EnsureEndsWith("/") + Path.GetFileName(path);

				var request = (FtpWebRequest)WebRequest.Create(fileUrl);
				request.Method = WebRequestMethods.Ftp.UploadFile;
				request.KeepAlive = (deploymentFiles.IndexOf(path) != lastIndex);
				request.UseBinary = true;
				request.Proxy = null;
				request.UsePassive = deployment.PassiveMode;
				request.EnableSsl = deployment.UseSsl;

				if (deployment.Username.HasValue())
					request.Credentials = new NetworkCredential(deployment.Username, deployment.Password);

				request.ContentLength = (new FileInfo(path)).Length;

				var requestStream = await request.GetRequestStreamAsync();

				using (var stream = new FileStream(path, FileMode.Open))
				{
					while ((bytesRead = stream.Read(buff, 0, buffLength)) != 0)
					{
						await requestStream.WriteAsync(buff, 0, bytesRead);
					}
				}

				requestStream.Close();

				var response = (FtpWebResponse)await request.GetResponseAsync();
				var statusCode = (int)response.StatusCode;

				if (statusCode >= 200 && statusCode <= 299)
				{
					++succeededFiles;
				}
				else
				{
					var msg = "The FTP transfer might fail. {0} ({1}), {2}. File {3}".FormatInvariant(
						response.StatusCode.ToString(), statusCode, response.StatusDescription.NaIfEmpty(), path);

					ctx.Log.Error(msg);
				}
			}

			ctx.Log.Information("{0} file(s) successfully uploaded via FTP.".FormatInvariant(succeededFiles));
		}

		#endregion

		#region Entity to expando

		private ExpandoObject ToExpando(ExportProfileTaskContext ctx, Currency currency)
		{
			if (currency == null)
				return null;

			dynamic expando = new ExpandoObject();
			expando._Entity = currency;

			expando.Id = currency.Id;
			expando.Name = currency.GetLocalized(x => x.Name, ctx.Projection.LanguageId ?? 0, true, false);
			expando.CurrencyCode = currency.CurrencyCode;
			expando.Rate = currency.Rate;
			expando.DisplayLocale = currency.DisplayLocale;
			expando.CustomFormatting = currency.CustomFormatting;
			expando.LimitedToStores = currency.LimitedToStores;
			expando.Published = currency.Published;
			expando.DisplayOrder = currency.DisplayOrder;
			expando.CreatedOnUtc = currency.CreatedOnUtc;
			expando.UpdatedOnUtc = currency.UpdatedOnUtc;
			expando.DomainEndings = currency.DomainEndings;

			return expando as ExpandoObject;
		}

		private ExpandoObject ToExpando(ExportProfileTaskContext ctx, Language language)
		{
			if (language == null)
				return null;

			dynamic expando = new ExpandoObject();
			expando._Entity = language;

			expando.Id = language.Id;
			expando.Name = language.Name;
			expando.LanguageCulture = language.LanguageCulture;
			expando.UniqueSeoCode = language.UniqueSeoCode;
			expando.FlagImageFileName = language.FlagImageFileName;
			expando.Rtl = language.Rtl;
			expando.LimitedToStores = language.LimitedToStores;
			expando.Published = language.Published;
			expando.DisplayOrder = language.DisplayOrder;

			return expando as ExpandoObject;
		}

		private ExpandoObject ToExpando(ExportProfileTaskContext ctx, Country country)
		{
			if (country == null)
				return null;

			dynamic expando = new ExpandoObject();
			expando._Entity = country;

			expando.Id = country.Id;
			expando.Name = country.GetLocalized(x => x.Name, ctx.Projection.LanguageId ?? 0, true, false);
			expando.AllowsBilling = country.AllowsBilling;
			expando.AllowsShipping = country.AllowsShipping;
			expando.TwoLetterIsoCode = country.TwoLetterIsoCode;
			expando.ThreeLetterIsoCode = country.ThreeLetterIsoCode;
			expando.NumericIsoCode = country.NumericIsoCode;
			expando.SubjectToVat = country.SubjectToVat;
			expando.Published = country.Published;
			expando.DisplayOrder = country.DisplayOrder;
			expando.LimitedToStores = country.LimitedToStores;

			return expando as ExpandoObject;
		}

		private ExpandoObject ToExpando(ExportProfileTaskContext ctx, Address address)
		{
			if (address == null)
				return null;

			dynamic expando = new ExpandoObject();
			expando._Entity = address;

			expando.Id = address.Id;
			expando.FirstName = address.FirstName;
			expando.LastName = address.LastName;
			expando.Email = address.Email;
			expando.Company = address.Company;
			expando.CountryId = address.CountryId;
			expando.StateProvinceId = address.StateProvinceId;
			expando.City = address.City;
			expando.Address1 = address.Address1;
			expando.Address2 = address.Address2;
			expando.ZipPostalCode = address.ZipPostalCode;
			expando.PhoneNumber = address.PhoneNumber;
			expando.FaxNumber = address.FaxNumber;
			expando.CreatedOnUtc = address.CreatedOnUtc;

			expando.Country = ToExpando(ctx, address.Country);

			dynamic sp = new ExpandoObject();
			sp._Entity = address.StateProvince;
			sp.Id = address.StateProvince.Id;
			sp.CountryId = address.StateProvince.CountryId;
			sp.Name = address.StateProvince.GetLocalized(x => x.Name, ctx.Projection.LanguageId ?? 0, true, false);
			sp.Abbreviation = address.StateProvince.Abbreviation;
			sp.Published = address.StateProvince.Published;
			sp.DisplayOrder = address.StateProvince.DisplayOrder;

			expando.StateProvince = sp as ExpandoObject;

			return expando as ExpandoObject;
		}

		private ExpandoObject ToExpando(ExportProfileTaskContext ctx, RewardPointsHistory points)
		{
			if (points == null)
				return null;

			dynamic expando = new ExpandoObject();
			expando._Entity = points;

			expando.Id = points.Id;
			expando.CustomerId = points.CustomerId;
			expando.Points = points.Points;
			expando.PointsBalance = points.PointsBalance;
			expando.UsedAmount = points.UsedAmount;
			expando.Message = points.Message;
			expando.CreatedOnUtc = points.CreatedOnUtc;

			return expando as ExpandoObject;
		}

		private ExpandoObject ToExpando(ExportProfileTaskContext ctx, Customer customer)
		{
			if (customer == null)
				return null;

			dynamic expando = new ExpandoObject();
			expando._Entity = customer;

			expando.Id = customer.Id;
			expando.CustomerGuid = customer.CustomerGuid;
			expando.Username = customer.Username;
			expando.Email = customer.Email;
			//Password... we do not provide that data
			expando.PasswordFormatId = customer.PasswordFormatId;
			expando.AdminComment = customer.AdminComment;
			expando.IsTaxExempt = customer.IsTaxExempt;
			expando.AffiliateId = customer.AffiliateId;
			expando.Active = customer.Active;
			expando.Deleted = customer.Deleted;
			expando.IsSystemAccount = customer.IsSystemAccount;
			expando.SystemName = customer.SystemName;
			expando.LastIpAddress = customer.LastIpAddress;
			expando.CreatedOnUtc = customer.CreatedOnUtc;
			expando.LastLoginDateUtc = customer.LastLoginDateUtc;
			expando.LastActivityDateUtc = customer.LastActivityDateUtc;

			expando.BillingAddress = ToExpando(ctx, customer.BillingAddress);
			expando.ShippingAddress = ToExpando(ctx, customer.ShippingAddress);

			expando.RewardPointsHistory = null;
			expando._RewardPointsBalance = 0;

			return expando as ExpandoObject;
		}

		private ExpandoObject ToExpando(ExportProfileTaskContext ctx, Store store)
		{
			if (store == null)
				return null;

			dynamic expando = new ExpandoObject();
			expando._Entity = store;

			expando.Id = store.Id;
			expando.Name = store.Name;
			expando.Url = store.Url;
			expando.SslEnabled = store.SslEnabled;
			expando.SecureUrl = store.SecureUrl;
			expando.Hosts = store.Hosts;
			expando.LogoPictureId = store.LogoPictureId;
			expando.DisplayOrder = store.DisplayOrder;
			expando.HtmlBodyId = store.HtmlBodyId;
			expando.ContentDeliveryNetwork = store.ContentDeliveryNetwork;
			expando.PrimaryStoreCurrencyId = store.PrimaryStoreCurrencyId;
			expando.PrimaryExchangeRateCurrencyId = store.PrimaryExchangeRateCurrencyId;

			expando.PrimaryStoreCurrency = ToExpando(ctx, store.PrimaryStoreCurrency);
			expando.PrimaryExchangeRateCurrency = ToExpando(ctx, store.PrimaryExchangeRateCurrency);

			return expando as ExpandoObject;
		}

		private ExpandoObject ToExpando(ExportProfileTaskContext ctx, DeliveryTime deliveryTime)
		{
			if (deliveryTime == null)
				return null;		

			dynamic expando = new ExpandoObject();
			expando._Entity = deliveryTime;

			expando.Id = deliveryTime.Id;
			expando.Name = GetLocalized(ctx.Localized.DeliveryTimes, deliveryTime, x => x.Name, ctx.Projection.LanguageId ?? 0);
			expando.DisplayLocale = deliveryTime.DisplayLocale;
			expando.ColorHexValue = deliveryTime.ColorHexValue;
			expando.DisplayOrder = deliveryTime.DisplayOrder;

			return expando as ExpandoObject;
		}

		private ExpandoObject ToExpando(ExportProfileTaskContext ctx, QuantityUnit quantityUnit)
		{
			if (quantityUnit == null)
				return null;

			dynamic expando = new ExpandoObject();
			expando._Entity = quantityUnit;

			expando.Id = quantityUnit.Id;
			expando.Name = GetLocalized(ctx.Localized.QuantityUnits, quantityUnit, x => x.Name, ctx.Projection.LanguageId ?? 0);
			expando.Description = GetLocalized(ctx.Localized.QuantityUnits, quantityUnit, x => x.Description, ctx.Projection.LanguageId ?? 0);
			expando.DisplayLocale = quantityUnit.DisplayLocale;
			expando.DisplayOrder = quantityUnit.DisplayOrder;
			expando.IsDefault = quantityUnit.IsDefault;

			return expando as ExpandoObject;
		}

		private ExpandoObject ToExpando(ExportProfileTaskContext ctx, Picture picture, int thumbPictureSize, int detailsPictureSize)
		{
			if (picture == null)
				return null;

			dynamic expando = new ExpandoObject();
			expando._Entity = picture;

			expando.Id = picture.Id;
			expando.SeoFilename = picture.SeoFilename;
			expando.MimeType = picture.MimeType;

			expando._ThumbImageUrl = _pictureService.GetPictureUrl(picture, thumbPictureSize, false, ctx.Store.Url);
			expando._ImageUrl = _pictureService.GetPictureUrl(picture, detailsPictureSize, false, ctx.Store.Url);
			expando._FullSizeImageUrl = _pictureService.GetPictureUrl(picture, 0, false, ctx.Store.Url);

			var relativeUrl = _pictureService.GetPictureUrl(picture);
			expando._FileName = relativeUrl.Substring(relativeUrl.LastIndexOf("/") + 1);

			return expando as ExpandoObject;
		}

		private ExpandoObject ToExpando(ExportProfileTaskContext ctx, ProductVariantAttribute pva)
		{
			if (pva == null)
				return null;

			dynamic expando = new ExpandoObject();
			expando._Entity = pva;

			expando.Id = pva.Id;
			expando.TextPrompt = pva.TextPrompt;
			expando.IsRequired = pva.IsRequired;
			expando.AttributeControlTypeId = pva.AttributeControlTypeId;
			expando.DisplayOrder = pva.DisplayOrder;

			dynamic attribute = new ExpandoObject();
			attribute._Entity = pva.ProductAttribute;
			attribute.Id = pva.ProductAttribute.Id;
			attribute.Alias = pva.ProductAttribute.Alias;
			attribute.Name = GetLocalized(ctx.Localized.ProductAttributes, pva.ProductAttribute, x => x.Name, ctx.Projection.LanguageId ?? 0);
			attribute.Description = GetLocalized(ctx.Localized.ProductAttributes, pva.ProductAttribute, x => x.Description, ctx.Projection.LanguageId ?? 0);

			attribute.Values = pva.ProductVariantAttributeValues
				.OrderBy(x => x.DisplayOrder)
				.Select(x =>
				{
					dynamic value = new ExpandoObject();
					value._Entity = x;
					value.Id = x.Id;
					value.Alias = x.Alias;
					value.Name = x.GetLocalized(y => y.Name, ctx.Projection.LanguageId ?? 0, true, false);
					value.ColorSquaresRgb = x.ColorSquaresRgb;
					value.PriceAdjustment = x.PriceAdjustment;
					value.WeightAdjustment = x.WeightAdjustment;
					value.IsPreSelected = x.IsPreSelected;
					value.DisplayOrder = x.DisplayOrder;
					value.ValueTypeId = x.ValueTypeId;
					value.LinkedProductId = x.LinkedProductId;
					value.Quantity = x.Quantity;

					return value as ExpandoObject;
				})
				.ToList();

			expando.Attribute = attribute as ExpandoObject;

			return expando as ExpandoObject;
		}

		private ExpandoObject ToExpando(ExportProfileTaskContext ctx, ProductVariantAttributeCombination pvac)
		{
			if (pvac == null)
				return null;

			dynamic expando = new ExpandoObject();
			expando._Entity = pvac;

			expando.Id = pvac.Id;
			expando.StockQuantity = pvac.StockQuantity;
			expando.AllowOutOfStockOrders = pvac.AllowOutOfStockOrders;
			expando.AttributesXml = pvac.AttributesXml;
			expando.Sku = pvac.Sku;
			expando.Gtin = pvac.Gtin;
			expando.ManufacturerPartNumber = pvac.ManufacturerPartNumber;
			expando.Price = pvac.Price;
			expando.Length = pvac.Length;
			expando.Width = pvac.Width;
			expando.Height = pvac.Height;
			expando.BasePriceAmount = pvac.BasePriceAmount;
			expando.BasePriceBaseAmount = pvac.BasePriceBaseAmount;
			expando.AssignedPictureIds = pvac.AssignedPictureIds;
			expando.DeliveryTimeId = pvac.DeliveryTimeId;
			expando.IsActive = pvac.IsActive;

			return expando as ExpandoObject;
		}

		private ExpandoObject ToExpando(ExportProfileTaskContext ctx, Manufacturer manufacturer)
		{
			if (manufacturer == null)
				return null;

			dynamic expando = new ExpandoObject();
			expando._Entity = manufacturer;

			expando.Id = manufacturer.Id;
			expando.Name = manufacturer.GetLocalized(x => x.Name, ctx.Projection.LanguageId ?? 0, true, false);
			expando.SeName = manufacturer.GetSeName(ctx.Projection.LanguageId ?? 0, true, false);
			expando.Description = manufacturer.GetLocalized(x => x.Description, ctx.Projection.LanguageId ?? 0, true, false);
			expando.ManufacturerTemplateId = manufacturer.ManufacturerTemplateId;
			expando.MetaKeywords = manufacturer.GetLocalized(x => x.MetaKeywords, ctx.Projection.LanguageId ?? 0, true, false);
			expando.MetaDescription = manufacturer.GetLocalized(x => x.MetaDescription, ctx.Projection.LanguageId ?? 0, true, false);
			expando.MetaTitle = manufacturer.GetLocalized(x => x.MetaTitle, ctx.Projection.LanguageId ?? 0, true, false);
			expando.PictureId = manufacturer.PictureId;
			expando.PageSize = manufacturer.PageSize;
			expando.AllowCustomersToSelectPageSize = manufacturer.AllowCustomersToSelectPageSize;
			expando.PageSizeOptions = manufacturer.PageSizeOptions;
			expando.PriceRanges = manufacturer.PriceRanges;
			expando.Published = manufacturer.Published;
			expando.Deleted = manufacturer.Deleted;
			expando.DisplayOrder = manufacturer.DisplayOrder;
			expando.CreatedOnUtc = manufacturer.CreatedOnUtc;
			expando.UpdatedOnUtc = manufacturer.UpdatedOnUtc;

			return expando as ExpandoObject;
		}

		private ExpandoObject ToExpando(ExportProfileTaskContext ctx, Category category)
		{
			if (category == null)
				return null;

			dynamic expando = new ExpandoObject();
			expando._Entity = category;

			expando.Id = category.Id;
			expando.Name = category.GetLocalized(x => x.Name, ctx.Projection.LanguageId ?? 0, true, false);
			expando.FullName = category.GetLocalized(x => x.FullName, ctx.Projection.LanguageId ?? 0, true, false);
			expando.Description = category.GetLocalized(x => x.Description, ctx.Projection.LanguageId ?? 0, true, false);
			expando.BottomDescription = category.GetLocalized(x => x.BottomDescription, ctx.Projection.LanguageId ?? 0, true, false);
			expando.CategoryTemplateId = category.CategoryTemplateId;
			expando.MetaKeywords = category.GetLocalized(x => x.MetaKeywords, ctx.Projection.LanguageId ?? 0, true, false);
			expando.MetaDescription = category.GetLocalized(x => x.MetaDescription, ctx.Projection.LanguageId ?? 0, true, false);
			expando.MetaTitle = category.GetLocalized(x => x.MetaTitle, ctx.Projection.LanguageId ?? 0, true, false);
			expando.SeName = category.GetSeName(ctx.Projection.LanguageId ?? 0, true, false);
			expando.ParentCategoryId = category.ParentCategoryId;
			expando.PictureId = category.PictureId;
			expando.PageSize = category.PageSize;
			expando.AllowCustomersToSelectPageSize = category.AllowCustomersToSelectPageSize;
			expando.PageSizeOptions = category.PageSizeOptions;
			expando.PriceRanges = category.PriceRanges;
			expando.ShowOnHomePage = category.ShowOnHomePage;
			expando.HasDiscountsApplied = category.HasDiscountsApplied;
			expando.Published = category.Published;
			expando.Deleted = category.Deleted;
			expando.DisplayOrder = category.DisplayOrder;
			expando.CreatedOnUtc = category.CreatedOnUtc;
			expando.UpdatedOnUtc = category.UpdatedOnUtc;
			expando.SubjectToAcl = category.SubjectToAcl;
			expando.LimitedToStores = category.LimitedToStores;
			expando.Alias = category.Alias;
			expando.DefaultViewMode = category.DefaultViewMode;

			return expando as ExpandoObject;
		}

		private ExpandoObject ToExpando(ExportProfileTaskContext ctx, Product product)
		{
			if (product == null)
				return null;

			dynamic expando = new ExpandoObject();
			expando._Entity = product;

			expando.Id = product.Id;
			expando.Name = product.GetLocalized(x => x.Name, ctx.Projection.LanguageId ?? 0, true, false);
			expando.SeName = product.GetSeName(ctx.Projection.LanguageId ?? 0, true, false);
			expando.ShortDescription = product.GetLocalized(x => x.ShortDescription, ctx.Projection.LanguageId ?? 0, true, false);
			expando.FullDescription = product.GetLocalized(x => x.FullDescription, ctx.Projection.LanguageId ?? 0, true, false);
			expando.AdminComment = product.AdminComment;
			expando.ProductTemplateId = product.ProductTemplateId;
			expando.ShowOnHomePage = product.ShowOnHomePage;
			expando.HomePageDisplayOrder = product.HomePageDisplayOrder;
			expando.MetaKeywords = product.GetLocalized(x => x.MetaKeywords, ctx.Projection.LanguageId ?? 0, true, false);
			expando.MetaDescription = product.GetLocalized(x => x.MetaDescription, ctx.Projection.LanguageId ?? 0, true, false);
			expando.MetaTitle = product.GetLocalized(x => x.MetaTitle, ctx.Projection.LanguageId ?? 0, true, false);
			expando.AllowCustomerReviews = product.AllowCustomerReviews;
			expando.ApprovedRatingSum = product.ApprovedRatingSum;
			expando.NotApprovedRatingSum = product.NotApprovedRatingSum;
			expando.ApprovedTotalReviews = product.ApprovedTotalReviews;
			expando.NotApprovedTotalReviews = product.NotApprovedTotalReviews;
			expando.Published = product.Published;
			expando.CreatedOnUtc = product.CreatedOnUtc;
			expando.UpdatedOnUtc = product.UpdatedOnUtc;
			expando.SubjectToAcl = product.SubjectToAcl;
			expando.LimitedToStores = product.LimitedToStores;
			expando.ProductTypeId = product.ProductTypeId;
			expando.ParentGroupedProductId = product.ParentGroupedProductId;
			expando.Sku = product.Sku;
			expando.ManufacturerPartNumber = product.ManufacturerPartNumber;
			expando.Gtin = product.Gtin;
			expando.IsGiftCard = product.IsGiftCard;
			expando.GiftCardTypeId = product.GiftCardTypeId;
			expando.RequireOtherProducts = product.RequireOtherProducts;
			expando.RequiredProductIds = product.RequiredProductIds;
			expando.AutomaticallyAddRequiredProducts = product.AutomaticallyAddRequiredProducts;
			expando.IsDownload = product.IsDownload;
			expando.DownloadId = product.DownloadId;
			expando.UnlimitedDownloads = product.UnlimitedDownloads;
			expando.MaxNumberOfDownloads = product.MaxNumberOfDownloads;
			expando.DownloadExpirationDays = product.DownloadExpirationDays;
			expando.DownloadActivationTypeId = product.DownloadActivationTypeId;
			expando.HasSampleDownload = product.HasSampleDownload;
			expando.SampleDownloadId = product.SampleDownloadId;
			expando.HasUserAgreement = product.HasUserAgreement;
			expando.UserAgreementText = product.UserAgreementText;
			expando.IsRecurring = product.IsRecurring;
			expando.RecurringCycleLength = product.RecurringCycleLength;
			expando.RecurringCyclePeriodId = product.RecurringCyclePeriodId;
			expando.RecurringTotalCycles = product.RecurringTotalCycles;
			expando.IsShipEnabled = product.IsShipEnabled;
			expando.IsFreeShipping = product.IsFreeShipping;
			expando.AdditionalShippingCharge = product.AdditionalShippingCharge;
			expando.IsTaxExempt = product.IsTaxExempt;
			expando.TaxCategoryId = product.TaxCategoryId;
			expando.ManageInventoryMethodId = product.ManageInventoryMethodId;
			expando.StockQuantity = product.StockQuantity;
			expando.DisplayStockAvailability = product.DisplayStockAvailability;
			expando.DisplayStockQuantity = product.DisplayStockQuantity;
			expando.MinStockQuantity = product.MinStockQuantity;
			expando.LowStockActivityId = product.LowStockActivityId;
			expando.NotifyAdminForQuantityBelow = product.NotifyAdminForQuantityBelow;
			expando.BackorderModeId = product.BackorderModeId;
			expando.AllowBackInStockSubscriptions = product.AllowBackInStockSubscriptions;
			expando.OrderMinimumQuantity = product.OrderMinimumQuantity;
			expando.OrderMaximumQuantity = product.OrderMaximumQuantity;
			expando.AllowedQuantities = product.AllowedQuantities;
			expando.DisableBuyButton = product.DisableBuyButton;
			expando.DisableWishlistButton = product.DisableWishlistButton;
			expando.AvailableForPreOrder = product.AvailableForPreOrder;
			expando.CallForPrice = product.CallForPrice;
			expando.Price = product.Price;
			expando.OldPrice = product.OldPrice;
			expando.ProductCost = product.ProductCost;
			expando.SpecialPrice = product.SpecialPrice;
			expando.SpecialPriceStartDateTimeUtc = product.SpecialPriceStartDateTimeUtc;
			expando.SpecialPriceEndDateTimeUtc = product.SpecialPriceEndDateTimeUtc;
			expando.CustomerEntersPrice = product.CustomerEntersPrice;
			expando.MinimumCustomerEnteredPrice = product.MinimumCustomerEnteredPrice;
			expando.MaximumCustomerEnteredPrice = product.MaximumCustomerEnteredPrice;
			expando.HasTierPrices = product.HasTierPrices;
			expando.HasDiscountsApplied = product.HasDiscountsApplied;
			expando.Weight = product.Weight;
			expando.Length = product.Length;
			expando.Width = product.Width;
			expando.Height = product.Height;
			expando.AvailableStartDateTimeUtc = product.AvailableStartDateTimeUtc;
			expando.AvailableEndDateTimeUtc = product.AvailableEndDateTimeUtc;
			expando.BasePriceEnabled = product.BasePriceEnabled;
			expando.BasePriceMeasureUnit = product.BasePriceMeasureUnit;
			expando.BasePriceAmount = product.BasePriceAmount;
			expando.BasePriceBaseAmount = product.BasePriceBaseAmount;
			expando.BasePriceHasValue = product.BasePriceHasValue;
			expando.VisibleIndividually = product.VisibleIndividually;
			expando.DisplayOrder = product.DisplayOrder;
			expando.BundleTitleText = product.GetLocalized(x => x.BundleTitleText, ctx.Projection.LanguageId ?? 0, true, false);
			expando.BundlePerItemPricing = product.BundlePerItemPricing;
			expando.BundlePerItemShipping = product.BundlePerItemShipping;
			expando.BundlePerItemShoppingCart = product.BundlePerItemShoppingCart;
			expando.LowestAttributeCombinationPrice = product.LowestAttributeCombinationPrice;
			expando.IsEsd = product.IsEsd;

			return expando as ExpandoObject;
		}

		private ExpandoObject ToExpando(ExportProfileTaskContext ctx, Order order)
		{
			if (order == null)
				return null;

			dynamic expando = new ExpandoObject();
			expando._Entity = order;

			expando.Id = order.Id;
			expando.OrderNumber = order.GetOrderNumber();
			expando.OrderGuid = order.OrderGuid;
			expando.StoreId = order.StoreId;
			expando.CustomerId = order.CustomerId;
			expando.BillingAddressId = order.BillingAddressId;
			expando.ShippingAddressId = order.ShippingAddressId;
			expando.OrderStatusId = order.OrderStatusId;
			expando.ShippingStatusId = order.ShippingStatusId;
			expando.PaymentStatusId = order.PaymentStatusId;
			expando.PaymentMethodSystemName = order.PaymentMethodSystemName;
			expando.CustomerCurrencyCode = order.CustomerCurrencyCode;
			expando.CurrencyRate = order.CurrencyRate;
			expando.CustomerTaxDisplayTypeId = order.CustomerTaxDisplayTypeId;
			expando.VatNumber = order.VatNumber;
			expando.OrderSubtotalInclTax = order.OrderSubtotalInclTax;
			expando.OrderSubtotalExclTax = order.OrderSubtotalExclTax;
			expando.OrderSubTotalDiscountInclTax = order.OrderSubTotalDiscountInclTax;
			expando.OrderSubTotalDiscountExclTax = order.OrderSubTotalDiscountExclTax;
			expando.OrderShippingInclTax = order.OrderShippingInclTax;
			expando.OrderShippingExclTax = order.OrderShippingExclTax;
			expando.OrderShippingTaxRate = order.OrderShippingTaxRate;
			expando.PaymentMethodAdditionalFeeInclTax = order.PaymentMethodAdditionalFeeInclTax;
			expando.PaymentMethodAdditionalFeeExclTax = order.PaymentMethodAdditionalFeeExclTax;
			expando.PaymentMethodAdditionalFeeTaxRate = order.PaymentMethodAdditionalFeeTaxRate;
			expando.TaxRates = order.TaxRates;
			expando.OrderTax = order.OrderTax;
			expando.OrderDiscount = order.OrderDiscount;
			expando.OrderTotal = order.OrderTotal;
			expando.RefundedAmount = order.RefundedAmount;
			expando.RewardPointsWereAdded = order.RewardPointsWereAdded;
			expando.CheckoutAttributeDescription = order.CheckoutAttributeDescription;
			expando.CheckoutAttributesXml = order.CheckoutAttributesXml;
			expando.CustomerLanguageId = order.CustomerLanguageId;
			expando.AffiliateId = order.AffiliateId;
			expando.CustomerIp = order.CustomerIp;
			expando.AllowStoringCreditCardNumber = order.AllowStoringCreditCardNumber;
			expando.CardType = order.CardType;
			expando.CardName = order.CardName;
			expando.CardNumber = order.CardNumber;
			expando.MaskedCreditCardNumber = order.MaskedCreditCardNumber;
			expando.CardCvv2 = order.CardCvv2;
			expando.CardExpirationMonth = order.CardExpirationMonth;
			expando.CardExpirationYear = order.CardExpirationYear;
			expando.AllowStoringDirectDebit = order.AllowStoringDirectDebit;
			expando.DirectDebitAccountHolder = order.DirectDebitAccountHolder;
			expando.DirectDebitAccountNumber = order.DirectDebitAccountNumber;
			expando.DirectDebitBankCode = order.DirectDebitBankCode;
			expando.DirectDebitBankName = order.DirectDebitBankName;
			expando.DirectDebitBIC = order.DirectDebitBIC;
			expando.DirectDebitCountry = order.DirectDebitCountry;
			expando.DirectDebitIban = order.DirectDebitIban;
			expando.CustomerOrderComment = order.CustomerOrderComment;
			expando.AuthorizationTransactionId = order.AuthorizationTransactionId;
			expando.AuthorizationTransactionCode = order.AuthorizationTransactionCode;
			expando.AuthorizationTransactionResult = order.AuthorizationTransactionResult;
			expando.CaptureTransactionId = order.CaptureTransactionId;
			expando.CaptureTransactionResult = order.CaptureTransactionResult;
			expando.SubscriptionTransactionId = order.SubscriptionTransactionId;
			expando.PurchaseOrderNumber = order.PurchaseOrderNumber;
			expando.PaidDateUtc = order.PaidDateUtc;
			expando.ShippingMethod = order.ShippingMethod;
			expando.ShippingRateComputationMethodSystemName = order.ShippingRateComputationMethodSystemName;
			expando.Deleted = order.Deleted;
			expando.CreatedOnUtc = order.CreatedOnUtc;
			expando.UpdatedOnUtc = order.UpdatedOnUtc;
			expando.RewardPointsRemaining = order.RewardPointsRemaining;
			expando.HasNewPaymentNotification = order.HasNewPaymentNotification;
			expando.OrderStatus = order.OrderStatus.GetLocalizedEnum(_services.Localization, ctx.Projection.LanguageId ?? 0);
			expando.PaymentStatus = order.PaymentStatus.GetLocalizedEnum(_services.Localization, ctx.Projection.LanguageId ?? 0);
			expando.ShippingStatus = order.ShippingStatus.GetLocalizedEnum(_services.Localization, ctx.Projection.LanguageId ?? 0);

			expando.Customer = null;
			expando.BillingAddress = null;
			expando.ShippingAddress = null;
			expando.Store = null;
			expando.Shipments = null;
			expando.RedeemedRewardPointsEntry = ToExpando(ctx, order.RedeemedRewardPointsEntry);

			return expando as ExpandoObject;
		}

		private ExpandoObject ToExpando(ExportProfileTaskContext ctx, OrderItem orderItem)
		{
			if (orderItem == null)
				return null;

			dynamic expando = new ExpandoObject();
			expando._Entity = orderItem;

			expando.Id = orderItem.Id;
			expando.OrderItemGuid = orderItem.OrderItemGuid;
			expando.OrderId = orderItem.OrderId;
			expando.ProductId = orderItem.ProductId;
			expando.Quantity = orderItem.Quantity;
			expando.UnitPriceInclTax = orderItem.UnitPriceInclTax;
			expando.UnitPriceExclTax = orderItem.UnitPriceExclTax;
			expando.PriceInclTax = orderItem.PriceInclTax;
			expando.PriceExclTax = orderItem.PriceExclTax;
			expando.TaxRate = orderItem.TaxRate;
			expando.DiscountAmountInclTax = orderItem.DiscountAmountInclTax;
			expando.DiscountAmountExclTax = orderItem.DiscountAmountExclTax;
			expando.AttributeDescription = orderItem.AttributeDescription;
			expando.AttributesXml = orderItem.AttributesXml;
			expando.DownloadCount = orderItem.DownloadCount;
			expando.IsDownloadActivated = orderItem.IsDownloadActivated;
			expando.LicenseDownloadId = orderItem.LicenseDownloadId;
			expando.ItemWeight = orderItem.ItemWeight;
			expando.BundleData = orderItem.BundleData;
			expando.ProductCost = orderItem.ProductCost;

			expando.Product = ToExpando(ctx, orderItem.Product);

			return expando as ExpandoObject;
		}

		private ExpandoObject ToExpando(ExportProfileTaskContext ctx, Shipment shipment)
		{
			if (shipment == null)
				return null;

			dynamic expando = new ExpandoObject();
			expando._Entity = shipment;

			expando.Id = shipment.Id;
			expando.OrderId = shipment.OrderId;
			expando.TrackingNumber = shipment.TrackingNumber;
			expando.TotalWeight = shipment.TotalWeight;
			expando.ShippedDateUtc = shipment.ShippedDateUtc;
			expando.DeliveryDateUtc = shipment.DeliveryDateUtc;
			expando.CreatedOnUtc = shipment.CreatedOnUtc;

			expando.ShipmentItems = shipment.ShipmentItems
				.Select(x =>
				{
					dynamic exp = new ExpandoObject();
					exp._Entity = x;
					exp.Id = x.Id;
					exp.ShipmentId = x.ShipmentId;
					exp.OrderItemId = x.OrderItemId;
					exp.Quantity = x.Quantity;
					return exp as ExpandoObject;
				})
				.ToList();

			return expando as ExpandoObject;
		}

		private ExpandoObject ToExpando(ExportProfileTaskContext ctx, Discount discount)
		{
			if (discount == null)
				return null;

			dynamic expando = new ExpandoObject();
			expando._Entity = discount;

			expando.Id = discount.Id;
			expando.Name = discount.Name;
			expando.DiscountTypeId = discount.DiscountTypeId;
			expando.UsePercentage = discount.UsePercentage;
			expando.DiscountPercentage = discount.DiscountPercentage;
			expando.DiscountAmount = discount.DiscountAmount;
			expando.StartDateUtc = discount.StartDateUtc;
			expando.EndDateUtc = discount.EndDateUtc;
			expando.RequiresCouponCode = discount.RequiresCouponCode;
			expando.CouponCode = discount.CouponCode;
			expando.DiscountLimitationId = discount.DiscountLimitationId;
			expando.LimitationTimes = discount.LimitationTimes;

			return expando as ExpandoObject;
		}

		private ExpandoObject ToExpando(ExportProfileTaskContext ctx, ProductSpecificationAttribute psa)
		{
			if (psa == null)
				return null;

			dynamic expando = new ExpandoObject();
			expando._Entity = psa;

			expando.Id = psa.Id;
			expando.ProductId = psa.ProductId;
			expando.SpecificationAttributeOptionId = psa.SpecificationAttributeOptionId;
			expando.AllowFiltering = psa.AllowFiltering;
			expando.ShowOnProductPage = psa.ShowOnProductPage;
			expando.DisplayOrder = psa.DisplayOrder;

			var option = psa.SpecificationAttributeOption;

			expando.SpecificationAttributeOption._Entity = option;
			expando.SpecificationAttributeOption.Id = option.Id;
			expando.SpecificationAttributeOption.SpecificationAttributeId = option.SpecificationAttributeId;
			expando.SpecificationAttributeOption.Name = option.GetLocalized(x => x.Name, ctx.Projection.LanguageId ?? 0, true, false);
			expando.SpecificationAttributeOption.DisplayOrder = option.DisplayOrder;

			expando.SpecificationAttributeOption.SpecificationAttribute._Entity = option.SpecificationAttribute;
			expando.SpecificationAttributeOption.SpecificationAttribute.Id = option.SpecificationAttribute.Id;
			expando.SpecificationAttributeOption.SpecificationAttribute.Name = option.SpecificationAttribute.GetLocalized(x => x.Name, ctx.Projection.LanguageId ?? 0, true, false);
			expando.SpecificationAttributeOption.SpecificationAttribute.DisplayOrder = option.SpecificationAttribute.DisplayOrder;

			return expando as ExpandoObject;
		}

		#endregion

		#region Segmenter callbacks

		private List<Product> GetProducts(ExportProfileTaskContext ctx, int pageIndex)
		{
			var result = new List<Product>();

			var searchContext = GetProductSearchContext(ctx, pageIndex, ctx.PageSize);

			var products = _productService.SearchProducts(searchContext);

			foreach (var product in products)
			{
				if (product.ProductType == ProductType.SimpleProduct || product.ProductType == ProductType.BundledProduct)
				{
					result.Add(product);
				}
				else if (product.ProductType == ProductType.GroupedProduct)
				{
					if (ctx.IsPreview)
					{
						result.Add(product);
					}
					else
					{
						var associatedSearchContext = new ProductSearchContext
						{
							OrderBy = ProductSortingEnum.CreatedOn,
							PageSize = int.MaxValue,
							StoreId = (ctx.Profile.PerStore ? ctx.Store.Id : ctx.Filter.StoreId),
							VisibleIndividuallyOnly = false,
							ParentGroupedProductId = product.Id
						};

						foreach (var associatedProduct in _productService.SearchProducts(associatedSearchContext))
						{
							result.Add(associatedProduct);
						}
					}
				}
			}

			// load data behind navigation properties for current page in one go
			ctx.ProductDataContext = new ExportProductDataContext(products,
				x => _productAttributeService.GetProductVariantAttributesByProductIds(x, null),
				x => _productAttributeService.GetProductVariantAttributeCombinations(x),
				x => _productService.GetTierPrices(x, ctx.ProjectionCustomer, ctx.Store.Id),
				x => _categoryService.GetProductCategoriesByProductIds(x),
				x => _manufacturerService.GetProductManufacturersByProductIds(x),
				x => _productService.GetProductPicturesByProductIds(x),
				x => _productService.GetProductTagsByProductIds(x),
				x => _productService.GetAppliedDiscountsByProductIds(x),
				x => _productService.GetProductSpecificationAttributesByProductIds(x),
				x => _productService.GetBundleItemsByProductIds(x)
			);

			SetProgress(ctx, products.Count);

			try
			{
				_services.DbContext.DetachEntities<Product>(result);
			}
			catch { }

			return result;
		}

		private List<Order> GetOrders(ExportProfileTaskContext ctx, int pageIndex, int pageSize, out int totalCount)
		{
			var orders = _orderService.SearchOrders(
				ctx.Profile.PerStore ? ctx.Store.Id : ctx.Filter.StoreId,
				ctx.Projection.CustomerId ?? 0,
				ctx.Filter.CreatedFrom.HasValue ? (DateTime?)_dateTimeHelper.ConvertToUtcTime(ctx.Filter.CreatedFrom.Value, _dateTimeHelper.CurrentTimeZone) : null,
				ctx.Filter.CreatedTo.HasValue ? (DateTime?)_dateTimeHelper.ConvertToUtcTime(ctx.Filter.CreatedTo.Value, _dateTimeHelper.CurrentTimeZone) : null,
				ctx.Filter.OrderStatusIds,
				ctx.Filter.PaymentStatusIds,
				ctx.Filter.ShippingStatusIds,
				null,
				null,
				null,
				pageIndex,
				pageSize,
				null,
				ctx.EntityIdsSelected
			);

			totalCount = orders.TotalCount;

			if (ctx.Projection.OrderStatusChange != ExportOrderStatusChange.None)
			{
				ctx.EntityIdsLoaded = ctx.EntityIdsLoaded
					.Union(orders.Select(x => x.Id))
					.Distinct()
					.ToList();
			}

			var result = orders as List<Order>;

			if (pageSize > 1)
			{
				ctx.OrderDataContext = new ExportOrderDataContext(result,
					x => _customerService.GetCustomersByIds(x),
					x => _customerService.GetRewardPointsHistoriesByCustomerIds(x),
					x => _addressesService.GetAddressByIds(x),
					x => _orderService.GetOrderItemsByOrderIds(x),
					x => _shipmentService.GetShipmentsByOrderIds(x)
				);

				SetProgress(ctx, orders.Count);
			}

			try
			{
				_services.DbContext.DetachEntities<Order>(result);
			}
			catch { }

			return result;
		}

		private List<ExpandoObject> ConvertToExpando(ExportProfileTaskContext ctx, Product product)
		{
			var result = new List<ExpandoObject>();

			var productTemplate = ctx.ProductTemplates.FirstOrDefault(x => x.Key == product.ProductTemplateId);
			var pictureSize = _mediaSettings.ProductDetailsPictureSize;

			if (ctx.Supporting[ExportProjectionSupport.MainPictureUrl] && ctx.Projection.PictureSize > 0)
				pictureSize = ctx.Projection.PictureSize;

			var productPictures = ctx.ProductDataContext.ProductPictures.Load(product.Id);
			var productManufacturers = ctx.ProductDataContext.ProductManufacturers.Load(product.Id);
			var productCategories = ctx.ProductDataContext.ProductCategories.Load(product.Id);
			var productAttributes = ctx.ProductDataContext.Attributes.Load(product.Id);
			var productAttributeCombinations = ctx.ProductDataContext.AttributeCombinations.Load(product.Id);

			dynamic expando = ToExpando(ctx, product);

			#region gerneral data

			expando._ProductTemplateViewPath = (productTemplate.Value == null ? "" : productTemplate.Value.ViewPath);

			expando._DetailUrl = ctx.Store.Url + (string)expando.SeName;

			expando._CategoryName = null;

			expando._CategoryPath = _categoryService.GetCategoryPath(
				product,
				null,
				x => ctx.CategoryPathes.ContainsKey(x) ? ctx.CategoryPathes[x] : null,
				(id, value) => ctx.CategoryPathes[id] = value,
				x => ctx.Categories.ContainsKey(x) ? ctx.Categories[x] : _categoryService.GetCategoryById(x),
				productCategories.OrderBy(x => x.DisplayOrder).FirstOrDefault()
			);

			expando.ProductPictures = productPictures
				.OrderBy(x => x.DisplayOrder)
				.Select(x =>
				{
					dynamic exp = new ExpandoObject();
					exp._Entity = x;
					exp.Id = x.Id;
					exp.DisplayOrder = x.DisplayOrder;
					exp.Picture = ToExpando(ctx, x.Picture, _mediaSettings.ProductThumbPictureSize, pictureSize);

					return exp as ExpandoObject;
				})
				.ToList();

			expando.ProductManufacturers = productManufacturers
				.OrderBy(x => x.DisplayOrder)
				.Select(x =>
				{
					dynamic exp = new ExpandoObject();
					exp._Entity = x;
					exp.Id = x.Id;
					exp.DisplayOrder = x.DisplayOrder;
					exp.IsFeaturedProduct = x.IsFeaturedProduct;
					exp.Manufacturer = ToExpando(ctx, x.Manufacturer);

					if (x.Manufacturer != null && x.Manufacturer.PictureId.HasValue)
						exp.Manufacturer.Picture = ToExpando(ctx, x.Manufacturer.Picture, _mediaSettings.ManufacturerThumbPictureSize, _mediaSettings.ManufacturerThumbPictureSize);
					else
						exp.Manufacturer.Picture = null;

					return exp as ExpandoObject;
				})
				.ToList();

			expando.ProductCategories = productCategories
				.OrderBy(x => x.DisplayOrder)
				.Select(x =>
				{
					dynamic exp = new ExpandoObject();
					exp._Entity = x;
					exp.Id = x.Id;
					exp.DisplayOrder = x.DisplayOrder;
					exp.IsFeaturedProduct = x.IsFeaturedProduct;
					exp.Category = ToExpando(ctx, x.Category);

					if (x.Category != null && x.Category.PictureId.HasValue)
						exp.Category.Picture = ToExpando(ctx, x.Category.Picture, _mediaSettings.CategoryThumbPictureSize, _mediaSettings.CategoryThumbPictureSize);
					else
						exp.Category.Picture = null;

					if (expando._CategoryName == null)
						expando._CategoryName = (string)exp.Category.Name;

					return exp as ExpandoObject;
				})
				.ToList();

			expando.ProductAttributes = productAttributes
				.OrderBy(x => x.DisplayOrder)
				.Select(x => ToExpando(ctx, x))
				.ToList();

			expando.ProductAttributeCombinations = productAttributeCombinations
				.Select(x =>
				{
					dynamic exp = ToExpando(ctx, x);
					var assignedPictures = new List<ExpandoObject>();

					GetDeliveryTimeAndQuantityUnit(ctx, expando, x.DeliveryTimeId, x.QuantityUnitId);

					foreach (int pictureId in x.GetAssignedPictureIds())
					{
						var assignedPicture = productPictures.FirstOrDefault(y => y.PictureId == pictureId);
						if (assignedPicture != null && assignedPicture.Picture != null)
						{
							assignedPictures.Add(ToExpando(ctx, assignedPicture.Picture, _mediaSettings.ProductThumbPictureSize, pictureSize));
						}
					}

					exp.Pictures = assignedPictures;

					return exp as ExpandoObject;
				})
				.ToList();

			if (product.HasTierPrices)
			{
				var tierPrices = ctx.ProductDataContext.TierPrices.Load(product.Id);

				expando.TierPrices = tierPrices
					.Select(x =>
					{
						dynamic exp = new ExpandoObject();
						exp._Entity = x;
						exp.Id = x.Id;
						exp.ProductId = x.ProductId;
						exp.StoreId = x.StoreId;
						exp.CustomerRoleId = x.CustomerRoleId;
						exp.Quantity = x.Quantity;
						exp.Price = x.Price;
						return exp as ExpandoObject;
					})
					.ToList();
			}
			else
			{
				expando.TierPrices = new List<ExpandoObject>();
			}

			if (product.HasDiscountsApplied)
			{
				var appliedDiscounts = ctx.ProductDataContext.AppliedDiscounts.Load(product.Id);

				expando.AppliedDiscounts = appliedDiscounts
					.Select(x => ToExpando(ctx, x))
					.ToList();
			}
			else
			{
				expando.AppliedDiscounts = new List<ExpandoObject>();
			}

			#endregion

			#region high data depth

			if (ctx.Supporting[ExportProjectionSupport.HighDataDepth])
			{
				var productTags = ctx.ProductDataContext.ProductTags.Load(product.Id);
				var specificationAttributes = ctx.ProductDataContext.ProductSpecificationAttributes.Load(product.Id);

				expando.ProductTags = productTags
					.Select(x =>
					{
						dynamic exp = new ExpandoObject();
						exp._Entity = x;
						exp.Id = x.Id;
						exp.Name = x.GetLocalized(y => y.Name, ctx.Projection.LanguageId ?? 0, true, false);
						return exp as ExpandoObject;
					})
					.ToList();

				expando.ProductSpecificationAttributes = specificationAttributes
					.Select(x => ToExpando(ctx, x))
					.ToList();

				if (product.ProductType == ProductType.BundledProduct)
				{
					var bundleItems = ctx.ProductDataContext.ProductBundleItems.Load(product.Id);

					expando.ProductBundleItems = bundleItems
						.Select(x =>
						{
							var name = x.GetLocalized(y => y.Name, ctx.Projection.LanguageId ?? 0, true, false);
							if (name.IsEmpty())
								name = (string)expando.Name;

							dynamic exp = new ExpandoObject();
							exp._Entity = x;
							exp.Id = x.Id;
							exp.ProductId = x.ProductId;
							exp.BundleProductId = x.BundleProductId;
							exp.Quantity = x.Quantity;
							exp.Discount = x.Discount;
							exp.DiscountPercentage = x.DiscountPercentage;
							exp.Name = name;
							exp.ShortDescription = x.GetLocalized(y => y.ShortDescription, ctx.Projection.LanguageId ?? 0, true, false);
							exp.FilterAttributes = x.FilterAttributes;
							exp.HideThumbnail = x.HideThumbnail;
							exp.Visible = x.Visible;
							exp.Published = x.Published;
							exp.DisplayOrder = x.DisplayOrder;
							exp.CreatedOnUtc = x.CreatedOnUtc;
							exp.UpdatedOnUtc = x.UpdatedOnUtc;
							return exp as ExpandoObject;
						})
						.ToList();
				}
				else
				{
					expando.ProductBundleItems = new List<ExpandoObject>();
				}
			}
			else
			{
				expando.ProductTags = new List<ExpandoObject>();
				expando.ProductSpecificationAttributes = new List<ExpandoObject>();
				expando.ProductBundleItems = new List<ExpandoObject>();
			}

			#endregion

			#region more attribute controlled data

			if (ctx.Supporting[ExportProjectionSupport.Description])
			{
				PrepareProductDescription(ctx, expando, product);
			}

			if (ctx.Supporting[ExportProjectionSupport.Brand])
			{
				string brand = null;
				var productManus = ctx.ProductDataContext.ProductManufacturers.Load(product.Id);

				if (productManus != null && productManus.Any())
					brand = productManus.First().Manufacturer.GetLocalized(x => x.Name, ctx.Projection.LanguageId ?? 0, true, false);

				if (brand.IsEmpty())
					brand = ctx.Projection.Brand;

				expando._Brand = brand;
			}

			if (ctx.Supporting[ExportProjectionSupport.MainPictureUrl])
			{
				if (productPictures != null && productPictures.Any())
					expando._MainPictureUrl = _pictureService.GetPictureUrl(productPictures.First().Picture, ctx.Projection.PictureSize, storeLocation: ctx.Store.Url);
				else
					expando._MainPictureUrl = _pictureService.GetDefaultPictureUrl(ctx.Projection.PictureSize, storeLocation: ctx.Store.Url);
			}

			#endregion

			#region matter of data merging

			Action<dynamic, ProductVariantAttributeCombination> matterOfDataMerging = (exp, combination) =>
			{
				product.MergeWithCombination(combination);

				exp.Price = CalculatePrice(ctx, product, combination != null);
				exp.StockQuantity = product.StockQuantity;
				exp.BackorderModeId = product.BackorderModeId;
				exp.Sku = product.Sku;
				exp.Gtin = product.Gtin;
				exp.ManufacturerPartNumber = product.ManufacturerPartNumber;
				exp.DeliveryTimeId = product.DeliveryTimeId;
				exp.QuantityUnitId = product.QuantityUnitId;
				exp.Length = product.Length;
				exp.Width = product.Width;
				exp.Height = product.Height;
				exp.BasePriceAmount = product.BasePriceAmount;
				exp.BasePriceBaseAmount = product.BasePriceBaseAmount;

				if (combination != null && ctx.Projection.AttributeCombinationValueMerging == ExportAttributeValueMerging.AppendAllValuesToName)
				{
					var values = _productAttributeParser.ParseProductVariantAttributeValues(combination.AttributesXml, productAttributes, ctx.Projection.LanguageId ?? 0);
					exp.Name = ((string)exp.Name).Grow(string.Join(", ", values), " ");
				}

				exp._BasePriceInfo = product.GetBasePriceInfo(_services.Localization, _priceFormatter, decimal.Zero, true);

				// navigation properties
				GetDeliveryTimeAndQuantityUnit(ctx, exp, product.DeliveryTimeId, product.QuantityUnitId);

				if (ctx.Supporting[ExportProjectionSupport.UseOwnProductNo] && product.ManufacturerPartNumber.IsEmpty())
				{
					exp.ManufacturerPartNumber = product.Sku;
				}

				if (ctx.Supporting[ExportProjectionSupport.ShippingTime])
				{
					dynamic deliveryTime = exp.DeliveryTime;
					exp._ShippingTime = (deliveryTime == null ? ctx.Projection.ShippingTime : deliveryTime.Name);
				}

				if (ctx.Supporting[ExportProjectionSupport.ShippingCosts])
				{
					exp._FreeShippingThreshold = ctx.Projection.FreeShippingThreshold;

					if (product.IsFreeShipping || (ctx.Projection.FreeShippingThreshold.HasValue && (decimal)exp.Price >= ctx.Projection.FreeShippingThreshold.Value))
						exp._ShippingCosts = decimal.Zero;
					else
						exp._ShippingCosts = ctx.Projection.ShippingCosts;
				}

				if (ctx.Supporting[ExportProjectionSupport.OldPrice])
				{
					if (product.OldPrice != decimal.Zero && product.OldPrice != (decimal)exp.Price && !(product.ProductType == ProductType.BundledProduct && product.BundlePerItemPricing))
					{
						if (ctx.Projection.ConvertNetToGrossPrices)
						{
							decimal taxRate;
							exp._OldPrice = _taxService.GetProductPrice(product, product.OldPrice, true, ctx.ProjectionCustomer, out taxRate);
						}
						else
						{
							exp._OldPrice = product.OldPrice;
						}
					}
					else
					{
						exp._OldPrice = null;
					}
				}

				if (ctx.Supporting[ExportProjectionSupport.SpecialPrice])
				{
					exp._SpecialPrice = null;
					exp._RegularPrice = null;	// price if a special price would not exist

					if (!(product.ProductType == ProductType.BundledProduct && product.BundlePerItemPricing))
					{
						var specialPrice = _priceCalculationService.GetSpecialPrice(product);

						exp._SpecialPrice = ConvertPrice(ctx, product, specialPrice);

						if (specialPrice.HasValue)
						{
							decimal tmpSpecialPrice = product.SpecialPrice.Value;
							product.SpecialPrice = null;
							exp._RegularPrice = CalculatePrice(ctx, product, combination != null);
							product.SpecialPrice = tmpSpecialPrice;
						}
					}
				}
			};

			#endregion

			if (ctx.Projection.AttributeCombinationAsProduct && productAttributeCombinations.Where(x => x.IsActive).Count() > 0)
			{
				// EF does not support entities to be cconstructed in a LINQ to entities query.
				// So it's not possible to join-query attribute combinations and products without losing ProductSearchContext ability.
				// We are reduced to somewhat compound data here.

				foreach (var combination in productAttributeCombinations.Where(x => x.IsActive))
				{
					var expandoCombination = ((IDictionary<string, object>)expando).ToExpandoObject();	// clone

					matterOfDataMerging(expandoCombination, combination);
					result.Add(expandoCombination);
				}
			}
			else
			{
				matterOfDataMerging(expando, null);
				result.Add(expando);
			}

			return result;
		}

		private List<ExpandoObject> ConvertToExpando(ExportProfileTaskContext ctx, Order order)
		{
			var result = new List<ExpandoObject>();

			ctx.OrderDataContext.Addresses.Collect(order.ShippingAddressId ?? 0);

			var addresses = ctx.OrderDataContext.Addresses.Load(order.BillingAddressId);
			var customers = ctx.OrderDataContext.Customers.Load(order.CustomerId);
			var rewardPointsHistories = ctx.OrderDataContext.RewardPointsHistories.Load(order.CustomerId);
			var orderItems = ctx.OrderDataContext.OrderItems.Load(order.Id);
			var shipments = ctx.OrderDataContext.Shipments.Load(order.Id);

			dynamic expando = ToExpando(ctx, order);

			if (ctx.Stores.ContainsKey(order.StoreId))
			{
				expando.Store = ToExpando(ctx, ctx.Stores[order.StoreId]);
			}

			expando.Customer = ToExpando(ctx, customers.FirstOrDefault(x => x.Id == order.CustomerId));

			expando.Customer.RewardPointsHistory = rewardPointsHistories
				.Select(x => ToExpando(ctx, x))
				.ToList();

			if (rewardPointsHistories.Count > 0)
			{
				expando.Customer._RewardPointsBalance = rewardPointsHistories
					.OrderByDescending(x => x.CreatedOnUtc)
					.ThenByDescending(x => x.Id)
					.FirstOrDefault()
					.PointsBalance;
			}

			expando.BillingAddress = ToExpando(ctx, addresses.FirstOrDefault(x => x.Id == order.BillingAddressId));

			if (order.ShippingAddressId.HasValue)
			{
				expando.ShippingAddress = ToExpando(ctx, addresses.FirstOrDefault(x => x.Id == order.ShippingAddressId.Value));
			}

			expando.OrderItems = orderItems
				.Select(e =>
				{
					dynamic exp = ToExpando(ctx, e);
					var productTemplate = ctx.ProductTemplates.FirstOrDefault(x => x.Key == e.Product.ProductTemplateId);

					exp.Product._ProductTemplateViewPath = (productTemplate.Value == null ? "" : productTemplate.Value.ViewPath);

					exp.Product._BasePriceInfo = e.Product.GetBasePriceInfo(_services.Localization, _priceFormatter, decimal.Zero, true);

					GetDeliveryTimeAndQuantityUnit(ctx, exp.Product, e.Product.DeliveryTimeId, e.Product.QuantityUnitId);

					return exp as ExpandoObject;
				})
				.ToList();

			expando.Shipments = shipments
				.Select(x => ToExpando(ctx, x))
				.ToList();

			result.Add(expando);
			return result;
		}

		#endregion

		private List<Store> Init(ExportProfileTaskContext ctx)
		{
			List<Store> result = null;

			if (ctx.Projection.CurrencyId.HasValue)
				ctx.ProjectionCurrency = _currencyService.GetCurrencyById(ctx.Projection.CurrencyId.Value);
			else
				ctx.ProjectionCurrency = _services.WorkContext.WorkingCurrency;

			if (ctx.Projection.CustomerId.HasValue)
				ctx.ProjectionCustomer = _customerService.GetCustomerById(ctx.Projection.CustomerId.Value);
			else
				ctx.ProjectionCustomer = _services.WorkContext.CurrentCustomer;

			if (ctx.Projection.LanguageId.HasValue)
				ctx.ProjectionLanguage = _languageService.GetLanguageById(ctx.Projection.LanguageId.Value);
			else
				ctx.ProjectionLanguage = _services.WorkContext.WorkingLanguage;

			ctx.Stores = _services.StoreService.GetAllStores().ToDictionary(x => x.Id, x => x);

			if (!ctx.IsPreview && ctx.Profile.PerStore)
			{
				result = new List<Store>(ctx.Stores.Values.Where(x => x.Id == ctx.Filter.StoreId || ctx.Filter.StoreId == 0));
			}
			else
			{
				int? storeId = (ctx.Filter.StoreId == 0 ? ctx.Projection.StoreId : ctx.Filter.StoreId);

				ctx.Store = ctx.Stores.Values.FirstOrDefault(x => x.Id == (storeId ?? _services.StoreContext.CurrentStore.Id));

				result = new List<Store> { ctx.Store };
			}

			// get total records for progress
			foreach (var store in result)
			{
				if (ctx.TotalRecords.HasValue)
				{
					ctx.RecordsPerStore.Add(store.Id, ctx.TotalRecords.Value);
				}
				else if (ctx.Provider.Value.EntityType == ExportEntityType.Product)
				{
					ctx.Store = store;
					var searchContext = GetProductSearchContext(ctx, ctx.Profile.Offset, 1);
					var anySingleProduct = _productService.SearchProducts(searchContext);
					ctx.RecordsPerStore.Add(store.Id, anySingleProduct.TotalCount);
				}
				else if (ctx.Provider.Value.EntityType == ExportEntityType.Order)
				{
					ctx.Store = store;
					int totalCount = 0;
					var unused = GetOrders(ctx, 0, 1, out totalCount);
					ctx.RecordsPerStore.Add(store.Id, totalCount);
				}
			}

			return result;
		}

		private void ExportCoreInner(ExportProfileTaskContext ctx, Store store)
		{
			if (ctx.Export.Abort != ExportAbortion.None)
				return;

			ctx.Store = store;

			{
				var logHead = new StringBuilder();
				logHead.AppendLine();
				logHead.AppendLine(new string('-', 40));
				logHead.AppendLine("SmartStore.NET:\t\tv." + SmartStoreVersion.CurrentFullVersion);
				logHead.AppendLine("Export profile:\t\t{0} (Id {1})".FormatInvariant(ctx.Profile.Name, ctx.Profile.Id));

				var plugin = ctx.Provider.Metadata.PluginDescriptor;
				logHead.Append("Plugin:\t\t\t\t");
				logHead.AppendLine(plugin == null ? "".NaIfEmpty() : "{0} ({1}) v.{2}".FormatInvariant(plugin.FriendlyName, plugin.SystemName, plugin.Version.ToString()));

				logHead.AppendLine("Export provider:\t{0} ({1})".FormatInvariant(ctx.Provider == null ? "".NaIfEmpty() : ctx.Provider.Metadata.FriendlyName, ctx.Profile.ProviderSystemName));

				var storeInfo = (ctx.Profile.PerStore ? "{0} (Id {1})".FormatInvariant(ctx.Store.Name, ctx.Store.Id) : "all stores");
				logHead.Append("Store:\t\t\t\t" + storeInfo);

				ctx.Log.Information(logHead.ToString());
			}

			ctx.Export.Store = ToExpando(ctx, ctx.Store);

			ctx.Export.MaxFileNameLength = _dataExchangeSettings.MaxFileNameLength;

			ctx.Export.FileExtension = (ctx.Provider.Value.FileExtension.HasValue() ? ctx.Provider.Value.FileExtension.ToLower().EnsureStartsWith(".") : "");

			ctx.Export.HasPublicDeployment = ctx.Profile.Deployments.Any(x => x.IsPublic && x.DeploymentType == ExportDeploymentType.FileSystem);

			ctx.Export.PublicFolderPath = (ctx.Export.HasPublicDeployment ? Path.Combine(HttpRuntime.AppDomainAppPath, PublicFolder) : null);

			var totalCount = ctx.RecordsPerStore.First(x => x.Key == ctx.Store.Id).Value;

			if (ctx.Provider.Value.EntityType == ExportEntityType.Product)
			{
				ctx.Export.Data = new ExportSegmenter<Product>(
					pageIndex => GetProducts(ctx, pageIndex),
					entity => ConvertToExpando(ctx, entity),
					new PagedList(ctx.Profile.Offset, ctx.Profile.Limit, ctx.PageIndex, ctx.PageSize, totalCount),
					ctx.IsPreview ? 0 : ctx.Profile.BatchSize
				);
			}
			else if (ctx.Provider.Value.EntityType == ExportEntityType.Order)
			{
				int unused;

				ctx.Export.Data = new ExportSegmenter<Order>(
					pageIndex => GetOrders(ctx, pageIndex, ctx.PageSize, out unused),
					entity => ConvertToExpando(ctx, entity),
					new PagedList(ctx.Profile.Offset, ctx.Profile.Limit, ctx.PageIndex, ctx.PageSize, totalCount),
					ctx.IsPreview ? 0 : ctx.Profile.BatchSize
				);
			}

			if (ctx.Export.Data == null)
			{
				throw new SmartException("Unsupported entity type '{0}'".FormatInvariant(ctx.Provider.Value.EntityType.ToString()));
			}
			else
			{
				(ctx.Export.Data as IExportExecuter).Start(() =>
				{
					ctx.Export.RecordsSucceeded = 0;

					if (!ctx.IsPreview)
					{
						if (ctx.IsFileBasedExport)
						{
							var resolvedPattern = ctx.Profile.ResolveFileNamePattern(ctx.Store, ctx.Export.Data.FileIndex + 1, ctx.Export.MaxFileNameLength);

							ctx.Export.FileName = resolvedPattern + ctx.Export.FileExtension;
							ctx.Export.FilePath = Path.Combine(ctx.Export.Folder, ctx.Export.FileName);

							if (ctx.Export.HasPublicDeployment)
								ctx.Export.PublicFileUrl = ctx.Store.Url.EnsureEndsWith("/") + PublicFolder.EnsureEndsWith("/") + ctx.Export.FileName;
						}

						try
						{
							ctx.Provider.Value.Execute(ctx.Export);

							ctx.Log.Information("Provider reports {0} successful exported record(s)".FormatInvariant(ctx.Export.RecordsSucceeded));

							// create info for deployment list in profile edit
							if (ctx.IsFileBasedExport && File.Exists(ctx.Export.FilePath))
							{
								ctx.Result.Files.Add(new ExportExecuteResult.ExportFileInfo
								{
									StoreId = ctx.Store.Id,
									FileName = ctx.Export.FileName
								});
							}
						}
						catch (Exception exc)
						{
							ctx.Export.Abort = ExportAbortion.Hard;
							ctx.Log.Error("The provider failed to execute the export: " + exc.ToAllMessages(), exc);
							ctx.Result.LastError = exc.ToString();
						}
					}
					else if (ctx.Export.Data.ReadNextSegment())
					{
						var segment = ctx.Export.Data.CurrentSegment;

						foreach (dynamic record in segment)
						{
							ctx.PreviewData(record);
						}
					}

					if (ctx.Export.IsMaxFailures)
						ctx.Log.Warning("Export aborted. The maximum number of failures has been reached");

					if (ctx.TaskContext.CancellationToken.IsCancellationRequested)
						ctx.Log.Warning("Export aborted. A cancellation has been requested");

					return (ctx.Export.Abort == ExportAbortion.None);
				});

				if (ctx.Export.Abort != ExportAbortion.Hard)
				{
					ctx.Provider.Value.ExecuteEnded(ctx.Export);
				}
			}
		}

		private void ExportCoreOuter(ExportProfileTaskContext ctx)
		{
			if (ctx.Profile == null || !ctx.Profile.Enabled)
				return;

			FileSystemHelper.Delete(ctx.LogPath);

			if (!ctx.IsPreview)
			{
				FileSystemHelper.ClearDirectory(ctx.FolderContent, false);
				FileSystemHelper.Delete(ctx.ZipPath);
			}

			using (var logger = new TraceLogger(ctx.LogPath))
			{
				try
				{
					if (ctx.Provider == null)
					{
						throw new SmartException("Export aborted because the export provider cannot be loaded");
					}

					if (!ctx.Provider.IsValid())
					{
						throw new SmartException("Export aborted because the export provider is not valid");
					}

					ctx.Log = logger;
					ctx.Export.Log = logger;
					ctx.ProgressInfo = _services.Localization.GetResource("Admin.DataExchange.Export.ProgressInfo");

					if (ctx.Profile.ProviderConfigData.HasValue())
					{
						var configInfo = ctx.Provider.Value.ConfigurationInfo;
						if (configInfo != null)
						{
							ctx.Export.ConfigurationData = XmlHelper.Deserialize(ctx.Profile.ProviderConfigData, configInfo.ModelType);
						}
					}

					using (var scope = new DbContextScope(_services.DbContext, autoDetectChanges: false, proxyCreation: true, validateOnSave: false, forceNoTracking: true))
					{
						ctx.DeliveryTimes = _deliveryTimeService.GetAllDeliveryTimes().ToDictionary(x => x.Id, x => x);
						ctx.QuantityUnits = _quantityUnitService.GetAllQuantityUnits().ToDictionary(x => x.Id, x => x);
						ctx.ProductTemplates = _productTemplateService.GetAllProductTemplates().ToDictionary(x => x.Id, x => x);

						ctx.Localized.DeliveryTimes = _localizedEntityService.GetLocalizedProperties("DeliveryTime", null).ToMultimap(x => x.EntityId, x => x);
						ctx.Localized.QuantityUnits = _localizedEntityService.GetLocalizedProperties("QuantityUnit", null).ToMultimap(x => x.EntityId, x => x);
						ctx.Localized.ProductAttributes = _localizedEntityService.GetLocalizedProperties("ProductAttribute", null).ToMultimap(x => x.EntityId, x => x);

						if (ctx.Provider.Value.EntityType == ExportEntityType.Product)
						{
							var allCategories = _categoryService.GetAllCategories(showHidden: true, applyNavigationFilters: false);
							ctx.Categories = allCategories.ToDictionary(x => x.Id);
						}

						if (ctx.Provider.Value.EntityType == ExportEntityType.Order)
						{
							ctx.Countries = _countryService.GetAllCountries(true).ToDictionary(x => x.Id, x => x);
						}

						var stores = Init(ctx);

						ctx.Export.Language = ToExpando(ctx, ctx.ProjectionLanguage);
						ctx.Export.Customer = ToExpando(ctx, ctx.ProjectionCustomer);
						ctx.Export.Currency = ToExpando(ctx, ctx.ProjectionCurrency);

						stores.ForEach(x => ExportCoreInner(ctx, x));
					}

					if (!ctx.IsPreview && ctx.Export.Abort != ExportAbortion.Hard)
					{
						if (ctx.IsFileBasedExport)
						{
							if (ctx.Profile.CreateZipArchive || ctx.Profile.Deployments.Any(x => x.Enabled && x.CreateZip))
							{
								ZipFile.CreateFromDirectory(ctx.FolderContent, ctx.ZipPath, CompressionLevel.Fastest, true);
							}

							foreach (var deployment in ctx.Profile.Deployments.OrderBy(x => x.DeploymentTypeId).Where(x => x.Enabled))
							{
								try
								{
									switch (deployment.DeploymentType)
									{
										case ExportDeploymentType.FileSystem:
											DeployFileSystem(ctx, deployment);
											break;
										case ExportDeploymentType.Email:
											DeployEmail(ctx, deployment);
											break;
										case ExportDeploymentType.Http:
											DeployHttp(ctx, deployment);
											break;
										case ExportDeploymentType.Ftp:
											DeployFtp(ctx, deployment);
											break;
									}
								}
								catch (Exception exc)
								{
									logger.Error("Deployment \"{0}\" of type {1} failed.".FormatInvariant(deployment.Name, deployment.DeploymentType.ToString()), exc);
								}
							}
						}

						if (ctx.Profile.EmailAccountId != 0 && ctx.Profile.CompletedEmailAddresses.HasValue())
						{
							SendCompletionEmail(ctx);
						}
					}
				}
				catch (Exception exc)
				{
					logger.Error(exc);
					ctx.Result.LastError = exc.ToString();
				}
				finally
				{
					try
					{
						if (!ctx.IsPreview && ctx.Profile.Id != 0)
						{
							ctx.Profile.ResultInfo = XmlHelper.Serialize<ExportExecuteResult>(ctx.Result);

							_exportService.UpdateExportProfile(ctx.Profile);
						}
					}
					catch { }

					try
					{
						if (!ctx.IsPreview && ctx.IsFileBasedExport && ctx.Export.Abort != ExportAbortion.Hard && ctx.Profile.Cleanup)
						{
							FileSystemHelper.ClearDirectory(ctx.FolderContent, false);
						}
					}
					catch { }

					try
					{
						ctx.Localized.DeliveryTimes.Clear();
						ctx.Localized.QuantityUnits.Clear();
						ctx.Localized.ProductAttributes.Clear();
						ctx.ProductTemplates.Clear();
						ctx.Countries.Clear();
						ctx.Stores.Clear();
						ctx.QuantityUnits.Clear();
						ctx.DeliveryTimes.Clear();
						ctx.CategoryPathes.Clear();
						ctx.Categories.Clear();
						ctx.EntityIdsSelected.Clear();
						ctx.ProductDataContext = null;
						ctx.OrderDataContext = null;

						(ctx.Export.Data as IExportExecuter).Dispose();

						ctx.Export.CustomProperties.Clear();
						ctx.Export.Log = null;
						ctx.Log = null;
					}
					catch { }
				}
			}

			if (ctx.IsPreview || ctx.Export.Abort == ExportAbortion.Hard)
				return;

			// post process order entities
			if (ctx.EntityIdsLoaded.Count > 0 && ctx.Provider.Value.EntityType == ExportEntityType.Order && ctx.Projection.OrderStatusChange != ExportOrderStatusChange.None)
			{
				using (var logger = new TraceLogger(ctx.LogPath))
				{
					try
					{
						int? orderStatusId = null;

						if (ctx.Projection.OrderStatusChange == ExportOrderStatusChange.Processing)
							orderStatusId = (int)OrderStatus.Processing;
						else if (ctx.Projection.OrderStatusChange == ExportOrderStatusChange.Complete)
							orderStatusId = (int)OrderStatus.Complete;

						using (var scope = new DbContextScope(_services.DbContext, false, null, false, false, false, false))
						{
							foreach (var chunk in ctx.EntityIdsLoaded.Chunk())
							{
								var entities = _orderRepository.Table.Where(x => chunk.Contains(x.Id)).ToList();

								entities.ForEach(x => x.OrderStatusId = (orderStatusId ?? x.OrderStatusId));

								_services.DbContext.SaveChanges();
							}
						}

						logger.Information("Updated order status for {0} order(s).".FormatInvariant(ctx.EntityIdsLoaded.Count()));
					}
					catch (Exception exc)
					{
						logger.Error(exc);
						ctx.Result.LastError = exc.ToString();
					}
				}
			}
		}


		/// <summary>
		/// The name of the public export folder
		/// </summary>
		public static string PublicFolder
		{
			get { return "Exchange"; }
		}

		/// <summary>
		/// Exports with a volatile profile
		/// </summary>
		/// <param name="providerSystemName">Provider system name</param>
		/// <param name="selectedEntityIds">Entity identifiers of entities to be exported. Can be <c>null</c> to export all entities.</param>
		/// <param name="error">Last error</param>
		/// <returns>File stream result of the first export data file</returns>
		public static FileStreamResult Export(string providerSystemName, string selectedEntityIds, string downloadFileName, out string error)
		{
			Guard.ArgumentNotEmpty(() => providerSystemName);

			error = null;
			var cancellation = new CancellationTokenSource(TimeSpan.FromHours(3.0));

			var task = AsyncRunner.Run<ExportExecuteResult>((container, ct) =>
			{
				var exportTask = new ExportProfileTask();
				return exportTask.Execute(providerSystemName, container, ct, null, selectedEntityIds);
			},
			cancellation.Token, TaskCreationOptions.LongRunning, TaskScheduler.Default);

			task.Wait();

			if (task.Result != null && task.Result.Succeeded && task.Result.FileFolder.HasValue() && task.Result.Files.Count > 0)
			{
				var fileName = task.Result.Files.First().FileName;
				var filePath = Path.Combine(task.Result.FileFolder, fileName);

				var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
				var result = new FileStreamResult(stream, MimeTypes.MapNameToMimeType(fileName));

				if (downloadFileName.IsEmpty())
					downloadFileName = providerSystemName;
				
				if (selectedEntityIds.HasValue() && !selectedEntityIds.Contains(","))
					downloadFileName = string.Concat(downloadFileName, "-", selectedEntityIds);

				result.FileDownloadName = downloadFileName.ToValidFileName() + Path.GetExtension(filePath);

				return result;
			}

			if (task.Result != null)
				error = task.Result.LastError;

			return null;
		}

		/// <summary>
		/// Executes the export profile task
		/// </summary>
		/// <param name="taskContext"></param>
		public void Execute(TaskExecutionContext taskContext)
		{
			InitDependencies(taskContext);

			var profileId = taskContext.ScheduleTask.Alias.ToInt();
			var profile = _exportService.GetExportProfileById(profileId);

			var selectedIdsCacheKey = profile.GetSelectedEntityIdsCacheKey();
			var selectedEntityIds = HttpRuntime.Cache[selectedIdsCacheKey] as string;

			var ctx = new ExportProfileTaskContext(taskContext, profile, _exportService.LoadProvider(profile.ProviderSystemName), selectedEntityIds);

			HttpRuntime.Cache.Remove(selectedIdsCacheKey);

			ExportCoreOuter(ctx);

			taskContext.CancellationToken.ThrowIfCancellationRequested();
		}

		/// <summary>
		/// Executes the export profile task with a volatile profile
		/// </summary>
		/// <param name="providerSystemName">Provider system name</param>
		/// <param name="context">Component context</param>
		/// <param name="cancellationToken">Cancellation token</param>
		/// <param name="providerConfigData">Provider specific configuration data. Cane be <c>null</c>.</param>
		/// <param name="selectedEntityIds">Entity identifiers of entities to be exported. Can be <c>null</c> to export all entities.</param>
		/// <param name="storeId">Store identifier</param>
		/// <returns>Export execute result</returns>
		public ExportExecuteResult Execute(string providerSystemName, IComponentContext context, CancellationToken cancellationToken, 
			string providerConfigData = null, string selectedEntityIds = null, int storeId = 0)
		{
			Guard.ArgumentNotEmpty(() => providerSystemName);
			Guard.ArgumentNotNull(() => context);

			var taskContext = new TaskExecutionContext(context, null);
			taskContext.CancellationToken = cancellationToken;

			InitDependencies(taskContext);

			var provider = _exportService.LoadProvider(providerSystemName);

			var profile = _exportService.CreateVolatileProfile(provider, providerConfigData, storeId);

			var ctx = new ExportProfileTaskContext(taskContext, profile, provider, selectedEntityIds);

			ExportCoreOuter(ctx);

			return ctx.Result;
		}

		/// <summary>
		/// Executes the export profile task to get preview data
		/// </summary>
		/// <param name="profile">Export profile</param>
		/// <param name="context">Component context</param>
		/// <param name="pageIndex">Page index</param>
		/// <param name="pageSize">Page size</param>
		/// <param name="totalRecords">Number of total records</param>
		/// <param name="previewData">Action to process preview data</param>
		public void Preview(ExportProfile profile, IComponentContext context, int pageIndex, int pageSize, int totalRecords, Action<dynamic> previewData)
		{
			Guard.ArgumentNotNull(() => profile);
			Guard.ArgumentNotNull(() => context);

			var taskContext = new TaskExecutionContext(context, null);
			var cancellation = new CancellationTokenSource(TimeSpan.FromMinutes(5.0));

			taskContext.CancellationToken = cancellation.Token;

			InitDependencies(taskContext);

			var ctx = new ExportProfileTaskContext(taskContext, profile, _exportService.LoadProvider(profile.ProviderSystemName), null, 
				pageIndex, pageSize, totalRecords, previewData);

			ExportCoreOuter(ctx);
		}

		/// <summary>
		/// Get the number of total records
		/// </summary>
		/// <param name="profile">Export profile</param>
		/// <param name="provider">Export provider</param>
		/// <param name="context">Component context</param>
		/// <returns>Number of total records</returns>
		public int GetRecordCount(ExportProfile profile, Provider<IExportProvider> provider, IComponentContext context)
		{
			Guard.ArgumentNotNull(() => profile);
			Guard.ArgumentNotNull(() => provider);
			Guard.ArgumentNotNull(() => context);

			var taskContext = new TaskExecutionContext(context, null);
			var cancellation = new CancellationTokenSource(TimeSpan.FromMinutes(5.0));

			taskContext.CancellationToken = cancellation.Token;

			InitDependencies(taskContext);

			var ctx = new ExportProfileTaskContext(taskContext, profile, provider, previewData: x => { });

			var unused = Init(ctx);

			int result = ctx.RecordsPerStore.First().Value;

			return result;
		}
	}
}
