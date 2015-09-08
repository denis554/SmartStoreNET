namespace SmartStore.Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
	using SmartStore.Data.Setup;

	public partial class ExportFramework1 : DbMigration, ILocaleResourcesProvider, IDataSeeder<SmartObjectContext>
    {
        public override void Up()
        {
            AddColumn("dbo.ExportDeployment", "CreateZip", c => c.Boolean(nullable: false));
            AddColumn("dbo.ExportDeployment", "HttpTransmissionTypeId", c => c.Int(nullable: false));
            AddColumn("dbo.ExportDeployment", "HttpTransmissionType", c => c.Int(nullable: false));
            AddColumn("dbo.ExportDeployment", "PassiveMode", c => c.Boolean(nullable: false));
            AddColumn("dbo.ExportDeployment", "UseSsl", c => c.Boolean(nullable: false));
            AddColumn("dbo.ExportProfile", "FileNamePattern", c => c.String(maxLength: 400));
            AddColumn("dbo.ExportProfile", "EmailAccountId", c => c.Int(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.ExportProfile", "EmailAccountId");
            DropColumn("dbo.ExportProfile", "FileNamePattern");
            DropColumn("dbo.ExportDeployment", "UseSsl");
            DropColumn("dbo.ExportDeployment", "PassiveMode");
            DropColumn("dbo.ExportDeployment", "HttpTransmissionType");
            DropColumn("dbo.ExportDeployment", "HttpTransmissionTypeId");
            DropColumn("dbo.ExportDeployment", "CreateZip");
        }

		public bool RollbackOnFailure
		{
			get { return false; }
		}

		public void Seed(SmartObjectContext context)
		{
			context.MigrateLocaleResources(MigrateLocaleResources);

			context.Execute("DELETE FROM [dbo].[ScheduleTask] WHERE [Type] = 'SmartStore.Billiger.StaticFileGenerationTask, SmartStore.Billiger'");

			context.MigrateSettings(x =>
			{
				x.DeleteGroup("BilligerSettings");
			});
		}

		public void MigrateLocaleResources(LocaleResourcesBuilder builder)
		{
			builder.AddOrUpdate("Admin.Common.ExportSelected", "Export selected", "Ausgew�hlte exportieren");
			builder.AddOrUpdate("Admin.Common.ExportAll", "Export all", "Alle exportieren");
			builder.AddOrUpdate("Common.Disabled", "Disabled", "Deaktiviert");

			builder.AddOrUpdate("Admin.System.ScheduleTask", "Scheduled task", "Geplante Aufgabe");

			builder.AddOrUpdate("Admin.DataExchange.Export.NoExportProvider",
				"There were no export provider found.",
				"Es wurden keine Export-Provider gefunden.");

			builder.AddOrUpdate("Admin.DataExchange.Export.ProgressInfo",
				"{0} of {1} records exported",
				"{0} von {1} Datens�tzen exportiert");

			builder.AddOrUpdate("Admin.DataExchange.Export.FileNamePattern",
				"Pattern for file names",
				"Muster f�r Dateinamen",
				"Specifies the pattern for creating file names.",
				"Legt das Muster fest, nach dem Dateinamen erzeugt werden.");

			builder.AddOrUpdate("Admin.DataExchange.Export.EmailAccountId",
				"Email notification",
				"E-Mail Benachrichtigung",
				"Specifies the email account used to send a notification message of the completion of the export.",
				"Legt das E-Mail Konto fest, �ber welches eine Benachrichtigung �ber die Fertigstellung des Exports verschickt werden soll.");

			builder.AddOrUpdate("Admin.DataExchange.Export.CompletedEmailAddresses",
				"Email addresses",
				"E-Mail-Addressen",
				"Specifies the email addresses where to send the notification message.",
				"Legt die E-Mail Addressen fest, an die die Benachrichtigung geschickt werden soll.");

			builder.AddOrUpdate("Admin.DataExchange.Export.CompletedEmail.Subject",
				"Export of profile \"{0}\" has been finished",
				"Export von Profile \"{0}\" ist abgeschlossen");

			builder.AddOrUpdate("Admin.DataExchange.Export.CompletedEmail.Body",
				"This is an automatic notification of store \"{0}\" about a recent data export. You can disable the sending of this message in the details of the export profile.",
				"Dies ist eine automatische Benachrichtung von Shop \"{0}\" �ber einen erfolgten Datenexport. Sie k�nnen den Versand dieser Mitteilung in den Details des Exportprofils deaktivieren.");

			builder.AddOrUpdate("Admin.DataExchange.Export.FolderName",
				"Folder name",
				"Ordnername",
				"Specifies the name of the folder where the data will be exported.",
				"Legt den Namen des Ordners fest, in den die Daten exportiert werden.");

			builder.AddOrUpdate("Admin.DataExchange.Export.FolderAndFileName.Validate",
				"Please enter a valid folder and file name. Example for file names: %Misc.FileNumber%-%ExportProfile.Id%-gmc-%Store.Name%",
				"Bitte einen g�ltigen Ordner- und Dateinamen eingeben. Beispiel f�r Dateinamen: %Misc.FileNumber%-%ExportProfile.Id%-gmc-%Store.Name%");


			builder.AddOrUpdate("Admin.DataExchange.Export.Deployment.CreateZip",
				"Create ZIP archive",
				"ZIP-Archiv erstellen",
				"Specifies whether to combine the export files in a ZIP archive and only to deploy the archive.",
				"Legt fest, ob die Exportdateien in einem ZIP-Archiv zusammengefasst und nur das Archiv bereitgestellt werden soll.");

			builder.AddOrUpdate("Admin.DataExchange.Export.Projection.AttributeCombinationAsProduct",
				"Export attribute combinations",
				"Attributkombinationen exportieren",
				"Specifies whether to export a standalone product for each active attribute combination.",
				"Legt fest, ob f�r jede aktive Attributkombination ein eigenst�ndiges Produkt exportiert werden soll.");

			builder.AddOrUpdate("Admin.DataExchange.Export.Projection.AttributeCombinationValueMerging",
				"Attribute values",
				"Attributwerte",
				"Specifies if and how to further process the attribute values.",
				"Legt fest, ob und wie die Werte der Attribute weiter verarbeitet werden sollen.");

			builder.AddOrUpdate("Enums.SmartStore.Core.Domain.DataExchange.ExportAttributeValueMerging.None",
				"Not specified", "Nicht spezifiziert");
			builder.AddOrUpdate("Enums.SmartStore.Core.Domain.DataExchange.ExportAttributeValueMerging.AppendAllValuesToName",
				"Append all values to the product name", "Alle Werte an den Produktnamen anh�ngen");

			builder.AddOrUpdate("Admin.DataExchange.Export.Deployment.HttpTransmissionType",
				"HTTP transmission type",
				"HTTP �bertragungsart",
				"Specifies how to transmit the export files via HTTP.",
				"Legt fest, aus welcher Art die Exportdateien per HTTP �bertragen werden sollen.");

			builder.AddOrUpdate("Enums.SmartStore.Core.Domain.DataExchange.ExportHttpTransmissionType.SimplePost", "Simple POST", "Einfacher POST");
			builder.AddOrUpdate("Enums.SmartStore.Core.Domain.DataExchange.ExportHttpTransmissionType.MultipartFormDataPost", "Multipart form data POST", "Multipart-Form-Data POST");

			builder.AddOrUpdate("Admin.DataExchange.Export.Deployment.PassiveMode",
				"Passive mode",
				"Passiver Modus",
				"Specifies whether to exchange data in active or passive mode.",
				"Legt fest, ob Daten im aktiven oder passiven Modus ausgetauscht werden sollen.");

			builder.AddOrUpdate("Admin.DataExchange.Export.Deployment.UseSsl",
				"Use SSL",
				"SSL verwenden",
				"Specifies whether to use a SSL (Secure Sockets Layer) connection.",
				"Legt fest, ob einen SSL (Secure Sockets Layer) Verbindung genutzt werden soll.");


			builder.AddOrUpdate("Admin.DataExchange.Export.Filter.Note",
				"Specify individual filters to limit the exported data.",
				"Legen Sie individuelle Filter fest, um die zu exportierenden Daten einzugrenzen.");

			builder.AddOrUpdate("Admin.DataExchange.Export.Projection.Note",
				"The following information will be taken into account during the export and integrated in the process.",
				"Die folgenden Angaben werden beim Export ber�cksichtigt und an entsprechenden Stellen in den Vorgang eingebunden.");

			builder.AddOrUpdate("Admin.DataExchange.Export.Configuration.Note",
				"The following specific information will be taken into account by the provider during the export.",
				"Die folgenden spezifischen Angaben werden durch den Provider beim Export ber�cksichtigt.");

			builder.AddOrUpdate("Admin.DataExchange.Export.Configuration.NotRequired",
				"The export provider <b>{0}</b> requires no further configuration.",
				"Der Export-Provider <b>{0}</b> ben�tigt keine weitergehende Konfiguration.");

			builder.AddOrUpdate("Admin.DataExchange.Export.Deployment.Note",
				"Click <b>Insert</b> to add one or multiple publishing profiles to specify how to further proceed with the export files.",
				"Legen Sie �ber <b>Hinzuf�gen</b> ein oder mehrere Ver�ffentlichungsprofile an, um festzulegen wie mit den Exportdateien weiter zu verfahren ist.");

			builder.AddOrUpdate("Enums.SmartStore.Core.Domain.DataExchange.ExportOrderStatusChange.None", "None", "Keine");
			builder.AddOrUpdate("Enums.SmartStore.Core.Domain.DataExchange.ExportOrderStatusChange.Processing", "Processing", "Wird bearbeitet");
			builder.AddOrUpdate("Enums.SmartStore.Core.Domain.DataExchange.ExportOrderStatusChange.Complete", "Complete", "Komplett");

			builder.AddOrUpdate("Admin.DataExchange.Export.Projection.OrderStatusChange",
				"Change order status to",
				"Auftragsstatus �ndern in",
				"Specifies if and how to change the status of the exported orders.",
				"Legt fest, ob und wie der Status der exportierten Auftr�ge ge�ndert werden soll.");

			builder.AddOrUpdate("Admin.DataExchange.Export.EnableProfileForPreview",
				"The export profile is disabled. It must be enabled to preview the export data.",
				"Das Exportprofil ist deaktiviert. F�r eine Exportvorschau muss das Exportprofil aktiviert sein.");

			builder.AddOrUpdate("Admin.DataExchange.Export.NoProfilesForProvider",
				"There were no export profile found. Create now a <a href=\"{0}\">new export profile</a>.",
				"Es wurde kein Exportprofil gefunden. Jetzt ein <a href=\"{0}\">neues Exportprofil anlegen</a>.");

			builder.AddOrUpdate("Admin.DataExchange.Export.ProfileForProvider",
				"Export profile",
				"Exportprofil",
				"The export profile for this export provider.",
				"Das Exportprofil f�r diesen Export-Provider.");

			builder.AddOrUpdate("Admin.DataExchange.Export.MoreThanOneProfile",
				"Other profiles exist for this provider",
				"F�r diesen Provider existieren weitere Profile");


			RemoveObsoleteResources(builder);
		}

		private void RemoveObsoleteResources(LocaleResourcesBuilder builder)
		{
			builder.Delete(
				"Plugins.Feed.FreeShippingThreshold"
			);

			builder.Delete(
				"Plugins.Feed.Billiger.ProductPictureSize",
				"Plugins.Feed.Billiger.ProductPictureSize.Hint",
				"Plugins.Feed.Billiger.TaskEnabled",
				"Plugins.Feed.Billiger.TaskEnabled.Hint",
				"Plugins.Feed.Billiger.StaticFileUrl",
				"Plugins.Feed.Billiger.StaticFileUrl.Hint",
				"Plugins.Feed.Billiger.GenerateStaticFileEachMinutes",
				"Plugins.Feed.Billiger.GenerateStaticFileEachMinutes.Hint",
				"Plugins.Feed.Billiger.BuildDescription",
				"Plugins.Feed.Billiger.BuildDescription.Hint",
				"Plugins.Feed.Billiger.Automatic",
				"Plugins.Feed.Billiger.DescShort",
				"Plugins.Feed.Billiger.DescLong",
				"Plugins.Feed.Billiger.DescTitleAndShort",
				"Plugins.Feed.Billiger.DescTitleAndLong",
				"Plugins.Feed.Billiger.DescManuAndTitleAndShort",
				"Plugins.Feed.Billiger.DescManuAndTitleAndLong",
				"Plugins.Feed.Billiger.DescriptionToPlainText",
				"Plugins.Feed.Billiger.DescriptionToPlainText.Hint",
				"Plugins.Feed.Billiger.ShippingCost",
				"Plugins.Feed.Billiger.ShippingCost.Hint",
				"Plugins.Feed.Billiger.ShippingTime",
				"Plugins.Feed.Billiger.ShippingTime.Hint",
				"Plugins.Feed.Billiger.Brand",
				"Plugins.Feed.Billiger.Brand.Hint",
				"Plugins.Feed.Billiger.UseOwnProductNo",
				"Plugins.Feed.Billiger.UseOwnProductNo.Hint",
				"Plugins.Feed.Billiger.Store",
				"Plugins.Feed.Billiger.Store.Hint",
				"Plugins.Feed.Billiger.ConvertNetToGrossPrices",
				"Plugins.Feed.Billiger.ConvertNetToGrossPrices.Hint",
				"Plugins.Feed.Billiger.LanguageId",
				"Plugins.Feed.Billiger.LanguageId.Hint",
				"Plugins.Feed.Billiger.ConfigSaveNote",
				"Plugins.Feed.Billiger.GeneratingNow",
				"Plugins.Feed.Billiger.SuccessResult"
			);
		}
    }
}
