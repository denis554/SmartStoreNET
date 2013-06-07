﻿using System;
using System.Collections.Generic;
using System.Linq;
using SmartStore.Core.Caching;
using SmartStore.Core.Data;
using SmartStore.Core.Domain.Messages;
using SmartStore.Services.Events;
using SmartStore.Services.Localization;

namespace SmartStore.Services.Messages
{
    public partial class MessageTemplateService: IMessageTemplateService
    {
        #region Constants

        private const string MESSAGETEMPLATES_ALL_KEY = "SmartStore.messagetemplate.all-{0}";
        private const string MESSAGETEMPLATES_BY_ID_KEY = "SmartStore.messagetemplate.id-{0}";
        private const string MESSAGETEMPLATES_BY_NAME_KEY = "SmartStore.messagetemplate.name-{0}-{1}";
        private const string MESSAGETEMPLATES_PATTERN_KEY = "SmartStore.messagetemplate.";

        #endregion

        #region Fields

        private readonly IRepository<MessageTemplate> _messageTemplateRepository;
		private readonly ILanguageService _languageService;
		private readonly ILocalizedEntityService _localizedEntityService;
        private readonly IEventPublisher _eventPublisher;
        private readonly ICacheManager _cacheManager;

        #endregion

        #region Ctor

        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="cacheManager">Cache manager</param>
		/// <param name="languageService">Language service</param>
		/// <param name="localizedEntityService">Localized entity service</param>
        /// <param name="messageTemplateRepository">Message template repository</param>
        /// <param name="eventPublisher">Event published</param>
        public MessageTemplateService(ICacheManager cacheManager,
			ILanguageService languageService,
			ILocalizedEntityService localizedEntityService,
            IRepository<MessageTemplate> messageTemplateRepository,
            IEventPublisher eventPublisher)
        {
			this._cacheManager = cacheManager;
			this._languageService = languageService;
			this._localizedEntityService = localizedEntityService;
			this._messageTemplateRepository = messageTemplateRepository;
			this._eventPublisher = eventPublisher;
        }

        #endregion

        #region Methods

		/// <summary>
		/// Delete a message template
		/// </summary>
		/// <param name="messageTemplate">Message template</param>
		public virtual void DeleteMessageTemplate(MessageTemplate messageTemplate)
		{
			if (messageTemplate == null)
				throw new ArgumentNullException("messageTemplate");

			_messageTemplateRepository.Delete(messageTemplate);

			_cacheManager.RemoveByPattern(MESSAGETEMPLATES_PATTERN_KEY);

			//event notification
			_eventPublisher.EntityDeleted(messageTemplate);
		}

        /// <summary>
        /// Inserts a message template
        /// </summary>
        /// <param name="messageTemplate">Message template</param>
        public virtual void InsertMessageTemplate(MessageTemplate messageTemplate)
        {
            if (messageTemplate == null)
                throw new ArgumentNullException("messageTemplate");

            _messageTemplateRepository.Insert(messageTemplate);

            _cacheManager.RemoveByPattern(MESSAGETEMPLATES_PATTERN_KEY);

            //event notification
            _eventPublisher.EntityInserted(messageTemplate);
        }

        /// <summary>
        /// Updates a message template
        /// </summary>
        /// <param name="messageTemplate">Message template</param>
        public virtual void UpdateMessageTemplate(MessageTemplate messageTemplate)
        {
            if (messageTemplate == null)
                throw new ArgumentNullException("messageTemplate");

            _messageTemplateRepository.Update(messageTemplate);

            _cacheManager.RemoveByPattern(MESSAGETEMPLATES_PATTERN_KEY);

            //event notification
            _eventPublisher.EntityUpdated(messageTemplate);
        }

        /// <summary>
        /// Gets a message template
        /// </summary>
        /// <param name="messageTemplateId">Message template identifier</param>
        /// <returns>Message template</returns>
        public virtual MessageTemplate GetMessageTemplateById(int messageTemplateId)
        {
            if (messageTemplateId == 0)
                return null;

            string key = string.Format(MESSAGETEMPLATES_BY_ID_KEY, messageTemplateId);
            return _cacheManager.Get(key, () =>
            {
                var manufacturer = _messageTemplateRepository.GetById(messageTemplateId);
                return manufacturer;
            });
        }

