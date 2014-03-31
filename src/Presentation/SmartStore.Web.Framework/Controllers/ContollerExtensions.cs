﻿﻿using System.IO;
using System.Web.Mvc;
using SmartStore.Web.Framework.UI;
using System.Collections.Generic;
using SmartStore.Services.Stores;
using SmartStore.Services.Common;
using SmartStore.Core;
using SmartStore.Core.Domain.Customers;
using SmartStore.Collections;

namespace SmartStore.Web.Framework.Controllers
{
    public static class ContollerExtensions
    {
        /// <summary>
        /// Render partial view to string
        /// </summary>
        /// <returns>Result</returns>
        public static string RenderPartialViewToString(this Controller controller)
        {
            return RenderPartialViewToString(controller, null, null);
        }
        /// <summary>
        /// Render partial view to string
        /// </summary>
        /// <param name="viewName">View name</param>
        /// <returns>Result</returns>
        public static string RenderPartialViewToString(this Controller controller, string viewName)
        {
            return RenderPartialViewToString(controller, viewName, null);
        }
        /// <summary>
        /// Render partial view to string
        /// </summary>
        /// <param name="model">Model</param>
        /// <returns>Result</returns>
        public static string RenderPartialViewToString(this Controller controller, object model)
        {
            return RenderPartialViewToString(controller, null, model);
        }
        /// <summary>
        /// Render partial view to string
        /// </summary>
        /// <param name="viewName">View name</param>
        /// <param name="model">Model</param>
        /// <returns>Result</returns>
        public static string RenderPartialViewToString(this Controller controller, string viewName, object model)
        {
            //Original source code: http://craftycodeblog.com/2010/05/15/asp-net-mvc-render-partial-view-to-string/
            if (string.IsNullOrEmpty(viewName))
                viewName = controller.ControllerContext.RouteData.GetRequiredString("action");

            controller.ViewData.Model = model;

            using (var sw = new StringWriter())
            {
                ViewEngineResult viewResult = System.Web.Mvc.ViewEngines.Engines.FindPartialView(controller.ControllerContext, viewName);
                var viewContext = new ViewContext(controller.ControllerContext, viewResult.View, controller.ViewData, controller.TempData, sw);
                viewResult.View.Render(viewContext, sw);

                return sw.GetStringBuilder().ToString();
            }
        }

		///// <summary>
		///// Pushes a message to the notification queue
		///// </summary>
		///// <param name="type">Notification type</param>
		///// <param name="message">Message</param>
		///// <param name="durable">A value indicating whether a message should be persisted for the next request</param>
		//public static void Notify(this ControllerBase controller, NotifyType type, string message, bool durable)
		//{
		//	if (message.IsEmpty())
		//		return;

		//	var storage = (durable ? (IDictionary<string, object>)controller.TempData : (IDictionary<string, object>)controller.ViewData);

		//	ICollection<string> messages = null;
		//	string key = string.Format("sm.notifications.{0}", type);

		//	if (!storage.ContainsKey(key))
		//	{
		//		storage[key] = new HashSet<string>();
		//	}

		//	messages = storage[key] as ICollection<string>;

		//	if (messages != null)
		//	{
		//		messages.Add(message);
		//	}
		//}

		/// <summary>
		/// Pushes a message to the notification queue
		/// </summary>
		/// <param name="type">Notification type</param>
		/// <param name="message">Message</param>
		/// <param name="durable">A value indicating whether a message should be persisted for the next request</param>
		public static void Notify(this ControllerBase controller, NotifyType type, string message, bool durable)
		{
			if (message.IsEmpty())
				return;

			var storage = (durable ? (IDictionary<string, object>)controller.TempData : (IDictionary<string, object>)controller.ViewData);

			Multimap<NotifyType, string> messages = null;
			string key = "sm.notifications.all";

			if (!storage.ContainsKey(key))
			{
				storage[key] = messages = new Multimap<NotifyType, string>();
			}

			if (messages != null && !messages.ContainsValue(type, message))
			{
				messages.Add(type, message);
			}
		}

		/// <summary>
		/// Get active store scope (for multi-store configuration mode)
		/// </summary>
		/// <param name="controller">Controller</param>
		/// <param name="storeService">Store service</param>
		/// <param name="workContext">Work context</param>
		/// <returns>Store ID; 0 if we are in a shared mode</returns>
		public static int GetActiveStoreScopeConfiguration(this Controller controller, IStoreService storeService, IWorkContext workContext)
		{
			//ensure that we have 2 (or more) stores
			if (storeService.GetAllStores().Count < 2)
				return 0;

			var storeId = workContext.CurrentCustomer.GetAttribute<int>(SystemCustomerAttributeNames.AdminAreaStoreScopeConfiguration);
			var store = storeService.GetStoreById(storeId);
			return store != null ? store.Id : 0;
		}
    }
}
