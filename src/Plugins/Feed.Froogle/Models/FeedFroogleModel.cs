﻿using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;
using SmartStore.Web.Framework;
using SmartStore.Web.Framework.Mvc;
using Newtonsoft.Json;
using SmartStore.Web.Framework.Plugins;

namespace SmartStore.Plugin.Feed.Froogle.Models
{
	public class FeedFroogleModel : PromotionFeedConfigModel
	{
		public string GridEditUrl { get; set; }

		[SmartResourceDisplayName("Plugins.Feed.Froogle.ProductPictureSize")]
		public int ProductPictureSize { get; set; }

		[SmartResourceDisplayName("Plugins.Feed.Froogle.Currency")]
		public int CurrencyId { get; set; }
		public List<SelectListItem> AvailableCurrencies { get; set; }

		[SmartResourceDisplayName("Plugins.Feed.Froogle.DefaultGoogleCategory")]
		public string DefaultGoogleCategory { get; set; }

		public string[] AvailableGoogleCategories { get; set; }
		public string AvailableGoogleCategoriesAsJson
		{
			get
			{
				if (AvailableGoogleCategories != null && AvailableGoogleCategories.Length > 0)
					return JsonConvert.SerializeObject(AvailableGoogleCategories);
				return "";
			}
		}

		[SmartResourceDisplayName("Plugins.Feed.Froogle.TaskEnabled")]
		public bool TaskEnabled { get; set; }
		[SmartResourceDisplayName("Plugins.Feed.Froogle.GenerateStaticFileEachMinutes")]
		public int GenerateStaticFileEachMinutes { get; set; }

		[SmartResourceDisplayName("Plugins.Feed.Froogle.BuildDescription")]
		public string BuildDescription { get; set; }

		[SmartResourceDisplayName("Plugins.Feed.Froogle.AppendDescriptionText")]
		public string AppendDescriptionText1 { get; set; }
		public string AppendDescriptionText2 { get; set; }
		public string AppendDescriptionText3 { get; set; }
		public string AppendDescriptionText4 { get; set; }
		public string AppendDescriptionText5 { get; set; }

		[SmartResourceDisplayName("Plugins.Feed.Froogle.AdditionalImages")]
		public bool AdditionalImages { get; set; }

		[SmartResourceDisplayName("Plugins.Feed.Froogle.Condition")]
		public string Condition { get; set; }

		[SmartResourceDisplayName("Plugins.Feed.Froogle.Availability")]
		public string Availability { get; set; }

		[SmartResourceDisplayName("Plugins.Feed.Froogle.SpecialPrice")]
		public bool SpecialPrice { get; set; }

		[SmartResourceDisplayName("Plugins.Feed.Froogle.Brand")]
		public string Brand { get; set; }

		[SmartResourceDisplayName("Plugins.Feed.Froogle.UseOwnProductNo")]
		public bool UseOwnProductNo { get; set; }

		[SmartResourceDisplayName("Plugins.Feed.Froogle.Gender")]
		public string Gender { get; set; }

		[SmartResourceDisplayName("Plugins.Feed.Froogle.AgeGroup")]
		public string AgeGroup { get; set; }

		[SmartResourceDisplayName("Plugins.Feed.Froogle.Color")]
		public string Color { get; set; }

		[SmartResourceDisplayName("Plugins.Feed.Froogle.Size")]
		public string Size { get; set; }

		[SmartResourceDisplayName("Plugins.Feed.Froogle.Material")]
		public string Material { get; set; }

		[SmartResourceDisplayName("Plugins.Feed.Froogle.Pattern")]
		public string Pattern { get; set; }

		[SmartResourceDisplayName("Plugins.Feed.Froogle.OnlineOnly")]
		public bool OnlineOnly { get; set; }

		[SmartResourceDisplayName("Plugins.Feed.Froogle.DescriptionToPlainText")]
		public bool DescriptionToPlainText { get; set; }

		[SmartResourceDisplayName("Plugins.Feed.Froogle.SearchProductName")]
		public string SearchProductName { get; set; }

		[SmartResourceDisplayName("Plugins.Feed.Froogle.Store")]
		public int StoreId { get; set; }

		[SmartResourceDisplayName("Plugins.Feed.Froogle.ExpirationDays")]
		public int ExpirationDays { get; set; }

		[SmartResourceDisplayName("Plugins.Feed.Froogle.ExportShipping")]
		public bool ExportShipping { get; set; }

