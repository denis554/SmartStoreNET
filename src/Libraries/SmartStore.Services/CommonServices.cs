﻿using System;
using System.Collections.Generic;
using System.Linq;
using SmartStore.Core;
using SmartStore.Core.Caching;
using SmartStore.Core.Data;
using SmartStore.Core.Events;
using SmartStore.Services.Localization;
using SmartStore.Core.Logging;

namespace SmartStore.Services
{
	public class CommonServices : ICommonServices
	{
		private readonly Lazy<ICacheManager> _cache;
		private readonly Lazy<IDbContext> _dbContext;
		private readonly Lazy<IStoreContext> _storeContext;
		private readonly Lazy<IWebHelper> _webHelper;
		private readonly Lazy<IWorkContext> _workContext;
		private readonly Lazy<IEventPublisher> _eventPublisher;
		private readonly Lazy<ILocalizationService> _localization;
		private readonly Lazy<ICustomerActivityService> _customerActivity;
		
		public CommonServices(
			Func<string, Lazy<ICacheManager>> cache,
			Lazy<IDbContext> dbContext,
			Lazy<IStoreContext> storeContext,
			Lazy<IWebHelper> webHelper,
			Lazy<IWorkContext> workContext,
			Lazy<IEventPublisher> eventPublisher,
			Lazy<ILocalizationService> localization,
			Lazy<ICustomerActivityService> customerActivity)
		{
			this._cache = cache("static");
			this._dbContext = dbContext;
			this._storeContext = storeContext;
			this._webHelper = webHelper;
			this._workContext = workContext;
			this._eventPublisher = eventPublisher;
			this._localization = localization;
			this._customerActivity = customerActivity;
		}
		
		public ICacheManager Cache
		{
			get
			{
				return _cache.Value;
			}
		}

		public IDbContext DbContext
		{
			get
			{
				return _dbContext.Value;
			}
		}

		public IStoreContext StoreContext
		{
			get
			{
				return _storeContext.Value;
			}
		}

		public IWebHelper WebHelper
		{
			get
			{
				return _webHelper.Value;
			}
		}

		public IWorkContext WorkContext
		{
			get
			{
				return _workContext.Value;
			}
		}

		public IEventPublisher EventPublisher
		{
			get
			{
				return _eventPublisher.Value;
			}
		}

		public ILocalizationService Localization
		{
			get
			{
				return _localization.Value;
			}
		}

		public ICustomerActivityService CustomerActivity
		{
			get
			{
				return _customerActivity.Value;
			}
		}
	}
}
