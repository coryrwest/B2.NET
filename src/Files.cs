using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using B2Net.Http;
using B2Net.Models;
using Newtonsoft.Json;

namespace B2Net {
	public class Files {
		private B2Options _options;

		public Files(B2Options options) {
			_options = options;
		}

		/// <summary>
		/// Lists the names of all files in a bucket, starting at a given name.
		/// </summary>
		/// <param name="bucketId"></param>
		/// <param name="startFileName"></param>
		/// <param name="maxFileCount"></param>
		/// <param name="cancelToken"></param>
		/// <returns></returns>
		public async Task<B2FileList> GetList(string bucketId = "", string startFileName = "", int maxFileCount = 100, CancellationToken cancelToken = default(CancellationToken)) {
			var operationalBucketId = Utilities.DetermineBucketId(_options, bucketId);

			var client = HttpClientFactory.CreateHttpClient();
			
			var requestMessage = FileMetaDataRequestGenerators.GetList(_options, operationalBucketId, startFileName, maxFileCount);
			var response = await client.SendAsync(requestMessage, cancelToken);

			return await ResponseParser.ParseResponse<B2FileList>(response);
		}

		/// <summary>
		/// Uploads one file to B2, returning its unique file ID.
		/// </summary>
		/// <param name="fileData"></param>
		/// <param name="fileName"></param>
		/// <param name="bucketId"></param>
		/// <param name="cancelToken"></param>
		/// <returns></returns>
		public async Task<B2File> Upload(byte[] fileData, string fileName, string bucketId = "", CancellationToken cancelToken = default(CancellationToken)) {
			var operationalBucketId = Utilities.DetermineBucketId(_options, bucketId);

			var client = HttpClientFactory.CreateHttpClient();

			// Get the upload url for this file
			// TODO: There must be a better way to do this
			var uploadUrlRequest = FileUploadRequestGenerators.GetUploadUrl(_options, operationalBucketId);
			var uploadUrlResponse = client.SendAsync(uploadUrlRequest, cancelToken).Result;
			var uploadUrlData = await uploadUrlResponse.Content.ReadAsStringAsync();
			var uploadUrlObject = JsonConvert.DeserializeObject<B2UploadUrl>(uploadUrlData);
			// Set the upload auth token
			_options.UploadAuthorizationToken = uploadUrlObject.AuthorizationToken;

			// Now we can upload the file
			var requestMessage = FileUploadRequestGenerators.Upload(_options, uploadUrlObject.UploadUrl, fileData, fileName);
			var response = await client.SendAsync(requestMessage, cancelToken);

			return await ResponseParser.ParseResponse<B2File>(response);
		}

		/// <summary>
		/// Deletes the specified file version
		/// </summary>
		/// <param name="fileId"></param>
		/// <param name="fileName"></param>
		/// <param name="cancelToken"></param>
		/// <returns></returns>
		public async Task<B2File> Delete(string fileId, string fileName, CancellationToken cancelToken = default(CancellationToken)) {
			var client = HttpClientFactory.CreateHttpClient();

			var requestMessage = FileDeleteRequestGenerator.Delete(_options, fileId, fileName);
			var response = await client.SendAsync(requestMessage, cancelToken);

			return await ResponseParser.ParseResponse<B2File>(response);
		}

		public async Task<B2File> Hide(string fileName, string bucketId = "", CancellationToken cancelToken = default(CancellationToken)) {
			var operationalBucketId = Utilities.DetermineBucketId(_options, bucketId);

			var client = HttpClientFactory.CreateHttpClient();

			var requestMessage = FileMetaDataRequestGenerators.HideFile(_options, operationalBucketId, fileName);
			var response = await client.SendAsync(requestMessage, cancelToken);

			return await ResponseParser.ParseResponse<B2File>(response);
		}

		internal class B2UploadUrl {
			public string BucketId { get; set; }
			public string UploadUrl { get; set; }
			public string AuthorizationToken { get; set; }
		}
	}
}
