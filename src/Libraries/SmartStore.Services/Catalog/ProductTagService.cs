using System;
using System.Collections.Generic;
using System.Linq;
using SmartStore.Core.Caching;
using SmartStore.Core.Data;
using SmartStore.Core.Domain.Catalog;
using SmartStore.Core.Domain.Stores;
using SmartStore.Services.Events;

namespace SmartStore.Services.Catalog
{
    /// <summary>
    /// Product tag service
    /// </summary>
    public partial class ProductTagService : IProductTagService
    {
		#region Constants

		/// <summary>
		/// Key for caching
		/// </summary>
		/// <remarks>
		/// {0} : store ID
		/// </remarks>
		private const string PRODUCTTAG_COUNT_KEY = "sm.producttag.count-{0}";

		/// <summary>
		/// Key pattern to clear cache
		/// </summary>
		private const string PRODUCTTAG_PATTERN_KEY = "sm.producttag.";

		#endregion

        #region Fields

        private readonly IRepository<ProductTag> _productTagRepository;
		private readonly IRepository<StoreMapping> _storeMappingRepository;
		private readonly ICacheManager _cacheManager;
        private readonly IEventPublisher _eventPublisher;

        #endregion

        #region Ctor

        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="productTagRepository">Product tag repository</param>
		/// <param name="storeMappingRepository">Store mapping repository</param>
		/// <param name="cacheManager">Cache manager</param>
        /// <param name="eventPublisher">Event published</param>
        public ProductTagService(IRepository<ProductTag> productTagRepository,
			IRepository<StoreMapping> storeMappingRepository,
			ICacheManager cacheManager,
            IEventPublisher eventPublisher)
        {
            _productTagRepository = productTagRepository;
			_storeMappingRepository = storeMappingRepository;
			_cacheManager = cacheManager;
            _eventPublisher = eventPublisher;
        }

        #endregion

		#region Utilities

		/// <summary>
		/// Get product count for each of existing product tag
		/// </summary>
		/// <param name="storeId">Store identifier</param>
		/// <returns>Dictionary of "product tag ID : product count"</returns>
		private Dictionary<int, int> GetProductCount(int storeId)
		{
			string key = string.Format(PRODUCTTAG_COUNT_KEY, storeId);
			return _cacheManager.Get(key, () =>
			{

				var query = from pt in _productTagRepository.Table
							select new
							{
								Id = pt.Id,
								ProductCount = pt.Products
									//published and not deleted product/variants
									.Count(p => !p.Deleted &&
										p.Published &&
										p.ProductVariants.Any(pv => !pv.Deleted && pv.Published))
								//UNDOEN filter by store identifier if specified ( > 0 )
							};

				var dictionary = new Dictionary<int, int>();
				foreach (var item in query)
					dictionary.Add(item.Id, item.ProductCount);

				return dictionary;
			});
		}

		#endregion

        #region Methods

        /// <summary>
        /// Delete a product tag
        /// </summary>
        /// <param name="productTag">Product tag</param>
        public virtual void DeleteProductTag(ProductTag productTag)
        {
            if (productTag == null)
                throw new ArgumentNullException("productTag");

            _productTagRepository.Delete(productTag);

			//cache
			_cacheManager.RemoveByPattern(PRODUCTTAG_PATTERN_KEY);

            //event notification
            _eventPublisher.EntityDeleted(productTag);
        }

        /// <summary>
        /// Gets all product tags
        /// </summary>
        /// <returns>Product tags</returns>
		public virtual IList<ProductTag> GetAllProductTags()
        {
            var query = _productTagRepository.Table;
            var productTags = query.ToList();
            return productTags;
        }

        /// <summary>
        /// Gets all product tag names
        /// </summary>
        /// <returns>Product tag names as list</returns>
        public virtual IList<string> GetAllProductTagNames()
        {
            var query = from pt in _productTagRepository.Table
                        orderby pt.Name ascending
                        select pt.Name;
            return query.ToList();
        }

        /// <summary>
        /// Gets product tag
        /// </summary>
        /// <param name="productTagId">Product tag identifier</param>
        /// <returns>Product tag</returns>
        public virtual ProductTag GetProductTagById(int productTagId)
        {
            if (productTagId == 0)
                return null;

            var productTag = _productTagRepository.GetById(productTagId);
            return productTag;
        }

        /// <summary>
        /// Gets product tag by name
        /// </summary>
        /// <param name="name">Product tag name</param>
        /// <returns>Product tag</returns>
        public virtual ProductTag GetProductTagByName(string name)
        {
            var query = from pt in _productTagRepository.Table
                        where pt.Name == name
                        select pt;

            var productTag = query.FirstOrDefault();
            return productTag;
        }

        /// <summary>
        /// Inserts a product tag
        /// </summary>
        /// <param name="productTag">Product tag</param>
        public virtual void InsertProductTag(ProductTag productTag)
        {
            if (productTag == null)
                throw new ArgumentNullException("productTag");

            _productTagRepository.Insert(productTag);

			//cache
			_cacheManager.RemoveByPattern(PRODUCTTAG_PATTERN_KEY);

            //event notification
            _eventPublisher.EntityInserted(productTag);
        }

        /// <summary>
        /// Updates the product tag
        /// </summary>
        /// <param name="productTag">Product tag</param>
        public virtual void UpdateProductTag(ProductTag productTag)
        {
            if (productTag == null)
                throw new ArgumentNullException("productTag");

            _productTagRepository.Update(productTag);

			//cache
			_cacheManager.RemoveByPattern(PRODUCTTAG_PATTERN_KEY);

            //event notification
            _eventPublisher.EntityUpdated(productTag);
        }

		/// <summary>
		/// Get number of products
		/// </summary>
		/// <param name="productTagId">Product tag identifier</param>
		/// <param name="storeId">Store identifier</param>
		/// <returns>Number of products</returns>
		public virtual int GetProductCount(int productTagId, int storeId)
		{
			var dictionary = GetProductCount(storeId);
			if (dictionary.ContainsKey(productTagId))
				return dictionary[productTagId];
			else
				return 0;
		}
        
        #endregion
    }
}
