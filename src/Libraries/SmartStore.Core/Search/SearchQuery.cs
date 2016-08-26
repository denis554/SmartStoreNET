﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartStore.Core.Search
{
	public class SearchQuery : SearchQueryBase<SearchQuery>
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="SearchQuery"/> class without a search term being set
		/// </summary>
		public SearchQuery()
			: base((string[])null, null)
		{
		}

		public SearchQuery(string field, string term, bool escape = false, bool isFuzzySearch = false)
			: base(new[] { field }, term, escape, isFuzzySearch)
		{
		}

		public SearchQuery(string[] fields, string term, bool escape = false, bool isFuzzySearch = false)
			: base(fields, term, escape, isFuzzySearch)
		{
		}
	}

	public class SearchQueryBase<TQuery> where TQuery : SearchQuery
	{
		protected SearchQueryBase(string[] fields, string term, bool escape = false, bool isFuzzySearch = false)
		{
			Fields = fields;
			Term = term;
			EscapeTerm = escape;
			IsFuzzySearch = isFuzzySearch;

			Filters = new List<SearchFilter>();
			Sorting = new List<SearchSort>();

			Take = Int16.MaxValue;
		}

		// Search term
		public string[] Fields { get; protected set; }
		public string Term { get; protected set; }
		public bool EscapeTerm { get; protected set; }
		public bool IsFuzzySearch { get; protected set; }

		// Filtering
		public ICollection<SearchFilter> Filters { get; }

		// Paging
		public int Skip { get; protected set; }
		public int Take { get; protected set; }

		// sorting
		public ICollection<SearchSort> Sorting { get; }

		#region Fluent builder

		public TQuery Slice(int skip, int take)
		{
			Guard.NotNegative(skip, nameof(skip));
			Guard.IsPositive(take, nameof(take));

			Skip = skip;
			Take = take;

			return (this as TQuery);
		}

		public TQuery WithFilter(SearchFilter filter)
		{
			Guard.NotNull(filter, nameof(filter));

			Filters.Add(filter);

			return (this as TQuery);
		}

		public TQuery SortBy(SearchSort sort)
		{
			Guard.NotNull(sort, nameof(sort));

			Sorting.Add(sort);

			return (this as TQuery);
		}

		#endregion
	}
}
