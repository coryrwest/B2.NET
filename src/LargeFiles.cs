using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using B2Net.Http;
using B2Net.Http.RequestGenerators;
using B2Net.Models;
using Newtonsoft.Json;

namespace B2Net {
	public class LargeFiles {
		private B2Options _options;
		private HttpClient _client;

		public LargeFiles(B2Options options) {
			_options = options;
			_client = HttpClientFactory.CreateHttpClient();
		}


        /// <summary>
        /// Starts a large file upload.
        /// </summary>
        /// <param name="fileId"></param>
        /// <param name="fileName"></param>
        /// <param name="bucketId"></param>
        /// <param name="cancelToken"></param>
        /// <returns></returns>
        public async Task<B2File> StartLargeFile(string fileName, string contentType = "", string bucketId = "", Dictionary<string, string> fileInfo = null, CancellationToken cancelToken = default(CancellationToken)) {
            var operationalBucketId = Utilities.DetermineBucketId(_options, bucketId);
            HttpRequestMessage request;
            request = LargeFileRequestGenerators.Start(_options, operationalBucketId, fileName, contentType, fileInfo);

            // Send the download request
            var response = await _client.SendAsync(request, cancelToken);

            Utilities.CheckForErrors(response);

            // Create B2File from response
            return await ParseDownloadResponse(response);
        }

        /// <summary>
        /// get an upload url for use with one Thread.
        /// </summary>
        /// <param name="bucketId"></param>
        /// <param name="cancelToken"></param>
        /// <returns></returns>
        public async Task<B2UploadPartUrl> GetUploadPartUrl(string fileId, CancellationToken cancelToken = default(CancellationToken)) {
            var request = LargeFileRequestGenerators.GetUploadPartUrl(_options, fileId);

            var uploadUrlResponse = await _client.SendAsync(request, cancelToken);

            var uploadUrl = await ResponseParser.ParseResponse<B2UploadPartUrl>(uploadUrlResponse);

            return uploadUrl;
        }

        /// <summary>
        /// DEPRECATED: This method has been deprecated in favor of the Upload that takes an UploadUrl parameter.
        /// The other Upload method is the preferred, and more efficient way, of uploading to B2.
        /// </summary>
        /// <param name="fileData"></param>
        /// <param name="fileName"></param>
        /// <param name="bucketId"></param>
        /// <param name="cancelToken"></param>
        /// <returns></returns>
        public async Task<B2UploadPart> UploadPart(byte[] fileData, int partNumber, CancellationToken cancelToken = default(CancellationToken)) {
            var request = LargeFileRequestGenerators.Upload(_options, fileData, partNumber);

            var response = _client.SendAsync(request, cancelToken).Result;
            
            return await ResponseParser.ParseResponse<B2UploadPart>(response);
        }
        
        /// <summary>
        /// Downloads one file by providing the name of the bucket and the name of the file.
        /// </summary>
        /// <param name="fileId"></param>
        /// <param name="fileName"></param>
        /// <param name="bucketId"></param>
        /// <param name="cancelToken"></param>
        /// <returns></returns>
        public async Task<B2File> FinishLargeFile(string fileId, string[] partSHA1Array, CancellationToken cancelToken = default(CancellationToken)) {
			var request = LargeFileRequestGenerators.Finish(_options, fileId, partSHA1Array);

			// Send the download request
			var response = await _client.SendAsync(request, cancelToken);

			Utilities.CheckForErrors(response);

			// Create B2File from response
			return await ParseDownloadResponse(response);
		}

		private async Task<B2File> ParseDownloadResponse(HttpResponseMessage response) {
			Utilities.CheckForErrors(response);

			var file = new B2File();
			IEnumerable<string> values;
			if (response.Headers.TryGetValues("X-Bz-Content-Sha1", out values)) {
				file.ContentSHA1 = values.First();
			}
			if (response.Headers.TryGetValues("X-Bz-File-Name", out values)) {
				file.FileName = values.First();
				// Decode file name
				file.FileName = file.FileName.b2UrlDecode();
			}
			if (response.Headers.TryGetValues("X-Bz-File-Id", out values)) {
				file.FileId = values.First();
			}
            // File Info Headers
            var fileInfoHeaders = response.Headers.Where(h => h.Key.ToLower().Contains("x-bz-info"));
            var infoData = new Dictionary<string, string>();
            if (fileInfoHeaders.Any()) {
                foreach (var fileInfo in fileInfoHeaders)
                {
                    // Substring to parse out the file info prefix.
                    infoData.Add(fileInfo.Key.Substring(10), fileInfo.Value.First());
                }
            }
            file.FileInfo = infoData;
            if (response.Content.Headers.ContentLength.HasValue) {
                file.Size = response.Content.Headers.ContentLength.Value;
            }
            file.FileData = await response.Content.ReadAsByteArrayAsync();

			return await Task.FromResult(file);
		}
	}
}
