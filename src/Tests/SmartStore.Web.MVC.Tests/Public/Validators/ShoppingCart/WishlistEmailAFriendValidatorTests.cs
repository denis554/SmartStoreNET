﻿using FluentValidation.TestHelper;
using SmartStore.Web.Models.ShoppingCart;
using SmartStore.Web.Validators.ShoppingCart;
using NUnit.Framework;

namespace SmartStore.Web.MVC.Tests.Public.Validators.ShoppingCart
{
    [TestFixture]
    public class WishlistEmailAFriendValidatorTests : BaseValidatorTests
    {
        private WishlistEmailAFriendValidator _validator;
        
        [SetUp]
        public new void Setup()
        {
            _validator = new WishlistEmailAFriendValidator(_localizationService);
        }
        
        [Test]
        public void Should_have_error_when_friendEmail_is_null_or_empty()
        {
            var model = new WishlistEmailAFriendModel();
            model.FriendEmail = null;
            _validator.ShouldHaveValidationErrorFor(x => x.FriendEmail, model);
            model.FriendEmail = "";
            _validator.ShouldHaveValidationErrorFor(x => x.FriendEmail, model);
        }

        [Test]
        public void Should_have_error_when_friendEmail_is_wrong_format()
        {
            var model = new WishlistEmailAFriendModel();
            model.FriendEmail = "adminexample.com";
            _validator.ShouldHaveValidationErrorFor(x => x.FriendEmail, model);
        }

        [Test]
        public void Should_not_have_error_when_friendEmail_is_correct_format()
        {
            var model = new WishlistEmailAFriendModel();
            model.FriendEmail = "admin@example.com";
            _validator.ShouldNotHaveValidationErrorFor(x => x.FriendEmail, model);
        }

        [Test]
        public void Should_have_error_when_yourEmailAddress_is_null_or_empty()
        {
            var model = new WishlistEmailAFriendModel();
            model.YourEmailAddress = null;
            _validator.ShouldHaveValidationErrorFor(x => x.YourEmailAddress, model);
            model.YourEmailAddress = "";
            _validator.ShouldHaveValidationErrorFor(x => x.YourEmailAddress, model);
        }

        [Test]
        public void Should_have_error_when_yourEmailAddress_is_wrong_format()
        {
            var model = new WishlistEmailAFriendModel();
            model.YourEmailAddress = "adminexample.com";
            _validator.ShouldHaveValidationErrorFor(x => x.YourEmailAddress, model);
        }

        [Test]
        public void Should_not_have_error_when_yourEmailAddress_is_correct_format()
        {
            var model = new WishlistEmailAFriendModel();
            model.YourEmailAddress = "admin@example.com";
            _validator.ShouldNotHaveValidationErrorFor(x => x.YourEmailAddress, model);
        }
    }
}
