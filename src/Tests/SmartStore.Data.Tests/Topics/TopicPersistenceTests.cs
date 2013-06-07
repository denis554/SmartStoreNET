﻿using SmartStore.Core.Domain.Topics;
using SmartStore.Tests;
using NUnit.Framework;

namespace SmartStore.Data.Tests.Topics
{
    [TestFixture]
    public class TopicPersistenceTests : PersistenceTest
    {
        [Test]
        public void Can_save_and_load_topic()
        {
            var topic = new Topic
            {
				StoreId = 1,
                SystemName = "SystemName 1",
                IncludeInSitemap = true,
                IsPasswordProtected = true,
                Password = "password",
                Title = "Title 1",
                Body = "Body 1",
                MetaKeywords = "Meta keywords",
                MetaDescription = "Meta description",
                MetaTitle = "Meta title",
            };

            var fromDb = SaveAndLoadEntity(topic);
            fromDb.ShouldNotBeNull();
			fromDb.StoreId.ShouldEqual(1);
            fromDb.SystemName.ShouldEqual("SystemName 1");
            fromDb.IncludeInSitemap.ShouldEqual(true);
            fromDb.IsPasswordProtected.ShouldEqual(true);
            fromDb.Password.ShouldEqual("password");
            fromDb.Title.ShouldEqual("Title 1");
            fromDb.Body.ShouldEqual("Body 1");
            fromDb.MetaKeywords.ShouldEqual("Meta keywords");
            fromDb.MetaDescription.ShouldEqual("Meta description");
            fromDb.MetaTitle.ShouldEqual("Meta title");
        }
    }
}