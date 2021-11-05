using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using B2Net.Models;

namespace B2Net {
	public interface IFiles {

		/// <summary>
		/// Deletes the specified file version
		/// </summary>
		/// <param name="fileId"></param>
		/// <param name="fileName"></param>
		/// <param name="cancelToken"></param>
		/// <returns></returns>
		Task<B2File> Delete(string fileId, string fileName, CancellationToken cancelToken = default(CancellationToken));

		/// <summary>
		/// Downloads one file from B2.
		/// </summary>
		/// <param name="fileId"></param>
		/// <param name="cancelToken"></param>
		/// <returns></returns>
		Task<B2File> DownloadById(string fileId, CancellationToken cancelToken = default(CancellationToken));

		/// <summary>
		/// Downloads a file from B2 using the byte range specified. For use with the Large File API.
		/// </summary>
		/// <param name="fileId"></param>
		/// <param name="cancelToken"></param>
		/// <returns></returns>
		Task<B2File> DownloadById(string fileId, int startByte, int endByte, CancellationToken cancelToken = default(CancellationToken));

		/// <summary>
		/// Downloads one file by providing the name of the bucket and the name of the file.
		/// </summary>
		/// <param name="fileId"></param>
		/// <param name="fileName"></param>
		/// <param name="bucketId"></param>
		/// <param name="cancelToken"></param>
		/// <returns></returns>
		Task<B2File> DownloadByName(string fileName, string bucketName, CancellationToken cancelToken = default(CancellationToken));
		/// <summary>
		/// Get the authorization token for a file download. Refer to B2 docs for further details on optional parameters.
		/// https://www.backblaze.com/b2/docs/b2_get_download_authorization.html
		/// </summary>
		/// <param name="fileNamePrefix"></param>
		/// <param name="validDurationInSeconds">The number of seconds before the authorization token will expire. The minimum value is 1 second. The maximum value is 604800 which is one week in seconds.</param>
		/// <param name="bucketId"></param>
		/// <param name="b2ContentDisposition">If this is present, download requests using the returned authorization must include the same value for b2ContentDisposition</param>
		/// <param name="b2ContentLanguage">If this is present, download requests using the returned authorization must include the same value for b2ContentLanguage</param>
		/// <param name="b2Expires">If this is present, download requests using the returned authorization must include the same value for b2Expires</param>
		/// <param name="b2CacheControl">If this is present, download requests using the returned authorization must include the same value for b2CacheControl</param>
		/// <param name="b2ContentEncoding">If this is present, download requests using the returned authorization must include the same value for b2ContentEncoding</param>
		/// <param name="b2ContentType">If this is present, download requests using the returned authorization must include the same value for b2ContentType</param>
		/// <param name="cancelToken"></param>
		/// <returns></returns>
		Task<B2DownloadAuthorization> GetDownloadAuthorization(string fileNamePrefix, int validDurationInSeconds, string bucketId = "", 
			string b2ContentDisposition = "", string b2ContentLanguage = "", string b2Expires = "", string b2CacheControl = "", string b2ContentEncoding = "", string b2ContentType = "", CancellationToken cancelToken = default(CancellationToken));

		/// <summary>
		/// Downloads a file part by providing the name of the bucket and the name and byte range of the file.
		/// For use with the Larg File API.
		/// </summary>
		/// <param name="fileName"></param>
		/// <param name="bucketName"></param>
		/// <param name="startBytes"></param>
		/// <param name="endBytes"></param>
		/// <param name="cancelToken"></param>
		/// <returns></returns>
		Task<B2File> DownloadByName(string fileName, string bucketName, int startByte, int endByte, CancellationToken cancelToken = default(CancellationToken));

		/// <summary>
		/// EXPERIMENTAL: This functionality is not officially part of the Backblaze B2 API and may change or break at any time.
		/// This will return a friendly URL that can be shared to download the file. This depends on the Bucket that the file resides
		/// in to be allPublic.
		/// </summary>
		/// <param name="fileName"></param>
		/// <param name="bucketName"></param>
		/// <param name="cancelToken"></param>
		/// <returns></returns>
		string GetFriendlyDownloadUrl(string fileName, string bucketName, CancellationToken cancelToken = default(CancellationToken));

		/// <summary>
		/// Gets information about one file stored in B2.
		/// </summary>
		/// <param name="fileId"></param>
		/// <param name="cancelToken"></param>
		/// <returns></returns>
		Task<B2File> GetInfo(string fileId, CancellationToken cancelToken = default(CancellationToken));

		/// <summary>
		/// Lists the names of all non-hidden files in a bucket, starting at a given name.
		/// </summary>
		/// <param name="bucketId"></param>
		/// <param name="startFileName"></param>
		/// <param name="maxFileCount"></param>
		/// <param name="cancelToken"></param>
		/// <returns></returns>
		Task<B2FileList> GetList(string startFileName = "", int? maxFileCount = null, string bucketId = "", CancellationToken cancelToken = default(CancellationToken));

