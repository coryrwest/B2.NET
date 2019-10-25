using System;
using B2Net.Http;
using B2Net.Models;
using Newtonsoft.Json;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace B2Net {
	public class B2Client : IB2Client {
		private B2Options _options;
		private B2Capabilities _capabilities { get; set; }

		public B2Capabilities Capabilities {
			get {
				if (_options.Authenticated) {
					return _capabilities;
				}
				else {
					throw new NotAuthorizedException("You attempted to load the cabapilities of this key before authenticating with Backblaze. You must Authorize before you can access Capabilities.");
				}
			}
		}

		public B2Client(B2Options options) {
			_options = Authorize(options);
			Buckets = new Buckets(options);
			Files = new Files(options);
			LargeFiles = new LargeFiles(options);
			_capabilities = options.Capabilities;
		}

		/// <summary>
		/// Simple method for instantiating the B2Client. Does auth for you. See https://www.backblaze.com/b2/docs/application_keys.html for details on application keys.
		/// This method defaults to not persisting a bucket. Manually build the options object if you wish to do that.
		/// </summary>
		/// <param name="accountId"></param>
		/// <param name="applicationkey"></param>
		/// <param name="requestTimeout"></param>
		public B2Client(string keyId, string applicationkey, int requestTimeout = 100) {
			_options = new B2Options() {
				KeyId = keyId,
				ApplicationKey = applicationkey,
				RequestTimeout = requestTimeout
			};
			_options = Authorize(_options);

			Buckets = new Buckets(_options);
			Files = new Files(_options);
			LargeFiles = new LargeFiles(_options);
			_capabilities = _options.Capabilities;
		}
		
		/// <summary>
		/// Simple method for instantiating the B2Client. Does auth for you. See https://www.backblaze.com/b2/docs/application_keys.html for details on application keys.
		/// This method defaults to not persisting a bucket. Manually build the options object if you wish to do that.
		/// </summary>
		/// <param name="accountId"></param>
		/// <param name="applicationkey"></param>
		/// <param name="requestTimeout"></param>
		[Obsolete("Use B2Client(string keyId, string applicationkey, int requestTimeout = 100) instead as AccountId is no longer needed")]
		public B2Client(string accountId, string applicationkey, string keyId, int requestTimeout = 100) :this(keyId, applicationkey, requestTimeout)
		{
		}
		
		public IBuckets Buckets { get; }
		public IFiles Files { get; }
		public ILargeFiles LargeFiles { get; }

		/// <summary>
		/// Authorize against the B2 storage service. Requires that KeyId and ApplicationKey on the options object be set.
		/// </summary>
		/// <returns>B2Options containing the download url, new api url, AccountID and authorization token.</returns>
		public async Task<B2Options> Authorize(CancellationToken cancelToken = default(CancellationToken)) {
			return Authorize(_options);
		}

		public static B2Options Authorize(string keyId, string applicationkey) {
			return Authorize(new B2Options() { ApplicationKey = applicationkey, KeyId = keyId });
		}

		/// <summary>
		/// Requires that AccountId and ApplicationKey on the options object be set. If you are using an application key you must specify the accountId, the keyId, and the applicationKey.
		/// </summary>
		/// <param name="options"></param>
		/// <returns></returns>
		public static B2Options Authorize(B2Options options) {
			// Return if already authenticated.
			if (options.Authenticated) {
				return options;
			}

			var client = HttpClientFactory.CreateHttpClient(options.RequestTimeout);
			
			var requestMessage = AuthRequestGenerator.Authorize(options);
			var response = client.SendAsync(requestMessage).Result;

			var jsonResponse = response.Content.ReadAsStringAsync().Result;
			if (response.IsSuccessStatusCode) {
				var authResponse = JsonConvert.DeserializeObject<B2AuthResponse>(jsonResponse);

				options.SetState(authResponse);
			} else if (response.StatusCode == HttpStatusCode.Unauthorized) {
				// Return a better exception because of confusing Keys api.
				throw new AuthorizationException("If you are using an Application key and not a Master key, make sure that you are supplying the Key ID and Key Value for that Application Key. Do not mix your Account ID with your Application Key.");
			} else {
				throw new AuthorizationException(jsonResponse);
			}

			return options;
		}
	}
}
