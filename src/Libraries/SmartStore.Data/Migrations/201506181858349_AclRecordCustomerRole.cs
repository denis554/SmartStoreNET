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

			builder.AddOrUpdate("Admin.Orders.Fields.OrderGuid",
				"Order reference number",
				"Bestellreferenznummer",
				"The internal order reference number. In contrast to the order number it already exists during checkout that is before order creation.",
				"Die interne Bestellreferenznummer. Im Gegensatz zur Auftragsnummer existiert diese bereits im Kassenbereich, d.h. vor der eigentlichen Erstelllung des Auftrags.");

			builder.AddOrUpdate("Admin.Configuration.Payment.CannotActivatePaymentMethod")
				.Value("de", "Das Plugin erlaubt keine Aktivierung dieser Zahlungsart.");

			builder.AddOrUpdate("Admin.Orders.Fields.PaymentMethod")
				.Value("de", "Zahlungsart");

			builder.AddOrUpdate("Admin.Orders.Fields.PaymentMethod.Hint")
				.Value("de", "Die Zahlungsart f�r diese Transaktion");

			builder.AddOrUpdate("Admin.System.Warnings.PaymentMethods.NoActive")
				.Value("de", "Es existieren keine aktiven Zahlungsarten.");

			builder.AddOrUpdate("Admin.System.Warnings.PaymentMethods.OK")
				.Value("de", "Die Zahlungsarten sind OK.");

			builder.AddOrUpdate("Checkout.NoPaymentMethods")
				.Value("de", "Es sind keine Zahlungsarten verf�gbar.");

			builder.AddOrUpdate("Checkout.PaymentMethod")
				.Value("de", "Zahlungsarten");

			builder.AddOrUpdate("Order.PaymentMethod")
				.Value("de", "Zahlungsart");

			builder.AddOrUpdate("Admin.Configuration.Payment.CannotActivatePaymentMethod")
				.Value("de", "Das Plugin erlaubt keine Aktivierung dieser Zahlungsart.");

        }
    }
}
