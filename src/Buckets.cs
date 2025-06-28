using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using B2Net.Http;
using B2Net.Models;

namespace B2Net {
	public class Buckets : IBuckets {
		private B2Options _options;
		private HttpClient _client;
		private Func<B2Options, B2Options> _authorize;
		private string _api = "Buckets";

		public Buckets(B2Options options, Func<B2Options, B2Options> authorizeFunc) {
			_options = options;
			_authorize = authorizeFunc;
			_client = HttpClientFactory.CreateHttpClient(options.RequestTimeout);
		}

		public async Task<List<B2Bucket>> GetList(CancellationToken cancelToken = default(CancellationToken)) {
			RefreshAuthorization(_options, _authorize);
			var requestMessage = BucketRequestGenerators.GetBucketList(_options);
			var response = await _client.SendAsync(requestMessage, cancelToken);

			var bucketList = await ResponseParser.ParseResponse<B2BucketListDeserializeModel>(response);
			return bucketList.Buckets;
		}

		/// <summary>
		/// Creates a new bucket. A bucket belongs to the account used to create it. If BucketType is not set allPrivate will be used by default.
		/// </summary>
		/// <param name="bucketName"></param>
		/// <param name="bucketType"></param>
		/// <param name="cancelToken"></param>
		/// <returns></returns>
		public async Task<B2Bucket> Create(string bucketName, BucketTypes bucketType,
			CancellationToken cancelToken = default(CancellationToken)) {
			RefreshAuthorization(_options, _authorize);
			var requestMessage = BucketRequestGenerators.CreateBucket(_options, bucketName, bucketType.ToString());
			var response = await _client.SendAsync(requestMessage, cancelToken);

			return await ResponseParser.ParseResponse<B2Bucket>(response, _api);
		}

		/// <summary>
		/// Creates a new bucket. A bucket belongs to the account used to create it. If BucketType is not set allPrivate will be used by default.
		/// Use this method to set Cache-Control.
		/// </summary>
		/// <param name="bucketName"></param>
		/// <param name="bucketType"></param>
		/// <param name="cancelToken"></param>
		/// <returns></returns>
		public async Task<B2Bucket> Create(string bucketName, B2BucketOptions options, CancellationToken cancelToken = default(CancellationToken)) {
			RefreshAuthorization(_options, _authorize);
			var requestMessage = BucketRequestGenerators.CreateBucket(_options, bucketName, options);
			var response = await _client.SendAsync(requestMessage, cancelToken);

			return await ResponseParser.ParseResponse<B2Bucket>(response, _api);
		}

		/// <summary>
		/// Deletes the bucket specified. Only buckets that contain no version of any files can be deleted.
		/// </summary>
		/// <param name="bucketId"></param>
		/// <param name="cancelToken"></param>
		/// <returns></returns>
		public async Task<B2Bucket> Delete(string bucketId = "", CancellationToken cancelToken = default(CancellationToken)) {
			RefreshAuthorization(_options, _authorize);
			var operationalBucketId = Utilities.DetermineBucketId(_options, bucketId);

			var requestMessage = BucketRequestGenerators.DeleteBucket(_options, operationalBucketId);
			var response = await _client.SendAsync(requestMessage, cancelToken);

			return await ResponseParser.ParseResponse<B2Bucket>(response, _api);
		}

		/// <summary>
		/// Update an existing bucket. bucketId is only optional if you are persisting a bucket for this client.
		/// </summary>
		/// <param name="bucketType"></param>
		/// <param name="bucketId"></param>
		/// <param name="cancelToken"></param>
		/// <returns></returns>
		public async Task<B2Bucket> Update(BucketTypes bucketType, string bucketId = "", CancellationToken cancelToken = default(CancellationToken)) {
			RefreshAuthorization(_options, _authorize);
			var operationalBucketId = Utilities.DetermineBucketId(_options, bucketId);
			var requestMessage = BucketRequestGenerators.UpdateBucket(_options, operationalBucketId, bucketType.ToString());
			var response = await _client.SendAsync(requestMessage, cancelToken);

			return await ResponseParser.ParseResponse<B2Bucket>(response, _api);
		}

		/// <summary>
		/// Update an existing bucket. bucketId is only optional if you are persisting a bucket for this client.
		/// Use this method to set Cache-Control, Lifecycle Rules, or CORS rules.
		/// </summary>
		/// <param name="bucketType"></param>
		/// <param name="bucketId"></param>
		/// <param name="cancelToken"></param>
		/// <returns></returns>
		public async Task<B2Bucket> Update(B2BucketOptions options, string bucketId = "", CancellationToken cancelToken = default(CancellationToken)) {
			RefreshAuthorization(_options, _authorize);
			var operationalBucketId = Utilities.DetermineBucketId(_options, bucketId);

			var requestMessage = BucketRequestGenerators.UpdateBucket(_options, operationalBucketId, options);
			var response = await _client.SendAsync(requestMessage, cancelToken);

			return await ResponseParser.ParseResponse<B2Bucket>(response, _api);
		}

		/// <summary>
		/// Update an existing bucket. bucketId is only optional if you are persisting a bucket for this client.
		/// Use this method to set Cache-Control, Lifecycle Rules, or CORS rules.
		/// </summary>
		/// <param name="bucketType"></param>
		/// <param name="bucketId"></param>
		/// <param name="cancelToken"></param>
		/// <returns></returns>
		public async Task<B2Bucket> Update(B2BucketOptions options, int revisionNumber, string bucketId = "", CancellationToken cancelToken = default(CancellationToken)) {
			RefreshAuthorization(_options, _authorize);
			var operationalBucketId = Utilities.DetermineBucketId(_options, bucketId);

			var requestMessage = BucketRequestGenerators.UpdateBucket(_options, operationalBucketId, options, revisionNumber);
			var response = await _client.SendAsync(requestMessage, cancelToken);

			return await ResponseParser.ParseResponse<B2Bucket>(response, _api);
		}

		/// <summary>
		/// Check that the options has a valid authorization token and if it does not, get one.
		/// </summary>
		/// <param name="options"></param>
		/// <param name="authorize"></param>
		/// <returns></returns>
		private void RefreshAuthorization(B2Options options, Func<B2Options, B2Options> authorize) {
			if (!options.AuthTokenNotExpired && !options.NoTokenRefresh) {
				options = authorize(options);
			}
		}
	}
}
