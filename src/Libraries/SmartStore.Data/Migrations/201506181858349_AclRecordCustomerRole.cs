namespace SmartStore.Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    using SmartStore.Data.Setup;

    public partial class AclRecordCustomerRole : DbMigration, ILocaleResourcesProvider, IDataSeeder<SmartObjectContext>
    {
        public override void Up()
        {
            CreateIndex("dbo.AclRecord", "CustomerRoleId");
            AddForeignKey("dbo.AclRecord", "CustomerRoleId", "dbo.CustomerRole", "Id", cascadeDelete: true);
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.AclRecord", "CustomerRoleId", "dbo.CustomerRole");
            DropIndex("dbo.AclRecord", new[] { "CustomerRoleId" });
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
			builder.AddOrUpdate("Common.Error.PreProcessPayment",
				"Unfortunately the selected payment method caused an error. Please correct your entries, try it again or select another payment method.",
				"Die gew�hlte Zahlungsart verursachte leider einen Fehler. Bitte korrigieren Sie Ihre Eingaben, versuchen Sie es erneut oder w�hlen Sie eine andere Zahlungsart.");

            builder.AddOrUpdate("Admin.Configuration.Category.Acl.AssignToSubCategoriesAndProducts",
                "Transfer this ACL configuration to children",
                "Diese Konfiguration f�r Kindelemente �bernehmen");

            builder.AddOrUpdate("Admin.Configuration.Category.Acl.AssignToSubCategoriesAndProducts.Hint",
                @"This function assigns the ACL configuration of this category to all subcategories and products included in this category.<br />
                    Please keep in mind you have to save changes in the ACL configuration <br/> 
                    before you can assign them to all subcategories and products. <br/>
                    <b>Attention:</b> Please keep in mind that <b>existing ACL records will be deleted</b>",
                @"Diese Funktion �bernimmt die Zugriffsrecht-Konfiguration dieser Warengruppe f�r alle Unterwarengruppen und Produkte.<br/>
                    Bitte beachten Sie, dass die �nderungen der Zugriffsrechte zun�chst gespeichert werden m�ssen, <br />
                    bevor diese f�r Unterkategorien und Produkte �bernommen werden k�nnen. <br />
                    <b>Vorsicht:</b> Bitte beachten Sie, <b>dass vorhandene Zugriffsrechte �berschrieben bzw. gel�scht werden</b>.");

            builder.AddOrUpdate("Admin.Configuration.Category.Stores.AssignToSubCategoriesAndProducts",
                "Transfer this store configuration to children",
                "Diese Konfiguration f�r Kindelemente �bernehmen");

            builder.AddOrUpdate("Admin.Configuration.Category.Stores.AssignToSubCategoriesAndProducts.Hint",
                @"This function assigns the store configuration of this category to all subcategories and products included in this category.<br />
                    Please keep in mind you have to save changes in the store configuration <br/> 
                    before you can assign them to all subcategories and products. <br/>
                    <b>Attention:</b> Please keep in mind that <b>existing store mappings will be deleted</b>",
                @"Diese Funktion �bernimmt die Shop-Konfiguration dieser Warengruppe f�r alle Unterwarengruppen und Produkte.<br/>
                    Bitte beachten Sie, dass die �nderungen an der Store-Konfiguration zun�chst gespeichert werden m�ssen, <br />
                    bevor diese f�r Unterkategorien und Produkte �bernommen werden k�nnen. <br />
                    <b>Vorsicht:</b> Bitte beachten Sie, <b>dass vorhandene Store-Konfiguration �berschrieben bzw. gel�scht werden</b>.");

            builder.AddOrUpdate("Admin.Configuration.Acl.NoRolesDefined",
                "No customer roles defined",
                "Es sind keine Kundengruppen definiert");
            
        }
    }
}
