using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using SmartStore.Core;
using SmartStore.Core.Domain.Catalog;
using SmartStore.Core.Domain.Orders;

namespace SmartStore.Services.Catalog
{
    /// <summary>
    /// Product service
    /// </summary>
    public partial interface IProductService
    {
        #region Products

        /// <summary>
        /// Delete a product
        /// </summary>
        /// <param name="product">Product</param>
        void DeleteProduct(Product product);

		IQueryable<Product> GetAllProducts(ProductAllContext context);

        /// <summary>
        /// Gets all products displayed on the home page
        /// </summary>
        /// <returns>Product collection</returns>
        IList<Product> GetAllProductsDisplayedOnHomePage();
        
        /// <summary>
        /// Gets product
        /// </summary>
        /// <param name="productId">Product identifier</param>
        /// <returns>Product</returns>
        Product GetProductById(int productId);

        /// <summary>
        /// Gets products by identifier
        /// </summary>
        /// <param name="productIds">Product identifiers</param>
        /// <returns>Products</returns>
        IList<Product> GetProductsByIds(int[] productIds);

        /// <summary>
        /// Inserts a product
        /// </summary>
        /// <param name="product">Product</param>
        void InsertProduct(Product product);

        /// <summary>
        /// Updates the product
        /// </summary>
        /// <param name="product">Product</param>
        void UpdateProduct(Product product);

        /// <summary>
        /// Gets the total count of products matching the criteria
        /// </summary>
        int CountProducts(ProductSearchContext productSearchContext);

        /// <summary>
        /// Search products
        /// </summary>
        IPagedList<Product> SearchProducts(ProductSearchContext productSearchContext);

        /// <summary>
        /// Update product review totals
        /// </summary>
        /// <param name="product">Product</param>
        void UpdateProductReviewTotals(Product product);
        
        /// <summary>
        /// Get low stock products
        /// </summary>
        /// <returns>Result</returns>
        IList<Product> GetLowStockProducts();

        /// <summary>
        /// Gets a product by SKU
        /// </summary>
        /// <param name="sku">SKU</param>
        /// <returns>Product</returns>
        Product GetProductBySku(string sku);

        /// <summary>
        /// Gets a product by GTIN
        /// </summary>
        /// <param name="sku">SKU</param>
        /// <returns>Product</returns>
        Product GetProductByGtin(string sku);
        
		/// <summary>
		/// Adjusts inventory
		/// </summary>
		/// <param name="cartItem">Shopping cart item</param>
		/// <param name="decrease">A value indicating whether to increase or descrease product stock quantity</param>
		void AdjustInventory(ShoppingCartItem cartItem, bool decrease);

		/// <summary>
		/// Adjusts inventory
		/// </summary>
		/// <param name="orderItem">Order item</param>
		/// <param name="decrease">A value indicating whether to increase or descrease product stock quantity</param>
		void AdjustInventory(OrderItem orderItem, bool decrease);

        /// <summary>
        /// Adjusts inventory
        /// </summary>
		/// <param name="product">Product</param>
		/// <param name="decrease">A value indicating whether to increase or descrease product stock quantity</param>
        /// <param name="quantity">Quantity</param>
        /// <param name="attributesXml">Attributes in XML format</param>
        void AdjustInventory(Product product, bool decrease, int quantity, string attributesXml);

        /// <summary>
        /// Update HasTierPrices property (used for performance optimization)
        /// </summary>
		/// <param name="product">Product</param>
        void UpdateHasTierPricesProperty(Product product);

        /// <summary>
        /// Update HasDiscountsApplied property (used for performance optimization)
        /// </summary>
		/// <param name="product">Product</param>
        void UpdateHasDiscountsApplied(Product product);

        #endregion

        #region Related products

        /// <summary>
        /// Deletes a related product
        /// </summary>
        /// <param name="relatedProduct">Related product</param>
        void DeleteRelatedProduct(RelatedProduct relatedProduct);

        /// <summary>
        /// Gets a related product collection by product identifier
        /// </summary>
        /// <param name="productId1">The first product identifier</param>
        /// <param name="showHidden">A value indicating whether to show hidden records</param>
        /// <returns>Related product collection</returns>
        IList<RelatedProduct> GetRelatedProductsByProductId1(int productId1, bool showHidden = false);

        /// <summary>
        /// Gets a related product
        /// </summary>
        /// <param name="relatedProductId">Related product identifier</param>
        /// <returns>Related product</returns>
        RelatedProduct GetRelatedProductById(int relatedProductId);

        /// <summary>
        /// Inserts a related product
        /// </summary>
        /// <param name="relatedProduct">Related product</param>
        void InsertRelatedProduct(RelatedProduct relatedProduct);

        /// <summary>
        /// Updates a related product
        /// </summary>
        /// <param name="relatedProduct">Related product</param>
        void UpdateRelatedProduct(RelatedProduct relatedProduct);

