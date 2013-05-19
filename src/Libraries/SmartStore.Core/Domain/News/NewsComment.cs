using SmartStore.Core.Domain.Customers;

namespace SmartStore.Core.Domain.News
{
    /// <summary>
    /// Represents a news comment
    /// </summary>
    public partial class NewsComment : CustomerContent
    {
        /// <summary>
        /// Gets or sets the comment title
        /// </summary>
        public string CommentTitle { get; set; }

        /// <summary>
        /// Gets or sets the comment text
        /// </summary>
        public string CommentText { get; set; }

        /// <summary>
        /// Gets or sets the news item identifier
        /// </summary>
        public int NewsItemId { get; set; }

        /// <summary>
        /// Gets or sets the news item
        /// </summary>
        public virtual NewsItem NewsItem { get; set; }
    }
}