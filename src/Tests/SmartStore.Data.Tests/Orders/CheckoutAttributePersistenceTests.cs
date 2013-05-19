﻿using System.Linq;
using SmartStore.Core.Domain.Catalog;
using SmartStore.Core.Domain.Orders;
using SmartStore.Tests;
using NUnit.Framework;

namespace SmartStore.Data.Tests.Orders
{
    [TestFixture]
    public class CheckoutAttributePersistenceTests : PersistenceTest
    {
        [Test]
        public void Can_save_and_load_checkoutAttribute()
        {
            var ca = new CheckoutAttribute
            {
                Name = "Name 1",
                TextPrompt = "TextPrompt 1",
                IsRequired = true,
                ShippableProductRequired  = true,
                IsTaxExempt = true,
                TaxCategoryId = 1,
                AttributeControlType = AttributeControlType.Datepicker,
                DisplayOrder = 2
            };

            var fromDb = SaveAndLoadEntity(ca);
            fromDb.ShouldNotBeNull();
            fromDb.Name.ShouldEqual("Name 1");
            fromDb.TextPrompt.ShouldEqual("TextPrompt 1");
            fromDb.IsRequired.ShouldEqual(true);
            fromDb.ShippableProductRequired.ShouldEqual(true);
            fromDb.IsTaxExempt.ShouldEqual(true);
            fromDb.TaxCategoryId.ShouldEqual(1);
            fromDb.AttributeControlType.ShouldEqual(AttributeControlType.Datepicker);
            fromDb.DisplayOrder.ShouldEqual(2);
        }

        [Test]
        public void Can_save_and_load_checkoutAttribute_with_values()
        {
            var ca = new CheckoutAttribute
            {
                Name = "Name 1",
                TextPrompt = "TextPrompt 1",
                IsRequired = true,
                ShippableProductRequired = true,
                IsTaxExempt = true,
                TaxCategoryId = 1,
                AttributeControlType = AttributeControlType.Datepicker,
                DisplayOrder = 2
            };
            ca.CheckoutAttributeValues.Add
                (
                    new CheckoutAttributeValue()
                    {
                        Name = "Name 2",
                        PriceAdjustment = 1,
                        WeightAdjustment = 2,
                        IsPreSelected = true,
                        DisplayOrder = 3,
                    }
                );
            var fromDb = SaveAndLoadEntity(ca);
            fromDb.ShouldNotBeNull();
            fromDb.Name.ShouldEqual("Name 1");

            fromDb.CheckoutAttributeValues.ShouldNotBeNull();
            (fromDb.CheckoutAttributeValues.Count == 1).ShouldBeTrue();
            fromDb.CheckoutAttributeValues.First().Name.ShouldEqual("Name 2");
        }
    }
}