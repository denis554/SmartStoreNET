﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SmartStore.Core.Caching
{
    
    public partial class RequestCache : ICache
    {
        private const string REGION_NAME = "$$SmartStoreNET$$";
        private readonly HttpContextBase _context;

        public RequestCache(HttpContextBase context)
        {
            this._context = context;
        }

        protected IDictionary GetItems()
        {
            if (_context != null)
                return _context.Items;

            return null;
        }

        public IEnumerable<KeyValuePair<string, object>> Entries
        {
            get
            {
                var items = GetItems();
                if (items == null)
                    yield break;

                var enumerator = items.GetEnumerator();
                while (enumerator.MoveNext())
                {
                    string key = enumerator.Key as string;
                    if (key == null)
                        continue;
                    if (key.StartsWith(REGION_NAME))
                    {
                        yield return new KeyValuePair<string, object>(key.Substring(REGION_NAME.Length), enumerator.Value);
                    }
                }
            }
        }

        public T Get<T>(string key, Func<T> acquirer, int? cacheTime)
        {
            var items = GetItems();
            if (items == null)
                return default(T);
            
            key = BuildKey(key);

            if (items.Contains(key))
            {
                return (T)items[key];
            }
            else
            {
                var value = acquirer();
                if (value != null)
                {
                    items.Add(key, value);
                }

                return value;
            }
        }

        public bool Contains(string key)
        {
            var items = GetItems();
            if (items == null)
                return false;

            return items.Contains(BuildKey(key));
        }

        public void Remove(string key)
        {
            var items = GetItems();
            if (items == null)
                return;

            items.Remove(BuildKey(key));
        }

        private string BuildKey(string key)
        {
            return key.HasValue() ? REGION_NAME + key : null;
        }

    }

}
