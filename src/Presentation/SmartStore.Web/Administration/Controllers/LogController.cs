﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using SmartStore.Admin.Models.Logging;
using SmartStore.Core;
using SmartStore.Core.Domain.Logging;
using SmartStore.Services.Helpers;
using SmartStore.Services.Localization;
using SmartStore.Services.Logging;
using SmartStore.Services.Security;
using SmartStore.Web.Framework;
using SmartStore.Web.Framework.Controllers;
using Telerik.Web.Mvc;

namespace SmartStore.Admin.Controllers
{
    [AdminAuthorize]
    public class LogController : AdminControllerBase
    {
        private readonly ILogger _logger;
        private readonly IWorkContext _workContext;
        private readonly ILocalizationService _localizationService;
        private readonly IDateTimeHelper _dateTimeHelper;
        private readonly IPermissionService _permissionService;

        public LogController(ILogger logger, IWorkContext workContext,
            ILocalizationService localizationService, IDateTimeHelper dateTimeHelper,
            IPermissionService permissionService)
        {
            this._logger = logger;
            this._workContext = workContext;
            this._localizationService = localizationService;
            this._dateTimeHelper = dateTimeHelper;
            this._permissionService = permissionService;
        }

        public ActionResult Index()
        {
            return RedirectToAction("List");
        }

        public ActionResult List()
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageSystemLog))
                return AccessDeniedView();

            var model = new LogListModel();
            model.AvailableLogLevels = LogLevel.Debug.ToSelectList(false).ToList();
            model.AvailableLogLevels.Insert(0, new SelectListItem() { Text = _localizationService.GetResource("Admin.Common.All"), Value = "0" });

            return View(model);
        }

        [GridAction(EnableCustomBinding = true)]
        public ActionResult LogList(GridCommand command, LogListModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageSystemLog))
                return AccessDeniedView();

            DateTime? createdOnFromValue = (model.CreatedOnFrom == null) ? null
                            : (DateTime?)_dateTimeHelper.ConvertToUtcTime(model.CreatedOnFrom.Value, _dateTimeHelper.CurrentTimeZone);

            DateTime? createdToFromValue = (model.CreatedOnTo == null) ? null
                            : (DateTime?)_dateTimeHelper.ConvertToUtcTime(model.CreatedOnTo.Value, _dateTimeHelper.CurrentTimeZone).AddDays(1);

            LogLevel? logLevel = model.LogLevelId > 0 ? (LogLevel?)(model.LogLevelId) : null;


            var logItems = _logger.GetAllLogs(createdOnFromValue, createdToFromValue, model.Message,
                logLevel, command.Page - 1, command.PageSize);
            var gridModel = new GridModel<LogModel>
            {
                Data = logItems.Select(x =>
                {
                    return new LogModel()
                    {
                        Id = x.Id,
                        LogLevel = x.LogLevel.GetLocalizedEnum(_localizationService, _workContext),
                        ShortMessage = x.ShortMessage,
                        FullMessage = x.FullMessage,
                        IpAddress = x.IpAddress,
                        CustomerId = x.CustomerId,
                        CustomerEmail = x.Customer != null ? x.Customer.Email : null,
                        PageUrl = x.PageUrl,
                        ReferrerUrl = x.ReferrerUrl,
                        CreatedOn = _dateTimeHelper.ConvertToUserTime(x.CreatedOnUtc, DateTimeKind.Utc)
                    };
                }),
                Total = logItems.TotalCount
            };
            return new JsonResult
            {
                Data = gridModel
            };
        }
        
        [HttpPost, ActionName("List")]
        [FormValueRequired("clearall")]
        public ActionResult ClearAll()
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageSystemLog))
                return AccessDeniedView();

            _logger.ClearLog();

            SuccessNotification(_localizationService.GetResource("Admin.System.Log.Cleared"));
            return RedirectToAction("List");
        }

        public ActionResult View(int id)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageSystemLog))
                return AccessDeniedView();

            var log = _logger.GetLogById(id);
            if (log == null)
                //No log found with the specified id
                return RedirectToAction("List");

            var model = new LogModel()
            {
                Id = log.Id,
                LogLevel = log.LogLevel.GetLocalizedEnum(_localizationService, _workContext),
                ShortMessage = log.ShortMessage,
                FullMessage = log.FullMessage,
                IpAddress = log.IpAddress,
                CustomerId = log.CustomerId,
                CustomerEmail = log.Customer != null ? log.Customer.Email : null,
                PageUrl = log.PageUrl,
                ReferrerUrl = log.ReferrerUrl,
                CreatedOn = _dateTimeHelper.ConvertToUserTime(log.CreatedOnUtc, DateTimeKind.Utc)
            };

            return View(model);
        }

        [HttpPost, ActionName("Delete")]
        public ActionResult DeleteConfirmed(int id)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageSystemLog))
                return AccessDeniedView();

            var log = _logger.GetLogById(id);
            if (log == null)
                //No log found with the specified id
                return RedirectToAction("List");

            _logger.DeleteLog(log);


            SuccessNotification(_localizationService.GetResource("Admin.System.Log.Deleted"));
            return RedirectToAction("List");
        }

        [HttpPost]
        public ActionResult DeleteSelected(ICollection<int> selectedIds)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageSystemLog))
                return AccessDeniedView();

            if (selectedIds != null)
            {
                var logItems = _logger.GetLogByIds(selectedIds.ToArray());
                foreach (var logItem in logItems)
                    _logger.DeleteLog(logItem);
            }

            return Json(new { Result = true});
        }
    }
}
