﻿using System.Collections.Generic;
using System.Reflection;

namespace SmartStore.Core.Plugins
{
    public interface IPluginFinder
    {
        /// <summary>Gets plugins found in the environment sorted.</summary>
        /// <typeparam name="T">The type of plugin to get.</typeparam>
        /// <returns>An enumeration of plugins.</returns>
        IEnumerable<T> GetPlugins<T>(bool installedOnly = true) where T : class, IPlugin;

        IEnumerable<PluginDescriptor> GetPluginDescriptors(bool installedOnly = true);

        IEnumerable<PluginDescriptor> GetPluginDescriptors<T>(bool installedOnly = true) where T : class, IPlugin;

        // codehint: sm-add
        PluginDescriptor GetPluginDescriptorByAssembly(Assembly assembly, bool installedOnly = true);

        PluginDescriptor GetPluginDescriptorBySystemName(string systemName, bool installedOnly = true);

        PluginDescriptor GetPluginDescriptorBySystemName<T>(string systemName, bool installedOnly = true) where T : class, IPlugin;

        /// <summary>
        /// Reload plugins
        /// </summary>
        void ReloadPlugins();
    }
}
