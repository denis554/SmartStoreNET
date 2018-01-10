namespace SmartStore.Data.Migrations
{
	using System;
	using System.Data.Entity;
	using System.Data.Entity.Migrations;
	using System.Linq;
	using Setup;
	using SmartStore.Utilities;
	using SmartStore.Core.Domain.Media;
	using Core.Domain.Configuration;

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
        }

		public void MigrateSettings(SmartObjectContext context)
		{
			// Change MediaSettings.MaximumImageSize to 2048
			var name = TypeHelper.NameOf<MediaSettings>(y => y.MaximumImageSize, true);
			var setting = context.Set<Setting>().FirstOrDefault(x => x.Name == name);
			if (setting != null && setting.Value.Convert<int>() < 2048)
			{
				setting.Value = "2048";
			}
		}

		public void MigrateLocaleResources(LocaleResourcesBuilder builder)
		{
			builder.AddOrUpdate("Admin.Orders.Shipment", "Shipment", "Lieferung");
			builder.AddOrUpdate("Admin.Order", "Order", "Auftrag");

			builder.AddOrUpdate("Admin.Order.ViaShippingMethod", "via {0}", "via {0}");
			builder.AddOrUpdate("Admin.Order.WithPaymentMethod", "with {0}", "per {0}");
			builder.AddOrUpdate("Admin.Order.FromStore", "from {0}", "von {0}");

            builder.AddOrUpdate("Admin.Configuration.Settings.Catalog.MaxItemsToDisplayInCatalogMenu",
                "Max items to display in catalog menu",
                "Maximale Anzahl von Elementen im Katalogmen�",
                "Defines the maximum number of top level items to be displayed in the main catalog menu. All menu items which are exceeding this limit will be placed in a new dropdown menu item.",
                "Legt die maximale Anzahl von Menu-Eintr�gen der obersten Hierarchie fest, die im Katalogmen� angezeigt werden. Alle weiteren Menu-Eintr�ge werden innerhalb eines neuen Dropdownmenus ausgegeben.");

            builder.AddOrUpdate("CatalogMenu.MoreLink", "More", "Mehr");

            builder.AddOrUpdate("Admin.CatalogSettings.Homepage", "Homepage", "Homepage");
            builder.AddOrUpdate("Admin.CatalogSettings.ProductDisplay", "Product display", "Produktdarstellung");
            builder.AddOrUpdate("Admin.CatalogSettings.Prices", "Prices", "Preise");
            builder.AddOrUpdate("Admin.CatalogSettings.CompareProducts", "Compare products", "Produktvergleich");

            builder.AddOrUpdate("Footer.Service.Mobile", "Service", "Service, Versand & Zahlung");
            builder.AddOrUpdate("Footer.Company.Mobile", "Company", "Firma, Impressum & Datenschutz");

            builder.AddOrUpdate("Enums.SmartStore.Core.Search.Facets.FacetSorting.LabelAsc",
                "Displayed Name: A to Z",
                "Angezeigter Name: A bis Z");

            builder.AddOrUpdate("Admin.Catalog.Products.Copy.NumberOfCopies",
                "Number of copies",
                "Anzahl an Kopien",
                "Defines the number of copies to be created.",
                "Legt die Anzahl der anzulegenden Kopien fest.");

            builder.AddOrUpdate("Admin.Configuration.Languages.OfType",
                "of type \"{0}\"",
                "vom Typ \"{0}\"");

            builder.AddOrUpdate("Admin.Configuration.Languages.CheckAvailableLanguagesFailed",
                "An error occurred while checking for other available languages.",
                "Bei der Suche nach weiteren verf�gbaren Sprachen trat ein Fehler auf.");

            builder.AddOrUpdate("Admin.Configuration.Languages.NoAvailableLanguagesFound",
				"There were no other available languages found for version {0}. On <a href='http://translate.smartstore.com/'>translate.smartstore.com</a> you will find more details about available resources.",
				"Es wurden keine weiteren verf�gbaren Sprachen f�r Version {0} gefunden. Auf <a href='http://translate.smartstore.com/'>translate.smartstore.com</a> finden Sie weitere Details zu verf�gbaren Ressourcen.");

            builder.AddOrUpdate("Admin.Configuration.Languages.InstalledLanguages",
                "Installed Languages",
                "Installierte Sprachen");
            builder.AddOrUpdate("Admin.Configuration.Languages.AvailableLanguages",
                "Available Languages",
                "Verf�gbare Sprachen");

            builder.AddOrUpdate("Admin.Configuration.Languages.AvailableLanguages.Note",
				"Click <b>Download</b> to install a new language including all localized resources. On <a href='http://translate.smartstore.com/'>translate.smartstore.com</a> you will find more details about available resources.",
				"Klicken Sie auf <b>Download</b>, um eine neue Sprache mit allen lokalisierten Ressourcen zu installieren. Auf <a href='http://translate.smartstore.com/'>translate.smartstore.com</a> finden Sie weitere Details zu verf�gbaren Ressourcen.");

            builder.AddOrUpdate("Common.Translated",
                "Translated",
                "�bersetzt");
            builder.AddOrUpdate("Admin.Configuration.Languages.TranslatedPercentage",
                "{0}% translated",
                "{0}% �bersetzt");
            builder.AddOrUpdate("Admin.Configuration.Languages.TranslatedPercentageAtLastImport",
                "{0}% at the last import",
                "{0}% beim letzten Import");

            builder.AddOrUpdate("Admin.Configuration.Languages.NumberOfTranslatedResources",
                "{0} of {1}",
                "{0} von {1}");

            builder.AddOrUpdate("Admin.Configuration.Languages.DownloadingResources",
                "Loading ressources",
                "Lade Ressourcen");
            builder.AddOrUpdate("Admin.Configuration.Languages.ImportResources",
                "Import resources",
                "Importiere Ressourcen");

            builder.AddOrUpdate("Admin.Configuration.Languages.OnePublishedLanguageRequired",
                "At least one published language is required.",
                "Mindestens eine ver�ffentlichte Sprache ist erforderlich.");

            builder.AddOrUpdate("Admin.Configuration.Languages.Fields.AvailableLanguageSetId",
                "Available Languages",
                "Verf�gbare Sprachen",
                "Specifies the available language whose localized resources are to be imported.",
                "Legt die verf�gbare Sprache fest, deren lokalisierte Ressourcen importiert werden sollen.");

            builder.AddOrUpdate("Admin.Configuration.Languages.UploadFileOrSelectLanguage",
                "Please upload an import file or select an available language whose resources are to be imported.",
                "Bitte laden Sie eine Importdatei hoch oder w�hlen Sie eine verf�gbare Sprache, deren Ressourcen importiert werden sollen.");

            builder.AddOrUpdate("Admin.Configuration.Settings.Shipping.ChargeOnlyHighestProductShippingSurcharge",
                "Charge the highest shipping surcharge only",
                "Nur den h�chsten Transportzuschlag berechnen",
                "Specifies  whether to charge only the highest additional shipping surcharge of products.",
                "Bestimmt ob bei der Berechnung der Versandkosten nur der h�chste Transportzuschlag von Produkten ber�cksichtigt wird.");

            builder.AddOrUpdate("Order.OrderDetails")
                .Value("en", "Order Details");

			builder.AddOrUpdate("Admin.Configuration.Settings.Media.AutoGenerateAbsoluteUrls",
				"Generate absolute URLs",
				"Absolute URLs erzeugen",
				"Generates absolute URLs for media files by prepending the current host name (e.g. http://myshop.com/media/image/1.jpg instead of /media/image/1.jpg). Has no effect if a CDN URL has been applied to the store.",
				"Erzeugt absolute URLs f�r Mediendateien, indem der aktuelle Hostname vorangestellt wird (z.B. http://meinshop.de/media/image/1.jpg statt /media/image/1.jpg). Hat keine Auswirkung, wenn f�r den Store eine CDN-URL eingerichtet wurde.");

			builder.AddOrUpdate("Admin.Configuration.Settings.Search.SearchFieldsNote",
				"The Name, SKU and Short Description fields can be searched in the standard search. Other fields require a search plugin such as the MegaSearch plugin from <a href='http://www.smartstore.com/de/net#section-pricing' target='_blank'>Premium Edition</a>.",
				"In der Standardsuche k�nnen die Felder Name, SKU und Kurzbeschreibung durchsucht werden. F�r weitere Felder ist ein Such-Plugin wie etwa das MegaSearch-Plugin aus der <a href='http://www.smartstore.com/de/net#section-pricing' target='_blank'>Premium Edition</a> notwendig.");

			builder.AddOrUpdate("Admin.DataExchange.Import.FolderName", "Folder path", "Ordnerpfad");
		}
	}
}
