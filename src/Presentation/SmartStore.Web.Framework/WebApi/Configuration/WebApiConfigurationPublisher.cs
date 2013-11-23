﻿using SmartStore.Core.Infrastructure;
using SmartStore.Core.Plugins;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SmartStore.Web.Framework.WebApi.Configuration
{
	public class WebApiConfigurationPublisher : IWebApiConfigurationPublisher
	{
		private readonly ITypeFinder _typeFinder;

		public WebApiConfigurationPublisher(ITypeFinder typeFinder)
		{
			_typeFinder = typeFinder;
		}

		public void Configure(WebApiConfigurationBroadcaster configData)
		{
			var providerTypes = _typeFinder.FindClassesOfType<IWebApiConfigurationProvider>();
			var providers = new List<IWebApiConfigurationProvider>();

			foreach (var providerType in providerTypes)
			{
				var pluginDescriptor = PluginManager.ReferencedPlugins.FirstOrDefault(x => x.ReferencedAssembly == providerType.Assembly);
				if (pluginDescriptor != null && !pluginDescriptor.Installed)
					continue;

				var provider = Activator.CreateInstance(providerType) as IWebApiConfigurationProvider;
				providers.Add(provider);
			}

			providers = providers.OrderByDescending(x => x.Priority).ToList();
			providers.Each(x => x.Configure(configData));
		}
	}
}
