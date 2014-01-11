using System.Collections.Generic;
using SmartStore.Core.Domain.Localization;

namespace SmartStore.Core.Domain.Catalog
{
    /// <summary>
    /// Represents a product variant attribute mapping
    /// </summary>
    public partial class ProductVariantAttribute : BaseEntity, ILocalizedEntity
    {
        private ICollection<ProductVariantAttributeValue> _productVariantAttributeValues;

        /// <summary>
        /// Gets or sets the product identifier
        /// </summary>
		public int ProductId { get; set; }

        /// <summary>
        /// Gets or sets the product attribute identifier
        /// </summary>
        public int ProductAttributeId { get; set; }

        /// <summary>
        /// Gets or sets a value a text prompt
        /// </summary>
        public string TextPrompt { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the entity is required
        /// </summary>
        public bool IsRequired { get; set; }

        /// <summary>
        /// Gets or sets the attribute control type identifier
        /// </summary>
        public int AttributeControlTypeId { get; set; }

        /// <summary>
        /// Gets or sets the display order
        /// </summary>
        public int DisplayOrder { get; set; }

        /// <summary>
        /// Gets the attribute control type
        /// </summary>
        public AttributeControlType AttributeControlType
        {
            get
            {
                return (AttributeControlType)this.AttributeControlTypeId;
            }
            set
            {
                this.AttributeControlTypeId = (int)value; 
            }
        }

        /// <summary>
        /// Gets the product attribute
        /// </summary>
        public virtual ProductAttribute ProductAttribute { get; set; }

        /// <summary>
        /// Gets the product
        /// </summary>
		public virtual Product Product { get; set; }
        
        /// <summary>
        /// Gets the product variant attribute values
        /// </summary>
        public virtual ICollection<ProductVariantAttributeValue> ProductVariantAttributeValues
        {
            get { return _productVariantAttributeValues ?? (_productVariantAttributeValues = new List<ProductVariantAttributeValue>()); }
            protected set { _productVariantAttributeValues = value; }
        }

    }

}
