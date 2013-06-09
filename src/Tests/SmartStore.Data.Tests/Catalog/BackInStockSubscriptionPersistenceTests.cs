﻿using System;
using SmartStore.Core.Domain.Catalog;
using SmartStore.Core.Domain.Customers;
using SmartStore.Tests;
using NUnit.Framework;
using SmartStore.Core.Domain.Stores;

namespace SmartStore.Data.Tests.Catalog
{
    [TestFixture]
    public class BackInStockSubscriptionPersistenceTests : PersistenceTest
    {
        [Test]
        public void Can_save_and_load_backInStockSubscription()
        {
            var backInStockSubscription = new BackInStockSubscription()
                                     {
										 Store = new Store
										 {
											 Name = "Store 1",
											 Url = "http://www.yourstore.com",
										 },
                                         ProductVariant = new ProductVariant
                                         {
                                             Name = "Product variant name 1",
                                             CreatedOnUtc = new DateTime(2010, 01, 03),
                                             UpdatedOnUtc = new DateTime(2010, 01, 04),
                                             Product = new Product()
                                             {
                                                 Name = "Name 1",
                                                 Published = true,
                                                 Deleted = false,
                                                 CreatedOnUtc = new DateTime(2010, 01, 01),
                                                 UpdatedOnUtc = new DateTime(2010, 01, 02)
                                             }
                                         },
                                         Customer = new Customer
                                         {
                                             CustomerGuid = Guid.NewGuid(),
                                             AdminComment = "some comment here",
                                             Active = true,
                                             Deleted = false,
                                             CreatedOnUtc = new DateTime(2010, 01, 01),
                                             LastActivityDateUtc = new DateTime(2010, 01, 02)
                                         },
                                         CreatedOnUtc = new DateTime(2010, 01, 02)
                                     };

            var fromDb = SaveAndLoadEntity(backInStockSubscription);
            fromDb.ShouldNotBeNull();

			fromDb.Store.ShouldNotBeNull();

            fromDb.ProductVariant.ShouldNotBeNull();
            fromDb.ProductVariant.Name.ShouldEqual("Product variant name 1");

            fromDb.Customer.ShouldNotBeNull();

            fromDb.CreatedOnUtc.ShouldEqual(new DateTime(2010, 01, 02));
        }
    }
}
