using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using B2Net.Http;
using B2Net.Models;

namespace B2Net {
	public class Buckets : IBuckets {
		private readonly B2BaseRequestFactory _requestFactory;
		private readonly HttpClient _client;
		private string _api = "Buckets";

		public Buckets(B2BaseRequestFactory requestFactory, HttpClient client) {
			_requestFactory = requestFactory;
			_client = client;
		}

		public async Task<List<B2Bucket>> GetList(CancellationToken cancelToken = default(CancellationToken)) {
			var requestMessage = BucketRequestGenerators.GetBucketList(await _requestFactory.GetOptions());
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
			var requestMessage = BucketRequestGenerators.CreateBucket(await _requestFactory.GetOptions(), bucketName,
				bucketType.ToString());
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
		public async Task<B2Bucket> Create(string bucketName, B2BucketOptions options,
			CancellationToken cancelToken = default(CancellationToken)) {
			var requestMessage =
				BucketRequestGenerators.CreateBucket(await _requestFactory.GetOptions(), bucketName, options);
			var response = await _client.SendAsync(requestMessage, cancelToken);

			return await ResponseParser.ParseResponse<B2Bucket>(response, _api);
		}

		/// <summary>
		/// Deletes the bucket specified. Only buckets that contain no version of any files can be deleted.
		/// </summary>
		/// <param name="bucketId"></param>
		/// <param name="cancelToken"></param>
		/// <returns></returns>
		public async Task<B2Bucket> Delete(string bucketId = "",
			CancellationToken cancelToken = default(CancellationToken)) {
			var operationalBucketId = _requestFactory.DetermineBucketId(bucketId);

			var requestMessage =
				BucketRequestGenerators.DeleteBucket(await _requestFactory.GetOptions(), operationalBucketId);
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
		public async Task<B2Bucket> Update(BucketTypes bucketType, string bucketId = "",
			CancellationToken cancelToken = default(CancellationToken)) {
			var operationalBucketId = _requestFactory.DetermineBucketId(bucketId);
			var requestMessage =
				BucketRequestGenerators.UpdateBucket(await _requestFactory.GetOptions(), operationalBucketId,
					bucketType.ToString());
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
		public async Task<B2Bucket> Update(B2BucketOptions options, string bucketId = "",
			CancellationToken cancelToken = default(CancellationToken)) {
			var operationalBucketId = _requestFactory.DetermineBucketId(bucketId);

			var requestMessage =
				BucketRequestGenerators.UpdateBucket(await _requestFactory.GetOptions(), operationalBucketId, options);
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
		public async Task<B2Bucket> Update(B2BucketOptions options, int revisionNumber, string bucketId = "",
			CancellationToken cancelToken = default(CancellationToken)) {
			var operationalBucketId = _requestFactory.DetermineBucketId(bucketId);

			var requestMessage =
				BucketRequestGenerators.UpdateBucket(await _requestFactory.GetOptions(), operationalBucketId, options,
					revisionNumber);
			var response = await _client.SendAsync(requestMessage, cancelToken);

			return await ResponseParser.ParseResponse<B2Bucket>(response, _api);
		}
	}
}
