﻿using SmartStore.Core.Domain.Localization;
using SmartStore.Tests;
using NUnit.Framework;

namespace SmartStore.Data.Tests.Localization
{
    [TestFixture]
    public class LocaleStringResourcePersistenceTests : PersistenceTest
    {
        [Test]
        public void Can_save_and_load_lst()
        {
            var lst = new LocaleStringResource
            {
                ResourceName = "ResourceName1",
                ResourceValue = "ResourceValue2",
                Language = new Language()
                               {
                                   Name = "English",
                                   LanguageCulture = "en-Us",
                                   FlagImageFileName = "us.png",
                                   Published = true,
                                   DisplayOrder = 1
                               }
            };

            var fromDb = SaveAndLoadEntity(lst);
            fromDb.ShouldNotBeNull();
            fromDb.ResourceName.ShouldEqual("ResourceName1");
            fromDb.ResourceValue.ShouldEqual("ResourceValue2");

            fromDb.Language.ShouldNotBeNull();
            fromDb.Language.Name.ShouldEqual("English");
        }
    }
}
