using System.Collections.Generic;
using SmartStore.Core.Domain.Catalog;
using SmartStore.Collections;

namespace SmartStore.Services.Catalog
{
    /// <summary>
    /// Product attribute parser interface
    /// </summary>
    public partial interface IProductAttributeParser
    {
        #region Product attributes

        /// <summary>
        /// Gets selected product variant attributes as a map of integer ids with their corresponding values.
        /// </summary>
        /// <param name="attributes">Attributes XML</param>
        /// <returns>The deserialized map</returns>
        Multimap<int, string> DeserializeProductVariantAttributes(string attributes);

        /// <summary>
        /// Gets selected product variant attributes
        /// </summary>
        /// <param name="attributes">Attributes</param>
        /// <returns>Selected product variant attributes</returns>
        IList<ProductVariantAttribute> ParseProductVariantAttributes(string attributes);

        /// <summary>
        /// Gets selected product variant attributes
        /// </summary>
        /// <param name="ids">The attribute ids</param>
        /// <returns>Selected product variant attributes</returns>
        IEnumerable<ProductVariantAttribute> ParseProductVariantAttributes(ICollection<int> ids);

        /// <summary>
        /// Get product variant attribute values
        /// </summary>
        /// <param name="attributes">Attributes</param>
        /// <returns>Product variant attribute values</returns>
        IEnumerable<ProductVariantAttributeValue> ParseProductVariantAttributeValues(string attributes);

        /// <summary>
        /// Gets selected product variant attribute value
        /// </summary>
        /// <param name="attributes">Attributes</param>
        /// <param name="productVariantAttributeId">Product variant attribute identifier</param>
        /// <returns>Product variant attribute value</returns>
        IList<string> ParseValues(string attributes, int productVariantAttributeId);

        /// <summary>
        /// Adds an attribute
        /// </summary>
        /// <param name="attributes">Attributes</param>
        /// <param name="pva">Product variant attribute</param>
        /// <param name="value">Value</param>
        /// <returns>Attributes</returns>
        string AddProductAttribute(string attributes, ProductVariantAttribute pva, string value);

        /// <summary>
        /// Are attributes equal
        /// </summary>
        /// <param name="attributes1">The attributes of the first product variant</param>
        /// <param name="attributes2">The attributes of the second product variant</param>
        /// <returns>Result</returns>
        bool AreProductAttributesEqual(string attributes1, string attributes2);

        /// <summary>
        /// Finds a product variant attribute combination by attributes stored in XML 
        /// </summary>
		/// <param name="product">Product</param>
        /// <param name="attributesXml">Attributes in XML format</param>
        /// <returns>Found product variant attribute combination</returns>
		ProductVariantAttributeCombination FindProductVariantAttributeCombination(Product product, string attributesXml);
		ProductVariantAttributeCombination FindProductVariantAttributeCombination(int productId, string attributesXml);

		List<List<int>> DeserializeQueryData(string jsonData);
		string SerializeQueryData(int productId, string attributesXml, bool urlEncode = true);

        #endregion

        #region Gift card attributes

        /// <summary>
        /// Add gift card attrbibutes
        /// </summary>
        /// <param name="attributes">Attributes</param>
        /// <param name="recipientName">Recipient name</param>
        /// <param name="recipientEmail">Recipient email</param>
        /// <param name="senderName">Sender name</param>
        /// <param name="senderEmail">Sender email</param>
        /// <param name="giftCardMessage">Message</param>
        /// <returns>Attributes</returns>
        string AddGiftCardAttribute(string attributes, string recipientName,
            string recipientEmail, string senderName, string senderEmail, string giftCardMessage);

        /// <summary>
        /// Get gift card attrbibutes
        /// </summary>
        /// <param name="attributes">Attributes</param>
        /// <param name="recipientName">Recipient name</param>
        /// <param name="recipientEmail">Recipient email</param>
        /// <param name="senderName">Sender name</param>
        /// <param name="senderEmail">Sender email</param>
        /// <param name="giftCardMessage">Message</param>
        void GetGiftCardAttribute(string attributes, out string recipientName,
            out string recipientEmail, out string senderName,
            out string senderEmail, out string giftCardMessage);

        #endregion
    }
}
