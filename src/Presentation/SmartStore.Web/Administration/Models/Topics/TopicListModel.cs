﻿using System.Collections.Generic;
using System.Web.Mvc;
using SmartStore.Web.Framework;
using SmartStore.Web.Framework.Mvc;

namespace SmartStore.Admin.Models.Topics
{
	public class TopicListModel : ModelBase
	{
		public TopicListModel()
		{
			AvailableStores = new List<SelectListItem>();
		}

		[SmartResourceDisplayName("Admin.ContentManagement.Topics.List.SearchStore")]
		public int SearchStoreId { get; set; }
		public IList<SelectListItem> AvailableStores { get; set; }
	}
}