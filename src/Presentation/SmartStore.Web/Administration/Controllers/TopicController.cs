﻿using System;
using System.Linq;
using System.Web.Mvc;
using SmartStore.Admin.Models.Topics;
using SmartStore.Core.Domain.Topics;
using SmartStore.Core.Logging;
using SmartStore.Services.Localization;
using SmartStore.Services.Security;
using SmartStore.Services.Seo;
using SmartStore.Services.Stores;
using SmartStore.Services.Topics;
using SmartStore.Web.Framework;
using SmartStore.Web.Framework.Controllers;
using SmartStore.Web.Framework.Filters;
using SmartStore.Web.Framework.Modelling;
using SmartStore.Web.Framework.Security;
using Telerik.Web.Mvc;

namespace SmartStore.Admin.Controllers
{
	[AdminAuthorize]
    public class TopicController : AdminControllerBase
    {
        private readonly ITopicService _topicService;
        private readonly ILanguageService _languageService;
        private readonly ILocalizedEntityService _localizedEntityService;
		private readonly IStoreMappingService _storeMappingService;
		private readonly IUrlRecordService _urlRecordService;

		public TopicController(
			ITopicService topicService, 
			ILanguageService languageService,
            ILocalizedEntityService localizedEntityService, 
            IStoreMappingService storeMappingService, 
			IUrlRecordService urlRecordService)
        {
            _topicService = topicService;
            _languageService = languageService;
            _localizedEntityService = localizedEntityService;
			_storeMappingService = storeMappingService;
			_urlRecordService = urlRecordService;
		}

        [NonAction]
        public void UpdateLocales(Topic topic, TopicModel model)
        {
            foreach (var localized in model.Locales)
            {
                _localizedEntityService.SaveLocalizedValue(topic,
                                                               x => x.Title,
                                                               localized.Title,
                                                               localized.LanguageId);

                _localizedEntityService.SaveLocalizedValue(topic,
                                                           x => x.Body,
                                                           localized.Body,
                                                           localized.LanguageId);

                _localizedEntityService.SaveLocalizedValue(topic,
                                                           x => x.MetaKeywords,
                                                           localized.MetaKeywords,
                                                           localized.LanguageId);

                _localizedEntityService.SaveLocalizedValue(topic,
                                                           x => x.MetaDescription,
                                                           localized.MetaDescription,
                                                           localized.LanguageId);

                _localizedEntityService.SaveLocalizedValue(topic,
                                                           x => x.MetaTitle,
                                                           localized.MetaTitle,
                                                           localized.LanguageId);

				var seName = topic.ValidateSeName(localized.SeName, localized.Title, false);
				_urlRecordService.SaveSlug(topic, seName, localized.LanguageId);

			}
        }

		[NonAction]
		private void PrepareStoresMappingModel(TopicModel model, Topic topic, bool excludeProperties)
		{
			Guard.NotNull(model, nameof(model));

			if (!excludeProperties)
			{
				model.SelectedStoreIds = _storeMappingService.GetStoresIdsWithAccess(topic);
			}

			model.AvailableStores = Services.StoreService.GetAllStores().ToSelectListItems(model.SelectedStoreIds);
		}

		private string GetTopicUrl(Topic topic)
		{
			string url = null;

			try
			{
				if (topic.LimitedToStores)
				{
					var storeMappings = _storeMappingService.GetStoreMappings(topic);
					if (storeMappings.FirstOrDefault(x => x.StoreId == Services.StoreContext.CurrentStore.Id) == null)
					{
						var storeMapping = storeMappings.FirstOrDefault();
						if (storeMapping != null)
						{
							var store = Services.StoreService.GetStoreById(storeMapping.StoreId);
							if (store != null)
							{
								var baseUri = new Uri(Services.StoreService.GetHost(store));
								url = baseUri.GetLeftPart(UriPartial.Authority) + Url.RouteUrl("Topic", new { SeName = topic.GetSeName() });
							}
						}
					}
				}

				if (url.IsEmpty())
				{
					url = Url.RouteUrl("Topic", new { SeName = topic.GetSeName() }, Request.Url.Scheme);
				}
			}
			catch (Exception exception)
			{
				Logger.Error(exception);
			}

			return url;
		}

		public ActionResult Index()
        {
            return RedirectToAction("List");
        }

        public ActionResult List()
        {
            if (!Services.Permissions.Authorize(StandardPermissionProvider.ManageTopics))
                return AccessDeniedView();

			var model = new TopicListModel();

			foreach (var s in Services.StoreService.GetAllStores())
			{
				model.AvailableStores.Add(new SelectListItem { Text = s.Name, Value = s.Id.ToString() });
			}

			return View(model);
        }

        [HttpPost, GridAction(EnableCustomBinding = true)]
		public ActionResult List(GridCommand command, TopicListModel model)
        {
			var gridModel = new GridModel<TopicModel>();

			if (Services.Permissions.Authorize(StandardPermissionProvider.ManageTopics))
			{
				var topics = _topicService.GetAllTopics(model.SearchStoreId, command.Page - 1, command.PageSize).AlterQuery(q =>
				{
					var q2 = q;

					if (model.SystemName.HasValue())
						q2 = q2.Where(x => x.SystemName.Contains(model.SystemName));

					if (model.Title.HasValue())
						q2 = q2.Where(x => x.Title.Contains(model.Title));

					if (model.RenderAsWidget.HasValue)
						q2 = q2.Where(x => x.RenderAsWidget == model.RenderAsWidget.Value);

					if (model.WidgetZone.HasValue())
						q2 = q2.Where(x => x.WidgetZone.Contains(model.WidgetZone));

					return q2;
				});
				
				gridModel.Data = topics.Select(x =>
				{
					var item = x.ToModel();
					item.WidgetZone = x.WidgetZone.SplitSafe(",");
					// otherwise maxJsonLength could be exceeded
					item.Body = "";
					return item;
				});

				gridModel.Total = topics.TotalCount;
			}
			else
			{
				gridModel.Data = Enumerable.Empty<TopicModel>();
				NotifyAccessDenied();
			}

            return new JsonResult
			{
				MaxJsonLength = int.MaxValue,
				Data = gridModel
			};
        }

