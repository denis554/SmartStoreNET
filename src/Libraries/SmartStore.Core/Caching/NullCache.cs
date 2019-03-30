using System;
using SmartStore.Utilities;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SmartStore.Core.Caching
{
    /// <summary>
    /// Represents a null cache
    /// </summary>
    public partial class NullCache : ICacheManager
    {
		private static readonly ICacheManager s_instance = new NullCache();

		public static ICacheManager Instance
		{
			get { return s_instance; }
		}

		public bool IsDistributedCache
		{
			get { return false; }
		}

		public T Get<T>(string key, bool independent = false)
		{
			return default(T);
		}

		public T Get<T>(string key, Func<T> acquirer, TimeSpan? duration = null, bool independent = false)
		{
			if (acquirer == null)
			{
				return default(T);
			}
			return acquirer();
		}

		public Task<T> GetAsync<T>(string key, Func<Task<T>> acquirer, TimeSpan? duration = null, bool independent = false)
		{
			if (acquirer == null)
			{
				return Task.FromResult(default(T));
			}
			return acquirer();
		}


		public ISet GetHashSet(string key, Func<IEnumerable<string>> acquirer = null)
		{
			return new MemorySet(this);
		}

		public void Put(string key, object value, TimeSpan? duration = null, IEnumerable<string> dependencies = null)
		{
		}

        public bool Contains(string key)
        {
            return false;
        }

        public void Remove(string key)
        {
        }

		public IEnumerable<string> Keys(string pattern)
		{
			return new string[0];
		}

		public int RemoveByPattern(string pattern)
        {
			return 0;
        }

        public void Clear()
        {
        }
	}
}