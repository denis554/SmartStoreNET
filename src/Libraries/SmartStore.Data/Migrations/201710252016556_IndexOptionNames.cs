namespace SmartStore.Data.Migrations
{
    using System.Data.Entity.Migrations;
    using Setup;

    public partial class IndexOptionNames : DbMigration, ILocaleResourcesProvider, IDataSeeder<SmartObjectContext>
    {
        public override void Up()
        {
            AddColumn("dbo.SpecificationAttribute", "IndexOptionNames", c => c.Boolean(nullable: false));
            AddColumn("dbo.ProductAttribute", "IndexOptionNames", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.ProductAttribute", "IndexOptionNames");
            DropColumn("dbo.SpecificationAttribute", "IndexOptionNames");
        }

        public bool RollbackOnFailure
        {
            get { return false; }
        }

        public void Seed(SmartObjectContext context)
        {
            context.MigrateLocaleResources(MigrateLocaleResources);

            context.SaveChanges();
        }

        public void MigrateLocaleResources(LocaleResourcesBuilder builder)
        {
            builder.AddOrUpdate("Search.Fields.SpecificationAttributeOptionName",
                "Option name of specification attributes",
                "Optionsname von Spezifikationsattributen");

            builder.AddOrUpdate("Search.Fields.ProductAttributeOptionName",
                "Option name of product attributes",
                "Optionsname von Produktattributen");

            builder.AddOrUpdate("Admin.Catalog.Attributes.SpecificationAttributes.Fields.IndexOptionNames",
                "Index option names",
                "Optionsnamen indexieren",
                "Specifies whether option names should be included in the search index so that products can be found by them. This setting is only effective by using the 'MegaSearchPlus' plugin. Changes will take effect after next update of the search index.",
                "Legt fest, ob Optionsnamen mit in den Suchindex aufgenommen werden sollen, damit Produkte �ber sie gefunden werden k�nnen. Diese Einstellung ist nur unter Verwendung des 'MegaSearchPlus' Plugins wirksam. �nderungen werden nach der n�chsten Aktualisierung des Suchindex wirksam.");

            builder.AddOrUpdate("Admin.Catalog.Attributes.ProductAttributes.Fields.IndexOptionNames",
                "Index option names",
                "Optionsnamen indexieren",
                "Specifies whether option names should be included in the search index so that products can be found by them. This setting is only effective by using the 'MegaSearchPlus' plugin. Changes will take effect after next update of the search index.",
                "Legt fest, ob Optionsnamen mit in den Suchindex aufgenommen werden sollen, damit Produkte �ber sie gefunden werden k�nnen. Diese Einstellung ist nur unter Verwendung des 'MegaSearchPlus' Plugins wirksam. �nderungen werden nach der n�chsten Aktualisierung des Suchindex wirksam.");
        }
    }
}
