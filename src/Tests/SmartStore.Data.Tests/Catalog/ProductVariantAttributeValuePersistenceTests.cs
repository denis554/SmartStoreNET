﻿using System;
using SmartStore.Core.Domain.Catalog;
using SmartStore.Tests;
using NUnit.Framework;

namespace SmartStore.Data.Tests.Catalog
{
    [TestFixture]
    public class ProductVariantAttributeValuePersistenceTests : PersistenceTest
    {
        [Test]
        public void Can_save_and_load_productVariantAttributeValue()
        {
            var pvav = new ProductVariantAttributeValue
            {
                Name = "Name 1",
                Color = "12FF33",
                PriceAdjustment = 1.1M,
                WeightAdjustment = 2.1M,
                IsPreSelected = true,
                DisplayOrder = 3,
				Quantity = 2,
                ProductVariantAttribute = new ProductVariantAttribute
                {
                    TextPrompt = "TextPrompt 1",
                    IsRequired = true,
                    AttributeControlType = AttributeControlType.DropdownList,
                    DisplayOrder = 1,
					Product = GetTestProduct(),
                    ProductAttribute = new ProductAttribute()
                    {
                        Name = "Name 1",
                        Description = "Description 1",
                    }
                }
            };

            var fromDb = SaveAndLoadEntity(pvav);
            fromDb.ShouldNotBeNull();
            fromDb.Name.ShouldEqual("Name 1");
            fromDb.Color.ShouldEqual("12FF33");
            fromDb.PriceAdjustment.ShouldEqual(1.1M);
            fromDb.WeightAdjustment.ShouldEqual(2.1M);
            fromDb.IsPreSelected.ShouldEqual(true);
            fromDb.DisplayOrder.ShouldEqual(3);
			fromDb.Quantity.ShouldEqual(2);

            fromDb.ProductVariantAttribute.ShouldNotBeNull();
            fromDb.ProductVariantAttribute.TextPrompt.ShouldEqual("TextPrompt 1");
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
