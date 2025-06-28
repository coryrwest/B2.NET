using System;
using B2Net.Http;
using B2Net.Http.RequestGenerators;
using B2Net.Models;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace B2Net {
	public class LargeFiles : ILargeFiles {
		private B2Options _options;
		private HttpClient _client;
		private Func<B2Options, B2Options> _authorize;
		private string _api = "Large Files";

		public LargeFiles(B2Options options, Func<B2Options, B2Options> authorizeFunc) {
			_options = options;
			_authorize = authorizeFunc;
			_client = HttpClientFactory.CreateHttpClient(options.RequestTimeout);
		}

		public async Task<B2File> StartLargeFile(string fileName, B2LargeFileRetention fileRetention, string contentType = "", string bucketId = "", Dictionary<string, string> fileInfo = null, CancellationToken cancelToken = default(CancellationToken)) {
			RefreshAuthorization(_options, _authorize);
			var operationalBucketId = Utilities.DetermineBucketId(_options, bucketId);

			var request = LargeFileRequestGenerators.Start(_options, operationalBucketId, fileName, contentType, fileRetention, fileInfo);

			// Send the download request
			var response = await _client.SendAsync(request, cancelToken);

			// Create B2File from response
			return await ResponseParser.ParseResponse<B2File>(response, _api);
		}

		public async Task<B2File> StartLargeFile(string fileName, string contentType = "", string bucketId = "", Dictionary<string, string> fileInfo = null, CancellationToken cancelToken = default(CancellationToken)) {
			RefreshAuthorization(_options, _authorize);
			var operationalBucketId = Utilities.DetermineBucketId(_options, bucketId);

			var request = LargeFileRequestGenerators.Start(_options, operationalBucketId, fileName, contentType, null, fileInfo);

			// Send the download request
			var response = await _client.SendAsync(request, cancelToken);

			// Create B2File from response
			return await ResponseParser.ParseResponse<B2File>(response, _api);
		}

		public async Task<B2UploadPartUrl> GetUploadPartUrl(string fileId, CancellationToken cancelToken = default(CancellationToken)) {
			RefreshAuthorization(_options, _authorize);
			var request = LargeFileRequestGenerators.GetUploadPartUrl(_options, fileId);

			var uploadUrlResponse = await _client.SendAsync(request, cancelToken);

			var uploadUrl = await ResponseParser.ParseResponse<B2UploadPartUrl>(uploadUrlResponse, _api);

			return uploadUrl;
		}

		public async Task<B2UploadPart> UploadPart(byte[] fileData, int partNumber, B2UploadPartUrl uploadPartUrl, CancellationToken cancelToken = default(CancellationToken)) {
			RefreshAuthorization(_options, _authorize);
			var request = LargeFileRequestGenerators.Upload(_options, fileData, partNumber, uploadPartUrl);

			var response = await _client.SendAsync(request, cancelToken);

			return await ResponseParser.ParseResponse<B2UploadPart>(response, _api);
		}

		public async Task<B2File> FinishLargeFile(string fileId, string[] partSHA1Array, CancellationToken cancelToken = default(CancellationToken)) {
			RefreshAuthorization(_options, _authorize);
			var request = LargeFileRequestGenerators.Finish(_options, fileId, partSHA1Array);

			// Send the request
			var response = await _client.SendAsync(request, cancelToken);

			// Create B2File from response
			return await ResponseParser.ParseResponse<B2File>(response, _api);
		}

		public async Task<B2LargeFileParts> ListPartsForIncompleteFile(string fileId, int startPartNumber, int maxPartCount, CancellationToken cancelToken = default(CancellationToken)) {
			RefreshAuthorization(_options, _authorize);
			var request = LargeFileRequestGenerators.ListParts(_options, fileId, startPartNumber, maxPartCount);

			// Send the request
			var response = await _client.SendAsync(request, cancelToken);

			// Create B2File from response
			return await ResponseParser.ParseResponse<B2LargeFileParts>(response, _api);
		}

		public async Task<B2CancelledFile> CancelLargeFile(string fileId, CancellationToken cancelToken = default(CancellationToken)) {
			RefreshAuthorization(_options, _authorize);
			var request = LargeFileRequestGenerators.Cancel(_options, fileId);

			// Send the request
			var response = await _client.SendAsync(request, cancelToken);

			// Create B2File from response
			return await ResponseParser.ParseResponse<B2CancelledFile>(response, _api);
		}

		public async Task<B2IncompleteLargeFiles> ListIncompleteFiles(string bucketId, string startFileId = "", string maxFileCount = "", CancellationToken cancelToken = default(CancellationToken)) {
			RefreshAuthorization(_options, _authorize);
			var request = LargeFileRequestGenerators.IncompleteFiles(_options, bucketId, startFileId, maxFileCount);

			// Send the request
			var response = await _client.SendAsync(request, cancelToken);

			// Create B2File from response
			return await ResponseParser.ParseResponse<B2IncompleteLargeFiles>(response, _api);
		}

		/// <summary>
		/// Copy a part from an existing file to a large file that is being uploaded
		/// </summary>
		/// <param name="sourceFileId">The ID of the source file to copy from</param>
		/// <param name="destinationLargeFileId">The ID of the large file that is being uploaded</param>
		/// <param name="destinationPartNumber">The part number to copy to (between 1 and 10000)</param>
		/// <param name="range">Optional byte range within the source file</param>
		/// <param name="cancelToken">Cancellation token</param>
		/// <returns>The part that was copied</returns>
		public async Task<B2LargeFilePart> CopyPart(string sourceFileId, string destinationLargeFileId, int destinationPartNumber, string range = "", CancellationToken cancelToken = default(CancellationToken)) {
			RefreshAuthorization(_options, _authorize);
			var request = LargeFileRequestGenerators.CopyPart(_options, sourceFileId, destinationLargeFileId, destinationPartNumber, range);

			// Send the request
			var response = await _client.SendAsync(request, cancelToken);

			// Create B2LargeFilePart from response
			return await ResponseParser.ParseResponse<B2LargeFilePart>(response, _api);
		}

		/// <summary>
		/// Check that the options has a valid authorization token and if it does not, get one.
		/// </summary>
		/// <param name="options"></param>
		/// <param name="authorize"></param>
		/// <returns></returns>
		private void RefreshAuthorization(B2Options options, Func<B2Options, B2Options> authorize) {
			if (!options.Authenticated && !options.NoTokenRefresh) {
				options = authorize(options);
			}
		}
	}
}
