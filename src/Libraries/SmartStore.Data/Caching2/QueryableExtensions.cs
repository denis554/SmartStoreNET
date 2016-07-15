﻿using System;
using System.Data.Entity.Core.Objects;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Reflection;

namespace SmartStore.Data.Caching2
{
	public static class QueryableExtensions
	{
		/// <summary>
		/// Forces query results to be cached.
		/// Allows caching results for queries using non-deterministic functions. 
		/// </summary>
		/// <typeparam name="T">Query element type.</typeparam>
		/// <param name="source">Query whose results will be cached. Must not be null.</param>
		public static IQueryable<T> FromCache<T>(this IQueryable<T> source, TimeSpan? duration = null)
			where T : class
		{
			// TODO: Implement expiration
			// TODO: Implement minCacheableRows and maxCacheableRows
			Guard.NotNull(source, nameof(source));

			var objectQuery = TryGetObjectQuery(source) ?? source as ObjectQuery;

			if (objectQuery != null)
			{
				SingletonQueries.Current.AddCachedQuery(
					objectQuery.Context.MetadataWorkspace, 
					objectQuery.ToTraceString());
			}

			return source;
		}

		public static IQueryable<T> FromRequestCache<T>(this IQueryable<T> source)
			where T : class
		{
			// TODO: implement
			throw new NotImplementedException();
		}

		private static ObjectQuery TryGetObjectQuery<T>(IQueryable<T> source)
		{
			var dbQuery = source as DbQuery<T>;

			if (dbQuery != null)
			{
				const BindingFlags privateFieldFlags =
					BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public;

				var internalQuery =
					source.GetType().GetProperty("InternalQuery", privateFieldFlags)
						.GetValue(source);

				return
					(ObjectQuery)internalQuery.GetType().GetProperty("ObjectQuery", privateFieldFlags)
						.GetValue(internalQuery);
			}

			return null;
		}
	}
}
