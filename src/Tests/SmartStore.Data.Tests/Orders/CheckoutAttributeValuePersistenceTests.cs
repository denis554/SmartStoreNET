﻿using SmartStore.Core.Domain.Catalog;
using SmartStore.Core.Domain.Orders;
using SmartStore.Tests;
using NUnit.Framework;

namespace SmartStore.Data.Tests.Orders
{
    [TestFixture]
    public class CheckoutAttributeValuePersistenceTests : PersistenceTest
    {
        [Test]
        public void Can_save_and_load_checkoutAttributeValue()
        {
            var cav = new CheckoutAttributeValue()
                    {
                        Name = "Name 2",
                        PriceAdjustment = 1.1M,
                        WeightAdjustment = 2.1M,
                        IsPreSelected = true,
                        DisplayOrder = 3,
                        CheckoutAttribute = new CheckoutAttribute
                        {
                            Name = "Name 1",
                            TextPrompt = "TextPrompt 1",
                            IsRequired = true,
                            ShippableProductRequired = true,
                            IsTaxExempt = true,
                            TaxCategoryId = 1,
                            AttributeControlType = AttributeControlType.Datepicker,
                            DisplayOrder = 2
                        }
                    };

            var fromDb = SaveAndLoadEntity(cav);
            fromDb.ShouldNotBeNull();
            fromDb.Name.ShouldEqual("Name 2");
            fromDb.PriceAdjustment.ShouldEqual(1.1M);
            fromDb.WeightAdjustment.ShouldEqual(2.1M);
            fromDb.IsPreSelected.ShouldEqual(true);
            fromDb.DisplayOrder.ShouldEqual(3);

            fromDb.CheckoutAttribute.ShouldNotBeNull();
            fromDb.CheckoutAttribute.Name.ShouldEqual("Name 1");
        }
    }
}