﻿using System;
using System.Linq;
using System.Web.Mvc;
using SmartStore.Admin.Models.Directory;
using SmartStore.Admin.Models.Stores;
using SmartStore.Core.Domain.Directory;
using SmartStore.Core.Domain.Stores;
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
		private readonly ILocalizationService _localizationService;
		private readonly IPermissionService _permissionService;

		public StoreController(IStoreService storeService,
			ILocalizationService localizationService,
			IPermissionService permissionService)
		{
			this._storeService = storeService;
			this._localizationService = localizationService;
			this._permissionService = permissionService;
		}

		public ActionResult List()
		{
			if (!_permissionService.Authorize(StandardPermissionProvider.ManageStores))
				return AccessDeniedView();

			return View();
		}

		[HttpPost, GridAction(EnableCustomBinding = true)]
		public ActionResult List(GridCommand command)
		{
			if (!_permissionService.Authorize(StandardPermissionProvider.ManageStores))
				return AccessDeniedView();

			var storeModels = _storeService.GetAllStores()
				.Select(x => new StoreModel()
				{
					Id = x.Id,
					Name = x.Name,
					DisplayOrder = x.DisplayOrder
				})
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
				var store = new Store()
				{
					Name = model.Name,
					DisplayOrder = model.DisplayOrder
				};
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

			var model = new StoreModel()
			{
				Id = store.Id,
				Name = store.Name,
				DisplayOrder = store.DisplayOrder
			};
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
				store.Name = model.Name;
				store.DisplayOrder = model.DisplayOrder;
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
