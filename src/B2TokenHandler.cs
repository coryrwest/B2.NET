using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using B2Net.Http;
using B2Net.Models;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;

namespace B2Net {
	public class B2TokenHandler {
		private const string CACHE_PREFIX = "B2_TOKEN";
		private readonly HttpClient _client;
		private readonly B2Config _b2Config;

		private readonly LockedCache _lockedCache;

		//private readonly IMemoryCache _cache;
		private static readonly SemaphoreSlim _lock = new SemaphoreSlim(1, 1);

		public B2TokenHandler(
			IOptions<B2Config> b2Config,
			HttpClient client,
			IMemoryCache cache) {
			_b2Config = b2Config.Value;
			_client = client;
			_lockedCache = new LockedCache(_lock, cache);
		}

		public async Task<B2AuthResponse>
			RefreshTokenAsync(CancellationToken cancelToken = default(CancellationToken)) {
			if (string.IsNullOrWhiteSpace(_b2Config.KeyId) || string.IsNullOrWhiteSpace(_b2Config.ApplicationKey)) {
				throw new AuthorizationException("Either KeyId or ApplicationKey were not specified.");
			}

			var cacheKey = $"{CACHE_PREFIX}:{_b2Config.KeyId}";

			var item = await _lockedCache.GetOrUpdate(cacheKey, TimeSpan.FromSeconds(_b2Config.RequestTimeout + 10),
				TimeSpan.FromDays(1),
				async () => await DoAuthRequest(_client, _b2Config.KeyId, _b2Config.ApplicationKey, cancelToken),
				cancelToken);

			return item;
		}

		public static async Task<B2AuthResponse> DoAuthRequest(HttpClient client, string keyId, string applicationKey,
			CancellationToken cancelToken) {
			var requestMessage = AuthRequestGenerator.Authorize(keyId, applicationKey);
			var response = await client.SendAsync(requestMessage, cancelToken);

			var jsonResponse = await response.Content.ReadAsStringAsync();
			if (response.IsSuccessStatusCode) {
				var authResponse = Utilities.Deserialize<B2AuthResponse>(jsonResponse);

				return authResponse;
			}
			else if (response.StatusCode == HttpStatusCode.Unauthorized) {
				// Return a better exception because of confusing Keys api.
				throw new AuthorizationException(
					"If you are using an Application key and not a Master key, make sure that you are supplying the Key ID and Key Value for that Application Key. Do not mix your Account ID with your Application Key.");
			}
			else {
				throw new AuthorizationException(jsonResponse);
			}
		}
	}
}
