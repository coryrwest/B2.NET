using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
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
		public async Task<B2FileList> GetList(string startFileName = "", int maxFileCount = 100, string bucketId = "", CancellationToken cancelToken = default(CancellationToken)) {
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
		public async Task<B2FileList> GetVersions(string startFileName = "", string startFileId = "", int maxFileCount = 100, string bucketId = "", CancellationToken cancelToken = default(CancellationToken)) {
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
		/// Uploads one file to B2, returning its unique file ID.
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
		/// Hides a file so that downloading by name will not find the file,
		/// but previous versions of the file are still stored. See File
		/// Versions about what it means to hide a file.
		/// </summary>
		/// <param name="fileName"></param>
		/// <param name="bucketId"></param>
		/// <param name="cancelToken"></param>
		/// <returns></returns>
		public async Task<B2File> Hide(string fileName, string bucketId = "", CancellationToken cancelToken = default(CancellationToken)) {
			var operationalBucketId = Utilities.DetermineBucketId(_options, bucketId);

			var requestMessage = FileMetaDataRequestGenerators.HideFile(_options, operationalBucketId, fileName);
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
			}
			if (response.Headers.TryGetValues("X-Bz-File-Id", out values)) {
				file.FileId = values.First();
			}
            // TODO: File Info headers
            var fileInfoHeaders = response.Headers.Where(h => h.Key.Contains("X-Bz-Info"));
            if (fileInfoHeaders.Count() > 0) {
                var infoData = new Dictionary<string, string>();
                foreach (var fileInfo in fileInfoHeaders)
                {
                    infoData.Add(fileInfo.Key, fileInfo.Value.First());
                }
                file.FileInfo = infoData;
            }

            file.FileData = await response.Content.ReadAsByteArrayAsync();

			return await Task.FromResult(file);
		}

		internal class B2UploadUrl {
			public string BucketId { get; set; }
			public string UploadUrl { get; set; }
			public string AuthorizationToken { get; set; }
		}
	}
}