        #endregion

        #region Cross-sell products

        /// <summary>
        /// Deletes a cross-sell product
        /// </summary>
        /// <param name="crossSellProduct">Cross-sell</param>
        void DeleteCrossSellProduct(CrossSellProduct crossSellProduct);

        /// <summary>
        /// Gets a cross-sell product collection by product identifier
        /// </summary>
        /// <param name="productId1">The first product identifier</param>
        /// <param name="showHidden">A value indicating whether to show hidden records</param>
        /// <returns>Cross-sell product collection</returns>
        IList<CrossSellProduct> GetCrossSellProductsByProductId1(int productId1, bool showHidden = false);

        /// <summary>
        /// Gets a cross-sell product
        /// </summary>
        /// <param name="crossSellProductId">Cross-sell product identifier</param>
        /// <returns>Cross-sell product</returns>
        CrossSellProduct GetCrossSellProductById(int crossSellProductId);

        /// <summary>
        /// Inserts a cross-sell product
        /// </summary>
        /// <param name="crossSellProduct">Cross-sell product</param>
        void InsertCrossSellProduct(CrossSellProduct crossSellProduct);

        /// <summary>
        /// Updates a cross-sell product
        /// </summary>
        /// <param name="crossSellProduct">Cross-sell product</param>
        void UpdateCrossSellProduct(CrossSellProduct crossSellProduct);
        
        /// <summary>
        /// Gets a cross-sells
        /// </summary>
        /// <param name="cart">Shopping cart</param>
        /// <param name="numberOfProducts">Number of products to return</param>
        /// <returns>Cross-sells</returns>
        IList<Product> GetCrosssellProductsByShoppingCart(IList<ShoppingCartItem> cart, int numberOfProducts);

        #endregion
        
        #region Tier prices

        /// <summary>
        /// Deletes a tier price
        /// </summary>
        /// <param name="tierPrice">Tier price</param>
        void DeleteTierPrice(TierPrice tierPrice);

        /// <summary>
        /// Gets a tier price
        /// </summary>
        /// <param name="tierPriceId">Tier price identifier</param>
        /// <returns>Tier price</returns>
        TierPrice GetTierPriceById(int tierPriceId);

        /// <summary>
        /// Inserts a tier price
        /// </summary>
        /// <param name="tierPrice">Tier price</param>
        void InsertTierPrice(TierPrice tierPrice);

        /// <summary>
        /// Updates the tier price
        /// </summary>
        /// <param name="tierPrice">Tier price</param>
        void UpdateTierPrice(TierPrice tierPrice);

        #endregion

        #region Product pictures

        /// <summary>
        /// Deletes a product picture
        /// </summary>
        /// <param name="productPicture">Product picture</param>
        void DeleteProductPicture(ProductPicture productPicture);

        /// <summary>
        /// Gets a product pictures by product identifier
        /// </summary>
        /// <param name="productId">The product identifier</param>
        /// <returns>Product pictures</returns>
        IList<ProductPicture> GetProductPicturesByProductId(int productId);

        /// <summary>
        /// Gets a product picture
        /// </summary>
        /// <param name="productPictureId">Product picture identifier</param>
        /// <returns>Product picture</returns>
        ProductPicture GetProductPictureById(int productPictureId);

        /// <summary>
        /// Inserts a product picture
        /// </summary>
        /// <param name="productPicture">Product picture</param>
        void InsertProductPicture(ProductPicture productPicture);

        /// <summary>
        /// Updates a product picture
        /// </summary>
        /// <param name="productPicture">Product picture</param>
        void UpdateProductPicture(ProductPicture productPicture);

        #endregion

		#region Bundled products

		/// <summary>
		/// Inserts a product bundle item
		/// </summary>
		/// <param name="bundleItem">Product bundle item</param>
		void InsertBundleItem(ProductBundleItem bundleItem);

		/// <summary>
		/// Updates a product bundle item
		/// </summary>
		/// <param name="bundleItem">Product bundle item</param>
		void UpdateBundleItem(ProductBundleItem bundleItem);

		/// <summary>
		/// Deletes a product bundle item
		/// </summary>
		/// <param name="bundleItem">Product bundle item</param>
		void DeleteBundleItem(ProductBundleItem bundleItem);

		/// <summary>
		/// Get a product bundle item by item identifier
		/// </summary>
		/// <param name="bundleItemId">Product bundle item identifier</param>
		/// <returns>Product bundle item</returns>
		ProductBundleItem GetBundleItemById(int bundleItemId);

		/// <summary>
		/// Gets a list of bundle items for a particular product identifier
		/// </summary>
		/// <param name="bundleProductId">Product identifier</param>
		/// <param name="showHidden">A value indicating whether to show hidden records</param>
		/// <returns>List of bundle items</returns>
		IList<ProductBundleItem> GetBundleItems(int bundleProductId, bool showHidden = false);

		#endregion

    }
}
