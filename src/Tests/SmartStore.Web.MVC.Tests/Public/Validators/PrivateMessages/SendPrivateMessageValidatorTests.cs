﻿using FluentValidation.TestHelper;
using NUnit.Framework;
using SmartStore.Web.Models.PrivateMessages;

namespace SmartStore.Web.MVC.Tests.Public.Validators.PrivateMessages
{
    [TestFixture]
    public class SendPrivateMessageValidatorTests : BaseValidatorTests
    {
        private SendPrivateMessageValidator _validator;
        
        [SetUp]
        public new void Setup()
        {
            _validator = new SendPrivateMessageValidator();
        }

        [Test]
        public void Should_have_error_when_subject_is_null_or_empty()
        {
            var model = new SendPrivateMessageModel();
            model.Subject = null;
            _validator.ShouldHaveValidationErrorFor(x => x.Subject, model);
            model.Subject = "";
            _validator.ShouldHaveValidationErrorFor(x => x.Subject, model);
        }

        [Test]
        public void Should_not_have_error_when_subject_is_specified()
        {
            var model = new SendPrivateMessageModel();
            model.Subject = "some comment";
            _validator.ShouldNotHaveValidationErrorFor(x => x.Subject, model);
        }

        [Test]
        public void Should_have_error_when_message_is_null_or_empty()
        {
            var model = new SendPrivateMessageModel();
            model.Message = null;
            _validator.ShouldHaveValidationErrorFor(x => x.Message, model);
            model.Message = "";
            _validator.ShouldHaveValidationErrorFor(x => x.Message, model);
        }

        [Test]
        public void Should_not_have_error_when_message_is_specified()
        {
            var model = new SendPrivateMessageModel();
            model.Message = "some comment";
            _validator.ShouldNotHaveValidationErrorFor(x => x.Message, model);
        }
    }
}
