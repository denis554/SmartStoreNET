using System;
using System.Linq;
using SmartStore.Core;
using SmartStore.Core.Caching;
using SmartStore.Core.Data;
using SmartStore.Core.Domain.News;
using SmartStore.Services.Events;

namespace SmartStore.Services.News
{
    /// <summary>
    /// News service
    /// </summary>
    public partial class NewsService : INewsService
    {
        #region Constants
        private const string NEWS_BY_ID_KEY = "SmartStore.news.id-{0}";
        private const string NEWS_PATTERN_KEY = "SmartStore.news.";
        #endregion

        #region Fields

        private readonly IRepository<NewsItem> _newsItemRepository;
        private readonly ICacheManager _cacheManager;
        private readonly IEventPublisher _eventPublisher;

        #endregion

        #region Ctor

        public NewsService(IRepository<NewsItem> newsItemRepository, ICacheManager cacheManager, IEventPublisher eventPublisher)
        {
            _newsItemRepository = newsItemRepository;
            _cacheManager = cacheManager;
            _eventPublisher = eventPublisher;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Deletes a news
        /// </summary>
        /// <param name="newsItem">News item</param>
        public virtual void DeleteNews(NewsItem newsItem)
        {
            if (newsItem == null)
                throw new ArgumentNullException("newsItem");

            _newsItemRepository.Delete(newsItem);

            _cacheManager.RemoveByPattern(NEWS_PATTERN_KEY);

            //event notification
            _eventPublisher.EntityDeleted(newsItem);
        }

        /// <summary>
        /// Gets a news
        /// </summary>
        /// <param name="newsId">The news identifier</param>
        /// <returns>News</returns>
        public virtual NewsItem GetNewsById(int newsId)
        {
            if (newsId == 0)
                return null;

            string key = string.Format(NEWS_BY_ID_KEY, newsId);
            return _cacheManager.Get(key, () =>
            {
                var n = _newsItemRepository.GetById(newsId);
                return n;
            });
        }

        /// <summary>
        /// Gets all news
        /// </summary>
        /// <param name="languageId">Language identifier; 0 if you want to get all records</param>
		/// <param name="storeId">Store identifier; 0 if you want to get all records</param>
        /// <param name="pageIndex">Page index</param>
        /// <param name="pageSize">Page size</param>
        /// <param name="showHidden">A value indicating whether to show hidden records</param>
        /// <returns>News items</returns>
		public virtual IPagedList<NewsItem> GetAllNews(int languageId, int storeId,
            int pageIndex, int pageSize, bool showHidden = false)
        {
            var query = _newsItemRepository.Table;
            if (languageId > 0)
                query = query.Where(n => languageId == n.LanguageId);
			if (storeId > 0)
				query = query.Where(n => storeId == n.StoreId);
            if (!showHidden)
            {
                var utcNow = DateTime.UtcNow;
                query = query.Where(n => n.Published);
                query = query.Where(n => !n.StartDateUtc.HasValue || n.StartDateUtc <= utcNow);
                query = query.Where(n => !n.EndDateUtc.HasValue || n.EndDateUtc >= utcNow);
            }
            query = query.OrderByDescending(b => b.CreatedOnUtc);

            var news = new PagedList<NewsItem>(query, pageIndex, pageSize);
            return news;
        }

        /// <summary>
        /// Inserts a news item
        /// </summary>
        /// <param name="news">News item</param>
        public virtual void InsertNews(NewsItem news)
        {
            if (news == null)
                throw new ArgumentNullException("news");

            _newsItemRepository.Insert(news);

            _cacheManager.RemoveByPattern(NEWS_PATTERN_KEY);

            //event notification
            _eventPublisher.EntityInserted(news);
        }

        /// <summary>
        /// Updates the news item
        /// </summary>
        /// <param name="news">News item</param>
        public virtual void UpdateNews(NewsItem news)
        {
            if (news == null)
                throw new ArgumentNullException("news");

            _newsItemRepository.Update(news);

            _cacheManager.RemoveByPattern(NEWS_PATTERN_KEY);

            //event notification
            _eventPublisher.EntityUpdated(news);
        }
        
        /// <summary>
        /// Update news item comment totals
        /// </summary>
        /// <param name="newsItem">News item</param>
        public virtual void UpdateCommentTotals(NewsItem newsItem)
        {
            if (newsItem == null)
                throw new ArgumentNullException("newsItem");

            int approvedCommentCount = 0;
            int notApprovedCommentCount = 0;
            var newsComments = newsItem.NewsComments;
            foreach (var nc in newsComments)
            {
                if (nc.IsApproved)
                    approvedCommentCount++;
                else
                    notApprovedCommentCount++;
            }

            newsItem.ApprovedCommentCount = approvedCommentCount;
            newsItem.NotApprovedCommentCount = notApprovedCommentCount;
            UpdateNews(newsItem);
        }

        #endregion
    }
}
