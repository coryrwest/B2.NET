using System;
using B2Net.Http;
using B2Net.Models;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;

namespace B2Net {
	public class B2Client : IB2Client {
		private B2Capabilities _capabilities { get; set; }
		private readonly HttpClient _client;
		private readonly B2BaseRequestFactory _requestFactory;
		private B2Options _options;

		public B2Capabilities Capabilities {
			get {
				if (_options.Authenticated) {
					return _capabilities;
				}
				else {
					throw new NotAuthorizedException(
						"You attempted to load the cabapilities of this key before authenticating with Backblaze. You must Authorize before you can access Capabilities.");
				}
			}
		}

		/// <summary>
		/// If you specify authorizeOnInitialize = false, you MUST call Initialize() once before you use the client.
		/// </summary>
		public B2Client(B2BaseRequestFactory requestFactory, HttpClient client, bool authorizeOnInitialize = true) {
			_client = client;

			Buckets = new Buckets(requestFactory, _client);
			Files = new Files(requestFactory, _client);
			LargeFiles = new LargeFiles(requestFactory, _client);
			if (authorizeOnInitialize) {
				Initialize().RunSynchronously();
			}
		}

		/// <summary>
		/// Simple method for instantiating the B2Client. Does auth for you. See https://www.backblaze.com/b2/docs/application_keys.html for details on application keys.
		/// This method defaults to not persisting a bucket. Manually build the options object if you wish to do that.
		/// </summary>
		public B2Client(string keyId, string applicationKey, HttpClient client, int requestTimeout = 100) {
			_client = client;
			var config = new B2Config(keyId: keyId, applicationKey: applicationKey, requestTimeout: requestTimeout);
			var memoryCache = new MemoryCache(Options.Create(new MemoryCacheOptions()));
			var tokenHandler = new B2TokenHandler(Options.Create(config), client,
				memoryCache);
			_requestFactory = new B2BaseRequestFactory(tokenHandler, Options.Create(config), client, memoryCache);

			Buckets = new Buckets(_requestFactory, _client);
			Files = new Files(_requestFactory, _client);
			LargeFiles = new LargeFiles(_requestFactory, _client);
		}


		public IBuckets Buckets { get; private set; }
		public IFiles Files { get; private set; }
		public ILargeFiles LargeFiles { get; private set; }

		/// <summary>
		/// Only call this method if you created a B2Client with authorizeOnInitalize = false. This method of using B2.NET is considered in Beta, as it has not been extensively tested.
		/// </summary>
		/// <returns></returns>
		public async Task Initialize() {
			_options = await _requestFactory.GetOptions();
			_capabilities = _options.Capabilities;
		}

		/// <summary>
		/// Authorize against the B2 storage service. Requires that KeyId and ApplicationKey on the options object be set.
		/// </summary>
		/// <returns>B2Options containing the download url, new api url, AccountID and authorization token.</returns>
		public async Task<B2Options> Authorize(CancellationToken cancelToken = default(CancellationToken)) {
			return await AuthorizeAsync(_options, _client);
		}

		public static async Task<B2Options> AuthorizeAsync(string keyId, string applicationkey, HttpClient client) {
			return await AuthorizeAsync(new B2Options() { ApplicationKey = applicationkey, KeyId = keyId }, client);
		}

		public static B2Options Authorize(string keyId, string applicationkey, HttpClient client) {
			return Authorize(new B2Options() { ApplicationKey = applicationkey, KeyId = keyId }, client);
		}

		/// <summary>
		/// Requires that KeyId and ApplicationKey on the options object be set. If you are using an application key you must specify the accountId, the keyId, and the applicationKey.
		/// </summary>
		public static async Task<B2Options> AuthorizeAsync(B2Options options, HttpClient client) {
			// Return if already authenticated.
			if (options.Authenticated) {
				return options;
			}

			var authRequest =
				await B2TokenHandler.DoAuthRequest(client, options.KeyId, options.ApplicationKey,
					CancellationToken.None);
			var outputOptions = new B2Options();
			outputOptions.SetState(authRequest);

			return outputOptions;
		}

		/// <summary>
		/// Requires that KeyId and ApplicationKey on the options object be set. If you are using an application key you must specify the accountId, the keyId, and the applicationKey.
		/// </summary>
		public static B2Options Authorize(B2Options options, HttpClient client) {
			return AuthorizeAsync(options, client).Result;
		}

		public static B2Options Authorize(B2Config config, HttpClient client) {
			return AuthorizeAsync(config.KeyId, config.ApplicationKey, client).Result;
		}
	}
}
