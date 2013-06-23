﻿using System;
using System.Linq;
using SmartStore.Core.Domain.Catalog;
using SmartStore.Core.Domain.Directory;
using SmartStore.Tests;
using NUnit.Framework;

namespace SmartStore.Data.Tests.Catalog
{
    [TestFixture]
    public class ProductTagPersistenceTests : PersistenceTest
    {
        [Test]
        public void Can_save_and_load_productTag()
        {
            var productTag = new ProductTag
                               {
                                   Name = "Name 1",
                                   ProductCount = 1,
                               };

            var fromDb = SaveAndLoadEntity(productTag);
            fromDb.ShouldNotBeNull();
            fromDb.Name.ShouldEqual("Name 1");
            fromDb.ProductCount.ShouldEqual(1);
        }
    }
}