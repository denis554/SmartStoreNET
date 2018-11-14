﻿using System;
using System.Collections.Generic;
using System.Linq;
using SmartStore.Core;
using SmartStore.Core.Domain.Catalog;
using SmartStore.Core.Search;
using SmartStore.Core.Search.Facets;

namespace SmartStore.Services.Search
{
    public partial class CatalogSearchResult
	{
		private readonly int _totalHitsCount;
		private readonly Func<IList<Product>> _hitsFactory;
		private IPagedList<Product> _hits;
		private bool? _isSubPage;

		public CatalogSearchResult(
			ISearchEngine engine,
			CatalogSearchQuery query,
			int totalHitsCount,
			Func<IList<Product>> hitsFactory,
			string[] spellCheckerSuggestions,
			IDictionary<string, FacetGroup> facets)
		{
			Guard.NotNull(query, nameof(query));

			Engine = engine;
			Query = query;
			SpellCheckerSuggestions = spellCheckerSuggestions ?? new string[0];
			Facets = facets ?? new Dictionary<string, FacetGroup>();

			_hitsFactory = hitsFactory ?? (() => new List<Product>());
			_totalHitsCount = totalHitsCount;
		}

		/// <summary>
		/// Constructor for an instance without any search hits
		/// </summary>
		/// <param name="query">Catalog search query</param>
		public CatalogSearchResult(CatalogSearchQuery query)
			: this(null, query, 0, () => new List<Product>(), null, null)
		{
		}

		/// <summary>
		/// Products found
		/// </summary>
		public IPagedList<Product> Hits
		{
			get
			{
				if (_hits == null)
				{
					var products = _totalHitsCount == 0 
						? new List<Product>() 
						: _hitsFactory.Invoke();

					_hits = new PagedList<Product>(products, Query.PageIndex, Query.Take, _totalHitsCount);
				}

				return _hits;
			}
		}

		public int TotalHitsCount
		{
			get { return _totalHitsCount; }
		}

		/// <summary>
		/// The original catalog search query
		/// </summary>
		public CatalogSearchQuery Query
		{
			get;
			private set;
		}

		/// <summary>
		/// Gets spell checking suggestions/corrections
		/// </summary>
		public string[] SpellCheckerSuggestions
		{
			get;
			set;
		}

		public IDictionary<string, FacetGroup> Facets
		{
			get;
			private set;
		}

		public ISearchEngine Engine
		{
			get;
			private set;
		}
	}
}