		[SmartResourceDisplayName("Plugins.Feed.Froogle.ExportBasePrice")]
		public bool ExportBasePrice { get; set; }

		public void Copy(FroogleSettings settings, bool fromSettings)
		{
			if (fromSettings)
			{
				AppendDescriptionText1 = settings.AppendDescriptionText1;
				AppendDescriptionText2 = settings.AppendDescriptionText2;
				AppendDescriptionText3 = settings.AppendDescriptionText3;
				AppendDescriptionText4 = settings.AppendDescriptionText4;
				AppendDescriptionText5 = settings.AppendDescriptionText5;
				ProductPictureSize = settings.ProductPictureSize;
				CurrencyId = settings.CurrencyId;
				DefaultGoogleCategory = settings.DefaultGoogleCategory;
				BuildDescription = settings.BuildDescription;
				AdditionalImages = settings.AdditionalImages;
				Condition = settings.Condition;
				Availability = settings.Availability;
				SpecialPrice = settings.SpecialPrice;
				Brand = settings.Brand;
				UseOwnProductNo = settings.UseOwnProductNo;
				Gender = settings.Gender;
				AgeGroup = settings.AgeGroup;
				Color = settings.Color;
				Size = settings.Size;
				Material = settings.Material;
				Pattern = settings.Pattern;
				OnlineOnly = settings.OnlineOnly;
				DescriptionToPlainText = settings.DescriptionToPlainText;
				StoreId = settings.StoreId;
				ExpirationDays = settings.ExpirationDays;
				ExportShipping = settings.ExportShipping;
				ExportBasePrice = settings.ExportBasePrice;
			}
			else
			{
				settings.AppendDescriptionText1 = AppendDescriptionText1;
				settings.AppendDescriptionText2 = AppendDescriptionText2;
				settings.AppendDescriptionText3 = AppendDescriptionText3;
				settings.AppendDescriptionText4 = AppendDescriptionText4;
				settings.AppendDescriptionText5 = AppendDescriptionText5;
				settings.ProductPictureSize = ProductPictureSize;
				settings.CurrencyId = CurrencyId;
				settings.DefaultGoogleCategory = DefaultGoogleCategory;
				settings.BuildDescription = BuildDescription;
				settings.AdditionalImages = AdditionalImages;
				settings.Condition = Condition;
				settings.Availability = Availability;
				settings.SpecialPrice = SpecialPrice;
				settings.Brand = Brand;
				settings.UseOwnProductNo = UseOwnProductNo;
				settings.Gender = Gender;
				settings.AgeGroup = AgeGroup;
				settings.Color = Color;
				settings.Size = Size;
				settings.Material = Material;
				settings.Pattern = Pattern;
				settings.OnlineOnly = OnlineOnly;
				settings.DescriptionToPlainText = DescriptionToPlainText;
				settings.StoreId = StoreId;
				settings.ExpirationDays = ExpirationDays;
				settings.ExportShipping = ExportShipping;
				settings.ExportBasePrice = ExportBasePrice;
			}
		}
	}


	public class GoogleProductModel : ModelBase
	{
		//this attribute is required to disable editing
		[ScaffoldColumn(false)]
		public int ProductId { get; set; }

		//this attribute is required to disable editing
		[ReadOnly(true)]
		[ScaffoldColumn(false)]
		[SmartResourceDisplayName("Plugins.Feed.Froogle.Products.ProductName")]
		public string ProductName { get; set; }

		[SmartResourceDisplayName("Plugins.Feed.Froogle.Products.GoogleCategory")]
		public string GoogleCategory { get; set; }

		[SmartResourceDisplayName("Plugins.Feed.Froogle.Gender")]
		public string Gender { get; set; }

		[SmartResourceDisplayName("Plugins.Feed.Froogle.AgeGroup")]
		public string AgeGroup { get; set; }

		[SmartResourceDisplayName("Plugins.Feed.Froogle.Color")]
		public string Color { get; set; }

		[SmartResourceDisplayName("Plugins.Feed.Froogle.Size")]
		public string GoogleSize { get; set; }

		[SmartResourceDisplayName("Plugins.Feed.Froogle.Material")]
		public string Material { get; set; }

		[SmartResourceDisplayName("Plugins.Feed.Froogle.Pattern")]
		public string Pattern { get; set; }

		public string GenderLocalize { get; set; }
		public string AgeGroupLocalize { get; set; }
	}
}