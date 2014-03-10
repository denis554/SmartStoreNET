﻿using System;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.Migrations;
using System.Transactions;
using SmartStore.Core.Data;
using SmartStore.Data.Migrations;

namespace SmartStore.Data.Setup
{
    /// <summary>
    /// An implementation of IDatabaseInitializer that will recreate and optionally re-seed the
    /// database only if the database does not exist.
    /// To seed the database, create a derived class and override the Seed method.
    /// </summary>
    /// <typeparam name="TContext">The type of the context.</typeparam>
    public class CreateCeDatabaseIfNotExists<TContext> : SqlCeInitializer<TContext> where TContext : DbContext
    {
        #region Strategy implementation

        public override void InitializeDatabase(TContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException("context");
            }
            var replacedContext = ReplaceSqlCeConnection(context);
			
            bool databaseExists;
            using (new TransactionScope(TransactionScopeOption.Suppress))
            {
                databaseExists = replacedContext.Database.Exists();
            }

			var config = new MigrationsConfiguration
			{
				TargetDatabase = new DbConnectionInfo(replacedContext.Database.Connection.ConnectionString, "System.Data.SqlServerCe.4.0")
			};

			var migrator = new DbMigrator(config);
			migrator.Update();

			//if (databaseExists)
			//{
			//	// If there is no metadata either in the model or in the database, then
			//	// we assume that the database matches the model because the common case for
			//	// these scenarios are database/model first and/or an existing database.
			//	////if (!context.Database.CompatibleWithModel(throwIfNoMetadata: false))
			//	////{
			//	////	throw new InvalidOperationException(string.Format("The model backing the '{0}' context has changed since the database was created. Either manually delete/update the database, or call Database.SetInitializer with an IDatabaseInitializer instance. For example, the DropCreateDatabaseIfModelChanges strategy will automatically delete and recreate the database, and optionally seed it with new data.", context.GetType().Name));
			//	////}

			//	//bool migrate = false;
			//	//try
			//	//{
			//	//	migrate = !replacedContext.Database.CompatibleWithModel(true);
			//	//}
			//	//catch (NotSupportedException)
			//	//{
			//	//	// if there are no metadata for migration
			//	//	migrate = true;
			//	//}

			//	//if (migrate)
			//	//{
			//	//	var migrator = new DbMigrator(config);
			//	//	migrator.Update();
			//	//}
			//}
			//else
			//{
			//	context.Database.Create();
			//	Seed(context);
			//	context.SaveChanges();

			//	//var migrator = new DbMigrator(config);
			//	//migrator.Update();
			//}
        }

        #endregion

        #region Seeding methods

        /// <summary>
        /// A that should be overridden to actually add data to the context for seeding. 
        /// The default implementation does nothing.
        /// </summary>
        /// <param name="context">The context to seed.</param>
        protected virtual void Seed(TContext context)
        {
        }

        #endregion
    }


}
