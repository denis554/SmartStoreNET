﻿namespace SmartStore.Data.Migrations
{
	using System;
	using System.Data.Entity;
	using System.Data.Entity.Migrations;
	using System.Linq;
	using Setup;

	public sealed class MigrationsConfiguration : DbMigrationsConfiguration<SmartObjectContext>
	{
		public MigrationsConfiguration()
		{
			AutomaticMigrationsEnabled = false;
			AutomaticMigrationDataLossAllowed = true;
			ContextKey = "SmartStore.Core";
		}

		public void SeedDatabase(SmartObjectContext context)
		{
			Seed(context);
		}

		protected override void Seed(SmartObjectContext context)
		{
			context.MigrateLocaleResources(MigrateLocaleResources);
			MigrateSettings(context);

			context.SaveChanges();
        }

		public void MigrateSettings(SmartObjectContext context)
		{

		}

		public void MigrateLocaleResources(LocaleResourcesBuilder builder)
		{
			builder.AddOrUpdate("Admin.ReturnRequests.MaxRefundAmount",
				"Maximum refund amount",
				"Maximaler Erstattungsbetrag",
				"The maximum amount that can be refunded for this return request.",
				"Der maximale Betrag, der für diesen Rücksendewunsch erstattet werden kann.");

			builder.AddOrUpdate("Admin.Customers.Customers.Fields.Title",
				"Title",
				"Titel",
				"Specifies the title.",
				"Legt den Titel fest.");

            builder.AddOrUpdate("Admin.DataExchange.Export.FolderName.Validate",
                "Please enter a valid, relative folder path for the export data. The path must be at least 3 characters long and not the application folder.",
                "Bitte einen gültigen, relativen Ordnerpfad für die zu exportierenden Daten eingeben. Der Pfad muss mindestens 3 Zeichen lang und nicht der Anwendungsordner sein.");

            builder.AddOrUpdate("Admin.Catalog.Customers.CustomerSearchType", "Search in:", "Suche in:");

			// Fix some FluentValidation german translations
			builder.AddOrUpdate("Validation.LengthValidator")
				.Value("de", "'{PropertyName}' muss zwischen {MinLength} und {MaxLength} Zeichen lang sein. Sie haben {TotalLength} Zeichen eingegeben.");
			builder.AddOrUpdate("Validation.MinimumLengthValidator")
				.Value("de", "'{PropertyName}' muss mind. {MinLength} Zeichen lang sein. Sie haben {TotalLength} Zeichen eingegeben.");
			builder.AddOrUpdate("Validation.MaximumLengthValidator")
				.Value("de", "'{PropertyName}' darf max. {MaxLength} Zeichen lang sein. Sie haben {TotalLength} Zeichen eingegeben.");
			builder.AddOrUpdate("Validation.ExactLengthValidator")
				.Value("de", "'{PropertyName}' muss genau {MaxLength} lang sein. Sie haben {TotalLength} Zeichen eingegeben.");
			builder.AddOrUpdate("Validation.ExclusiveBetweenValidator")
				.Value("de", "'{PropertyName}' muss größer als {From} und kleiner als {To} sein. Sie haben '{Value}' eingegeben.");
			builder.AddOrUpdate("Validation.InclusiveBetweenValidator")
				.Value("de", "'{PropertyName}' muss zwischen {From} and {To} liegen. Sie haben '{Value}' eingegeben.");
			builder.AddOrUpdate("Validation.NotNullValidator")
				.Value("de", "'{PropertyName}' ist erforderlich.");
			builder.AddOrUpdate("Validation.NotEmptyValidator")
				.Value("de", "'{PropertyName}' ist erforderlich.");
			builder.AddOrUpdate("Validation.LessThanValidator")
				.Value("de", "'{PropertyName}' muss kleiner sein als '{ComparisonValue}'.");
			builder.AddOrUpdate("Validation.RegularExpressionValidator")
				.Value("de", "'{PropertyName}' entspricht nicht dem erforderlichen Muster.");
			builder.AddOrUpdate("Validation.ScalePrecisionValidator")
				.Value("de", "'{PropertyName}' darf insgesamt nicht mehr als {expectedPrecision} Ziffern enthalten, unter Berücksichtigung von {expectedScale} Dezimalstellen. {digits} Ziffern und {actualScale} Dezimalstellen wurden gefunden.");

			// Some new resources for custom validators
			builder.AddOrUpdate("Validation.CreditCardCvvNumberValidator",
				"'{PropertyName}' is invalid.",
				"'{PropertyName}' ist ungültig.");

			// Get rid of duplicate validator resource entries
			builder.Delete(
				"Admin.Catalog.Products.Fields.Name.Required",
				"Admin.Catalog.Categories.Fields.Name.Required",
				"Admin.Catalog.Manufacturers.Fields.Name.Required",
				"Admin.Validation.RequiredField",
				"Admin.Catalog.Attributes.ProductAttributes.Fields.Name.Required",
				"Admin.Catalog.ProductReviews.Fields.Title.Required",
				"Admin.Catalog.ProductReviews.Fields.ReviewText.Required",
				"Admin.Catalog.ProductTags.Fields.Name.Required",
				"Admin.Catalog.Products.ProductVariantAttributes.Attributes.Values.Fields.Name.Required",
				"Admin.Catalog.Products.ProductVariantAttributes.Attributes.Values.Fields.Quantity.GreaterOrEqualToOne",
				"Admin.Catalog.Attributes.SpecificationAttributes.Options.Fields.Name.Required",
				"Admin.Catalog.Attributes.SpecificationAttributes.Fields.Name.Required",
                "Admin.ContentManagement.Blog.BlogPosts.Fields.Title.Required",
                "Admin.ContentManagement.Blog.BlogPosts.Fields.Body.Required",
                "Admin.Common.GenericAttributes.Fields.Name.Required",
                "Admin.Customers.CustomerRoles.Fields.Name.Required",
                "Admin.Configuration.Countries.Fields.Name.Required",
                "Admin.Configuration.Countries.Fields.TwoLetterIsoCode.Required",
                "Admin.Configuration.Countries.Fields.TwoLetterIsoCode.Length",
                "Admin.Configuration.Countries.Fields.ThreeLetterIsoCode.Required",
                "Admin.Configuration.Countries.Fields.ThreeLetterIsoCode.Length",
                "Admin.Configuration.Measures.Dimensions.Fields.Name.Required",
                "Admin.Configuration.Measures.Dimensions.Fields.SystemKeyword.Required",
                "Admin.Configuration.Measures.Weights.Fields.Name.Required",
                "Admin.Configuration.Measures.Weights.Fields.SystemKeyword.Required",
                "Admin.Configuration.Countries.States.Fields.Name.Required",
                "Admin.Configuration.DeliveryTimes.Fields.Name.Required",
                "Admin.Configuration.DeliveryTimes.Fields.ColorHexValue.Required",
                "Admin.Configuration.DeliveryTimes.Fields.ColorHexValue.Range",
                "Admin.Configuration.DeliveryTimes.Fields.Name.Range",
                "Admin.Configuration.Currencies.Fields.Name.Required",
                "Admin.Configuration.Currencies.Fields.Name.Range",
                "Admin.Configuration.Currencies.Fields.CurrencyCode.Required",
                "Admin.Configuration.Currencies.Fields.CurrencyCode.Range",
                "Admin.Configuration.Currencies.Fields.Rate.Range",
                "Admin.Configuration.Currencies.Fields.CustomFormatting.Validation",
                "Admin.Promotions.Discounts.Fields.Name.Required",
                "Admin.ContentManagement.Forums.ForumGroup.Fields.Name.Required",
                "Admin.ContentManagement.Forums.Forum.Fields.Name.Required",
                "Admin.ContentManagement.Forums.Forum.Fields.ForumGroupId.Required",
                "Admin.Configuration.Languages.Resources.Fields.Name.Required",
                "Admin.Configuration.Languages.Resources.Fields.Value.Required",
                "Admin.Configuration.Languages.Fields.Name.Required",
                "Admin.Configuration.Languages.Fields.UniqueSeoCode.Required",
                "Admin.Configuration.Languages.Fields.UniqueSeoCode.Length",
                "Admin.Promotions.Campaigns.Fields.Name.Required",
                "Admin.Promotions.Campaigns.Fields.Subject.Required",
                "Admin.Promotions.Campaigns.Fields.Body.Required",
                "Admin.ContentManagement.MessageTemplates.Fields.Subject.Required",
                "Admin.ContentManagement.MessageTemplates.Fields.Body.Required",
                "Admin.Promotions.NewsLetterSubscriptions.Fields.Email.Required",
                "Admin.System.QueuedEmails.Fields.Priority.Required",
                "Admin.System.QueuedEmails.Fields.From.Required",
                "Admin.System.QueuedEmails.Fields.To.Required",
                "Admin.System.QueuedEmails.Fields.SentTries.Required",
                "Admin.System.QueuedEmails.Fields.Priority.Range",
                "Admin.System.QueuedEmails.Fields.SentTries.Range",
                "Admin.ContentManagement.News.NewsItems.Fields.Title.Required",
                "Admin.ContentManagement.News.NewsItems.Fields.Short.Required",
                "Admin.ContentManagement.News.NewsItems.Fields.Full.Required",
                "Admin.Catalog.Attributes.CheckoutAttributes.Fields.Name.Required",
                "Admin.Catalog.Attributes.CheckoutAttributes.Values.Fields.Name.Required",
                "Admin.Configuration.Plugins.Fields.FriendlyName.Required",
                "Admin.ContentManagement.Polls.Answers.Fields.Name.Required",
                "Admin.ContentManagement.Polls.Fields.Name.Required",
                "Admin.Configuration.Shipping.Methods.Fields.Name.Required",
                "Admin.Configuration.Stores.Fields.Name.Required",
                "Admin.Configuration.Stores.Fields.Url.Required",
                "Admin.Configuration.Settings.AllSettings.Fields.Name.Required",
                "Admin.System.ScheduleTasks.Name.Required",
                "Admin.Configuration.Tax.Categories.Fields.Name.Required",
                "Admin.ContentManagement.Topics.Fields.SystemName.Required",
                "Admin.Address.Fields.FirstName.Required",
                "Admin.Address.Fields.LastName.Required",
                "Admin.Address.Fields.Email.Required",
                "Admin.Address.Fields.Company.Required",
                "Admin.Address.Fields.City.Required",
                "Admin.Address.Fields.Address1.Required",
                "Admin.Address.Fields.Address2.Required",
                "Admin.Address.Fields.ZipPostalCode.Required",
                "Admin.Address.Fields.PhoneNumber.Required",
                "Admin.Address.Fields.FaxNumber.Required",
                "Admin.Address.Fields.EmailMatch.Required",
                "Admin.Customers.Customers.Fields.FirstName.Required",
                "Admin.Customers.Customers.Fields.LastName.Required",
                "Admin.Customers.Customers.Fields.Company.Required",
                "Admin.Customers.Customers.Fields.StreetAddress.Required",
                "Admin.Customers.Customers.Fields.StreetAddress2.Required",
                "Admin.Customers.Customers.Fields.ZipPostalCode.Required",
                "Admin.Customers.Customers.Fields.City.Required",
                "Admin.Customers.Customers.Fields.Phone.Required",
                "Admin.Customers.Customers.Fields.Fax.Required",
                "Admin.Validation.Name",
                "Admin.Validation.EmailAddress",
                "Admin.Validation.Url",
                "Admin.Validation.UsernamePassword",
                "Admin.DataExchange.Export.FileNamePattern.Validate",
                "Admin.DataExchange.Export.Partition.Validate",
                "Admin.Common.WrongEmail",
				"Payment.CardCode.Wrong"
			);

            // Get rid of duplicate CreatedOn resources also
            builder.Delete(
                "Admin.Affiliates.Orders.CreatedOn",
                "Admin.ContentManagement.Blog.Comments.Fields.CreatedOn",
                "Admin.ContentManagement.Blog.BlogPosts.Fields.CreatedOn",
                "Admin.ContentManagement.Blog.BlogPosts.Fields.CreatedOn",
                "Admin.Catalog.ProductReviews.Fields.CreatedOn",
                "Admin.Customers.Customers.Fields.CreatedOn",
                "Admin.Customers.Customers.Orders.CreatedOn",
                "Admin.Customers.Customers.ActivityLog.CreatedOn",
                "Admin.Orders.Fields.CreatedOn",
                "Admin.Customers.Customers.Fields.CreatedOn",
                "Admin.Promotions.NewsLetterSubscriptions.Fields.CreatedOn",
                "Admin.Configuration.Currencies.Fields.CreatedOn",
                "Admin.Promotions.Discounts.History.CreatedOn",
                "Admin.ContentManagement.Forums.ForumGroup.Fields.CreatedOn",
                "Admin.ContentManagement.Forums.Forum.Fields.CreatedOn",
                "Admin.Configuration.ActivityLog.ActivityLog.Fields.CreatedOn",
                "Admin.System.Log.Fields.CreatedOn",
                "Admin.Promotions.Campaigns.Fields.CreatedOn",
                "Admin.Promotions.NewsLetterSubscriptions.Fields.CreatedOn",
                "Admin.System.QueuedEmails.Fields.CreatedOn",
                "Admin.ContentManagement.News.Comments.Fields.CreatedOn",
                "Admin.ContentManagement.News.NewsItems.Fields.CreatedOn",
                "Admin.GiftCards.Fields.CreatedOn",
                "Admin.GiftCards.History.CreatedOn",
                "Admin.Orders.Fields.CreatedOn",
                "Admin.Orders.OrderNotes.Fields.CreatedOn",
                "Admin.RecurringPayments.History.CreatedOn",
                "Admin.ReturnRequests.Fields.CreatedOn"
            );

            // duplicate validator resource entries in frontend
            builder.Delete(
                "Blog.Comments.CommentText.Required",
                "Forum.TextCannotBeEmpty",
                "Forum.TopicSubjectCannotBeEmpty",
                "Forum.TextCannotBeEmpty",
                "Account.Fields.Email.Required",
                "Products.AskQuestion.Question.Required",
                "Account.Fields.FullName.Required",
                "Products.EmailAFriend.FriendEmail.Required",
                "Products.EmailAFriend.YourEmailAddress.Required",
                "Reviews.Fields.Title.Required",
                "Reviews.Fields.Title.MaxLengthValidation",
                "Reviews.Fields.ReviewText.Required",
                "Address.Fields.FirstName.Required",
                "Address.Fields.LastName.Required",
                "Address.Fields.Email.Required",
                "Account.Fields.Company.Required",
                "Account.Fields.StreetAddress.Required",
                "Account.Fields.StreetAddress2.Required",
                "Account.Fields.ZipPostalCode.Required",
                "Account.Fields.City.Required",
                "Account.Fields.Phone.Required",
                "Account.Fields.Fax.Required",
                "Admin.Address.Fields.EmailMatch.Required",
                "ContactUs.Email.Required",
                "ContactUs.Enquiry.Required",
                "ContactUs.FullName.Required",
                "Account.ChangePassword.Fields.OldPassword.Required",
                "Account.ChangePassword.Fields.NewPassword.Required",
                "Account.ChangePassword.Fields.NewPassword.LengthValidation",
                "Account.ChangePassword.Fields.ConfirmNewPassword.Required",
                "Account.ChangePassword.Fields.NewPassword.LengthValidation",
                "Account.Fields.Email.Required",
                "Account.Fields.FirstName.Required",
                "Account.Fields.LastName.Required",
                "Account.Fields.Company.Required",
                "Account.Fields.StreetAddress.Required",
                "Account.Fields.StreetAddress2.Required",
                "Account.Fields.ZipPostalCode.Required",
                "Account.Fields.City.Required",
                "Account.Fields.Phone.Required",
                "Account.Fields.Fax.Required",
                "Account.Fields.Password.Required",
                "Account.Fields.Vat.Required",
                "Account.PasswordRecovery.NewPassword.Required",
                "Account.PasswordRecovery.NewPassword.LengthValidation",
                "Account.PasswordRecovery.ConfirmNewPassword.Required",
                "Account.PasswordRecovery.Email.Required",
                "News.Comments.CommentTitle.Required",
                "News.Comments.CommentTitle.MaxLengthValidation",
                "News.Comments.CommentText.Required",
                "PrivateMessages.SubjectCannotBeEmpty",
                "PrivateMessages.MessageCannotBeEmpty",
                "Wishlist.EmailAFriend.FriendEmail.Required",
                "Wishlist.EmailAFriend.YourEmailAddress.Required"
            );

            // remove duplicate resources for display order
            builder.Delete(
                "Admin.Catalog.Categories.Fields.DisplayOrder",
                "Admin.Catalog.Categories.Products.Fields.DisplayOrder",
                "Admin.Catalog.Manufacturers.Fields.DisplayOrder",
                "Admin.Catalog.Manufacturers.Products.Fields.DisplayOrder",
                "Admin.Catalog.Products.ProductVariantAttributes.Attributes.Values.Fields.DisplayOrder",
                "Admin.Catalog.Products.BundleItems.Fields.DisplayOrder",
                "Admin.Catalog.Products.Fields.HomePageDisplayOrder",
                "Admin.Catalog.Products.SpecificationAttributes.Fields.DisplayOrder",
                "Admin.Catalog.Products.Pictures.Fields.DisplayOrder",
                "Admin.Catalog.Products.Categories.Fields.DisplayOrder",
                "Admin.Catalog.Products.Manufacturers.Fields.DisplayOrder",
                "Admin.Catalog.Products.RelatedProducts.Fields.DisplayOrder",
                "Admin.Catalog.Products.AssociatedProducts.Fields.DisplayOrder",
                "Admin.Catalog.Products.BundleItems.Fields.DisplayOrder",
                "Admin.Catalog.Products.ProductVariantAttributes.Attributes.Fields.DisplayOrder",
                "Admin.Catalog.Products.ProductVariantAttributes.Attributes.Values.Fields.DisplayOrder",
                "Admin.Catalog.Products.SpecificationAttributes.Fields.DisplayOrder",
                "Admin.Catalog.Attributes.SpecificationAttributes.Fields.DisplayOrder",
                "Admin.Catalog.Attributes.SpecificationAttributes.Options.Fields.DisplayOrder",
                "Admin.Catalog.Categories.Fields.DisplayOrder",
                "Admin.Catalog.Manufacturers.Fields.DisplayOrder",
                "Admin.Configuration.Countries.Fields.DisplayOrder",
                "Admin.Configuration.Currencies.Fields.DisplayOrder",
                "Admin.Configuration.DeliveryTimes.Fields.DisplayOrder",
                "Admin.Configuration.Measures.Dimensions.Fields.DisplayOrder",
                "Admin.Configuration.Measures.Weights.Fields.DisplayOrder",
                "Admin.Configuration.Countries.States.Fields.DisplayOrder",
                "Admin.ContentManagement.Forums.ForumGroup.Fields.DisplayOrder",
                "Admin.ContentManagement.Forums.Forum.Fields.DisplayOrder",
                "Admin.Configuration.Languages.Fields.DisplayOrder",
                "Admin.Catalog.Attributes.CheckoutAttributes.Fields.DisplayOrder",
                "Admin.Catalog.Attributes.CheckoutAttributes.Values.Fields.DisplayOrder",
                "Admin.Configuration.Plugins.Fields.DisplayOrder",
                "Admin.ContentManagement.Polls.Answers.Fields.DisplayOrder",
                "Admin.ContentManagement.Polls.Fields.DisplayOrder",
                "Admin.Configuration.Shipping.Methods.Fields.DisplayOrder",
                "Admin.Configuration.Stores.Fields.DisplayOrder",
                "Admin.Configuration.Tax.Categories.Fields.DisplayOrder"
            );

            builder.AddOrUpdate("Common.DisplayOrder.Hint",
                "Specifies display order. 1 represents the top of the list.",
                "Legt die Anzeige-Priorität fest. 1 steht bspw. für das erste Element in der Liste.");

			builder.AddOrUpdate("Admin.Configuration.Settings.GeneralCommon.UseInvisibleReCaptcha",
				"Use invisible reCAPTCHA",
				"Unsichtbaren reCAPTCHA verwenden",
				"Does not require the user to click on a checkbox, instead it is invoked directly when the user submits a form. By default only the most suspicious traffic will be prompted to solve a captcha.",
				"Der Benutzer muss nicht auf ein Kontrollkästchen klicken, sondern die Validierung erfolgt direkt beim Absenden eines Formulars. Nur bei 'verdächtigem' Traffic wird der Benutzer aufgefordert, ein Captcha zu lösen.");

			builder.AddOrUpdate("Admin.ContentManagement.Topics.Fields.ShortTitle",
				"Short title",
				"Kurztitel",
				"Optional. Used as link text. If empty, 'Title' sets the link text.",
				"Optional. Wird u.A. als Linktext verwendet. Wenn leer, stellt 'Titel' den Linktext.");

			builder.AddOrUpdate("Admin.ContentManagement.Topics.Fields.Intro",
				"Intro",
				"Intro",
				"Optional. Short introduction / teaser.",
				"Optional. Einleitung / Teaser.");
			
            builder.AddOrUpdate("Common.Download.Versions", "Versions", "Versionen");
            builder.AddOrUpdate("Common.Download.Version", "Version", "Version");
            builder.AddOrUpdate("Common.Download.Delete", "Delete download", "Download löschen");
            builder.AddOrUpdate("Common.Downloads", "Downloads", "Downloads");
            
            builder.AddOrUpdate("Admin.Catalog.Products.Fields.NewVersionDownloadId", 
                "New download version",
                "Neue Version des Downloads",
                "Upload a new version of the download file here.",
                "Laden Sie hier eine neue Version der Download-Datei hoch.");

            builder.AddOrUpdate("Admin.Catalog.Products.Download.VersionDelete", "Delete this file version.", "Diese Dateiversion löschen.");
            builder.AddOrUpdate("Admin.Catalog.Products.Download.AddChangelog", "Edit changelog", "Änderungshistorie bearbeiten");
            builder.AddOrUpdate("Customer.Downloads.NoChangelogAvailable", "No changelog available.", "Keine Änderungshistorie verfügbar.");

            builder.AddOrUpdate("Admin.Catalog.Products.Download.SemanticVersion.NotValid",
                "The specified version information is not valid. Please enter the version number in the correct format (e.g.: 1.0.0.0, 2.0 or 3.1.5).", 
                "Die angegebenen Versionsinformationen sind nicht gültig. Bitte geben Sie die Versionsnummer in korrektem Format an (z.B.: 1.0.0.0, 2.0 oder 3.1.5).");

            builder.AddOrUpdate("Admin.Catalog.Products.Fields.HasPreviewPicture",
				"Exclude first image from gallery",
				"Erstes Bild aus Gallerie ausschließen",
				"Activate this option if the first image should be displayed as a preview in product lists but not in the product detail gallery.",
                "Aktivieren Sie diese Option, wenn das erste Bild als Vorschau in Produktlisten, nicht aber in der Produktdetail-Gallerie angezeigt werden soll.");

            builder.AddOrUpdate("Products.Free", "Free", "Kostenlos");

            builder.AddOrUpdate("Admin.Catalog.Products.Fields.ProductTags.Hint",
                "Product tags are keywords that this product can also be identified by. Enter a list of the tags to be associated with this product. The more products associated with a particular tag, the larger it will show on the tag cloud.",
                "Eine Liste von Schlüsselwörtern, die das Produkt taxonomisch charakterisieren. Je mehr Produkte einem Schlüsselwort (Tag) zugeordnet sind, desto mehr visuelles Gewicht erhält das Tag.");

            builder.AddOrUpdate("Enums.SmartStore.Core.Domain.Forums.ForumTopicSorting.Initial", "Position", "Position");
            builder.AddOrUpdate("Enums.SmartStore.Core.Domain.Forums.ForumTopicSorting.Relevance", "Relevance", "Beste Ergebnisse");
            builder.AddOrUpdate("Enums.SmartStore.Core.Domain.Forums.ForumTopicSorting.SubjectAsc", "Title: A to Z", "Titel: A bis Z");
            builder.AddOrUpdate("Enums.SmartStore.Core.Domain.Forums.ForumTopicSorting.SubjectDesc", "Title: Z to A", "Titel: Z bis A");
            builder.AddOrUpdate("Enums.SmartStore.Core.Domain.Forums.ForumTopicSorting.UserNameAsc", "User name: A to Z", "Benutzername: A bis Z");
            builder.AddOrUpdate("Enums.SmartStore.Core.Domain.Forums.ForumTopicSorting.UserNameDesc", "User name: Z to A", "Benutzername: Z bis A");
            builder.AddOrUpdate("Enums.SmartStore.Core.Domain.Forums.ForumTopicSorting.CreatedOnAsc", "Created on: Oldest first", "Erstellt am: ältere zuerst");
            builder.AddOrUpdate("Enums.SmartStore.Core.Domain.Forums.ForumTopicSorting.CreatedOnDesc", "Created on: Newest first", "Erstellt am: neuere zuerst");
            builder.AddOrUpdate("Enums.SmartStore.Core.Domain.Forums.ForumTopicSorting.PostsAsc", "Post number: ascending", "Anzahl Beiträge: aufsteigend");
            builder.AddOrUpdate("Enums.SmartStore.Core.Domain.Forums.ForumTopicSorting.PostsDesc", "Post number: descending", "Anzahl Beiträge: absteigend");

            builder.AddOrUpdate("Enums.SmartStore.Core.Domain.Forums.ForumDateFilter.LastVisit", "Since last visit", "Seit dem letzten Besuch");
            builder.AddOrUpdate("Enums.SmartStore.Core.Domain.Forums.ForumDateFilter.Yesterday", "Yesterday", "Gestern");
            builder.AddOrUpdate("Enums.SmartStore.Core.Domain.Forums.ForumDateFilter.LastWeek", "Last week", "In der letzten Woche");
            builder.AddOrUpdate("Enums.SmartStore.Core.Domain.Forums.ForumDateFilter.LastTwoWeeks", "Last 2 weeks", "In den letzten 2 Wochen");
            builder.AddOrUpdate("Enums.SmartStore.Core.Domain.Forums.ForumDateFilter.LastMonth", "Last month", "Im letzten Monat");
            builder.AddOrUpdate("Enums.SmartStore.Core.Domain.Forums.ForumDateFilter.LastThreeMonths", "Last 3 months", "In den letzten 3 Monaten");
            builder.AddOrUpdate("Enums.SmartStore.Core.Domain.Forums.ForumDateFilter.LastSixMonths", "Last 6 months", "In den letzten 6 Monaten");
            builder.AddOrUpdate("Enums.SmartStore.Core.Domain.Forums.ForumDateFilter.LastYear", "Last year", "Im letzten Jahr");

            builder.AddOrUpdate("Search.Facet.Forum", "Forum", "Forum");
            builder.AddOrUpdate("Search.Facet.Customer", "User name", "Benutzername");
            builder.AddOrUpdate("Search.Facet.Date", "Period", "Zeitraum");
            builder.AddOrUpdate("Search.Facet.Date.Newer", "and newer", "und neuer");
            builder.AddOrUpdate("Search.Facet.Date.Older", "and older", "und älter");

            builder.AddOrUpdate("Forum.PostText", "Post text", "Beitragstext");
            builder.AddOrUpdate("Forum.Sticky", "Sticky topic", "Festes Thema");

            builder.AddOrUpdate("Search.HitsFor", "{0} hits for {1}", "{0} Treffer für {1}");
            builder.AddOrUpdate("Search.NoMoreHitsFound", "There were no more hits found.", "Es wurden keine weiteren Treffer gefunden.");

            builder.AddOrUpdate("Admin.Configuration.Settings.Search.WildcardSearchNote",
                "The wildcard mode can slow down the search for a large number of objects.",
                "Der Wildcard-Modus kann bei einer großen Anzahl an Objekten die Suche verlangsamen.");

            builder.AddOrUpdate("Admin.Configuration.Settings.Search.SearchMode",
                "Search mode",
                "Suchmodus",
                "Specifies the search mode. Please keep in mind that the search mode can - depending on number of objects - strongly affect search performance. 'Is equal to' is the fastest, 'Contains' the slowest.",
                "Legt den Suchmodus fest. Bitte beachten Sie, dass der Suchmodus die Geschwindigkeit der Suche (abhängig von der Objektanzahl) beeinflusst. 'Ist gleich' ist am schnellsten, 'Beinhaltet' am langsamsten.");

            builder.AddOrUpdate("Admin.Configuration.Settings.Search.Forum.SearchFields",
                "Search fields",
                "Suchfelder",
                "Specifies additional search fields. The topic title is always searched.",
                "Legt zusätzlich zu durchsuchende Felder fest. Der Thementitel wird grundsätzlich immer durchsucht.");

            builder.AddOrUpdate("Admin.Configuration.Settings.Search.DefaultSortOrder",
                "Default sort order",
                "Standardsortierreihenfolge",
                "Specifies the default sort order in search results.",
                "Legt die Standardsortierreihenfolge in den Suchergebnissen fest.");

            builder.AddOrUpdate("Admin.Configuration.Settings.Search.InstantSearchNumberOfHits",
                "Number of hits",
                "Anzahl der Treffer",
                "Specifies the number of hits displayed in instant search.",
                "Legt die Anzahl der angezeigten Suchtreffer in der Instantsuche fest.");

            builder.AddOrUpdate("Admin.Configuration.Settings.Forums.AllowSorting",
                "Allow sorting",
                "Sortierung zulassen",
                "Specifies whether forum posts can be sorted.",
                "Legt fest, ob Forenbeiträge sortiert werden können.");

            builder.AddOrUpdate("Admin.Common.DefaultPageSizeOptions",
                "Page size options",
                "Auswahlmöglichkeiten für Seitengröße",
                "Comma-separated page size options that a customer can select in lists.",
                "Kommagetrennte Liste mit Optionen für Seitengröße, die ein Kunde in Listen wählen kann.");

            builder.AddOrUpdate("Admin.Common.AllowCustomersToSelectPageSize",
                "Allow customers to select page size",
                "Kunde kann Listengröße ändern",
                "Whether customers are allowed to select the page size from a predefined list of options.",
                "Kunden können die Listengröße mit Hilfe einer vorgegebenen Optionsliste ändern.");


            builder.Delete(
                "Admin.Configuration.Settings.Search.DefaultSortOrderMode",
                "Admin.Configuration.Settings.Search.InstantSearchNumberOfProducts",
                "Forum.Search.LimitResultsToPrevious.AllResults",
                "Forum.Search.LimitResultsToPrevious.1day",
                "Forum.Search.LimitResultsToPrevious.7days",
                "Forum.Search.LimitResultsToPrevious.2weeks",
                "Forum.Search.LimitResultsToPrevious.1month",
                "Forum.Search.LimitResultsToPrevious.3months",
                "Forum.Search.LimitResultsToPrevious.6months",
                "Forum.Search.LimitResultsToPrevious.1year",
                "Forum.Search.SearchInForum.All",
                "Forum.Search.SearchWithin.All",
                "Forum.Search.SearchWithin.TopicTitlesOnly",
                "Forum.Search.SearchWithin.PostTextOnly",
                "Forum.SearchTermMinimumLengthIsNCharacters",
                "Enums.SmartStore.Core.Domain.Forums.ForumSearchType.All",
                "Enums.SmartStore.Core.Domain.Forums.ForumSearchType.PostTextOnly",
                "Enums.SmartStore.Core.Domain.Forums.ForumSearchType.TopicTitlesOnly",
                "Forum.AdvancedSearch",
                "Forum.SearchButton",
                "Forum.PageTitle.Search");

            builder.AddOrUpdate("Admin.Configuration.Settings.Catalog.PriceDisplayStyle",
                "Price display style",
                "Preisdarstellung",
                "Specifies the form in which prices are displayed in product lists and on the product detail page.",
                "Bestimmt die Darstellungform von Preisen in Produktlisten und auf der Produktdetailseite.");

            builder.AddOrUpdate("Admin.Configuration.Settings.Catalog.DisplayTextForZeroPrices",
                "Display text when prices are 0,00",
                "Zeige Text wenn Preise 0,00 sind",
                "Specifies whether to display a textual resource (free) instead of the value 0.00.",
                "Bestimmt, ob statt dem Wert 0,00 eine textuelle Resource (kostenlos) angezeigt werden soll.");

            builder.AddOrUpdate("Enums.SmartStore.Core.Domain.Catalog.PriceDisplayStyle.Default", "Default", "Standard");
            builder.AddOrUpdate("Enums.SmartStore.Core.Domain.Catalog.PriceDisplayStyle.BadgeAll", "In bagdes", "Markiert");
            builder.AddOrUpdate("Enums.SmartStore.Core.Domain.Catalog.PriceDisplayStyle.BadgeFreeProductsOnly", "Badge free products only", "Nur kostenlose Produkte markieren");

            builder.AddOrUpdate("Admin.DataExchange.Export.Filter.WorkingLanguageId",
                "Language",
                "Sprache",
                "Filter by language",
                "Nach Sprache filtern");
            

            builder.AddOrUpdate("Admin.Configuration.Settings.GeneralCommon.CaptchaShowOnForumPage",
                "Show on forum pages",
                "Auf Forenseiten anzeigen",
                "Specifies whether to display a CAPTCHA on forum pages when creating or replying to a topic.",
                "Legt fest, ob ein CAPTCHA auf Forenseiten angezeigt werden soll, wenn ein Thema erstellt oder darauf geantwortet wird.");
        }
    }
}
