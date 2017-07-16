namespace SmartStore.Data.Migrations
{
	using System;
	using System.Data.Entity;
	using System.Data.Entity.Migrations;
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
        }

		public void MigrateSettings(SmartObjectContext context)
		{
		}

		public void MigrateLocaleResources(LocaleResourcesBuilder builder)
		{
            builder.AddOrUpdate("Admin.Configuration.Settings.Search.DefaultSortOrderMode",
                "Default product sort order",
                "Standardsortierreihenfolge f�r Produkte",
                "Specifies the default product sort order in search results.",
                "Legt die Standardsortierreihenfolge f�r Produkte in den Suchergebnissen fest.");

			builder.AddOrUpdate("Common.Recommended", "Recommended", "Empfohlen");

			builder.AddOrUpdate("Admin.Configuration.Themes.Option.AssetCachingEnabled",
				"Enable asset caching",
				"Asset Caching aktivieren",
				"Determines whether compiled asset files should be cached in file system in order to speed up application restarts. Select 'Auto', if caching should depend on the debug setting in web.config.",
				"Legt fest, ob kompilierte JS- und CSS-Dateien wie bspw. 'Sass' im Dateisystem zwischengespeichert werden sollen, um den Programmstart zu beschleunigen. W�hlen Sie 'Automatisch', wenn das Caching von der Debug-Einstellung in der web.config abh�ngig sein soll.");

			builder.AddOrUpdate("Admin.Configuration.Themes.ClearAssetCache",
				"Clear asset cache",
				"Asset Cache leeren");
		}
    }
}
