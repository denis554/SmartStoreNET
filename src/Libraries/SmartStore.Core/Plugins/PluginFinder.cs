﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace SmartStore.Core.Plugins
{
    /// <summary>
	/// Plugin finder
    /// </summary>
    public class PluginFinder : IPluginFinder
    {
        private IList<PluginDescriptor> _plugins;
        private bool _arePluginsLoaded = false;

		protected virtual void EnsurePluginsAreLoaded()
		{
			if (!_arePluginsLoaded)
			{
				var foundPlugins = PluginManager.ReferencedPlugins.ToList();
				foundPlugins.Sort(); //sort
				_plugins = foundPlugins.ToList();

				_arePluginsLoaded = true;
			}
		}

		/// <summary>
		/// Gets plugins
		/// </summary>
		/// <typeparam name="T">The type of plugins to get.</typeparam>
		/// <param name="installedOnly">A value indicating whether to load only installed plugins</param>
		/// <returns>Plugins</returns>
		public virtual IEnumerable<T> GetPlugins<T>(bool installedOnly = true) where T : class, IPlugin
        {
            EnsurePluginsAreLoaded();

            foreach (var plugin in _plugins)
                if (typeof(T).IsAssignableFrom(plugin.PluginType))
                    if (!installedOnly || plugin.Installed)
						yield return plugin.Instance<T>();
        }

		/// <summary>
		/// Get plugin descriptors
		/// </summary>
		/// <param name="installedOnly">A value indicating whether to load only installed plugins</param>
		/// <returns>Plugin descriptors</returns>
		public virtual IEnumerable<PluginDescriptor> GetPluginDescriptors(bool installedOnly = true)
        {
            EnsurePluginsAreLoaded();

            foreach (var plugin in _plugins)
                if (!installedOnly || plugin.Installed)
					yield return plugin;
        }

		/// <summary>
		/// Get plugin descriptors
		/// </summary>
		/// <typeparam name="T">The type of plugin to get.</typeparam>
		/// <param name="installedOnly">A value indicating whether to load only installed plugins</param>
		/// <returns>Plugin descriptors</returns>
		public virtual IEnumerable<PluginDescriptor> GetPluginDescriptors<T>(bool installedOnly = true)
			where T : class, IPlugin
        {
            EnsurePluginsAreLoaded();

            foreach (var plugin in _plugins)
                if (typeof(T).IsAssignableFrom(plugin.PluginType))
                    if (!installedOnly || plugin.Installed)
						yield return plugin;
        }

        public virtual PluginDescriptor GetPluginDescriptorByAssembly(Assembly assembly, bool installedOnly = true)
        {
            Guard.ArgumentNotNull(() => assembly);
            return GetPluginDescriptors(installedOnly).FirstOrDefault(p => p.ReferencedAssembly == assembly);
        }

		/// <summary>
		/// Get a plugin descriptor by its system name
		/// </summary>
		/// <param name="systemName">Plugin system name</param>
		/// <param name="installedOnly">A value indicating whether to load only installed plugins</param>
		/// <returns>>Plugin descriptor</returns>
        public virtual PluginDescriptor GetPluginDescriptorBySystemName(string systemName, bool installedOnly = true)
        {
            return GetPluginDescriptors(installedOnly)
                .SingleOrDefault(p => p.SystemName.Equals(systemName, StringComparison.InvariantCultureIgnoreCase));
        }

		/// <summary>
		/// Get a plugin descriptor by its system name
		/// </summary>
		/// <typeparam name="T">The type of plugin to get.</typeparam>
		/// <param name="systemName">Plugin system name</param>
		/// <param name="installedOnly">A value indicating whether to load only installed plugins</param>
		/// <returns>>Plugin descriptor</returns>
        public virtual PluginDescriptor GetPluginDescriptorBySystemName<T>(string systemName, bool installedOnly = true) where T : class, IPlugin
        {
            return GetPluginDescriptors<T>(installedOnly)
                .SingleOrDefault(p => p.SystemName.Equals(systemName, StringComparison.InvariantCultureIgnoreCase));
        }
        
        /// <summary>
        /// Reload plugins
        /// </summary>
        public virtual void ReloadPlugins()
        {
            _arePluginsLoaded = false;
            EnsurePluginsAreLoaded();
        }
    }
}
