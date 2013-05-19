﻿using System;
using System.Linq;
using SmartStore.Core.Domain.Customers;
using SmartStore.Core.Domain.Logging;
using SmartStore.Tests;
using NUnit.Framework;

namespace SmartStore.Data.Tests.Logging
{
    [TestFixture]
    public class ActivityLogPersistenceTests : PersistenceTest
    {
        [Test]
        public void Can_save_and_load_activityLogType()
        {
            var activityLogType = new ActivityLogType
                               {
                                   SystemKeyword = "SystemKeyword 1",
                                   Name = "Name 1",
                                   Enabled = true,
                               };

            var fromDb = SaveAndLoadEntity(activityLogType);
            fromDb.ShouldNotBeNull();
            fromDb.SystemKeyword.ShouldEqual("SystemKeyword 1");
            fromDb.Name.ShouldEqual("Name 1");
            fromDb.Enabled.ShouldEqual(true);
        }


        protected Customer GetTestCustomer()
        {
            return new Customer
            {
                CustomerGuid = Guid.NewGuid(),
                CreatedOnUtc = new DateTime(2010, 01, 01),
                LastActivityDateUtc = new DateTime(2010, 01, 02)
            };
        }
    }
}