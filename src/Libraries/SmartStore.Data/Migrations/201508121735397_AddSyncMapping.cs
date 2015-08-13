namespace SmartStore.Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
	using SmartStore.Data.Setup;

	public partial class AddSyncMapping : DbMigration, ILocaleResourcesProvider, IDataSeeder<SmartObjectContext>
    {
        public override void Up()
        {
            CreateTable(
                "dbo.SyncMapping",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        EntityId = c.Int(nullable: false),
                        SourceKey = c.String(nullable: false, maxLength: 150),
                        EntityName = c.String(nullable: false, maxLength: 100),
                        ContextName = c.String(nullable: false, maxLength: 100),
                        SourceHash = c.String(maxLength: 40),
                        CustomInt = c.Int(),
                        CustomString = c.String(),
                        CustomBool = c.Boolean(),
                        SyncedOnUtc = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .Index(t => new { t.EntityId, t.EntityName, t.ContextName }, unique: true, name: "IX_SyncMapping_ByEntity")
                .Index(t => new { t.SourceKey, t.EntityName, t.ContextName }, unique: true, name: "IX_SyncMapping_BySource");
            
        }
        
        public override void Down()
        {
            DropIndex("dbo.SyncMapping", "IX_SyncMapping_BySource");
            DropIndex("dbo.SyncMapping", "IX_SyncMapping_ByEntity");
            DropTable("dbo.SyncMapping");
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
			string attachHint = "A file that is to be appended to each sent email (eg Terms, Conditions etc.)";
			string attachHintDe = "Eine Datei, die jedem gesendeten E-Mail angehangen werden soll (z.B. AGB, Widerrufsbelehrung etc.)";
			
			builder.AddOrUpdate("Admin.ContentManagement.MessageTemplates.Fields.Attachment1FileId",
				"Attachment 1",
				"Anhang 1",
				attachHint,
				attachHintDe);
			builder.AddOrUpdate("Admin.ContentManagement.MessageTemplates.Fields.Attachment2FileId",
				"Attachment 2",
				"Anhang 2",
				attachHint,
				attachHintDe);
			builder.AddOrUpdate("Admin.ContentManagement.MessageTemplates.Fields.Attachment3FileId",
				"Attachment 3",
				"Anhang 3",
				attachHint,
				attachHintDe);

			builder.AddOrUpdate("Common.FileUploader.EnterUrl",
				"Enter URL",
				"URL eingeben");
		}
    }
}
