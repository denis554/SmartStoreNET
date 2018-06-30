﻿using System;

namespace SmartStore.Core.Plugins
{
	public interface IProvider
	{
	}

	public sealed class Provider<TProvider> where TProvider : IProvider
	{
		private readonly Lazy<TProvider, ProviderMetadata> _lazy;

		public Provider(Lazy<TProvider, ProviderMetadata> lazy) 
		{
			this._lazy = lazy;
		}

		public TProvider Value
		{
			get { return _lazy.Value; }
		}

		public ProviderMetadata Metadata
		{
			get { return _lazy.Metadata; }
		}

		public bool IsValueCreated
		{
			get { return _lazy.IsValueCreated; }
		}

		public Lazy<TProvider, ProviderMetadata> ToLazy()
		{
			return _lazy;
		}

		public override string ToString()
		{
			return _lazy.Metadata.SystemName;
		}
	}
}
