﻿using SmartStore.Core.Domain.Security;
using SmartStore.Services.Security;
using SmartStore.Tests;
using NUnit.Framework;

namespace SmartStore.Services.Tests.Security
{
    [TestFixture]
    public class EncryptionServiceTests : ServiceTest
    {
        IEncryptionService _encryptionService;
        SecuritySettings _securitySettings;

        [SetUp]
        public new void SetUp()
        {
            _securitySettings = new SecuritySettings()
            {
                EncryptionKey = "273ece6f97dd844d"
            };
            _encryptionService = new EncryptionService(_securitySettings);
        }

        [Test]
        public void Can_hash()
        {
            string password = "MyLittleSecret";
            var saltKey = "salt1";
            var hashedPassword = _encryptionService.CreatePasswordHash(password, saltKey);
            //hashedPassword.ShouldBeNotBeTheSameAs(password);
            hashedPassword.ShouldEqual("A07A9638CCE93E48E3F26B37EF7BDF979B8124D6");
        }

        [Test]
        public void Can_encrypt_and_decrypt()
        {
            var password = "MyLittleSecret";
            string encryptedPassword = _encryptionService.EncryptText(password);
            var decryptedPassword = _encryptionService.DecryptText(encryptedPassword);
            decryptedPassword.ShouldEqual(password);
        }
    }
}