        /// <summary>
        /// Gets a message template
        /// </summary>
        /// <param name="messageTemplateName">Message template name</param>
		/// <param name="storeId">Store identifier</param>
        /// <returns>Message template</returns>
		public virtual MessageTemplate GetMessageTemplateByName(string messageTemplateName, int storeId)
        {
            if (string.IsNullOrWhiteSpace(messageTemplateName))
                throw new ArgumentException("messageTemplateName");

            string key = string.Format(MESSAGETEMPLATES_BY_NAME_KEY, messageTemplateName, storeId);
            return _cacheManager.Get(key, () =>
            {
                var query = from mt in _messageTemplateRepository.Table
                                   where mt.Name == messageTemplateName &&
								   mt.StoreId == storeId
                                   select mt;
                return query.FirstOrDefault();
            });

        }

        /// <summary>
        /// Gets all message templates
        /// </summary>
		/// <param name="storeId">Store identifier; pass 0 to load all records</param>
        /// <returns>Message template list</returns>
		public virtual IList<MessageTemplate> GetAllMessageTemplates(int storeId)
        {
			string key = string.Format(MESSAGETEMPLATES_ALL_KEY, storeId);
			return _cacheManager.Get(key, () =>
            {
				var query = _messageTemplateRepository.Table;
				query = query.OrderBy(mt => mt.Name).ThenBy(mt => mt.StoreId);
				var allMessageTemplates = query.ToList();
				if (storeId == 0)
					return allMessageTemplates;

				//filter message templates
				var messageTemplates = new List<MessageTemplate>();
				var allSystemNames = allMessageTemplates
					.Select(x => x.Name)
					.Distinct(StringComparer.InvariantCultureIgnoreCase)
					.ToList();
				foreach (var systemName in allSystemNames)
				{
					//find a message template assigned to the passed storeId
					var messageTemplate = allMessageTemplates
						.FirstOrDefault(x => x.Name.Equals(systemName, StringComparison.InvariantCultureIgnoreCase) &&
							x.StoreId == storeId);

					//not found. let's find a message template assigned to all stores in this case
					if (messageTemplate == null)
						messageTemplate = allMessageTemplates
							.FirstOrDefault(x => x.Name.Equals(systemName, StringComparison.InvariantCultureIgnoreCase) &&
							x.StoreId == 0);

					if (messageTemplate != null)
						messageTemplates.Add(messageTemplate);
				}
				return messageTemplates;
            });
        }

		/// <summary>
		/// Create a copy of message template with all depended data
		/// </summary>
		/// <param name="messageTemplate">Message template</param>
		/// <returns>Message template copy</returns>
		public virtual MessageTemplate CopyMessageTemplate(MessageTemplate messageTemplate)
		{
			if (messageTemplate == null)
				throw new ArgumentNullException("messageTemplate");

			var mtCopy = new MessageTemplate()
			{
				StoreId = messageTemplate.StoreId,
				Name = messageTemplate.Name,
				BccEmailAddresses = messageTemplate.BccEmailAddresses,
				Subject = messageTemplate.Subject,
				Body = messageTemplate.Body,
				IsActive = messageTemplate.IsActive,
				EmailAccountId = messageTemplate.EmailAccountId,
			};

			InsertMessageTemplate(mtCopy);

			var languages = _languageService.GetAllLanguages(true);

			//localization
			foreach (var lang in languages)
			{
				var bccEmailAddresses = messageTemplate.GetLocalized(x => x.BccEmailAddresses, lang.Id, false, false);
				if (!String.IsNullOrEmpty(bccEmailAddresses))
					_localizedEntityService.SaveLocalizedValue(mtCopy, x => x.BccEmailAddresses, bccEmailAddresses, lang.Id);

				var subject = messageTemplate.GetLocalized(x => x.Subject, lang.Id, false, false);
				if (!String.IsNullOrEmpty(subject))
					_localizedEntityService.SaveLocalizedValue(mtCopy, x => x.Subject, subject, lang.Id);

				var body = messageTemplate.GetLocalized(x => x.Body, lang.Id, false, false);
				if (!String.IsNullOrEmpty(body))
					_localizedEntityService.SaveLocalizedValue(mtCopy, x => x.Body, subject, lang.Id);

				var emailAccountId = messageTemplate.GetLocalized(x => x.EmailAccountId, lang.Id, false, false);
				if (emailAccountId > 0)
					_localizedEntityService.SaveLocalizedValue(mtCopy, x => x.EmailAccountId, emailAccountId, lang.Id);
			}

			return mtCopy;
		}

        #endregion
    }
}
