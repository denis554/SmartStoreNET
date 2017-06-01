namespace SmartStore.Data.Migrations
{
	using System;
	using System.Data.Entity.Migrations;
	using Setup;

	public partial class MoreIndexes : DbMigration, ILocaleResourcesProvider, IDataSeeder<SmartObjectContext>
	{
		public override void Up()
		{
			CreateIndex("dbo.Product_Category_Mapping", "IsFeaturedProduct");
			CreateIndex("dbo.Product_Manufacturer_Mapping", "IsFeaturedProduct");
			CreateIndex("dbo.SpecificationAttribute", "AllowFiltering");
			CreateIndex("dbo.Product_ProductAttribute_Mapping", "AttributeControlTypeId");
			CreateIndex("dbo.ProductAttribute", "AllowFiltering");
			CreateIndex("dbo.ProductVariantAttributeValue", "Name");
			CreateIndex("dbo.ProductVariantAttributeValue", "ValueTypeId");
		}

		public override void Down()
		{
			DropIndex("dbo.ProductVariantAttributeValue", new[] { "ValueTypeId" });
			DropIndex("dbo.ProductVariantAttributeValue", new[] { "Name" });
			DropIndex("dbo.ProductAttribute", new[] { "AllowFiltering" });
			DropIndex("dbo.Product_ProductAttribute_Mapping", new[] { "AttributeControlTypeId" });
			DropIndex("dbo.SpecificationAttribute", new[] { "AllowFiltering" });
			DropIndex("dbo.Product_Manufacturer_Mapping", new[] { "IsFeaturedProduct" });
			DropIndex("dbo.Product_Category_Mapping", new[] { "IsFeaturedProduct" });
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
			builder.AddOrUpdate("Common.For", "For: {0}", "F�r: {0}");
		}
	}
}
