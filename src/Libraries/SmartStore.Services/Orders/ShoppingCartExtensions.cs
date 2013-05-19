using System.Collections.Generic;
using SmartStore.Core;
using SmartStore.Core.Domain.Catalog;
using SmartStore.Core.Domain.Customers;
using SmartStore.Core.Domain.Orders;

namespace SmartStore.Services.Orders
{
    /// <summary>
    /// Represents a shopping cart
    /// </summary>
    public static class ShoppingCartExtensions
    {
        /// <summary>
        /// Indicates whether the shopping cart requires shipping
        /// </summary>
        /// <param name="shoppingCart">Shopping cart</param>
        /// <returns>True if the shopping cart requires shipping; otherwise, false.</returns>
        public static bool RequiresShipping(this IList<ShoppingCartItem> shoppingCart)
        {
            foreach (var shoppingCartItem in shoppingCart)
                if (shoppingCartItem.IsShipEnabled)
                    return true;
            return false;
        }

        /// <summary>
        /// Gets a number of product in the cart
        /// </summary>
        /// <param name="shoppingCart">Shopping cart</param>
        /// <returns>Result</returns>
        public static int GetTotalProducts(this IList<ShoppingCartItem> shoppingCart)
        {
            int result = 0;
            foreach (ShoppingCartItem sci in shoppingCart)
            {
                result += sci.Quantity;
            }
            return result;
        }

        /// <summary>
        /// Gets a value indicating whether shopping cart is recurring
        /// </summary>
        /// <param name="shoppingCart">Shopping cart</param>
        /// <returns>Result</returns>
        public static bool IsRecurring(this IList<ShoppingCartItem> shoppingCart)
        {
            foreach (ShoppingCartItem sci in shoppingCart)
            {
                var productVariant = sci.ProductVariant;
                if (productVariant != null && productVariant.IsRecurring)
                    return true;
            }
            return false;
        }

        /// <summary>
        /// Get a recurring cycle information
        /// </summary>
        /// <param name="shoppingCart">Shopping cart</param>
        /// <param name="cycleLength">Cycle length</param>
        /// <param name="cyclePeriod">Cycle period</param>
        /// <param name="totalCycles">Total cycles</param>
        /// <returns>Error (if exists); otherwise, empty string</returns>
        public static string GetRecurringCycleInfo(this IList<ShoppingCartItem> shoppingCart,
            out int cycleLength, out RecurringProductCyclePeriod cyclePeriod, out int totalCycles)
        {
            string error = "";

            cycleLength = 0;
            cyclePeriod = 0;
            totalCycles = 0;

            int? _cycleLength = null;
            RecurringProductCyclePeriod? _cyclePeriod = null;
            int? _totalCycles = null;

            foreach (var sci in shoppingCart)
            {
                var productVariant = sci.ProductVariant;
                if (productVariant == null)
                {
                    throw new SmartException(string.Format("Product variant (Id={0}) cannot be loaded", sci.ProductVariantId));
                }

                string conflictError = "Your cart has auto-ship (recurring) items with conflicting shipment schedules. Only one auto-ship schedule is allowed per order.";
                if (productVariant.IsRecurring)
                {
                    //cycle length
                    if (_cycleLength.HasValue && _cycleLength.Value != productVariant.RecurringCycleLength)
                    {
                        error = conflictError;
                        return error;
                    }
                    else
                    {
                        _cycleLength = productVariant.RecurringCycleLength;
                    }

                    //cycle period
                    if (_cyclePeriod.HasValue && _cyclePeriod.Value != productVariant.RecurringCyclePeriod)
                    {
                        error = conflictError;
                        return error;
                    }
                    else
                    {
                        _cyclePeriod = productVariant.RecurringCyclePeriod;
                    }

                    //total cycles
                    if (_totalCycles.HasValue && _totalCycles.Value != productVariant.RecurringTotalCycles)
                    {
                        error = conflictError;
                        return error;
                    }
                    else
                    {
                        _totalCycles = productVariant.RecurringTotalCycles;
                    }
                }
            }

            if (!_cycleLength.HasValue || !_cyclePeriod.HasValue || !_totalCycles.HasValue)
            {
                error = "No recurring products";
            }
            else
            {
                cycleLength = _cycleLength.Value;
                cyclePeriod = _cyclePeriod.Value;
                totalCycles = _totalCycles.Value;
            }

            return error;
        }

        /// <summary>
        /// Get customer of shopping cart
        /// </summary>
        /// <param name="shoppingCart">Shopping cart</param>
        /// <returns>Customer of shopping cart</returns>
        public static Customer GetCustomer(this IList<ShoppingCartItem> shoppingCart)
        {
            if (shoppingCart.Count == 0)
                return null;

            return shoppingCart[0].Customer;
        }

    }
}
