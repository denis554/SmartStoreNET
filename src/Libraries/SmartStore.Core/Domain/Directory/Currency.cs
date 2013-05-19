using System;
using SmartStore.Core.Domain.Localization;

namespace SmartStore.Core.Domain.Directory
{
    /// <summary>
    /// Represents a currency
    /// </summary>
    public partial class Currency : BaseEntity, ILocalizedEntity
    {
        /// <summary>
        /// Gets or sets the name
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the currency code
        /// </summary>
        public string CurrencyCode { get; set; }

        /// <summary>
        /// Gets or sets the rate
        /// </summary>
        public decimal Rate { get; set; }

        /// <summary>
        /// Gets or sets the display locale
        /// </summary>
        public string DisplayLocale { get; set; }

        /// <summary>
        /// Gets or sets the custom formatting
        /// </summary>
        public string CustomFormatting { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the entity is published
        /// </summary>
        public bool Published { get; set; }

        /// <summary>
        /// Gets or sets the display order
        /// </summary>
        public int DisplayOrder { get; set; }

        /// <summary>
        /// Gets or sets the date and time of instance creation
        /// </summary>
        public DateTime CreatedOnUtc { get; set; }

        /// <summary>
        /// Gets or sets the date and time of instance update
        /// </summary>
        public DateTime UpdatedOnUtc { get; set; }
    }

}
