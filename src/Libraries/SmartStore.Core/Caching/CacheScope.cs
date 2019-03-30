﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace SmartStore.Core.Caching
{
	public class CacheScope
	{
		private HashSet<string> _dependencies;

		public CacheScope(string key)
		{
			Key = key;
		}

		public string Key { get; private set; }

		public void AddDependency(string key)
		{
			if (_dependencies == null)
			{
				_dependencies = new HashSet<string>();
			}

			_dependencies.Add(key);
		}

		public IEnumerable<string> Dependencies
		{
			get
			{
				return _dependencies ?? Enumerable.Empty<string>();
			}
		}
	}
}
