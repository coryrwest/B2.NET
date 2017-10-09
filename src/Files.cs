using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using B2Net.Http;
using B2Net.Models;
using Newtonsoft.Json;

namespace B2Net {
	public class Files {
		private B2Options _options;
		private HttpClient _client;

		public Files(B2Options options) {
			_options = options;
			_client = HttpClientFactory.CreateHttpClient();
		}

		/// <summary>
		/// Lists the names of all  non-hidden files in a bucket, starting at a given name.
		/// </summary>
		/// <param name="bucketId"></param>
		/// <param name="startFileName"></param>
		/// <param name="maxFileCount"></param>
		/// <param name="cancelToken"></param>
		/// <returns></returns>
		public async Task<B2FileList> GetList(string startFileName = "", int? maxFileCount = null, string bucketId = "", CancellationToken cancelToken = default(CancellationToken)) {
			var operationalBucketId = Utilities.DetermineBucketId(_options, bucketId);

			var requestMessage = FileMetaDataRequestGenerators.GetList(_options, operationalBucketId, startFileName, maxFileCount);
			var response = await _client.SendAsync(requestMessage, cancelToken);

			return await ResponseParser.ParseResponse<B2FileList>(response);
		}

		/// <summary>
		/// Lists all of the versions of all of the files contained in one bucket,
		/// in alphabetical order by file name, and by reverse of date/time uploaded
		/// for versions of files with the same name.
		/// </summary>
		/// <param name="startFileName"></param>
		/// <param name="startFileId"></param>
		/// <param name="maxFileCount"></param>
		/// <param name="bucketId"></param>
		/// <param name="cancelToken"></param>
		/// <returns></returns>
		public async Task<B2FileList> GetVersions(string startFileName = "", string startFileId = "", int? maxFileCount = null, string bucketId = "", CancellationToken cancelToken = default(CancellationToken)) {
			var operationalBucketId = Utilities.DetermineBucketId(_options, bucketId);

			var requestMessage = FileMetaDataRequestGenerators.ListVersions(_options, operationalBucketId, startFileName, startFileId, maxFileCount);
			var response = await _client.SendAsync(requestMessage, cancelToken);

			return await ResponseParser.ParseResponse<B2FileList>(response);
		}

		/// <summary>
		/// Gets information about one file stored in B2.
		/// </summary>
		/// <param name="fileId"></param>
		/// <param name="cancelToken"></param>
		/// <returns></returns>
		public async Task<B2File> GetInfo(string fileId, CancellationToken cancelToken = default(CancellationToken)) {
			var requestMessage = FileMetaDataRequestGenerators.GetInfo(_options, fileId);
			var response = await _client.SendAsync(requestMessage, cancelToken);

			return await ResponseParser.ParseResponse<B2File>(response);
        }

