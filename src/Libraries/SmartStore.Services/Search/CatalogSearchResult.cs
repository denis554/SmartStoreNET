﻿using System.Collections.Generic;
using SmartStore.Core;
using SmartStore.Core.Domain.Catalog;
using SmartStore.Core.Search;

namespace SmartStore.Services.Search
{
	public partial class CatalogSearchResult
	{
		public CatalogSearchResult(
			ISearchEngine engine,
			IPagedList<Product> hits,
			CatalogSearchQuery query,
			string[] spellCheckerSuggestions,
			Dictionary<int, string> topCategories,
			Dictionary<int, string> topManufacturers)
		{
			Guard.NotNull(hits, nameof(hits));
			Guard.NotNull(query, nameof(query));

			Engine = engine;
			Hits = hits;
			Query = query;
			SpellCheckerSuggestions = spellCheckerSuggestions ?? new string[0];
			TopCategories = topCategories ?? new Dictionary<int, string>();
			TopManufacturers = topManufacturers ?? new Dictionary<int, string>();
		}

		/// <summary>
		/// Products found
		/// </summary>
		public IPagedList<Product> Hits
		{
			get;
			private set;
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

		/// <summary>
		/// Gets top categories
		/// </summary>
		public Dictionary<int, string> TopCategories
		{
			get;
			private set;
		}

		/// <summary>
		/// Gets top manufacturers
		/// </summary>
		public Dictionary<int, string> TopManufacturers
		{
			get;
			private set;
		}

		public ISearchEngine Engine
		{
			get;
			private set;
		}

		/// <summary>
		/// Highlights chosen terms in a text, extracting the most relevant sections
		/// </summary>
		/// <param name="input">Text to highlight terms in</param>
		/// <returns>Highlighted text fragments </returns>
		public string Highlight(string input, string preMatch = "<strong>", string postMatch = "</strong>")
		{
			if (Query?.Term == null || input.IsEmpty())
				return input;

			string hilite = null;

			if (Engine != null)
			{
				try
				{
					hilite = Engine.Highlight(input, preMatch, postMatch);
				}
				catch { }
			}

			if (hilite.HasValue())
			{
				return hilite;
			}

			return input.HighlightKeywords(Query.Term, preMatch, postMatch);
		}
	}
}
