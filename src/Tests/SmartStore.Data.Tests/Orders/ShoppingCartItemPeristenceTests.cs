﻿using System;
using SmartStore.Core.Domain.Catalog;
using SmartStore.Core.Domain.Customers;
using SmartStore.Core.Domain.Orders;
using SmartStore.Core.Domain.Stores;
using SmartStore.Tests;
using NUnit.Framework;

namespace SmartStore.Data.Tests.Orders
{
    [TestFixture]
    public class ShoppingCartItemPeristenceTests : PersistenceTest
    {
        [Test]
        public void Can_save_and_load_shoppingCartItem()
        {
            var sci = new ShoppingCartItem
            {
                ShoppingCartType = ShoppingCartType.ShoppingCart,
                AttributesXml = "AttributesXml 1",
                CustomerEnteredPrice = 1.1M,
                Quantity= 2,
                CreatedOnUtc = new DateTime(2010, 01, 01),
                UpdatedOnUtc = new DateTime(2010, 01, 02),
                Customer = GetTestCustomer(),
				Product = GetTestProduct()
            };

            var fromDb = SaveAndLoadEntity(sci);
            fromDb.ShouldNotBeNull();

            fromDb.ShoppingCartType.ShouldEqual(ShoppingCartType.ShoppingCart);
            fromDb.AttributesXml.ShouldEqual("AttributesXml 1");
            fromDb.CustomerEnteredPrice.ShouldEqual(1.1M);
            fromDb.Quantity.ShouldEqual(2);
            fromDb.CreatedOnUtc.ShouldEqual(new DateTime(2010, 01, 01));
            fromDb.UpdatedOnUtc.ShouldEqual(new DateTime(2010, 01, 02));

            fromDb.Customer.ShouldNotBeNull();
			fromDb.Product.ShouldNotBeNull();
        }

        protected Customer GetTestCustomer()
        {
            return new Customer
            {
                CustomerGuid = Guid.NewGuid(),
                AdminComment = "some comment here",
                IsTaxExempt = true,
                Active = true,
                Deleted = false,
                CreatedOnUtc = new DateTime(2010, 01, 01),
                LastActivityDateUtc = new DateTime(2010, 01, 02)
            };
        }

		protected Product GetTestProduct()
		{
			return new Product
			{
				Name = "Product name 1",
				CreatedOnUtc = new DateTime(2010, 01, 03),
				UpdatedOnUtc = new DateTime(2010, 01, 04),
			};
		}
    }
}