		/// <summary>
		/// BETA: Lists the names of all non-hidden files in a bucket, starting at a given name. With an optional file prefix or delimiter.
		/// See here for more details: https://www.backblaze.com/b2/docs/b2_list_file_names.html
		/// </summary>
		/// <param name="startFileName"></param>
		/// <param name="prefix"></param>
		/// <param name="delimiter"></param>
		/// <param name="maxFileCount"></param>
		/// <param name="bucketId"></param>
		/// <param name="cancelToken"></param>
		/// <returns></returns>
		Task<B2FileList> GetListWithPrefixOrDemiliter(string startFileName = "", string prefix = "", string delimiter = "", int? maxFileCount = null, string bucketId = "", CancellationToken cancelToken = default(CancellationToken));

		/// <summary>
		/// get an upload url for use with one Thread.
		/// </summary>
		/// <param name="bucketId"></param>
		/// <param name="cancelToken"></param>
		/// <returns></returns>
		Task<B2UploadUrl> GetUploadUrl(string bucketId = "", CancellationToken cancelToken = default(CancellationToken));

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
		Task<B2FileList> GetVersions(string startFileName = "", string startFileId = "", int? maxFileCount = null, string bucketId = "", CancellationToken cancelToken = default(CancellationToken));

		/// <summary>
		/// BETA: Lists all of the versions of all of the files contained in one bucket,
		/// in alphabetical order by file name, and by reverse of date/time uploaded
		/// for versions of files with the same name. With an optional file prefix or delimiter.
		/// See here for more details: https://www.backblaze.com/b2/docs/b2_list_file_versions.html
		/// </summary>
		/// <param name="startFileName"></param>
		/// <param name="startFileId"></param>
		/// <param name="prefix"></param>
		/// <param name="delimiter"></param>
		/// <param name="maxFileCount"></param>
		/// <param name="bucketId"></param>
		/// <param name="cancelToken"></param>
		/// <returns></returns>
		Task<B2FileList> GetVersionsWithPrefixOrDelimiter(string startFileName = "", string startFileId = "", string prefix = "", string delimiter = "", int? maxFileCount = null, string bucketId = "", CancellationToken cancelToken = default(CancellationToken));

		/// <summary>
		/// Hides or Unhides a file so that downloading by name will not find the file,
		/// but previous versions of the file are still stored. See File
		/// Versions about what it means to hide a file.
		/// </summary>
		/// <param name="fileName"></param>
		/// <param name="bucketId"></param>
		/// <param name="cancelToken"></param>
		/// <returns></returns>
		Task<B2File> Hide(string fileName, string bucketId = "", string fileId = "", CancellationToken cancelToken = default(CancellationToken));

		/// <summary>
		/// DEPRECATED: This method has been deprecated in favor of the Upload that takes an UploadUrl parameter.
		/// The other Upload method is the preferred, and more efficient way, of uploading to B2.
		/// </summary>
		/// <param name="fileData"></param>
		/// <param name="fileName"></param>
		/// <param name="bucketId"></param>
		/// <param name="cancelToken"></param>
		/// <returns></returns>
		[Obsolete("This method has been deprecated in favor of the Upload that takes an UploadUrl parameter. The other Upload method is the preferred, and more efficient way, of uploading to B2.", false)]
		Task < B2File> Upload(byte[] fileData, string fileName, string bucketId = "", Dictionary<string, string> fileInfo = null, CancellationToken cancelToken = default(CancellationToken));

		/// <summary>
		/// Uploads one file to B2, returning its unique file ID. Filename will be URL Encoded.
		/// </summary>
		/// <param name="fileData"></param>
		/// <param name="fileName"></param>
		/// <param name="bucketId"></param>
		/// <param name="cancelToken"></param>
		/// <returns></returns>
		[Obsolete("This method has been deprecated in favor of the B2FileUploadContext overload.", false)]
		Task<B2File> Upload(byte[] fileData, string fileName, B2UploadUrl uploadUrl, string bucketId = "", Dictionary<string, string> fileInfo = null, CancellationToken cancelToken = default(CancellationToken));

		/// <summary>
		/// Uploads one file to B2, returning its unique file ID. Filename will be URL Encoded. If auto retry
		/// is set true it will retry a failed upload once after 1 second.
		/// </summary>
		/// <param name="fileData"></param>
		/// <param name="fileName"></param>
		/// <param name="uploadUrl"></param>
		/// <param name="bucketId"></param>
		/// <param name="autoRetry">Retry a failed upload one time after waiting for 1 second.</param>
		/// <param name="fileInfo"></param>
		/// <param name="cancelToken"></param>
		/// <returns></returns>
		[Obsolete("This method has been deprecated in favor of the B2FileUploadContext overload.", false)]
		Task<B2File> Upload(byte[] fileData, string fileName, B2UploadUrl uploadUrl, bool autoRetry, string bucketId = "", Dictionary<string, string> fileInfo = null, CancellationToken cancelToken = default(CancellationToken));

