using System;
using B2Net.Http;
using B2Net.Models;
using System.Text.Json;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace B2Net {
	public class Files : IFiles {
		private B2Options _options;
		private HttpClient _client;
		private Func<B2Options, B2Options> _authorize;
		private string _api = "Files";

		public Files(B2Options options, Func<B2Options, B2Options> authorizeFunc) {
			_options = options;
			_authorize = authorizeFunc;
			_client = HttpClientFactory.CreateHttpClient(options.RequestTimeout);
		}

		public async Task<B2FileList> GetList(string startFileName = "", int? maxFileCount = null, string bucketId = "", CancellationToken cancelToken = default(CancellationToken)) {
			RefreshAuthorization(_options, _authorize);
			return await GetListWithPrefixOrDemiliter(startFileName, "", "", maxFileCount, bucketId, cancelToken);
		}

		public async Task<B2FileList> GetListWithPrefixOrDemiliter(string startFileName = "", string prefix = "", string delimiter = "", int? maxFileCount = null, string bucketId = "", CancellationToken cancelToken = default(CancellationToken)) {
			RefreshAuthorization(_options, _authorize);
			var operationalBucketId = Utilities.DetermineBucketId(_options, bucketId);

			var requestMessage = FileMetaDataRequestGenerators.GetList(_options, operationalBucketId, startFileName, maxFileCount, prefix, delimiter);
			var response = await _client.SendAsync(requestMessage, cancelToken);

			return await ResponseParser.ParseResponse<B2FileList>(response, _api);
		}

		public async Task<B2FileList> GetVersions(string startFileName = "", string startFileId = "", int? maxFileCount = null, string bucketId = "", CancellationToken cancelToken = default(CancellationToken)) {
			RefreshAuthorization(_options, _authorize);
			return await GetVersionsWithPrefixOrDelimiter(startFileName, startFileId, "", "", maxFileCount, bucketId, cancelToken);
		}

		public async Task<B2FileList> GetVersionsWithPrefixOrDelimiter(string startFileName = "", string startFileId = "", string prefix = "", string delimiter = "", int? maxFileCount = null, string bucketId = "",
			CancellationToken cancelToken = default(CancellationToken)) {
			RefreshAuthorization(_options, _authorize);
			var operationalBucketId = Utilities.DetermineBucketId(_options, bucketId);

			var requestMessage = FileMetaDataRequestGenerators.ListVersions(_options, operationalBucketId, startFileName, startFileId, maxFileCount, prefix, delimiter);
			var response = await _client.SendAsync(requestMessage, cancelToken);

			return await ResponseParser.ParseResponse<B2FileList>(response, _api);
		}

		public async Task<B2File> GetInfo(string fileId, CancellationToken cancelToken = default(CancellationToken)) {
			RefreshAuthorization(_options, _authorize);
			var requestMessage = FileMetaDataRequestGenerators.GetInfo(_options, fileId);
			var response = await _client.SendAsync(requestMessage, cancelToken);

			return await ResponseParser.ParseResponse<B2File>(response, _api);
		}

		public async Task<B2UploadUrl> GetUploadUrl(string bucketId = "", CancellationToken cancelToken = default(CancellationToken)) {
			RefreshAuthorization(_options, _authorize);
			var operationalBucketId = Utilities.DetermineBucketId(_options, bucketId);

			// send the request.
			var uploadUrlRequest = FileUploadRequestGenerators.GetUploadUrl(_options, operationalBucketId);
			var uploadUrlResponse = await _client.SendAsync(uploadUrlRequest, cancelToken);

			// parse response and return it.
			var uploadUrl = await ResponseParser.ParseResponse<B2UploadUrl>(uploadUrlResponse);

			// Set the upload auth token
			_options.UploadAuthorizationToken = uploadUrl.AuthorizationToken;

			return uploadUrl;
		}

		[Obsolete("This method has been deprecated in favor of the B2FileUploadContext overload.", false)]
		public async Task<B2File> Upload(byte[] fileData, string fileName, string bucketId = "", Dictionary<string, string> fileInfo = null, CancellationToken cancelToken = default(CancellationToken)) {
			RefreshAuthorization(_options, _authorize);
			var operationalBucketId = Utilities.DetermineBucketId(_options, bucketId);

			// Get the upload url for this file
			var uploadUrlRequest = FileUploadRequestGenerators.GetUploadUrl(_options, operationalBucketId);
			var uploadUrlResponse = await _client.SendAsync(uploadUrlRequest, cancelToken);
			var uploadUrlData = await uploadUrlResponse.Content.ReadAsStringAsync();
			var uploadUrlObject = JsonSerializer.Deserialize<B2UploadUrl>(uploadUrlData);
			// Set the upload auth token
			_options.UploadAuthorizationToken = uploadUrlObject.AuthorizationToken;

			// Now we can upload the file
			return await Upload(fileData, new B2FileUploadContext() {
				FileName = fileName,
				AutoRetry = false,
				BucketId = bucketId,
				AdditionalFileInfo = fileInfo,
				B2UploadUrl = uploadUrlObject
			}, cancelToken);
		}

		[Obsolete("This method has been deprecated in favor of the B2FileUploadContext overload.", false)]
		public async Task<B2File> Upload(byte[] fileData, string fileName, B2UploadUrl uploadUrl, string bucketId = "", Dictionary<string, string> fileInfo = null, CancellationToken cancelToken = default(CancellationToken)) {
			RefreshAuthorization(_options, _authorize);
			return await Upload(fileData, new B2FileUploadContext() {
				FileName = fileName,
				AutoRetry = false,
				BucketId = bucketId,
				AdditionalFileInfo = fileInfo,
				B2UploadUrl = uploadUrl
			}, cancelToken);
		}

		[Obsolete("This method has been deprecated in favor of the B2FileUploadContext overload.", false)]
		public async Task<B2File> Upload(byte[] fileData, string fileName, B2UploadUrl uploadUrl, bool autoRetry, string bucketId = "", Dictionary<string, string> fileInfo = null, CancellationToken cancelToken = default(CancellationToken)) {
			RefreshAuthorization(_options, _authorize);
			return await Upload(fileData, new B2FileUploadContext() {
				FileName = fileName,
				AutoRetry = autoRetry,
				BucketId = bucketId,
				AdditionalFileInfo = fileInfo,
				B2UploadUrl = uploadUrl
			}, cancelToken);
		}

		[Obsolete("This method has been deprecated in favor of the B2FileUploadContext overload.", false)]
		public async Task<B2File> Upload(byte[] fileData, string fileName, B2UploadUrl uploadUrl, string contentType, bool autoRetry, string bucketId = "", Dictionary<string, string> fileInfo = null, CancellationToken cancelToken = default(CancellationToken)) {
			RefreshAuthorization(_options, _authorize);
			return await Upload(fileData, new B2FileUploadContext() {
				FileName = fileName,
				ContentType = contentType,
				AutoRetry = autoRetry,
				BucketId = bucketId,
				AdditionalFileInfo = fileInfo,
				B2UploadUrl = uploadUrl
			}, cancelToken);
		}

		public async Task<B2File> Upload(byte[] fileData, B2FileUploadContext uploadContext, CancellationToken cancelToken = default(CancellationToken)) {
			if (uploadContext.B2UploadUrl?.UploadUrl == null) {
				throw new ArgumentNullException(nameof(B2UploadUrl.UploadUrl), "You did not provide an UploadUrl in the B2UploadUrl object.");
			}
			RefreshAuthorization(_options, _authorize);
			// Now we can upload the file
			var streamify = new MemoryStream(fileData);
			var requestMessage = FileUploadRequestGenerators.Upload(_options, uploadContext.B2UploadUrl.UploadUrl, streamify, uploadContext);

			var response = await _client.SendAsync(requestMessage, cancelToken);
			// Auto retry
			if (uploadContext.AutoRetry && (
				response.StatusCode == (HttpStatusCode)429 ||
				response.StatusCode == HttpStatusCode.RequestTimeout ||
				response.StatusCode == HttpStatusCode.ServiceUnavailable)) {
				Task.Delay(1000, cancelToken).Wait(cancelToken);
				var retryMessage = FileUploadRequestGenerators.Upload(_options, uploadContext.B2UploadUrl.UploadUrl, fileData, uploadContext.FileName, uploadContext.AdditionalFileInfo, uploadContext.ContentType);
				response = await _client.SendAsync(retryMessage, cancelToken);
			}

			return await ResponseParser.ParseResponse<B2File>(response, _api);
		}

		public async Task<B2File> Upload(Stream fileDataWithSHA, string fileName, B2UploadUrl uploadUrl, string contentType, bool autoRetry, string bucketId = "", Dictionary<string, string> fileInfo = null, bool dontSHA = false,
			CancellationToken cancelToken = default(CancellationToken)) {
			return await Upload(fileDataWithSHA, new B2FileUploadContext() {
				FileName = fileName,
				ContentType = contentType,
				BucketId = bucketId, 
				AdditionalFileInfo = fileInfo,
				B2UploadUrl = uploadUrl,
				AutoRetry = autoRetry
			}, dontSHA, cancelToken);
		}

		public async Task<B2File> Upload(Stream fileDataWithSHA, B2FileUploadContext uploadContext, bool dontSHA = false,
			CancellationToken cancelToken = default(CancellationToken)) {
			RefreshAuthorization(_options, _authorize);
			// Now we can upload the file
			var requestMessage = FileUploadRequestGenerators.Upload(_options, uploadContext.B2UploadUrl.UploadUrl, fileDataWithSHA, uploadContext.FileName, uploadContext.AdditionalFileInfo, uploadContext.ContentType, dontSHA);

			var response = await _client.SendAsync(requestMessage, cancelToken);
			// Auto retry
			if (uploadContext.AutoRetry && (
				response.StatusCode == (HttpStatusCode)429 ||
				response.StatusCode == HttpStatusCode.RequestTimeout ||
				response.StatusCode == HttpStatusCode.ServiceUnavailable)) {
				Task.Delay(1000, cancelToken).Wait(cancelToken);
				var retryMessage = FileUploadRequestGenerators.Upload(_options, uploadContext.B2UploadUrl.UploadUrl, fileDataWithSHA, uploadContext.FileName, uploadContext.AdditionalFileInfo, uploadContext.ContentType, dontSHA);
				response = await _client.SendAsync(retryMessage, cancelToken);
			}

			return await ResponseParser.ParseResponse<B2File>(response, _api);
		}

		public async Task<B2File> DownloadByName(string fileName, string bucketName, int startByte, int endByte,
		CancellationToken cancelToken = default(CancellationToken)) {
			RefreshAuthorization(_options, _authorize);
			// Are we searching by name or id?
			HttpRequestMessage request;
			request = FileDownloadRequestGenerators.DownloadByName(_options, bucketName, fileName, $"{startByte}-{endByte}");

			// Send the download request
			var response = await _client.SendAsync(request, cancelToken);

			// Create B2File from response
			return await ParseDownloadResponse(response);
		}

		public async Task<B2File> DownloadByName(string fileName, string bucketName, CancellationToken cancelToken = default(CancellationToken)) {
			RefreshAuthorization(_options, _authorize);
			// Are we searching by name or id?
			HttpRequestMessage request;
			request = FileDownloadRequestGenerators.DownloadByName(_options, bucketName, fileName);

			// Send the download request
			var response = await _client.SendAsync(request, cancelToken);

			// Create B2File from response
			return await ParseDownloadResponse(response);
		}

		public async Task<B2File> DownloadById(string fileId, int startByte, int endByte, CancellationToken cancelToken = default(CancellationToken)) {
			RefreshAuthorization(_options, _authorize);
			// Are we searching by name or id?
			var request = FileDownloadRequestGenerators.DownloadById(_options, fileId, $"{startByte}-{endByte}");

			// Send the download request
			var response = await _client.SendAsync(request, cancelToken);

			// Create B2File from response
			return await ParseDownloadResponse(response);
		}

		public async Task<B2File> DownloadById(string fileId, CancellationToken cancelToken = default(CancellationToken)) {
			RefreshAuthorization(_options, _authorize);
			// Are we searching by name or id?
			HttpRequestMessage request;
			request = FileDownloadRequestGenerators.DownloadById(_options, fileId);

			// Send the download request
			var response = await _client.SendAsync(request, cancelToken);

			// Create B2File from response
			return await ParseDownloadResponse(response);
		}

		public async Task<B2File> Delete(string fileId, string fileName, CancellationToken cancelToken = default(CancellationToken)) {
			RefreshAuthorization(_options, _authorize);
			var requestMessage = FileDeleteRequestGenerator.Delete(_options, fileId, fileName);
			var response = await _client.SendAsync(requestMessage, cancelToken);

			return await ResponseParser.ParseResponse<B2File>(response, _api);
		}


		public string GetFriendlyDownloadUrl(string fileName, string bucketName, CancellationToken cancelToken = default(CancellationToken)) {
			var downloadUrl = _options.DownloadUrl;
			var friendlyUrl = "";
			if (!string.IsNullOrEmpty(downloadUrl)) {
				friendlyUrl = $"{downloadUrl}/file/{bucketName}/{fileName}";
			}
			return friendlyUrl;
		}

		public async Task<B2File> Hide(string fileName, string bucketId = "", string fileId = "", CancellationToken cancelToken = default(CancellationToken)) {
			RefreshAuthorization(_options, _authorize);
			var operationalBucketId = Utilities.DetermineBucketId(_options, bucketId);

			var requestMessage = FileMetaDataRequestGenerators.HideFile(_options, operationalBucketId, fileName, fileId);
			var response = await _client.SendAsync(requestMessage, cancelToken);

			return await ResponseParser.ParseResponse<B2File>(response, _api);
		}

		public async Task<B2File> Copy(string sourceFileId, string newFileName,
			B2MetadataDirective metadataDirective = B2MetadataDirective.COPY, string contentType = "",
			Dictionary<string, string> fileInfo = null, string range = "", string destinationBucketId = "",
			CancellationToken cancelToken = default(CancellationToken)) {
			RefreshAuthorization(_options, _authorize);

			if (metadataDirective == B2MetadataDirective.COPY && (!string.IsNullOrWhiteSpace(contentType) || fileInfo != null)) {
				throw new CopyReplaceSetupException("Copy operations cannot specify fileInfo or contentType.");
			}

			if (metadataDirective == B2MetadataDirective.REPLACE &&
			    (string.IsNullOrWhiteSpace(contentType) || fileInfo == null)) {
				throw new CopyReplaceSetupException("Replace operations must specify fileInfo and contentType.");
			}
			
			var request = FileCopyRequestGenerators.Copy(_options, sourceFileId, newFileName, metadataDirective, contentType, fileInfo, range, destinationBucketId);

			// Send the download request
			var response = await _client.SendAsync(request, cancelToken);

			// Create B2File from response
			return await ResponseParser.ParseResponse<B2File>(response, _api);
		}

		public async Task<B2FileRetentionResponse> UpdateFileRetention(string fileName, string fileId, B2DefaultRetention fileRetention, bool bypassGovernance = false, CancellationToken cancelToken = default(CancellationToken)) {
			RefreshAuthorization(_options, _authorize);
			
			var request = FileMetaDataRequestGenerators.UpdateFileRetention(_options, fileName, fileId, fileRetention, bypassGovernance);

			// Send the download request
			var response = await _client.SendAsync(request, cancelToken);

			// Create B2File from response
			return await ResponseParser.ParseResponse<B2FileRetentionResponse>(response, _api);
		}

		public async Task<B2DownloadAuthorization> GetDownloadAuthorization(string fileNamePrefix, int validDurationInSeconds, string bucketId = "", string b2ContentDisposition = "", CancellationToken cancelToken = default(CancellationToken)) {
			return await GetDownloadAuthorization(fileNamePrefix, validDurationInSeconds, bucketId, b2ContentDisposition, "", "", "", "", "", cancelToken);
		}

		public async Task<B2DownloadAuthorization> GetDownloadAuthorization(string fileNamePrefix, int validDurationInSeconds, string bucketId = "",
			string b2ContentDisposition = "", string b2ContentLanguage = "", string b2Expires = "", string b2CacheControl = "", string b2ContentEncoding = "", string b2ContentType = "", CancellationToken cancelToken = default(CancellationToken)) {
			RefreshAuthorization(_options, _authorize);
			var operationalBucketId = Utilities.DetermineBucketId(_options, bucketId);

			var request = FileDownloadRequestGenerators.GetDownloadAuthorization(_options, fileNamePrefix, validDurationInSeconds, operationalBucketId, b2ContentDisposition);

			// Send the download request
			var response = await _client.SendAsync(request, cancelToken);

			// Create B2File from response
			return await ResponseParser.ParseResponse<B2DownloadAuthorization>(response, _api);
		}

		private async Task<B2File> ParseDownloadResponse(HttpResponseMessage response) {
			await Utilities.CheckForErrors(response, _api);

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
			// File Lock Headers
			if (response.Headers.TryGetValues("X-Bz-File-Retention-Mode", out values)) {
				try {
					file.FileRetention = JsonSerializer.Deserialize<FileRetentionReturn>(values.First());
				}
				catch (Exception e) {
					throw new Exception("Could not deserialize the FileRetention Headers from the download response. See inner exception for details.", e);
				}
			}
			if (response.Headers.TryGetValues("X-Bz-File-Retention-Retain-Until-Timestamp", out values)) {
				file.FileRetentionRetainUntilTimestamp = Convert.ToInt32(values.First());
			}
			if (response.Headers.TryGetValues("X-Bz-File-Legal-Hold", out values)) {
				file.LegalHold = new LegalHold() {IsClientAuthorizedToRead = true, Value = values.First()};
			}
			else {
				file.LegalHold = new LegalHold() {IsClientAuthorizedToRead = false};
			}
			if (response.Headers.TryGetValues("X-Bz-Client-Unauthorized-To-Read", out values)) {
				file.ClientUnauthorizedToRead = values.First().Split(',');
			}

			// File Info Headers
			var fileInfoHeaders = response.Headers.Where(h => h.Key.ToLower().Contains("x-bz-info"));
			var infoData = new Dictionary<string, string>();
			if (fileInfoHeaders.Any()) {
				foreach (var fileInfo in fileInfoHeaders) {
					// Substring to parse out the file info prefix.
					infoData.Add(fileInfo.Key.Substring(10), fileInfo.Value.First());
				}
			}
			file.FileInfo = infoData;
			if (response.Content.Headers.ContentLength.HasValue) {
				// This sucks, but the ContentLength property is a string.
				file.ContentLength = response.Content.Headers.ContentLength.Value.ToString();
			}
			file.FileData = await response.Content.ReadAsByteArrayAsync();

			return await Task.FromResult(file);
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
