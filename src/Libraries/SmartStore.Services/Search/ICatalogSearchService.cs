﻿using SmartStore.Services.Catalog;

namespace SmartStore.Services.Search
{
	public partial interface ICatalogSearchService
	{
		/// <summary>
		/// Searches for products
		/// </summary>
		/// <param name="searchQuery">Search term, filters and other parameters used for searching</param>
		/// <param name="loadFlags">Which product navigation properties to eager load</param>
		/// <returns>Catalog search result</returns>
		CatalogSearchResult Search(CatalogSearchQuery searchQuery, ProductLoadFlags loadFlags = ProductLoadFlags.None);
	}
}
