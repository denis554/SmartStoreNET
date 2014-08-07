namespace SmartStore.Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
	using SmartStore.Data.Setup;

	public partial class BetterReturnRequest : DbMigration, ILocaleResourcesProvider, IDataSeeder<SmartObjectContext>
    {
        public override void Up()
        {
			AddColumn("dbo.Order", "UpdatedOnUtc", c => c.DateTime(nullable: false));
			AddColumn("dbo.Order", "RewardPointsRemaining", c => c.Int());

			try
			{
				Sql("Update [dbo].[Order] Set UpdatedOnUtc = CreatedOnUtc");
			}
			catch (Exception exc)
			{
				exc.Dump();
			}
        }
        
        public override void Down()
        {
			DropColumn("dbo.Order", "UpdatedOnUtc");
			DropColumn("dbo.Order", "RewardPointsRemaining");
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
			builder.Delete("Admin.Orders.Products.ReturnRequests");

			builder.AddOrUpdate("Admin.Orders.Products.ReturnRequest",
				"Return request",
				"R�cksendeauftrag");

			builder.AddOrUpdate("Admin.Orders.Products.ReturnRequest.Create",
				"Create return",
				"R�cksendung erstellen");

			builder.AddOrUpdate("Admin.ReturnRequests.Accept",
				"Accept",
				"Akzeptieren");

			builder.AddOrUpdate("Admin.ReturnRequests.Accept.Caption",
				"Accept return request",
				"R�cksendung akzeptieren");


			builder.AddOrUpdate("Admin.Orders.OrderItem.Cancel.Info",
				"Stock quantity old {0}, new {1}.<br />Reward points old {2}, new {3}.",
				"Lagerbestand alt {0}, neu {1}.<br />Bonuspunkte alt {2}, neu {3}.");

			builder.AddOrUpdate("Admin.Orders.OrderItem.Cancel.Caption",
				"Remove product",
				"Produkt entfernen");

			builder.AddOrUpdate("Admin.Orders.OrderItem.Cancel.Fields.AdjustInventory",
				"Adjust inventory",
				"Lagerbestand anpassen",
				"Determines whether to increase the stock quantity.",
				"Legt fest, ob der Lagerbestand wieder erh�ht werden soll.");

			builder.AddOrUpdate("Admin.Orders.OrderItem.Cancel.Fields.ReduceRewardPoints",
				"Adjust reward points",
				"Bonuspunkte anpassen",
				"Determines whether granted reward points should be deducted again proportionately.",
				"Legt fest, ob gew�hrte Bonuspunkte wieder anteilig abgezogen werden sollen.");


			builder.AddOrUpdate("Admin.Common.ExportToPdf.All",
				"Export to PDF (all)",
				"Alles nach PDF exportieren");

			builder.AddOrUpdate("Admin.Common.ExportToPdf.Selected",
				"Export to PDF (selected)",
				"Nur Ausgew�hlte nach PDF exportieren");


			builder.AddOrUpdate("Admin.Catalog.Categories.Delete.Caption",
				"Delete category",
				"Warengruppe l�schen");

			builder.AddOrUpdate("Admin.Catalog.Categories.Delete.Hint",
				"How to treat child categories?",
				"Wie soll mit Unterwarengruppen verfahren werden?");

			builder.AddOrUpdate("Admin.Catalog.Categories.Delete.ResetParentOfChilds",
				"Remove the mapping to the parent category.",
				"Die Zuordnung zur �bergeordneten Warengruppe entfernen.");

			builder.AddOrUpdate("Admin.Catalog.Categories.Delete.DeleteChilds",
				"Delete as well.",
				"Ebenfalls l�schen.");


			builder.AddOrUpdate("Admin.Configuration.Settings.RewardPoints.PointsForProductReview",
				"Points for a product review",
				"Punkte f�r eine Produkt Rezension",
				"Specify number of points awarded for adding an approved product review.",
				"Bonuspunkte, die f�r das Verfassen einer genehmigten Produkt Rezension gew�hrt werden.");

			builder.AddOrUpdate("RewardPoints.Message.EarnedForProductReview",
				"Earned reward points for a product review at \"{0}\"",
				"Erhaltene Bonuspunkte f�r eine Produkt Rezension zu \"{0}\"");

			builder.AddOrUpdate("RewardPoints.Message.ReducedForProductReview",
				"Reduced reward points for a product review at \"{0}\"",
				"Abgezogene Bonuspunkte f�r eine Produkt Rezension zu \"{0}\"");
		}
    }
}
