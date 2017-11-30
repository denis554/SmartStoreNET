﻿using System;
using System.Collections.Generic;
using DotLiquid;

namespace SmartStore.Templating.Liquid
{
	internal interface ISafeObject
	{
	}

	internal abstract class SafeDropBase : ILiquidizable, IIndexable, IContextAware, ISafeObject
	{
		private readonly IDictionary<string, object> _safeObjects = new Dictionary<string, object>();

		protected object GetOrCreateSafeObject(string name)
		{
			if (!_safeObjects.TryGetValue(name, out var safeObject))
			{
				safeObject = LiquidUtil.CreateSafeObject(InvokeMember(name));
				if (safeObject is ISafeObject)
				{
					_safeObjects[name] = safeObject;
				}
			}

			return safeObject;
		}

		public Context Context { get;  set; }

		public abstract bool ContainsKey(object key);

		public object this[object key]
		{
			get
			{
				return (key is string s) ? GetOrCreateSafeObject(s) : null;
			}
		}

		protected abstract object InvokeMember(string name);

		public object ToLiquid()
		{
			return this;
		}
	}
}
