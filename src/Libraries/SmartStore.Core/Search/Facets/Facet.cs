﻿namespace SmartStore.Core.Search.Facets
{
	public class Facet
	{
		public Facet(string key, string value, long hitCount)
		{
			Guard.NotEmpty(key, nameof(key));
			Guard.NotNull(value, nameof(value));

			Key = key;
			Value = value;
			HitCount = hitCount;
		}

		public string Key
		{
			get;
			private set;
		}

		public string Value
		{
			get;
			private set;
		}

		public long HitCount
		{
			get;
			private set;
		}

		public FacetGroup FacetGroup
		{
			get;
			internal set;
		}
	}
}
