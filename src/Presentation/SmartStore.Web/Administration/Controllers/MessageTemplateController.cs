﻿using System.Linq;
using System.Text;
using System.Web.Mvc;
using SmartStore.Admin.Models.Messages;
using SmartStore.Core.Domain.Messages;
using SmartStore.Services.Localization;
using SmartStore.Services.Messages;
using SmartStore.Services.Security;
using SmartStore.Services.Stores;
using SmartStore.Web.Framework.Controllers;
using Telerik.Web.Mvc;

namespace SmartStore.Admin.Controllers
{
    [AdminAuthorize]
    public class MessageTemplateController : AdminControllerBase
    {
        #region Fields

        private readonly IMessageTemplateService _messageTemplateService;
        private readonly IEmailAccountService _emailAccountService;
        private readonly ILanguageService _languageService;
        private readonly ILocalizedEntityService _localizedEntityService;
        private readonly ILocalizationService _localizationService;
        private readonly IMessageTokenProvider _messageTokenProvider;
        private readonly IPermissionService _permissionService;
		private readonly IStoreService _storeService;
        private readonly EmailAccountSettings _emailAccountSettings;
        #endregion Fields

        #region Constructors

        public MessageTemplateController(IMessageTemplateService messageTemplateService, 
            IEmailAccountService emailAccountService, ILanguageService languageService, 
            ILocalizedEntityService localizedEntityService,
            ILocalizationService localizationService, IMessageTokenProvider messageTokenProvider,
			IPermissionService permissionService, IStoreService storeService, 
			EmailAccountSettings emailAccountSettings)
        {
            this._messageTemplateService = messageTemplateService;
            this._emailAccountService = emailAccountService;
            this._languageService = languageService;
            this._localizedEntityService = localizedEntityService;
            this._localizationService = localizationService;
            this._messageTokenProvider = messageTokenProvider;
            this._permissionService = permissionService;
			this._storeService = storeService;
            this._emailAccountSettings = emailAccountSettings;
        }

        private string FormatTokens(string[] tokens)
        {
            var sb = new StringBuilder();
            for (int i = 0; i < tokens.Length; i++)
            {
                string token = tokens[i];
                sb.Append(token);
                if (i != tokens.Length - 1)
                    sb.Append(", ");
            }

            return sb.ToString();
        }

        #endregion
        
        #region Utilities

        [NonAction]
        public void UpdateLocales(MessageTemplate mt, MessageTemplateModel model)
        {
            foreach (var localized in model.Locales)
            {
                _localizedEntityService.SaveLocalizedValue(mt,
                                                           x => x.BccEmailAddresses,
                                                           localized.BccEmailAddresses,
                                                           localized.LanguageId);

                _localizedEntityService.SaveLocalizedValue(mt,
                                                           x => x.Subject,
                                                           localized.Subject,
                                                           localized.LanguageId);

                _localizedEntityService.SaveLocalizedValue(mt,
                                                           x => x.Body,
                                                           localized.Body,
                                                           localized.LanguageId);

                _localizedEntityService.SaveLocalizedValue(mt,
                                                           x => x.EmailAccountId,
                                                           localized.EmailAccountId,
                                                           localized.LanguageId);
            }
        }
        
        #endregion
        
        #region Methods

        public ActionResult Index()
        {
            return RedirectToAction("List");
        }

        public ActionResult List()
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageMessageTemplates))
                return AccessDeniedView();

            return View();
        }

        [HttpPost, GridAction(EnableCustomBinding = true)]
        public ActionResult List(GridCommand command)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageMessageTemplates))
                return AccessDeniedView();

            var messageTemplates = _messageTemplateService.GetAllMessageTemplates(0);
			var gridModel = new GridModel<MessageTemplateModel>
			{
				Data = messageTemplates.Select(x =>
				{
					var model = x.ToModel();
					var store = _storeService.GetStoreById(x.StoreId);
					model.StoreName = store != null ? store.Name : "Unknown";
					return model;
				}),
				Total = messageTemplates.Count
			};
            return new JsonResult
            {
                Data = gridModel
            };
        }

        public ActionResult Edit(int id)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageMessageTemplates))
                return AccessDeniedView();

            var messageTemplate = _messageTemplateService.GetMessageTemplateById(id);
            if (messageTemplate == null)
                //No message template found with the specified id
                return RedirectToAction("List");


            var model = messageTemplate.ToModel();
            model.AllowedTokens = FormatTokens(_messageTokenProvider.GetListOfAllowedTokens());
            //available email accounts
            foreach (var ea in _emailAccountService.GetAllEmailAccounts())
                model.AvailableEmailAccounts.Add(ea.ToModel());
			//stores
			foreach (var store in _storeService.GetAllStores())
				model.AvailableStores.Add(store.ToModel());
			model.AvailableStores = _storeService
				.GetAllStores()
				.Select(s => s.ToModel())
				.ToList();
            //locales
            AddLocales(_languageService, model.Locales, (locale, languageId) =>
            {
                locale.BccEmailAddresses = messageTemplate.GetLocalized(x => x.BccEmailAddresses, languageId, false, false);
                locale.Subject = messageTemplate.GetLocalized(x => x.Subject, languageId, false, false);
                locale.Body = messageTemplate.GetLocalized(x => x.Body, languageId, false, false);

                var emailAccountId = messageTemplate.GetLocalized(x => x.EmailAccountId, languageId, false, false);
                locale.EmailAccountId = emailAccountId > 0 ? emailAccountId : _emailAccountSettings.DefaultEmailAccountId;
            });

            return View(model);
        }

        [HttpPost, ParameterBasedOnFormNameAttribute("save-continue", "continueEditing")]
        public ActionResult Edit(MessageTemplateModel model, bool continueEditing)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageMessageTemplates))
                return AccessDeniedView();

            var messageTemplate = _messageTemplateService.GetMessageTemplateById(model.Id);
            if (messageTemplate == null)
                //No message template found with the specified id
                return RedirectToAction("List");
            
            if (ModelState.IsValid)
            {
                messageTemplate = model.ToEntity(messageTemplate);
                _messageTemplateService.UpdateMessageTemplate(messageTemplate);
                //locales
                UpdateLocales(messageTemplate, model);

                SuccessNotification(_localizationService.GetResource("Admin.ContentManagement.MessageTemplates.Updated"));
                return continueEditing ? RedirectToAction("Edit", messageTemplate.Id) : RedirectToAction("List");
            }


            //If we got this far, something failed, redisplay form
            model.AllowedTokens = FormatTokens(_messageTokenProvider.GetListOfAllowedTokens());
            //available email accounts
            foreach (var ea in _emailAccountService.GetAllEmailAccounts())
                model.AvailableEmailAccounts.Add(ea.ToModel());
			//stores
			foreach (var store in _storeService.GetAllStores())
				model.AvailableStores.Add(store.ToModel());
            return View(model);
        }
        
        #endregion
    }
}