		/// <summary>
		/// Uploads one file to B2, returning its unique file ID. Filename will be URL Encoded. If auto retry
		/// is set true it will retry a failed upload once after 1 second.
		/// </summary>
		/// <param name="fileData"></param>
		/// <param name="fileName"></param>
		/// <param name="uploadUrl"></param>
		/// <param name="bucketId"></param>
		/// <param name="contentType"></param>
		/// <param name="autoRetry">Retry a failed upload one time after waiting for 1 second.</param>
		/// <param name="fileInfo"></param>
		/// <param name="cancelToken"></param>
		/// <returns></returns>
		[Obsolete("This method has been deprecated in favor of the B2FileUploadContext overload.", false)]
		Task<B2File> Upload(byte[] fileData, string fileName, B2UploadUrl uploadUrl, string contentType, bool autoRetry, string bucketId = "", Dictionary<string, string> fileInfo = null, CancellationToken cancelToken = default(CancellationToken));

		/// <summary>
		/// Uploads one file to B2, returning its unique file ID. Filename will be URL Encoded. If auto retry
		/// is set true it will retry a failed upload once after 1 second. WARNING: This method will convert your byte array
		/// into a stream before uploading. If you want to avoid this and control your allocations, use the Stream compatible overload.
		/// </summary>
		/// <param name="fileData"></param>
		/// <param name="uploadContext"></param>
		/// <param name="cancelToken"></param>
		/// <returns></returns>
		Task<B2File> Upload(byte[] fileData, B2FileUploadContext uploadContext, CancellationToken cancelToken = default(CancellationToken));

		/// <summary>
		/// Uploads one file to B2 using a stream, returning its unique file ID. Filename will be URL Encoded. If auto retry
		/// is set true it will retry a failed upload once after 1 second. If you don't want to use a SHA1 for the stream set dontSHA.
		/// </summary>
		/// <param name="fileDataWithSHA"></param>
		/// <param name="fileName"></param>
		/// <param name="uploadUrl"></param>
		/// <param name="contentType"></param>
		/// <param name="autoRetry"></param>
		/// <param name="bucketId"></param>
		/// <param name="fileInfo"></param>
		/// <param name="dontSHA"></param>
		/// <param name="cancelToken"></param>
		/// <returns></returns>
		[Obsolete("This method has been deprecated in favor of the B2FileUploadContext overload.", false)]
		Task<B2File> Upload(Stream fileDataWithSHA, string fileName, B2UploadUrl uploadUrl, string contentType, bool autoRetry, string bucketId = "", Dictionary<string, string> fileInfo = null, bool dontSHA = false, CancellationToken cancelToken = default(CancellationToken));

		/// <summary>
		/// Uploads one file to B2 using a stream, returning its unique file ID. Filename will be URL Encoded. If auto retry
		/// is set true it will retry a failed upload once after 1 second. If you don't want to use a SHA1 for the stream set dontSHA.
		/// </summary>
		/// <param name="fileDataWithSHA"></param>
		/// <param name="uploadUrl"></param>
		/// <param name="autoRetry"></param>
		/// <param name="uploadContext">Used for additional options when uploading to B2. File Locks, Legal Holds, Content Disposition, etc.</param>
		/// <param name="dontSHA"></param>
		/// <param name="cancelToken"></param>
		/// <returns></returns>
		Task<B2File> Upload(Stream fileDataWithSHA, B2FileUploadContext uploadContext, bool dontSHA = false, CancellationToken cancelToken = default(CancellationToken));

		/// <summary>
		/// Copy or Replace a file stored in B2. This will copy the file on B2's servers, resulting in no download or upload charges.
		/// </summary>
		/// <param name="sourceFileId"></param>
		/// <param name="newFileName"></param>
		/// <param name="metadataDirective">COPY or REPLACE. COPY will not allow any changes to File Info or Content Type. REPLACE will.</param>
		/// <param name="contentType"></param>
		/// <param name="fileInfo"></param>
		/// <param name="range">byte range to copy.</param>
		/// <param name="cancelToken"></param>
		/// <returns></returns>
		Task<B2File> Copy(string sourceFileId, string newFileName, B2MetadataDirective metadataDirective = B2MetadataDirective.COPY, string contentType = "", Dictionary<string, string> fileInfo = null, string range = "", string destinationBucketId = "", CancellationToken cancelToken = default(CancellationToken));
		/// <summary>
		/// Update the File Lock retention settings for an existing file.
		/// https://www.backblaze.com/b2/docs/b2_update_file_retention.html
		/// </summary>
		/// <param name="fileName"></param>
		/// <param name="fileId"></param>
		/// <param name="fileRetention"></param>
		/// <param name="bypassGovernance">Must be specified and set to true if deleting an existing governance mode retention setting or shortening an existing governance mode retention period</param>
		/// <returns></returns>
		Task<B2FileRetentionResponse> UpdateFileRetention(string fileName, string fileId, B2DefaultRetention fileRetention, bool bypassGovernance = false, CancellationToken cancelToken = default(CancellationToken));
	}
}