        /// <summary>
        /// get an upload url for use with one Thread.
        /// </summary>
        /// <param name="bucketId"></param>
        /// <param name="cancelToken"></param>
        /// <returns></returns>
        public async Task<B2UploadUrl> GetUploadUrl(string bucketId = "", CancellationToken cancelToken = default(CancellationToken)) {
            var operationalBucketId = Utilities.DetermineBucketId(_options, bucketId);

            // sent the request.
            var uploadUrlRequest = FileUploadRequestGenerators.GetUploadUrl(_options, operationalBucketId);
            var uploadUrlResponse = await _client.SendAsync(uploadUrlRequest, cancelToken);

            // parse response and return it.
            var uploadUrl = await ResponseParser.ParseResponse<B2UploadUrl>(uploadUrlResponse);

            // Set the upload auth token
            _options.UploadAuthorizationToken = uploadUrl.AuthorizationToken;

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
        public async Task<B2File> Upload(byte[] fileData, string fileName, string bucketId = "", Dictionary<string, string> fileInfo = null, CancellationToken cancelToken = default(CancellationToken)) {
            var operationalBucketId = Utilities.DetermineBucketId(_options, bucketId);

            // Get the upload url for this file
            // TODO: There must be a better way to do this
            var uploadUrlRequest = FileUploadRequestGenerators.GetUploadUrl(_options, operationalBucketId);
            var uploadUrlResponse = _client.SendAsync(uploadUrlRequest, cancelToken).Result;
            var uploadUrlData = await uploadUrlResponse.Content.ReadAsStringAsync();
            var uploadUrlObject = JsonConvert.DeserializeObject<B2UploadUrl>(uploadUrlData);
            // Set the upload auth token
            _options.UploadAuthorizationToken = uploadUrlObject.AuthorizationToken;

            // Now we can upload the file
            var requestMessage = FileUploadRequestGenerators.Upload(_options, uploadUrlObject.UploadUrl, fileData, fileName, fileInfo);
            var response = await _client.SendAsync(requestMessage, cancelToken);

            return await ResponseParser.ParseResponse<B2File>(response);
        }

        /// <summary>
        /// Uploads one file to B2, returning its unique file ID. Filename will be URL Encoded.
        /// </summary>
        /// <param name="fileData"></param>
        /// <param name="fileName"></param>
        /// <param name="bucketId"></param>
        /// <param name="cancelToken"></param>
        /// <returns></returns>
        public async Task<B2File> Upload(byte[] fileData, string fileName, B2UploadUrl uploadUrl, string bucketId = "", Dictionary<string, string> fileInfo = null, CancellationToken cancelToken = default(CancellationToken)) {
            var operationalBucketId = Utilities.DetermineBucketId(_options, bucketId);
            
            // Now we can upload the file
            var requestMessage = FileUploadRequestGenerators.Upload(_options, uploadUrl.UploadUrl, fileData, fileName, fileInfo);
            var response = await _client.SendAsync(requestMessage, cancelToken);

            return await ResponseParser.ParseResponse<B2File>(response);
        }

        /// <summary>
        /// Downloads one file by providing the name of the bucket and the name of the file.
        /// </summary>
        /// <param name="fileId"></param>
        /// <param name="fileName"></param>
        /// <param name="bucketId"></param>
        /// <param name="cancelToken"></param>
        /// <returns></returns>
        public async Task<B2File> DownloadByName(string fileName, string bucketName, CancellationToken cancelToken = default(CancellationToken)) {
			// Are we searching by name or id?
			HttpRequestMessage request;
			request = FileDownloadRequestGenerators.DownloadByName(_options, bucketName, fileName);

			// Send the download request
			var response = await _client.SendAsync(request, cancelToken);

			Utilities.CheckForErrors(response);

			// Create B2File from response
			return await ParseDownloadResponse(response);
		}

		/// <summary>
		/// Downloads one file from B2.
		/// </summary>
		/// <param name="fileId"></param>
		/// <param name="cancelToken"></param>
		/// <returns></returns>
		public async Task<B2File> DownloadById(string fileId, CancellationToken cancelToken = default(CancellationToken)) {
			// Are we searching by name or id?
			HttpRequestMessage request;
			request = FileDownloadRequestGenerators.DownloadById(_options, fileId);

			// Send the download request
			var response = await _client.SendAsync(request, cancelToken);

			// Create B2File from response
			return await ParseDownloadResponse(response);
		}

		/// <summary>
		/// Deletes the specified file version
		/// </summary>
		/// <param name="fileId"></param>
		/// <param name="fileName"></param>
		/// <param name="cancelToken"></param>
		/// <returns></returns>
		public async Task<B2File> Delete(string fileId, string fileName, CancellationToken cancelToken = default(CancellationToken)) {
			var requestMessage = FileDeleteRequestGenerator.Delete(_options, fileId, fileName);
			var response = await _client.SendAsync(requestMessage, cancelToken);

			return await ResponseParser.ParseResponse<B2File>(response);
		}

		/// <summary>
		/// Hides or Unhides a file so that downloading by name will not find the file,
		/// but previous versions of the file are still stored. See File
		/// Versions about what it means to hide a file.
		/// </summary>
		/// <param name="fileName"></param>
		/// <param name="bucketId"></param>
		/// <param name="cancelToken"></param>
		/// <returns></returns>
		public async Task<B2File> Hide(string fileName, string bucketId = "", string fileId = "", CancellationToken cancelToken = default(CancellationToken)) {
			var operationalBucketId = Utilities.DetermineBucketId(_options, bucketId);

			var requestMessage = FileMetaDataRequestGenerators.HideFile(_options, operationalBucketId, fileName, fileId);
			var response = await _client.SendAsync(requestMessage, cancelToken);

			return await ResponseParser.ParseResponse<B2File>(response);
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
            if (fileInfoHeaders.Count() > 0) {
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
