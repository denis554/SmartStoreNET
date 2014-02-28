﻿
using SmartStore.Core.Configuration;

namespace SmartStore.Core.Domain.Orders
{
    public class ShoppingCartSettings : ISettings
    {
        /// <summary>
        /// Gets or sets a value indicating whether a custoemr should be redirected to the shopping cart page after adding a product to the cart/wishlist
        /// </summary>
        public bool DisplayCartAfterAddingProduct { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether a custoemr should be redirected to the shopping cart page after adding a product to the cart/wishlist
        /// </summary>
        public bool DisplayWishlistAfterAddingProduct { get; set; }

        /// <summary>
        /// Gets or sets a value indicating maximum number of items in the shopping cart
        /// </summary>
        public int MaximumShoppingCartItems { get; set; }
        
        /// <summary>
        /// Gets or sets a value indicating maximum number of items in the wishlist
        /// </summary>
        public int MaximumWishlistItems { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to show product images in the mini-shopping cart block
        /// </summary>
        public bool AllowOutOfStockItemsToBeAddedToWishlist { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to move items from wishlist to cart when clicking "Add to cart" button. Otherwise, they are copied.
        /// </summary>
        public bool MoveItemsFromWishlistToCart { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to show product image on shopping cart page
        /// </summary>
        public bool ShowProductImagesOnShoppingCart { get; set; }

		/// <summary>
		/// Gets or sets a value indicating whether to show product bundle images on shopping cart page
		/// </summary>
		public bool ShowProductBundleImagesOnShoppingCart { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to show product image on wishlist page
        /// </summary>
        public bool ShowProductImagesOnWishList { get; set; }

		/// <summary>
		/// Gets or sets a value indicating whether to show product image on wishlist page
		/// </summary>
		public bool ShowProductBundleImagesOnWishList { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to show discount box on shopping cart page
        /// </summary>
        public bool ShowDiscountBox { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to show gift card box on shopping cart page
        /// </summary>
        public bool ShowGiftCardBox { get; set; }

        /// <summary>
        /// Gets or sets a number of "Cross-sells" on shopping cart page
        /// </summary>
        public int CrossSellsNumber { get; set; }
        
        /// <summary>
        /// Gets or sets a value indicating whether "email a wishlist" feature is enabled
        /// </summary>
        public bool EmailWishlistEnabled { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to enabled "email a wishlist" for anonymous users.
        /// </summary>
        public bool AllowAnonymousUsersToEmailWishlist { get; set; }
        
        /// <summary>Gets or sets a value indicating whether mini-shopping cart is enabled
        /// </summary>
        public bool MiniShoppingCartEnabled { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to show product images in the mini-shopping cart block
        /// </summary>
        public bool ShowProductImagesInMiniShoppingCart { get; set; }

        /// <summary>Gets or sets a maximum number of products which can be displayed in the mini-shopping cart block
        /// </summary>
        public int MiniShoppingCartProductNumber { get; set; }
        
        //Round is already an issue. 
        /// <summary>
        /// Gets or sets a value indicating whether to round calculated prices and total during calculation
        /// </summary>
        public bool RoundPricesDuringCalculation { get; set; }

        // codehint: sm-add
        /// <summary>
        /// Gets or sets a value indicating whether to show a legal hint in the order summary
        /// </summary>
        public bool ShowConfirmOrderLegalHint { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to show delivery times in the order summary
        /// </summary>
        public bool ShowDeliveryTimes { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to show the product short description in the order summary
        /// </summary>
        public bool ShowShortDesc { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to show the product short description in the order summary
        /// </summary>
        public bool ShowBasePrice { get; set; }
    }
}