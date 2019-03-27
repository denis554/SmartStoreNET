using System;
using System.Linq;
using System.Runtime.Caching;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using System.Threading;
using SmartStore.Core.Async;
using System.Collections;
using System.Collections.Generic;
using SmartStore.Utilities;
using System.Text.RegularExpressions;

namespace SmartStore.Core.Caching
{
    public partial class MemoryCacheManager : DisposableObject, ICacheManager
    {
		// Wwe put a special string into cache if value is null,
		// otherwise our 'Contains()' would always return false,
		// which is bad if we intentionally wanted to save NULL values.
		public const string FakeNull = "__[NULL]__";

		private readonly MemoryCache _cache;
		private readonly ConcurrentDictionary<string, SemaphoreSlim> _keyLocks = new ConcurrentDictionary<string, SemaphoreSlim>();

		private string _currentScope;

		public MemoryCacheManager()
		{
			_cache = new MemoryCache("SmartStore");
		}

		public bool IsDistributedCache
		{
			get { return false; }
		}

		private bool TryGet<T>(string key, out T value)
		{
			value = default(T);

			object obj = _cache.Get(key);

			if (obj != null)
			{
				if (obj.Equals(FakeNull))
				{
					return true;
				}

				value = (T)obj;
				return true;
			}

			return false;
		}

		public T Get<T>(string key)
		{
			using (CacheAcquireContext.Begin(key))
			{
				TryGet(key, out T value);
				return value;
			}
		}

        public T Get<T>(string key, Func<T> acquirer, TimeSpan? duration = null)
        {
			using (CacheAcquireContext.Begin(key))
			{
				if (TryGet(key, out T value))
				{
					return value;
				}

				lock (String.Intern(key))
				{
					// Atomic operation must be outer locked
					if (!TryGet(key, out value))
					{
						value = acquirer();
						Put(key, value, duration, CacheAcquireContext.Current.DependentKeys);
						return value;
					}
				}

				return value;
			}
		}

		public async Task<T> GetAsync<T>(string key, Func<Task<T>> acquirer, TimeSpan? duration = null)
		{
			using (CacheAcquireContext.Begin(key))
			{
				if (TryGet(key, out T value))
				{
					return value;
				}

				// get the async (semaphore) locker specific to this key
				var keyLock = AsyncLock.Acquire(key);

				using (await keyLock.LockAsync())
				{
					if (!TryGet(key, out value))
					{
						value = await acquirer().ConfigureAwait(false);
						Put(key, value, duration, CacheAcquireContext.Current.DependentKeys);
						return value;
					}
				}

				return value;
			}
		}

		public void Put(string key, object value, TimeSpan? duration = null, IEnumerable<string> dependencies = null)
		{
			_cache.Set(key, value ?? FakeNull, GetCacheItemPolicy(duration, dependencies));
		}

        public bool Contains(string key)
        {
			return _cache.Contains(key);
        }

        public void Remove(string key)
        {
			_cache.Remove(key);
        }

		public IEnumerable<string> Keys(string pattern)
		{
			Guard.NotEmpty(pattern, nameof(pattern));

			var keys = _cache.AsParallel().Select(x => x.Key);

			if (pattern.IsEmpty() || pattern == "*")
			{
				return keys.ToArray();
			}

			var wildcard = new Wildcard(pattern, RegexOptions.IgnoreCase);
			return keys.Where(x => wildcard.IsMatch(x));
		}

		public int RemoveByPattern(string pattern)
        {
            var keysToRemove = Keys(pattern);
			int count = 0;

			lock (_cache)
			{
				// lock atomic operation
				foreach (string key in keysToRemove)
				{
					_cache.Remove(key);
					count++;
				}
			}

			return count;
		}

        public void Clear()
        {
			RemoveByPattern("*");
        }

		public virtual ISet GetHashSet(string key, Func<IEnumerable<string>> acquirer = null)
		{
			var result = Get(key, () => 
			{
				var set = new MemorySet(this);
				var items = acquirer?.Invoke();
				if (items != null)
				{
					set.AddRange(items);
				}

				return set;
			});

			return result;
		}

		private CacheItemPolicy GetCacheItemPolicy(TimeSpan? duration, IEnumerable<string> dependencies)
		{
			var absoluteExpiration = ObjectCache.InfiniteAbsoluteExpiration;

			if (duration.HasValue)
			{
				absoluteExpiration = DateTime.UtcNow + duration.Value;
			}
			
			var cacheItemPolicy = new CacheItemPolicy
			{
				AbsoluteExpiration = absoluteExpiration,
				SlidingExpiration = ObjectCache.NoSlidingExpiration
			};

			if (dependencies != null && dependencies.Any())
			{
				cacheItemPolicy.ChangeMonitors.Add(_cache.CreateCacheEntryChangeMonitor(dependencies));
			}

			return cacheItemPolicy;
		}


		protected override void OnDispose(bool disposing)
		{
			if (disposing)
				_cache.Dispose();
		}
	}
}