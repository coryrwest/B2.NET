using B2Net.Http;
using B2Net.Http.RequestGenerators;
using B2Net.Models;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace B2Net {
	public class LargeFiles : ILargeFiles {
		private readonly B2BaseRequestFactory _requestFactory;
		private readonly HttpClient _client;
		private const string Api = "Large Files";

		public LargeFiles(B2BaseRequestFactory requestFactory, HttpClient client) {
			_requestFactory = requestFactory;
			_client = client;
		}

		/// <summary>
		/// Starts a large file upload.
		/// </summary>
		/// <param name="fileId"></param>
		/// <param name="fileName"></param>
		/// <param name="bucketId"></param>
		/// <param name="cancelToken"></param>
		/// <returns></returns>
		public async Task<B2File> StartLargeFile(string fileName, string contentType = "", string bucketId = "",
			Dictionary<string, string> fileInfo = null, CancellationToken cancelToken = default(CancellationToken)) {
			var operationalBucketId = _requestFactory.DetermineBucketId(bucketId);

			var request =
				LargeFileRequestGenerators.Start(await _requestFactory.GetOptions(), operationalBucketId, fileName,
					contentType, fileInfo);

			// Send the download request
			var response = await _client.SendAsync(request, cancelToken);

			// Create B2File from response
			return await ResponseParser.ParseResponse<B2File>(response, Api);
		}

		/// <summary>
		/// Get an upload url for use with one Thread.
		/// </summary>
		/// <param name="bucketId"></param>
		/// <param name="cancelToken"></param>
		/// <returns></returns>
		public async Task<B2UploadPartUrl> GetUploadPartUrl(string fileId,
			CancellationToken cancelToken = default(CancellationToken)) {
			var request = LargeFileRequestGenerators.GetUploadPartUrl(await _requestFactory.GetOptions(), fileId);

			var uploadUrlResponse = await _client.SendAsync(request, cancelToken);

			var uploadUrl = await ResponseParser.ParseResponse<B2UploadPartUrl>(uploadUrlResponse, Api);

			return uploadUrl;
		}

		/// <summary>
		/// Upload one part of an already started large file upload.
		/// </summary>
		public async Task<B2UploadPart> UploadPart(byte[] fileData, int partNumber, B2UploadPartUrl uploadPartUrl,
			CancellationToken cancelToken = default(CancellationToken)) {
			var request = LargeFileRequestGenerators.Upload(await _requestFactory.GetOptions(), fileData, partNumber,
				uploadPartUrl);

			var response = await _client.SendAsync(request, cancelToken);

			return await ResponseParser.ParseResponse<B2UploadPart>(response, Api);
		}

		/// <summary>
		/// Downloads one file by providing the name of the bucket and the name of the file.
		/// </summary>
		public async Task<B2File> FinishLargeFile(string fileId, string[] partSHA1Array,
			CancellationToken cancelToken = default(CancellationToken)) {
			var request = await LargeFileRequestGenerators.Finish(_requestFactory, fileId, partSHA1Array);

			// Send the request
			var response = await _client.SendAsync(request, cancelToken);

			// Create B2File from response
			return await ResponseParser.ParseResponse<B2File>(response, Api);
		}

		/// <summary>
		/// List the parts of an incomplete large file upload.
		/// </summary>
		public async Task<B2LargeFileParts> ListPartsForIncompleteFile(string fileId, int startPartNumber,
			int maxPartCount, CancellationToken cancelToken = default(CancellationToken)) {
			var request =
				await LargeFileRequestGenerators.ListParts(_requestFactory, fileId, startPartNumber, maxPartCount);

			// Send the request
			var response = await _client.SendAsync(request, cancelToken);

			// Create B2File from response
			return await ResponseParser.ParseResponse<B2LargeFileParts>(response, Api);
		}

		/// <summary>
		/// Cancel a large file upload
		/// </summary>
		public async Task<B2CancelledFile> CancelLargeFile(string fileId,
			CancellationToken cancelToken = default(CancellationToken)) {
			var request = await LargeFileRequestGenerators.Cancel(_requestFactory, fileId);

			// Send the request
			var response = await _client.SendAsync(request, cancelToken);

			// Create B2File from response
			return await ResponseParser.ParseResponse<B2CancelledFile>(response, Api);
		}

		/// <summary>
		/// List all the incomplete large file uploads for the supplied bucket
		/// </summary>
		public async Task<B2IncompleteLargeFiles> ListIncompleteFiles(string bucketId, string startFileId = "",
			string maxFileCount = "", CancellationToken cancelToken = default(CancellationToken)) {
			var request =
				await LargeFileRequestGenerators.IncompleteFiles(_requestFactory, bucketId, startFileId, maxFileCount);

			// Send the request
			var response = await _client.SendAsync(request, cancelToken);

			// Create B2File from response
			return await ResponseParser.ParseResponse<B2IncompleteLargeFiles>(response, Api);
		}

		/// <summary>
		/// Copy a source file into part of a large file
		/// </summary>
		/// <param name="fileName"></param>
		/// <param name="contentType"></param>
		/// <param name="bucketId"></param>
		/// <param name="fileInfo"></param>
		/// <param name="cancelToken"></param>
		/// <returns></returns>
		//public async Task<B2LargeFilePart> CopyPart(string sourceFileId, string destinationLargeFileId, int destinationPartNumber, string range = "", CancellationToken cancelToken = default(CancellationToken)) {
		//	var request = LargeFileRequestGenerators.CopyPart(_options, sourceFileId, destinationLargeFileId, destinationPartNumber, range);

		//	// Send the download request
		//	var response = await _client.SendAsync(request, cancelToken);

		//	// Create B2File from response
		//	return await ResponseParser.ParseResponse<B2LargeFilePart>(response, _api);
		//}
	}
}
