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

		public Buckets Buckets { get; }
	    public Files Files { get; }
	    public LargeFiles LargeFiles { get; }

        /// <summary>
        /// Authorize against the B2 storage service.
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
	}
}
