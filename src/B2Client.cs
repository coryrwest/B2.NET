using System.Threading;
using System.Threading.Tasks;
using B2Net.Http;
using B2Net.Models;
using Newtonsoft.Json;

namespace B2Net {
	public class B2Client {
	    private B2Options _options;

		public B2Client(B2Options options) {
			_options = options;
			Buckets = new Buckets(options);
			Files = new Files(options);
            LargeFiles = new LargeFiles(options);
		}

        /// <summary>
        /// Simple method for instantiating the B2Client. Does auth for you.
        /// </summary>
        /// <param name="accountId"></param>
        /// <param name="applicationkey"></param>
        /// <param name="requestTimeout"></param>
	    public B2Client(string accountId, string applicationkey, int requestTimeout = 100) {
	        _options = new B2Options() {
	            AccountId = accountId,
                ApplicationKey = applicationkey,
                RequestTimeout = requestTimeout
	        };
            _options = Authorize(_options);

            Buckets = new Buckets(_options);
	        Files = new Files(_options);
	        LargeFiles = new LargeFiles(_options);
        }

		public IBuckets Buckets { get; }
	    public IFiles Files { get; }
	    public ILargeFiles LargeFiles { get; }

        /// <summary>
        /// Authorize against the B2 storage service. Requires that AccountId and ApplicationKey on the options object be set.
        /// </summary>
        /// <returns>B2Options containing the download url, new api url, and authorization token.</returns>
        public async Task<B2Options> Authorize(CancellationToken cancelToken = default(CancellationToken)) {
			var client = HttpClientFactory.CreateHttpClient(_options.RequestTimeout);

			var requestMessage = AuthRequestGenerator.Authorize(_options);
			var response = await client.SendAsync(requestMessage, cancelToken);

			var jsonResponse = await response.Content.ReadAsStringAsync();
			if (response.IsSuccessStatusCode) {
				var authResponse = JsonConvert.DeserializeObject<B2AuthResponse>(jsonResponse);
				
				_options.SetState(authResponse);
				
				return _options;
			} else {
				throw new AuthorizationException(jsonResponse);
			}
		}

	    public static B2Options Authorize(string accountId, string applicationkey) {
	        return Authorize(new B2Options() {AccountId = accountId, ApplicationKey = applicationkey});
	    }

        /// <summary>
        /// Requires that AccountId and ApplicationKey on the options object be set.
        /// </summary>
        /// <param name="options"></param>
        /// <returns></returns>
	    public static B2Options Authorize(B2Options options) {
            var client = HttpClientFactory.CreateHttpClient(options.RequestTimeout);

            var requestMessage = AuthRequestGenerator.Authorize(options);
            var response = client.SendAsync(requestMessage).Result;

            var jsonResponse = response.Content.ReadAsStringAsync().Result;
            if (response.IsSuccessStatusCode) {
                var authResponse = JsonConvert.DeserializeObject<B2AuthResponse>(jsonResponse);

                options.SetState(authResponse);
            } else {
                throw new AuthorizationException(jsonResponse);
            }

	        return options;
        }
	}
}
