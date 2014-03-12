﻿using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.Migrations;
using System.Data.Entity.Migrations.Infrastructure;
using System.Linq;
using System.Text.RegularExpressions;

namespace SmartStore.Data.Setup
{
	
	/// <summary>
	/// Provides advanced migrations by providing a seeding platform for each migration.
	/// This allows for initial seed data after each new database version (for example when 
	/// deploying new features and you want to include initial data). Seeders will be executing 
	/// in the correct order after all migrations have been completed.
	/// </summary>
	public class DbSeedingMigrator<TContext> : DbMigrator where TContext : DbContext
	{
		private static readonly Regex _migrationIdPattern = new Regex(@"\d{15}_.+");
		private const string _migrationTypeFormat = "{0}.{1}, {2}";
		private const string _automaticMigration = "AutomaticMigration";

		/// <summary>
		/// Initializes a new instance of the DbMigrator class.
		/// </summary>
		/// <param name="configuration">Configuration to be used for the migration process.</param>
		public DbSeedingMigrator(DbMigrationsConfiguration configuration)
			: base(configuration)
		{ }

		/// <summary>
		/// Migrates the database to the latest version
		/// </summary>
		public void RunPendingMigrations(TContext context)
		{
			var coreSeeders = new List<IDataSeeder<SmartObjectContext>>();
			var externalSeeders = new List<IDataSeeder<TContext>>();
			var isCoreMigration = context is SmartObjectContext;

			// Apply migrations
			foreach (var migrationId in GetPendingMigrations())
			{
				if (IsAutomaticMigration(migrationId))
					continue;

				if (!IsValidMigrationId(migrationId))
					continue;

				// Resolve and instantiate the DbMigration instance from the assembly
				var migration = CreateMigrationInstanceByMigrationId(migrationId);
				
				// Seeders for the core DbContext must be run in any case 
				// (e.g. for Resource or Setting updates even from external plugins)
				IDataSeeder<SmartObjectContext> coreSeeder = migration as IDataSeeder<SmartObjectContext>;
				IDataSeeder<TContext> externalSeeder = null;

				if (!isCoreMigration)
				{
					// Context specific seeders should only be resolved
					// when origin is external (e.g. a Plugin)
					externalSeeder = migration as IDataSeeder<TContext>;
				}

				try
				{
					// Call the actual update to execute this migration
					base.Update(migrationId);
				}
				catch (AutomaticMigrationsDisabledException)
				{
					if (context is SmartObjectContext)
					{
						throw;
					}

					// DbContexts in plugin assemblies tend to produce
					// this error, but obviously without any negative side-effect.
					// Therefore catch and forget!
					// TODO: (MC) investigate this and implement a cleaner solution
				}

				if (coreSeeder != null)
					coreSeeders.Add(coreSeeder);

				if (externalSeeder != null)
					externalSeeders.Add(externalSeeder);
			}

			// Apply core data seeders first
			if (coreSeeders.Any())
			{
				var coreContext = isCoreMigration ? context as SmartObjectContext : new SmartObjectContext();
				foreach (var seeder in coreSeeders)
				{
					seeder.Seed(coreContext);
				}
			}

			// Apply external data seeders
			foreach (var seeder in externalSeeders)
			{
				seeder.Seed(context);
			}
		}

		/// <summary>
		/// Creates a full type instance for the migration id by using the current migrations namespace
		/// ie: SmartStore.Data.Migrations.34589734533_Initial
		/// </summary>
		/// <param name="migrator">The migrator context</param>
		/// <param name="migrationId">The migration id from the migrations list of the migrator</param>
		/// <returns>The full DbMigration instance</returns>
		private DbMigration CreateMigrationInstanceByMigrationId(string migrationId)
		{
			string migrationTypeName =
				string.Format(_migrationTypeFormat,
							  Configuration.MigrationsNamespace,
							  GetMigrationClassName(migrationId),
							  Configuration.MigrationsAssembly.FullName);

			return CreateTypeInstance<DbMigration>(migrationTypeName);
		}

		/// <summary>
		/// Creates a new instance of a typename
		/// </summary>
		/// <typeparam name="TType">The type of the return instance</typeparam>
		/// <param name="typeName">The full name (including assembly and namespaces) of the type to create</param>
		/// <returns>
		/// A new instance of the type if it is (or boxable to) <typeparamref name="TType"/>, 
		/// otherwise the default of <typeparamref name="TType"/>
		/// </returns>
		private TType CreateTypeInstance<TType>(string typeName) where TType : class
		{
			Type classType = Type.GetType(typeName, false);

			if (classType == null)
				return default(TType);

			object newType = Activator.CreateInstance(classType);

			return newType as TType;
		}

		#region "Migration ID validation"

		/// <summary>
		/// Checks if the migration id is valid
		/// </summary>
		/// <param name="migrationId">The migration id from the migrations list of the migrator</param>
		/// <returns>true if valid, otherwise false</returns>
		/// <remarks>
		/// This snippet has been copied from the EntityFramework source (http://entityframework.codeplex.com/)
		/// </remarks>
		private bool IsValidMigrationId(string migrationId)
		{
			if (string.IsNullOrWhiteSpace(migrationId))
				return false;

			return _migrationIdPattern.IsMatch(migrationId) || migrationId == DbMigrator.InitialDatabase;
		}

		/// <summary>
		/// Checks if the the migration id belongs to an automatic migration
		/// </summary>
		/// <param name="migrationId">The migration id from the migrations list of the migrator</param>
		/// <returns>true if automatic, otherwise false</returns>
		/// <remarks>
		/// This snippet has been copied from the EntityFramework source (http://entityframework.codeplex.com/)
		/// </remarks>
		private bool IsAutomaticMigration(string migrationId)
		{
			if (string.IsNullOrWhiteSpace(migrationId))
				return false;

			return migrationId.EndsWith(_automaticMigration, StringComparison.Ordinal);
		}

		/// <summary>
		/// Gets the ClassName from a migration id
		/// </summary>
		/// <param name="migrationId">The migration id from the migrations list of the migrator</param>
		/// <returns>The class name for this migration id</returns>
		/// <remarks>
		/// This snippet has been copied from the EntityFramework source (http://entityframework.codeplex.com/)
		/// </remarks>
		private string GetMigrationClassName(string migrationId)
		{
			if (string.IsNullOrWhiteSpace(migrationId))
				return string.Empty;

			return migrationId.Substring(16);
		}

		#endregion
	}
}
