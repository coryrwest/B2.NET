using System;
using System.Collections.Generic;
using System.Net.Configuration;
using System.Threading;
using System.Threading.Tasks;
using B2Net.Http;
using B2Net.Models;
using Newtonsoft.Json;

namespace B2Net {
	public class Buckets {
		private B2Options _options;

		public Buckets(B2Options options) {
			_options = options;
		}

		public async Task<List<B2Bucket>> GetList(CancellationToken cancelToken = default(CancellationToken)) {
			var client = HttpClientFactory.CreateHttpClient();

			var requestMessage = BucketRequestGenerators.GetBucketList(_options);
			var response = await client.SendAsync(requestMessage, cancelToken);

			var bucketList = ResponseParser.ParseResponse<B2BucketListDeserializeModel>(response).Result;
			return bucketList.Buckets;
		}

		/// <summary>
		/// Creates a new bucket. A bucket belongs to the account used to create it.
		/// </summary>
		/// <param name="bucketName"></param>
		/// <param name="bucketType"></param>
		/// <param name="cancelToken"></param>
		/// <returns></returns>
		public async Task<B2Bucket> Create(string bucketName, BucketTypes bucketType, CancellationToken cancelToken = default(CancellationToken)) {
			var client = HttpClientFactory.CreateHttpClient();

			var requestMessage = BucketRequestGenerators.CreateBucket(_options, bucketName, bucketType.ToString());
			var response = await client.SendAsync(requestMessage, cancelToken);

			return await ResponseParser.ParseResponse<B2Bucket>(response);
		}

		/// <summary>
		/// Deletes the bucket specified. Only buckets that contain no version of any files can be deleted.
		/// </summary>
		/// <param name="bucketId"></param>
		/// <param name="cancelToken"></param>
		/// <returns></returns>
		public async Task<B2Bucket> Delete(string bucketId = "", CancellationToken cancelToken = default(CancellationToken)) {
			var operationalBucketId = Utilities.DetermineBucketId(_options, bucketId);

			var client = HttpClientFactory.CreateHttpClient();

			var requestMessage = BucketRequestGenerators.DeleteBucket(_options, operationalBucketId);
			var response = await client.SendAsync(requestMessage, cancelToken);

			return await ResponseParser.ParseResponse<B2Bucket>(response);
		}

		/// <summary>
		/// Update an existing bucket. bucketId is only optional if you are persisting a bucket for this client.
		/// </summary>
		/// <param name="bucketType"></param>
		/// <param name="bucketId"></param>
		/// <param name="cancelToken"></param>
		/// <returns></returns>
		public async Task<B2Bucket> Update(BucketTypes bucketType, string bucketId = "", CancellationToken cancelToken = default(CancellationToken)) {
			var operationalBucketId = Utilities.DetermineBucketId(_options, bucketId);

			var client = HttpClientFactory.CreateHttpClient();

			var requestMessage = BucketRequestGenerators.UpdateBucket(_options, operationalBucketId, bucketType.ToString());
			var response = await client.SendAsync(requestMessage, cancelToken);

			return await ResponseParser.ParseResponse<B2Bucket>(response);
		}
	}
}