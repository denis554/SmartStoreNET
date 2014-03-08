using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using SmartStore.Core;
using SmartStore.Data;
using SmartStore.Data.Setup;
using SmartStore.Plugin.Shipping.ByWeight.Data.Migrations;

namespace SmartStore.Plugin.Shipping.ByWeight.Data
{
    /// <summary>
    /// Object context
    /// </summary>
    public class ShippingByWeightObjectContext : ObjectContextBase
    {
        public const string ALIASKEY = "sm_object_context_shipping_weight_zip";
        
		static ShippingByWeightObjectContext()
		{
			Database.SetInitializer(new MigrateDatabaseInitializer<ShippingByWeightObjectContext, Configuration>(new[] { "ShippingByWeight" }));
		}

		/// <summary>
		/// For tooling support, e.g. EF Migrations
		/// </summary>
		public ShippingByWeightObjectContext()
			: base()
		{
		}

        public ShippingByWeightObjectContext(string nameOrConnectionString)
            : base(nameOrConnectionString, ALIASKEY)
        {
        }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Configurations.Add(new ShippingByWeightRecordMap());

            //disable EdmMetadata generation
            //modelBuilder.Conventions.Remove<IncludeMetadataConvention>();
            base.OnModelCreating(modelBuilder);
        }
       
    }
}