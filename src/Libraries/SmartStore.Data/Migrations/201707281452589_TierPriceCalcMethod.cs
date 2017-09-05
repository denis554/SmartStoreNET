namespace SmartStore.Data.Migrations
{
    using Setup;
    using System;
    using System.Data.Entity.Migrations;

    public partial class TierPriceCalcMethod : DbMigration, ILocaleResourcesProvider, IDataSeeder<SmartObjectContext>
    {
        public override void Up()
        {
            AddColumn("dbo.TierPrice", "CalculationMethod", c => c.Int(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.TierPrice", "CalculationMethod");
        }

		public bool RollbackOnFailure
		{
			get { return false; }
		}
        
        public void Seed(SmartObjectContext context)
        {
            context.MigrateLocaleResources(MigrateLocaleResources);
        }

        public void MigrateLocaleResources(LocaleResourcesBuilder builder)
        {
            builder.AddOrUpdate("Admin.Product.Price.Tierprices.Fixed", "Fixed Value", "Fester Wert");
            builder.AddOrUpdate("Admin.Product.Price.Tierprices.Percental", "Percental", "Prozentual");
            builder.AddOrUpdate("Admin.Product.Price.Tierprices.Adjustment", "Adjustment", "Auf-/Abpreis");
            builder.AddOrUpdate("Admin.Catalog.Products.TierPrices.Fields.CalculationMethod", "Calculation Method", "Berechnungsmethode");
            builder.AddOrUpdate("Admin.Catalog.Products.TierPrices.Fields.Price", "Value", "Wert");

            // settings
            builder.AddOrUpdate("Admin.Configuration.Settings.Catalog.ApplyTierPricePercentageToAttributePriceAdjustments",
                "Apply tierprice percentage to attribute price adjustments",
                "Prozentuale Erm��igungen von Staffelpreisen auf Auf- & Abpreise von Attributen anwenden",
                "Specifies whether to apply tierprice percentage to attribute price adjustments",
                "Bestimmt ob prozentuale Erm��igungen von Staffelpreisen auf Auf- & Abpreise von Attributen angewendet werden sollen");

			builder.AddOrUpdate("Admin.Header.Account", "Account", "Account");
        }
    }
}