        public ActionResult Create()
        {
            if (!Services.Permissions.Authorize(StandardPermissionProvider.ManageTopics))
                return AccessDeniedView();

            var model = new TopicModel();
			//Stores
			PrepareStoresMappingModel(model, null, false);
            //locales
            AddLocales(_languageService, model.Locales);

            model.TitleTag = "h1";

            return View(model);
        }

        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        public ActionResult Create(TopicModel model, bool continueEditing)
        {
            if (!Services.Permissions.Authorize(StandardPermissionProvider.ManageTopics))
                return AccessDeniedView();

            if (ModelState.IsValid)
            {
                if (!model.IsPasswordProtected)
                {
                    model.Password = null;
                }

                var topic = model.ToEntity();

                if (model.WidgetZone != null)
                {
                    topic.WidgetZone = string.Join(",", model.WidgetZone);
                }

                _topicService.InsertTopic(topic);

				model.SeName = topic.ValidateSeName(model.SeName, topic.Title.NullEmpty() ?? topic.SystemName, true);
				_urlRecordService.SaveSlug(topic, model.SeName, 0);

                UpdateLocales(topic, model);

                NotifySuccess(T("Admin.ContentManagement.Topics.Added"));
                return continueEditing ? RedirectToAction("Edit", new { id = topic.Id }) : RedirectToAction("List");
            }

            // If we got this far, something failed, redisplay form.
			PrepareStoresMappingModel(model, null, true);

            return View(model);
        }

        public ActionResult Edit(int id)
        {
            if (!Services.Permissions.Authorize(StandardPermissionProvider.ManageTopics))
                return AccessDeniedView();

            var topic = _topicService.GetTopicById(id);
            if (topic == null)
                return RedirectToAction("List");

            var model = topic.ToModel();
			model.Url = GetTopicUrl(topic);
			
			PrepareStoresMappingModel(model, topic, false);
            
            AddLocales(_languageService, model.Locales, (locale, languageId) =>
            {
                locale.Title = topic.GetLocalized(x => x.Title, languageId, false, false);
                locale.Body = topic.GetLocalized(x => x.Body, languageId, false, false);
                locale.MetaKeywords = topic.GetLocalized(x => x.MetaKeywords, languageId, false, false);
                locale.MetaDescription = topic.GetLocalized(x => x.MetaDescription, languageId, false, false);
                locale.MetaTitle = topic.GetLocalized(x => x.MetaTitle, languageId, false, false);
				locale.SeName = topic.GetSeName(languageId, false, false);
            });

			model.WidgetZone = topic.WidgetZone.SplitSafe(",");

			return View(model);
        }

        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        [ValidateInput(false)]
        public ActionResult Edit(TopicModel model, bool continueEditing, FormCollection form)
        {
            if (!Services.Permissions.Authorize(StandardPermissionProvider.ManageTopics))
                return AccessDeniedView();

            var topic = _topicService.GetTopicById(model.Id);
            if (topic == null)
                return RedirectToAction("List");

            if (!model.IsPasswordProtected)
            {
                model.Password = null;
            }

            if (ModelState.IsValid)
            {
                topic = model.ToEntity(topic);

				if (model.WidgetZone != null)
				{
					topic.WidgetZone = string.Join(",", model.WidgetZone);
				}
				
				_topicService.UpdateTopic(topic);

				model.SeName = topic.ValidateSeName(model.SeName, topic.Title.NullEmpty() ?? topic.SystemName, true);
				_urlRecordService.SaveSlug(topic, model.SeName, 0);

				_storeMappingService.SaveStoreMappings<Topic>(topic, model.SelectedStoreIds);
                
                UpdateLocales(topic, model);

                Services.EventPublisher.Publish(new ModelBoundEvent(model, topic, form));

                NotifySuccess(T("Admin.ContentManagement.Topics.Updated"));
                return continueEditing ? RedirectToAction("Edit", topic.Id) : RedirectToAction("List");
            }

			// If we got this far, something failed, redisplay form.
			model.Url = GetTopicUrl(topic);
			PrepareStoresMappingModel(model, topic, true);

            return View(model);
        }

        [HttpPost, ActionName("Delete")]
        public ActionResult DeleteConfirmed(int id)
        {
            if (!Services.Permissions.Authorize(StandardPermissionProvider.ManageTopics))
                return AccessDeniedView();

            var topic = _topicService.GetTopicById(id);
            if (topic == null)
                return RedirectToAction("List");

            if (topic.IsSystemTopic)
            {
                NotifyError(T("Admin.ContentManagement.Topics.CannotBeDeleted"));
                return RedirectToAction("List");
            }
            
            _topicService.DeleteTopic(topic);

            NotifySuccess(T("Admin.ContentManagement.Topics.Deleted"));
            return RedirectToAction("List");
        }
    }
}
