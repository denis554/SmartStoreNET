﻿using SmartStore.Core.Domain.Messages;
using SmartStore.Tests;
using NUnit.Framework;

namespace SmartStore.Data.Tests.Messages
{
    [TestFixture]
    public class MessageTemplatePersistenceTests : PersistenceTest
    {
        [Test]
        public void Can_save_and_load_messageTemplate()
        {
            var mt = new MessageTemplate()
            {
				StoreId = 1,
                Name = "Template1",
                BccEmailAddresses = "Bcc",
                Subject = "Subj",
                Body = "Some text",
                IsActive = true,
                EmailAccountId = 1
            };


            var fromDb = SaveAndLoadEntity(mt);
            fromDb.ShouldNotBeNull();
			fromDb.StoreId.ShouldEqual(1);
            fromDb.Name.ShouldEqual("Template1");
            fromDb.BccEmailAddresses.ShouldEqual("Bcc");
            fromDb.Subject.ShouldEqual("Subj");
            fromDb.Body.ShouldEqual("Some text");
            fromDb.IsActive.ShouldBeTrue();

            fromDb.EmailAccountId.ShouldEqual(1);
        }
    }
}