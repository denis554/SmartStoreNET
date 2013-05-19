using System;
using System.Collections.Generic;
using System.Linq;
using SmartStore.Core.Data;
using SmartStore.Core.Domain.Catalog;
using SmartStore.Services.Events;

namespace SmartStore.Services.Catalog
{
    /// <summary>
    /// Product tag service
    /// </summary>
    public partial class ProductTagService : IProductTagService
    {
        #region Fields

        private readonly IRepository<ProductTag> _productTagRepository;
        private readonly IEventPublisher _eventPublisher;

        #endregion

        #region Ctor

        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="productTagRepository">Product tag repository</param>
        /// <param name="eventPublisher">Event published</param>
        public ProductTagService(IRepository<ProductTag> productTagRepository,
            IEventPublisher eventPublisher)
        {
            _productTagRepository = productTagRepository;
            _eventPublisher = eventPublisher;
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

            //event notification
            _eventPublisher.EntityDeleted(productTag);
        }

        /// <summary>
        /// Gets all product tags
        /// </summary>
        /// <param name="showHidden">A value indicating whether to show hidden records</param>
        /// <returns>Product tags</returns>
        public virtual IList<ProductTag> GetAllProductTags(bool showHidden = false)
        {
            var query = _productTagRepository.Table;
            if (!showHidden)
                query = query.Where(pt => pt.ProductCount > 0);
                query = query.OrderByDescending(pt => pt.ProductCount);
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

            //event notification
            _eventPublisher.EntityUpdated(productTag);
        }

        /// <summary>
        /// Updates the product tag
        /// </summary>
        /// <param name="productTag">Product tag</param>
        public virtual void UpdateProductTagTotals(ProductTag productTag)
        {
            if (productTag == null)
                throw new ArgumentNullException("productTag");

            int newTotal = productTag.Products
                .Where(p => !p.Deleted && p.Published && p.ProductVariants.Where(pv => !pv.Deleted && pv.Published).Count() > 0)
                .Count();

            //we do not delete product tags with 0 product count
            productTag.ProductCount = newTotal;
            UpdateProductTag(productTag);
            
        }
        
        #endregion
    }
}
