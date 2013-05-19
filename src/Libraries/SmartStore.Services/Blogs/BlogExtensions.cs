using System;
using System.Collections.Generic;
using System.Linq;
using SmartStore.Core.Domain.Blogs;

namespace SmartStore.Services.Blogs
{
    /// <summary>
    /// Extensions
    /// </summary>
    public static class BlogExtensions
    {
        /// <summary>
        /// Returns all posts published between the two dates.
        /// </summary>
        /// <param name="source">Source</param>
        /// <param name="dateFrom">Date from</param>
        /// <param name="dateTo">Date to</param>
        /// <returns>Filtered posts</returns>
        public static IList<BlogPost> GetPostsByDate(this IList<BlogPost> source,
            DateTime dateFrom, DateTime dateTo)
        {
            var list = source.ToList().FindAll(delegate(BlogPost p)
            {
                return (dateFrom.Date <= p.CreatedOnUtc && p.CreatedOnUtc.Date <= dateTo);
            });

            list.TrimExcess();
            return list;
        }
    }
}
