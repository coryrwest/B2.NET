using System;
using System.Diagnostics.CodeAnalysis;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using B2Net.Http;
using B2Net.Http.RequestGenerators;
using B2Net.Models;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;

namespace B2Net {
	public class B2BaseRequestFactory {
		private readonly B2TokenHandler _tokenHandler;
		private readonly HttpClient _client;
		private readonly B2Config _b2Config;
		private readonly LockedCache _lockedCache;
		private static readonly SemaphoreSlim _requestLock = new SemaphoreSlim(1, 1);

		public B2BaseRequestFactory(B2TokenHandler tokenHandler, IOptions<B2Config> b2Config, HttpClient client,
			IMemoryCache cache) {
			_tokenHandler = tokenHandler;
			_client = client;
			_b2Config = b2Config?.Value ?? throw new ArgumentNullException(nameof(b2Config));
			_lockedCache = new LockedCache(_requestLock, cache);
		}

		public async Task<HttpRequestMessage> PostRequestJson(string endpoint, string body) {
			var authInfo = await _tokenHandler.RefreshTokenAsync();
			var requestMessage = BaseRequestGenerator.Request(authInfo.authorizationToken)
				.ToEndpoint(HttpMethod.Post, authInfo.apiUrl, endpoint).WithJson(body);

			return requestMessage;
		}

		public string DetermineBucketId(string bucketId) {
			return Utilities.DetermineBucketId(_b2Config, bucketId);
		}

		public async Task<B2Options> GetOptions() {
			var authInfo = await _tokenHandler.RefreshTokenAsync();
			var options = new B2Options();
			options.SetState(authInfo);

			return options;
		}

		/// <summary>
		/// If using multiple threads to upload, give each thread a name and pass that name to the uploadQueueName.
		/// </summary>
		public async Task<B2UploadUrl> GetUploadUrl(string bucketId = "", string uploadQueueName = null,
			CancellationToken cancelToken = default(CancellationToken)) {
			var operationalBucketId = DetermineBucketId(bucketId);
			var itemKey = string.IsNullOrEmpty(uploadQueueName)
				? $"{operationalBucketId}:B2UploadUrl"
				: $"{operationalBucketId}:B2UploadUrl:{uploadQueueName}";
			var uploadUrl = await _lockedCache.GetOrUpdate<B2UploadUrl>(itemKey, TimeSpan.FromSeconds(90),
				TimeSpan.FromDays(0.95),
				() => SendGetUploadUrl(_client, operationalBucketId, cancelToken), cancelToken);

			return uploadUrl;
		}

		/// <summary>
		/// An uploadUrl and upload authorizationToken are valid for 24 hours or until the endpoint rejects an upload,
		/// see b2_upload_file. You can upload as many files to this URL as you need. To achieve faster upload speeds,
		/// request multiple uploadUrls and upload your files to these different endpoints in parallel.
		/// </summary>
		private async Task<B2UploadUrl> SendGetUploadUrl(HttpClient client, string bucketId,
			CancellationToken cancelToken = default(CancellationToken)) {
			var start = await StartEndpointRequest(FileUploadRequestGenerators.ToFileUploadEndpoint, cancelToken);
			var uploadUrlRequest = start.WithGetUploadUrlBody(bucketId);
			// send the request.
			var uploadUrlResponse = await client.SendAsync(uploadUrlRequest, cancelToken);

			// parse response and return it.
			var uploadUrl = await ResponseParser.ParseResponse<B2UploadUrl>(uploadUrlResponse);

			return uploadUrl;
		}

		private async Task<HttpRequestMessage> StartEndpointRequest(
			Func<HttpRequestMessage, string, HttpRequestMessage> endpointConfig,
			CancellationToken cancelToken) {
			var (authInfo, startRequest) = await StartRequest(cancelToken);
			startRequest = endpointConfig(startRequest, authInfo.apiUrl);

			return startRequest;
		}

		public async Task<(B2AuthResponse authInfo, HttpRequestMessage startRequest)> StartRequest(
			CancellationToken cancelToken = default(CancellationToken)) {
			var authInfo = await _tokenHandler.RefreshTokenAsync(cancelToken);
			var startRequest = BaseRequestGenerator.Request(authInfo.authorizationToken)
				.ToFileUploadEndpoint(authInfo.apiUrl);

			return (authInfo, startRequest);
		}
	}
}
