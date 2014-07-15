using System;
using System.Linq;
using System.Linq.Expressions;
using SmartStore.Core;
using SmartStore.Core.Caching;
using SmartStore.Core.Data;
using SmartStore.Core.Domain.Seo;

namespace SmartStore.Services.Seo
{
    /// <summary>
    /// Provides information about URL records
    /// </summary>
    public partial class UrlRecordService : IUrlRecordService
    {
        #region Constants

        private const string URLRECORD_ACTIVE_BY_ID_NAME_LANGUAGE_KEY = "SmartStore.urlrecord.active.id-name-language-{0}-{1}-{2}";
        private const string URLRECORD_PATTERN_KEY = "SmartStore.urlrecord.";

        #endregion

        #region Fields

        private readonly IRepository<UrlRecord> _urlRecordRepository;
        private readonly ICacheManager _cacheManager;

        #endregion

        #region Ctor

        public UrlRecordService(ICacheManager cacheManager, IRepository<UrlRecord> urlRecordRepository)
        {
            this._cacheManager = cacheManager;
            this._urlRecordRepository = urlRecordRepository;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Deletes an URL record
        /// </summary>
        /// <param name="urlRecord">URL record</param>
        public virtual void DeleteUrlRecord(UrlRecord urlRecord)
        {
            if (urlRecord == null)
                throw new ArgumentNullException("urlRecord");

            _urlRecordRepository.Delete(urlRecord);

            //cache
            _cacheManager.RemoveByPattern(URLRECORD_PATTERN_KEY);
        }

        /// <summary>
        /// Gets an URL record
        /// </summary>
        /// <param name="urlRecordId">URL record identifier</param>
        /// <returns>URL record</returns>
        public virtual UrlRecord GetUrlRecordById(int urlRecordId)
        {
            if (urlRecordId == 0)
                return null;

            var urlRecord = _urlRecordRepository.GetById(urlRecordId);
            return urlRecord;
        }

        /// <summary>
        /// Inserts an URL record
        /// </summary>
        /// <param name="urlRecord">URL record</param>
        public virtual void InsertUrlRecord(UrlRecord urlRecord)
        {
            if (urlRecord == null)
                throw new ArgumentNullException("urlRecord");

            _urlRecordRepository.Insert(urlRecord);

            //cache
            _cacheManager.RemoveByPattern(URLRECORD_PATTERN_KEY);
        }

        /// <summary>
        /// Updates the URL record
        /// </summary>
        /// <param name="urlRecord">URL record</param>
        public virtual void UpdateUrlRecord(UrlRecord urlRecord)
        {
            if (urlRecord == null)
                throw new ArgumentNullException("urlRecord");

            _urlRecordRepository.Update(urlRecord);

            //cache
            _cacheManager.RemoveByPattern(URLRECORD_PATTERN_KEY);
        }

        /// <summary>
        /// Find URL record
        /// </summary>
        /// <param name="slug">Slug</param>
        /// <returns>Found URL record</returns>
        public virtual UrlRecord GetBySlug(string slug)
        {
            if (String.IsNullOrEmpty(slug))
                return null;

            var query = from ur in _urlRecordRepository.Table
                        where ur.Slug == slug
                        select ur;
            var urlRecord = query.FirstOrDefault();
            return urlRecord;
        }

        /// <summary>
        /// Gets all URL records
        /// </summary>
        /// <param name="slug">Slug</param>
        /// <param name="pageIndex">Page index</param>
        /// <param name="pageSize">Page size</param>
        /// <returns>Customer collection</returns>
        public virtual IPagedList<UrlRecord> GetAllUrlRecords(string slug, int pageIndex, int pageSize)
        {
            var query = _urlRecordRepository.Table;
            if (!String.IsNullOrWhiteSpace(slug))
                query = query.Where(ur => ur.Slug.Contains(slug));
                query = query.OrderBy(ur => ur.Slug);

                var urlRecords = new PagedList<UrlRecord>(query, pageIndex, pageSize);
                return urlRecords;
        }

        /// <summary>
        /// Find slug
        /// </summary>
        /// <param name="entityId">Entity identifier</param>
        /// <param name="entityName">Entity name</param>
        /// <param name="languageId">Language identifier</param>
        /// <returns>Found slug</returns>
        public virtual string GetActiveSlug(int entityId, string entityName, int languageId)
        {   
            string key = string.Format(URLRECORD_ACTIVE_BY_ID_NAME_LANGUAGE_KEY, entityId, entityName, languageId);
            return _cacheManager.Get(key, () =>
            {
                var query = from ur in _urlRecordRepository.Table
                            where ur.EntityId == entityId &&
                            ur.EntityName == entityName &&
                            ur.LanguageId == languageId &&
                            ur.IsActive
                            orderby ur.Id descending 
                            select ur.Slug;
                var slug = query.FirstOrDefault();
                return slug ?? "";
            });
        }

        /// <summary>
        /// Save slug
        /// </summary>
        /// <typeparam name="T">Type</typeparam>
        /// <param name="entity">Entity</param>
        /// <param name="slug">Slug</param>
        /// <param name="languageId">Language ID</param>
        public virtual UrlRecord SaveSlug<T>(T entity, string slug, int languageId) where T : BaseEntity, ISlugSupported
        {
            if (entity == null)
                throw new ArgumentNullException("entity");

            int entityId = entity.Id;
            string entityName = typeof(T).Name;
			UrlRecord result = null;

            var query = from ur in _urlRecordRepository.Table
                        where ur.EntityId == entityId &&
                        ur.EntityName == entityName &&
                        ur.LanguageId == languageId
                        orderby ur.Id descending 
                        select ur;
            var allUrlRecords = query.ToList();

            var activeUrlRecord = allUrlRecords.FirstOrDefault(x => x.IsActive);
            if (activeUrlRecord == null && !string.IsNullOrWhiteSpace(slug))
            {
                //find in non-active records with the specified slug
                var nonActiveRecordWithSpecifiedSlug = allUrlRecords.FirstOrDefault(x => x.Slug.Equals(slug, StringComparison.InvariantCultureIgnoreCase) && !x.IsActive);
                if (nonActiveRecordWithSpecifiedSlug != null)
                {
                    //mark non-active record as active
                    nonActiveRecordWithSpecifiedSlug.IsActive = true;
                    UpdateUrlRecord(nonActiveRecordWithSpecifiedSlug);
                }
                else
                {
                    //new record
                    var urlRecord = new UrlRecord()
                    {
                        EntityId = entity.Id,
                        EntityName = entityName,
                        Slug = slug,
                        LanguageId = languageId,
                        IsActive = true,
                    };
                    InsertUrlRecord(urlRecord);
					result = urlRecord;
                }
            }

            if (activeUrlRecord != null && string.IsNullOrWhiteSpace(slug))
            {
                //disable the previous active URL record
                activeUrlRecord.IsActive = false;
                UpdateUrlRecord(activeUrlRecord);
            }

            if (activeUrlRecord != null && !string.IsNullOrWhiteSpace(slug))
            {
                //is it the same slug as in active URL record?
                if (activeUrlRecord.Slug.Equals(slug, StringComparison.InvariantCultureIgnoreCase))
                {
                    //yes. do nothing
                    //P.S. wrote this way for more source code readability
                }
                else
                {
                    //find in non-active records with the specified slug
                    var nonActiveRecordWithSpecifiedSlug = allUrlRecords
                        .FirstOrDefault(x => x.Slug.Equals(slug, StringComparison.InvariantCultureIgnoreCase) && !x.IsActive);
                    if (nonActiveRecordWithSpecifiedSlug != null)
                    {
                        //mark non-active record as active
                        nonActiveRecordWithSpecifiedSlug.IsActive = true;
                        UpdateUrlRecord(nonActiveRecordWithSpecifiedSlug);

                        //disable the previous active URL record
                        activeUrlRecord.IsActive = false;
                        UpdateUrlRecord(activeUrlRecord);
                    }
                    else
                    {
						// MC: Absolutely ensure, that we have no duplicate active record for this entity.
						// In such case a record other than "activeUrlRecord" could have same seName
						// and the DB would report an Index error.
						var alreadyActiveDuplicate = allUrlRecords.FirstOrDefault(x => x.Slug.IsCaseInsensitiveEqual(slug) && x.IsActive);
						if (alreadyActiveDuplicate != null)
						{
							// deactivate all
							allUrlRecords.Each(x => x.IsActive = false);
							// set the existing one to active again
							alreadyActiveDuplicate.IsActive = true;
							// update all records
							allUrlRecords.Each(x => UpdateUrlRecord(x));
						}
						else
						{
							//insert new record
							//we do not update the existing record because we should track all previously entered slugs
							//to ensure that URLs will work fine
							var urlRecord = new UrlRecord()
							{
								EntityId = entity.Id,
								EntityName = entityName,
								Slug = slug,
								LanguageId = languageId,
								IsActive = true,
							};
							InsertUrlRecord(urlRecord);
							result = urlRecord;

							//disable the previous active URL record
							activeUrlRecord.IsActive = false;
							UpdateUrlRecord(activeUrlRecord);
						}
                    }

                }
            }

			return result;
        }

		/// <summary>
		/// Save slug
		/// </summary>
		/// <typeparam name="T">Type</typeparam>
		/// <param name="entity">Entity</param>
		/// <param name="nameProperty">Name of a property</param>
		/// <returns>Url record</returns>
		public virtual UrlRecord SaveSlug<T>(T entity, Expression<Func<T, string>> nameProperty) where T : BaseEntity, ISlugSupported
		{
			string name = nameProperty.Compile().Invoke(entity);

			string existingSeName = entity.GetSeName<T>(0, true, false);
			existingSeName = entity.ValidateSeName(existingSeName, name, true);

			return SaveSlug(entity, existingSeName, 0);
		}

        #endregion
    }
}