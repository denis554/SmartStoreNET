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
	using SmartStore.Core.Domain.Customers;

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
			// Change MediaSettings.MaximumImageSize to 2048
			var name = TypeHelper.NameOf<MediaSettings>(y => y.MaximumImageSize, true);
			var setting = context.Set<Setting>().FirstOrDefault(x => x.Name == name);
			if (setting != null && setting.Value.Convert<int>() < 2048)
			{
				setting.Value = "2048";
			}

			// Change MediaSettings.AvatarPictureSize to 250
			name = TypeHelper.NameOf<MediaSettings>(y => y.AvatarPictureSize, true);
			setting = context.Set<Setting>().FirstOrDefault(x => x.Name == name);
			if (setting != null && setting.Value.Convert<int>() < 250)
			{
				setting.Value = "250";
			}

			// Change MediaSettings.AvatarMaximumSizeBytes to 512000 (500 KB)
			name = TypeHelper.NameOf<CustomerSettings>(y => y.AvatarMaximumSizeBytes, true);
			setting = context.Set<Setting>().FirstOrDefault(x => x.Name == name);
			if (setting != null && setting.Value.Convert<int>() < 512000)
			{
				setting.Value = "512000";
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
				"There were no other available languages found for version {0}. On <a href='http://translate.smartstore.com/' target='_blank'>translate.smartstore.com</a> you will find more details about available resources.",
				"Es wurden keine weiteren verf�gbaren Sprachen f�r Version {0} gefunden. Auf <a href='http://translate.smartstore.com/ target='_blank''>translate.smartstore.com</a> finden Sie weitere Details zu verf�gbaren Ressourcen.");

            builder.AddOrUpdate("Admin.Configuration.Languages.InstalledLanguages",
                "Installed Languages",
                "Installierte Sprachen");
            builder.AddOrUpdate("Admin.Configuration.Languages.AvailableLanguages",
                "Available Languages",
                "Verf�gbare Sprachen");

            builder.AddOrUpdate("Admin.Configuration.Languages.AvailableLanguages.Note",
				"Click <b>Download</b> to install a new language including all localized resources. On <a href='http://translate.smartstore.com/' target='_blank'>translate.smartstore.com</a> you will find more details about available resources.",
				"Klicken Sie auf <b>Download</b>, um eine neue Sprache mit allen lokalisierten Ressourcen zu installieren. Auf <a href='http://translate.smartstore.com/' target='_blank'>translate.smartstore.com</a> finden Sie weitere Details zu verf�gbaren Ressourcen.");

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

			builder.AddOrUpdate("Admin.MessageTemplate.Preview.From", "From", "Von");
			builder.AddOrUpdate("Admin.MessageTemplate.Preview.To", "To", "An");
			builder.AddOrUpdate("Admin.MessageTemplate.Preview.ReplyTo", "Reply To", "Antwort an");
			builder.AddOrUpdate("Admin.MessageTemplate.Preview.SendTestMail", "Test-E-mail to...", "Test E-Mail an...");
			builder.AddOrUpdate("Admin.MessageTemplate.Preview.TestMailSent", "E-mail has been sent.", "E-Mail gesendet.");
			builder.AddOrUpdate("Admin.MessageTemplate.Preview.NoBody",
				"The generated preview file seems to have expired. Please reload the page.", 
				"Die generierte Vorschaudatei scheint abgelaufen zu sein. Laden Sie die Seite bitte neu.");

			builder.AddOrUpdate("Admin.ContentManagement.MessageTemplates.Preview.SuccessfullySent",
				"The email has been sent successfully.", 
				"Die E-Mail wurde erfolgreich versendet.");

			builder.AddOrUpdate("Admin.ContentManagement.MessageTemplates.SuccessfullyCopied",
				"The message template has been copied successfully.",
				"Die Nachrichtenvorlage wurde erfolgreich kopiert.");
			

			builder.AddOrUpdate("Enums.SmartStore.Core.Domain.DataExchange.ExportEntityType.ShoppingCartItem", "Shopping Cart", "Warenkorb");
			builder.AddOrUpdate("Enums.SmartStore.Core.Domain.Orders.ShoppingCartType.ShoppingCart", "Shopping Cart", "Warenkorb");
			builder.AddOrUpdate("Enums.SmartStore.Core.Domain.Orders.ShoppingCartType.Wishlist", "Wishlist", "Wunschliste");

			builder.AddOrUpdate("Admin.DataExchange.Export.Projection.NoBundleProducts",
				"Do not export bundled products",
				"Keine Produkt-Bundle exportieren",
				"Specifies whether to export bundled products. If this option is activated, then the associated bundle items will be exported.",
				"Legt fest, ob Produkt-Bundle exportiert werden sollen. Ist diese Option aktiviert, so werden die zum Bundle geh�renden Produkte (Bundle-Bestandteile) exportiert.");

			builder.AddOrUpdate("Admin.DataExchange.Export.Filter.ShoppingCartTypeId",
				"Shopping cart type",
				"Warenkorbtyp",
				"Filter by shopping cart type.",
				"Nach Warenkorbtyp filtern.");

			builder.AddOrUpdate("Common.CustomerId", "Customer ID", "Kunden ID");

			builder.AddOrUpdate("Account.AccountActivation.InvalidEmailOrToken",
				"Unknown email or token. Please register again.",
				"Unbekannte E-Mail oder Token. Bitte f�hren Sie die Registrierung erneut durch.");

			builder.AddOrUpdate("Account.PasswordRecoveryConfirm.InvalidEmailOrToken",
				"Unknown email or token. Please click \"Forgot password\" again, if you want to renew your password.",
				"Unbekannte E-Mail oder Token. Klicken Sie bitte erneut \"Passwort vergessen\", falls Sie Ihr Passwort erneuern m�chten.");

			builder.Delete("Account.PasswordRecoveryConfirm.InvalidEmail");
			builder.Delete("Account.PasswordRecoveryConfirm.InvalidToken");

			builder.AddOrUpdate("Admin.Common.Acl.SubjectTo",
				"Restrict access",
				"Zugriff einschr�nken",
				"Determines whether this entity is subject to access restrictions (no = no restriction, yes = accessible only for selected customer groups)",
				"Legt fest, ob dieser Datensatz Zugriffsbeschr�nkungen unterliegt (Nein = keine Beschr�nkung, Ja = zug�nglich nur f�r gew�hlte Kundengruppen)");

			builder.AddOrUpdate("Admin.Common.Acl.AvailableFor",
				"Customer roles",
				"Kundengruppen",
				"Select customer roles who can access the entity. For all inactive roles, this record is hidden.",
				"W�hlen Sie Kundengruppen, die auf den Datensatz zugreifen k�nnen. Bei allen nicht aktivierten Gruppen wird dieser Datensatz ausgeblendet.");

			builder.Delete(
				"Admin.Catalog.Categories.Fields.SubjectToAcl",
				"Admin.Catalog.Categories.Fields.AclCustomerRoles",
				"Admin.Catalog.Products.Fields.SubjectToAcl",
				"Admin.Catalog.Products.Fields.AclCustomerRoles",
				"Common.Options.Count");

			builder.AddOrUpdate("Admin.Common.ApplyFilter", "Apply filter", "Filter anwenden");
			builder.AddOrUpdate("Time.Milliseconds", "Milliseconds", "Millisekunden");
			builder.AddOrUpdate("Common.Pixel", "Pixel", "Pixel");
			builder.AddOrUpdate("Admin.DataExchange.Export.Deployment.ShowPlaceholder", "Show placeholder", "Zeige Platzhalter");
			builder.AddOrUpdate("Admin.DataExchange.Export.Deployment.HidePlaceholder", "Hide placeholder", "Verberge Platzhalter");
			builder.AddOrUpdate("Admin.DataExchange.Export.Deployment.UpdateExampleFileName", "Update example", "Aktualisiere Beispiel");

			builder.AddOrUpdate("Admin.Configuration.Themes.AvailableDesktopThemes", "Installed themes", "Installierte Themes");

			builder.AddOrUpdate("Admin.Catalog.Products.List.GoDirectlyToSku", "Find by SKU", "Nach SKU suchen");
			builder.AddOrUpdate("Admin.Orders.List.GoDirectlyToNumber", "Find by order id", "Nach Auftragsnummer suchen");
			
			builder.AddOrUpdate("Admin.Configuration.Settings.CustomerUser.StoreLastIpAddress",
				"Store IP address",
				"IP-Adresse speichern",
				"Specifies whether to store the IP address in the customer data set.",
				"Legt fest, ob die IP-Adresse im Kundendatensatz gespeichert werden soll.");

			builder.AddOrUpdate("Admin.Orders.Info", "General", "Allgemein");
			builder.AddOrUpdate("Admin.Orders.BillingAndShipment", "Billing & Shipping", "Rechnung & Versand");
			builder.AddOrUpdate("Admin.Orders.Fields.ShippingAddress.ViewOnGoogleMaps", "View on Google Maps", "Auf Google Maps ansehen");

			builder.AddOrUpdate("Admin.Configuration.Settings.GeneralCommon.SocialSettings.InstagramLink",
				"Instagram Link",
				"Instagram Link",
				"Leave this field empty if the Instagram link should not be shown",
				"Lassen Sie dieses Feld leer, wenn der Instagram Link nicht angezeigt werden soll");

			builder.AddOrUpdate("Common.License", "License", "Lizenz");

			builder.AddOrUpdate("Enums.SmartStore.Core.Domain.Payments.CapturePaymentReason.OrderShipped",
				"The order has been marked as shipped",
				"Der Auftrag wurde als versendet markiert");

			builder.AddOrUpdate("Enums.SmartStore.Core.Domain.Payments.CapturePaymentReason.OrderDelivered",
				"The order has been marked as delivered",
				"Der Auftrag wurde als ausgeliefert markiert");

			builder.AddOrUpdate("Admin.Configuration.Settings.Payment.CapturePaymentReason",
				"Capture payment amount when�",
				"Zahlungsbetrag einziehen, wenn�",
				"Specifies the event when the payment amount is automatically captured. The selected payment method must support capturing for this.",
				"Legt das Ereignis fest, zu dem der Zahlunsgbetrag automatisch eingezogen wird. Die gew�hlte Zahlart muss hierf�r Buchungen unterst�tzen.");
			

			#region taken from V22Final, because they were never added yet

			builder.AddOrUpdate("Common.Next",
				"Next",
				"Weiter");
			builder.AddOrUpdate("Admin.Common.BackToConfiguration",
				"Back to configuration",
				"Zur�ck zur Konfiguration");
			builder.AddOrUpdate("Admin.Common.UploadFileSucceeded",
				"The file has been successfully uploaded.",
				"Die Datei wurde erfolgreich hochgeladen.");
			builder.AddOrUpdate("Admin.Common.UploadFileFailed",
				"The upload has failed.",
				"Der Upload ist leider fehlgeschlagen.");
			builder.AddOrUpdate("Admin.Common.ImportAll",
				"Import all",
				"Alle importieren");
			builder.AddOrUpdate("Admin.Common.ImportSelected",
				"Import selected",
				"Ausgew�hlte importieren");
			builder.AddOrUpdate("Admin.Common.UnknownError",
				"An unknown error has occurred.",
				"Es ist ein unbekannter Fehler aufgetreten.");
			builder.AddOrUpdate("Plugins.Feed.FreeShippingThreshold",
				"Free shipping threshold",
				"Kostenloser Versand ab",
				"Amount as from shipping is free.",
				"Betrag, ab dem keine Versandkosten anfallen.");

			#endregion

			builder.AddOrUpdate("Admin.Product.Picture.Added",
				"The picture has successfully been added",
				"Das Bild wurde erfolgreich zugef�gt");

			builder.AddOrUpdate("HtmlEditor.ClickToEdit", "Click to edit HTML...", "Hier klicken, um HTML zu editieren...");

			builder.AddOrUpdate("Admin.Catalog.Attributes.ProductAttributes.Fields.ExportMappings.Note",
				"Define mappings of attribute values to export fields according to the pattern <b>&lt;Format prefix&gt;:&lt;Export field name&gt;</b>. Example: <b>gmc:color</b> exports the attribute values for colors to the field <b>color</b> during the Google Merchant Center Export. The mappings are only effective when exporting attribute combinations.",
				"Legen Sie Zuordnungen von Attributwerten zu Exportfeldern nach dem Muster <b>&lt;Formatpr�fix&gt;:&lt;Export-Feldname&gt;</b> fest. Beispiel: <b>gmc:color</b> exportiert beim Google Merchant Center Export die Attributwerte f�r Farben in das Feld <b>color</b>. Die Zuordnungen sind nur beim Export von Attributkombinationen wirksam.");

			builder.AddOrUpdate("Admin.Catalog.Attributes.ProductAttributes.Fields.ExportMappings",
				"Mappings to export fields",
				"Zuordnungen zu Exportfeldern",
				"Allows to map attribute values to export fields. Each entry has to be entered in a new line.",
				"Erm�glicht die Zuordnung von Attributwerten zu Exportfeldern. Jeder Eintrag muss in einer neuen Zeile erfolgen.");
		}
	}
}
