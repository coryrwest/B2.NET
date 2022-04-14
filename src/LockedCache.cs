using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using B2Net.Models;
using Microsoft.Extensions.Caching.Memory;

namespace B2Net {
	internal class LockedCache {
		private readonly SemaphoreSlim _lock;
		private readonly IMemoryCache _cache;

		public LockedCache(SemaphoreSlim @lock, IMemoryCache cache) {
			_lock = @lock;
			_cache = cache;
		}

		public async Task<T> GetOrUpdate<T>(string cacheKey, TimeSpan wait, TimeSpan expiry,
			Func<Task<T>> func, CancellationToken cancelToken) {
			if (_cache.TryGetValue(cacheKey, out T value)) {
				return value;
			}

			if (!await _lock.WaitAsync(wait.Add(TimeSpan.FromSeconds(1)), cancelToken)) {
				throw new Exception("Failed to acquire token lock");
			}

			try {
				if (_cache.TryGetValue(cacheKey, out T value2)) {
					return value2;
				}

				var response = await func();

				_cache.Set(cacheKey, response, expiry);

				return response;
			}
			finally {
				_lock.Release();
			}
		}
	}
}
