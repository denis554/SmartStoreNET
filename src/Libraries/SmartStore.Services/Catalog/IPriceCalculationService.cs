using System.Collections.Generic;
using SmartStore.Core.Domain.Catalog;
using SmartStore.Core.Domain.Customers;
using SmartStore.Core.Domain.Discounts;
using SmartStore.Core.Domain.Orders;

namespace SmartStore.Services.Catalog
{
    /// <summary>
    /// Price calculation service
    /// </summary>
    public partial interface IPriceCalculationService
    {
        /// <summary>
        /// Gets a product variant with minimal price
        /// </summary>
        /// <param name="variants">Product variants</param>
        /// <param name="customer">The customer</param>
        /// <param name="includeDiscounts">A value indicating whether include discounts or not for final price computation</param>
        /// <param name="quantity">Quantity</param>
        /// <param name="minPrice">Calcualted minimal price</param>
        /// <returns>A product variant with minimal price</returns>
        ProductVariant GetProductVariantWithMinimalPrice(IList<ProductVariant> variants,
            Customer customer, bool includeDiscounts, int quantity, out decimal? minPrice);
        
        /// <summary>
        /// Get product variant special price (is valid)
        /// </summary>
        /// <param name="productVariant">Product variant</param>
        /// <returns>Product variant special price</returns>
        decimal? GetSpecialPrice(ProductVariant productVariant);

        /// <summary>
        /// Gets the final price
        /// </summary>
        /// <param name="productVariant">Product variant</param>
        /// <param name="includeDiscounts">A value indicating whether include discounts or not for final price computation</param>
        /// <returns>Final price</returns>
        decimal GetFinalPrice(ProductVariant productVariant, bool includeDiscounts);

        /// <summary>
        /// Gets the final price
        /// </summary>
        /// <param name="productVariant">Product variant</param>
        /// <param name="customer">The customer</param>
        /// <param name="includeDiscounts">A value indicating whether include discounts or not for final price computation</param>
        /// <returns>Final price</returns>
        decimal GetFinalPrice(ProductVariant productVariant,
            Customer customer, 
            bool includeDiscounts);

        /// <summary>
        /// Gets the final price
        /// </summary>
        /// <param name="productVariant">Product variant</param>
        /// <param name="customer">The customer</param>
        /// <param name="additionalCharge">Additional charge</param>
        /// <param name="includeDiscounts">A value indicating whether include discounts or not for final price computation</param>
        /// <returns>Final price</returns>
        decimal GetFinalPrice(ProductVariant productVariant, 
            Customer customer, 
            decimal additionalCharge, 
            bool includeDiscounts);

        /// <summary>
        /// Gets the final price
        /// </summary>
        /// <param name="productVariant">Product variant</param>
        /// <param name="customer">The customer</param>
        /// <param name="additionalCharge">Additional charge</param>
        /// <param name="includeDiscounts">A value indicating whether include discounts or not for final price computation</param>
        /// <param name="quantity">Shopping cart item quantity</param>
        /// <returns>Final price</returns>
        decimal GetFinalPrice(ProductVariant productVariant,
            Customer customer,
            decimal additionalCharge, 
            bool includeDiscounts, 
            int quantity);



        /// <summary>
        /// Gets discount amount
        /// </summary>
        /// <param name="productVariant">Product variant</param>
        /// <returns>Discount amount</returns>
        decimal GetDiscountAmount(ProductVariant productVariant);

        /// <summary>
        /// Gets discount amount
        /// </summary>
        /// <param name="productVariant">Product variant</param>
        /// <param name="customer">The customer</param>
        /// <returns>Discount amount</returns>
        decimal GetDiscountAmount(ProductVariant productVariant, 
            Customer customer);

        /// <summary>
        /// Gets discount amount
        /// </summary>
        /// <param name="productVariant">Product variant</param>
        /// <param name="customer">The customer</param>
        /// <param name="additionalCharge">Additional charge</param>
        /// <returns>Discount amount</returns>
        decimal GetDiscountAmount(ProductVariant productVariant, 
            Customer customer, 
            decimal additionalCharge);

        /// <summary>
        /// Gets discount amount
        /// </summary>
        /// <param name="productVariant">Product variant</param>
        /// <param name="customer">The customer</param>
        /// <param name="additionalCharge">Additional charge</param>
        /// <param name="appliedDiscount">Applied discount</param>
        /// <returns>Discount amount</returns>
        decimal GetDiscountAmount(ProductVariant productVariant, 
            Customer customer,
            decimal additionalCharge, 
            out Discount appliedDiscount);

        /// <summary>
        /// Gets discount amount
        /// </summary>
        /// <param name="productVariant">Product variant</param>
        /// <param name="customer">The customer</param>
        /// <param name="additionalCharge">Additional charge</param>
        /// <param name="quantity">Product quantity</param>
        /// <param name="appliedDiscount">Applied discount</param>
        /// <returns>Discount amount</returns>
        decimal GetDiscountAmount(ProductVariant productVariant, 
            Customer customer,
            decimal additionalCharge, 
            int quantity, 
            out Discount appliedDiscount);


        /// <summary>
        /// Gets the shopping cart item sub total
        /// </summary>
        /// <param name="shoppingCartItem">The shopping cart item</param>
        /// <param name="includeDiscounts">A value indicating whether include discounts or not for price computation</param>
        /// <returns>Shopping cart item sub total</returns>
        decimal GetSubTotal(ShoppingCartItem shoppingCartItem, bool includeDiscounts);

        /// <summary>
        /// Gets the shopping cart unit price (one item)
        /// </summary>
        /// <param name="shoppingCartItem">The shopping cart item</param>
        /// <param name="includeDiscounts">A value indicating whether include discounts or not for price computation</param>
        /// <returns>Shopping cart unit price (one item)</returns>
        decimal GetUnitPrice(ShoppingCartItem shoppingCartItem, bool includeDiscounts);
        



        /// <summary>
        /// Gets discount amount
        /// </summary>
        /// <param name="shoppingCartItem">The shopping cart item</param>
        /// <returns>Discount amount</returns>
        decimal GetDiscountAmount(ShoppingCartItem shoppingCartItem);

        /// <summary>
        /// Gets discount amount
        /// </summary>
        /// <param name="shoppingCartItem">The shopping cart item</param>
        /// <param name="appliedDiscount">Applied discount</param>
        /// <returns>Discount amount</returns>
        decimal GetDiscountAmount(ShoppingCartItem shoppingCartItem, out Discount appliedDiscount);
        
    }
}
