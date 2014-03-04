using System.Runtime.Serialization;
using SmartStore.Core.Domain.Localization;

namespace SmartStore.Core.Domain.Catalog
{
    /// <summary>
    /// Represents a product variant attribute value
    /// </summary>
    [DataContract]
	public partial class ProductVariantAttributeValue : BaseEntity, ILocalizedEntity
    {
        /// <summary>
        /// Gets or sets the product variant attribute mapping identifier
        /// </summary>
		[DataMember]
		public int ProductVariantAttributeId { get; set; }

        /// <summary>
        /// Gets or sets the product variant attribute alias 
        /// (an optional key for advanced customization)
        /// </summary>
		[DataMember]
		public string Alias { get; set; }

        /// <summary>
        /// Gets or sets the product variant attribute name
        /// </summary>
		[DataMember]
		public string Name { get; set; }

        /// <summary>
        /// Gets or sets the color RGB value (used with "Color squares" attribute type)
        /// </summary>
		[DataMember]
		public virtual string ColorSquaresRgb { get; set; }

        /// <summary>
        /// Gets or sets the price adjustment
        /// </summary>
		[DataMember]
		public decimal PriceAdjustment { get; set; }

        /// <summary>
        /// Gets or sets the weight adjustment
        /// </summary>
		[DataMember]
		public decimal WeightAdjustment { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the value is pre-selected
        /// </summary>
		[DataMember]
		public bool IsPreSelected { get; set; }

        /// <summary>
        /// Gets or sets the display order
        /// </summary>
		[DataMember]
		public int DisplayOrder { get; set; }

		/// <summary>
		/// Gets or sets the type Id
		/// </summary>
		[DataMember]
		public int TypeId { get; set; }

		/// <summary>
		/// Gets or sets the linked product Id
		/// </summary>
		[DataMember]
		public int LinkedProductId { get; set; }
        
        /// <summary>
        /// Gets the product variant attribute
        /// </summary>
		[DataMember]
		public virtual ProductVariantAttribute ProductVariantAttribute { get; set; }

		/// <summary>
		/// Gets or sets the product attribute value type
		/// </summary>
		public ProductVariantAttributeValueType ValueType
		{
			get
			{
				return (ProductVariantAttributeValueType)this.TypeId;
			}
			set
			{
				this.TypeId = (int)value;
			}
		}

		public string ValueTypeLabelHint
		{
			get
			{
				switch (ValueType)
				{
					case ProductVariantAttributeValueType.Simple:
						return "smnet-hide";
					case ProductVariantAttributeValueType.ProductLinkage:
						return "warning";
					default:
						return "";
				}
			}
		}
    }
}
