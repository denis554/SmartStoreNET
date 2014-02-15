﻿using System;
using System.Threading;
using System.Web;
using Autofac;
using Autofac.Integration.Mvc;

namespace SmartStore.Core.Infrastructure.DependencyManagement
{
    /// <summary>
    /// An <see cref="IHttpModule"/> and <see cref="ILifetimeScopeProvider"/> implementation 
    /// that creates a nested lifetime scope for each HTTP request.
    /// </summary>
    public class AutofacRequestLifetimeHttpModule : IHttpModule
	{
		#region New

		public void Init(HttpApplication context)
		{
			Guard.ArgumentNotNull(() => context);

			//context.EndRequest += OnEndRequest;
			// IMPORTANT: call this manually in Global.asax
		}

		public static void OnEndRequest(object sender, EventArgs e)
		{
			if (LifetimeScopeProvider != null)
			{
				LifetimeScopeProvider.EndLifetimeScope();
			}
		}

		public static void SetLifetimeScopeProvider(ILifetimeScopeProvider lifetimeScopeProvider)
		{
			if (lifetimeScopeProvider == null)
			{
				throw new ArgumentNullException("lifetimeScopeProvider");
			}
			LifetimeScopeProvider = lifetimeScopeProvider;
		}


		internal static ILifetimeScopeProvider LifetimeScopeProvider
		{
			get;
			set;
		}

		public void Dispose()
		{
		}

		#endregion

		#region OBSOLETE

		//private static readonly ThreadLocal<ILifetimeScope> _lifetimeScope = new ThreadLocal<ILifetimeScope>(false);

		///// <summary>
		///// Gets a nested lifetime scope that services can be resolved from.
		///// </summary>
		///// <param name="container">The parent container.</param>
		///// <param name="configurationAction">Action on a <see cref="ContainerBuilder"/>
		///// that adds component registations visible only in nested lifetime scopes.</param>
		///// <returns>A new or existing nested lifetime scope.</returns>
		//public static ILifetimeScope GetLifetimeScope(ILifetimeScope container, Action<ContainerBuilder> configurationAction)
		//{
		//	return LifetimeScope ?? (LifetimeScope = InitializeLifetimeScope(configurationAction, container));
			
		//	////little hack here to get dependencies when HttpContext is not available
		//	//if (HttpContext.Current != null)
		//	//{
		//	//	return LifetimeScope ?? (LifetimeScope = InitializeLifetimeScope(configurationAction, container));
		//	//}
		//	//else
		//	//{
		//	//	//throw new InvalidOperationException("HttpContextNotAvailable");
		//	//	return InitializeLifetimeScope(configurationAction, container);
		//	//}
		//}

		//static ILifetimeScope LifetimeScope
		//{
		//	get 
		//	{
		//		if (HttpContext.Current != null)
		//		{
		//			return (ILifetimeScope)HttpContext.Current.Items[typeof(ILifetimeScope)]; 
		//		}
		//		else
		//		{
		//			return _lifetimeScope.Value;
		//		}
		//	}
		//	set 
		//	{
		//		if (HttpContext.Current != null)
		//		{
		//			HttpContext.Current.Items[typeof(ILifetimeScope)] = value; 
		//		}
		//		else
		//		{
		//			_lifetimeScope.Value = value;
		//		}
		//	}
		//}

		#endregion
	}
}
