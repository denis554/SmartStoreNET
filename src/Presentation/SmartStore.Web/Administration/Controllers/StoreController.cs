﻿using System;
using System.Linq;
using System.Web.Mvc;
using SmartStore.Admin.Models.Directory;
using SmartStore.Admin.Models.Stores;
using SmartStore.Core.Domain.Directory;
using SmartStore.Core.Domain.Stores;
using SmartStore.Services.Configuration;
using SmartStore.Services.Directory;
using SmartStore.Services.Localization;
using SmartStore.Services.Security;
using SmartStore.Services.Stores;
using SmartStore.Web.Framework.Controllers;
using Telerik.Web.Mvc;

namespace SmartStore.Admin.Controllers
{
	[AdminAuthorize]
	public partial class StoreController : AdminControllerBase
	{
		private readonly IStoreService _storeService;
		private readonly ISettingService _settingService;
		private readonly ILocalizationService _localizationService;
		private readonly IPermissionService _permissionService;

		public StoreController(IStoreService storeService,
			ISettingService settingService,
			ILocalizationService localizationService,
			IPermissionService permissionService)
		{
			this._storeService = storeService;
			this._settingService = settingService;
			this._localizationService = localizationService;
			this._permissionService = permissionService;
		}

		public ActionResult List()
		{
			if (!_permissionService.Authorize(StandardPermissionProvider.ManageStores))
				return AccessDeniedView();

			return View();
		}

		/// <remarks>codehint: sm-add</remarks>
		public ActionResult AllStores(string label, int selectedId)
		{
			var stores = _storeService.GetAllStores();

			stores.Insert(0, new Store 
			{
				Id = 0,
				Name = _localizationService.GetResource("Admin.Common.StoresAll")
			});

			var list = 
				from m in stores
				select new
				{
					id = m.Id.ToString(),
					text = m.Name,
					selected = m.Id == selectedId
				};

			return new JsonResult { Data = list.ToList(), JsonRequestBehavior = JsonRequestBehavior.AllowGet };
		}

		[HttpPost, GridAction(EnableCustomBinding = true)]
		public ActionResult List(GridCommand command)
		{
			if (!_permissionService.Authorize(StandardPermissionProvider.ManageStores))
				return AccessDeniedView();

			var storeModels = _storeService.GetAllStores()
				.Select(x => x.ToModel())
				.ToList();

			var gridModel = new GridModel<StoreModel>
			{
				Data = storeModels,
				Total = storeModels.Count()
			};

			return new JsonResult
			{
				Data = gridModel
			};
		}

		public ActionResult Create()
		{
			if (!_permissionService.Authorize(StandardPermissionProvider.ManageStores))
				return AccessDeniedView();

			var model = new StoreModel();
			return View(model);
		}

		[HttpPost, ParameterBasedOnFormNameAttribute("save-continue", "continueEditing")]
		public ActionResult Create(StoreModel model, bool continueEditing)
		{
			if (!_permissionService.Authorize(StandardPermissionProvider.ManageStores))
				return AccessDeniedView();

			if (ModelState.IsValid)
			{
				var store = model.ToEntity();
				//ensure we have "/" at the end
				store.Url.EnsureEndsWith("/");
				_storeService.InsertStore(store);

				SuccessNotification(_localizationService.GetResource("Admin.Configuration.Stores.Added"));
				return continueEditing ? RedirectToAction("Edit", new { id = store.Id }) : RedirectToAction("List");
			}

			//If we got this far, something failed, redisplay form
			return View(model);
		}

		public ActionResult Edit(int id)
		{
			if (!_permissionService.Authorize(StandardPermissionProvider.ManageStores))
				return AccessDeniedView();

			var store = _storeService.GetStoreById(id);
			if (store == null)
				//No store found with the specified id
				return RedirectToAction("List");

			var model = store.ToModel();
			return View(model);
		}

		[HttpPost, ParameterBasedOnFormNameAttribute("save-continue", "continueEditing")]
		[FormValueRequired("save", "save-continue")]
		public ActionResult Edit(StoreModel model, bool continueEditing)
		{
			if (!_permissionService.Authorize(StandardPermissionProvider.ManageStores))
				return AccessDeniedView();

			var store = _storeService.GetStoreById(model.Id);
			if (store == null)
				//No store found with the specified id
				return RedirectToAction("List");

			if (ModelState.IsValid)
			{
				store = model.ToEntity(store);
				//ensure we have "/" at the end
				store.Url.EnsureEndsWith("/");
				_storeService.UpdateStore(store);

				SuccessNotification(_localizationService.GetResource("Admin.Configuration.Stores.Updated"));
				return continueEditing ? RedirectToAction("Edit", new { id = store.Id }) : RedirectToAction("List");
			}

			//If we got this far, something failed, redisplay form
			return View(model);
		}

		[HttpPost]
		public ActionResult Delete(int id)
		{
			if (!_permissionService.Authorize(StandardPermissionProvider.ManageStores))
				return AccessDeniedView();

			var store = _storeService.GetStoreById(id);
			if (store == null)
				//No store found with the specified id
				return RedirectToAction("List");

			try
			{
				_storeService.DeleteStore(store);

				//when we delete a store we should also ensure that all "per store" settings will also be deleted
				var settingsToDelete = _settingService
					.GetAllSettings()
					.Where(s => s.StoreId == id)
					.ToList();
				foreach (var setting in settingsToDelete)
					_settingService.DeleteSetting(setting);
				//when we had two stores and now have only one store, we also should delete all "per store" settings
				var allStores = _storeService.GetAllStores();
				if (allStores.Count == 1)
				{
					settingsToDelete = _settingService
						.GetAllSettings()
						.Where(s => s.StoreId == allStores[0].Id)
						.ToList();
					foreach (var setting in settingsToDelete)
						_settingService.DeleteSetting(setting);
				}

				SuccessNotification(_localizationService.GetResource("Admin.Configuration.Stores.Deleted"));
				return RedirectToAction("List");
			}
			catch (Exception exc)
			{
				ErrorNotification(exc);
				return RedirectToAction("Edit", new { id = store.Id });
			}
		}
	}
}
