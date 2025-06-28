using B2Net.Models;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace B2Net {
	public interface ILargeFiles {

		/// <summary>
		/// Cancel a large file upload
		/// </summary>
		/// <param name="fileId"></param>
		/// <param name="cancelToken"></param>
		/// <returns></returns>
		Task<B2CancelledFile> CancelLargeFile(string fileId, CancellationToken cancelToken = default(CancellationToken));

		/// <summary>
		/// Downloads one file by providing the name of the bucket and the name of the file.
		/// </summary>
		/// <param name="fileId"></param>
		/// <param name="fileName"></param>
		/// <param name="bucketId"></param>
		/// <param name="cancelToken"></param>
		/// <returns></returns>
		Task<B2File> FinishLargeFile(string fileId, string[] partSHA1Array, CancellationToken cancelToken = default(CancellationToken));

		/// <summary>
		/// Get an upload url for use with one Thread.
		/// </summary>
		/// <param name="bucketId"></param>
		/// <param name="cancelToken"></param>
		/// <returns></returns>
		Task<B2UploadPartUrl> GetUploadPartUrl(string fileId, CancellationToken cancelToken = default(CancellationToken));

		/// <summary>
		/// List all the incomplete large file uploads for the supplied bucket
		/// </summary>
		/// <param name="bucketId"></param>
		/// <param name="startFileId"></param>
		/// <param name="maxFileCount"></param>
		/// <param name="cancelToken"></param>
		/// <returns></returns>
		Task<B2IncompleteLargeFiles> ListIncompleteFiles(string bucketId, string startFileId = "", string maxFileCount = "", CancellationToken cancelToken = default(CancellationToken));

		/// <summary>
		/// List the parts of an incomplete large file upload.
		/// </summary>
		/// <param name="fileId"></param>
		/// <param name="startPartNumber"></param>
		/// <param name="maxPartCount"></param>
		/// <param name="cancelToken"></param>
		/// <returns></returns>
		Task<B2LargeFileParts> ListPartsForIncompleteFile(string fileId, int startPartNumber, int maxPartCount, CancellationToken cancelToken = default(CancellationToken));

		/// <summary>
		/// Starts a large file upload.
		/// </summary>
		/// <param name="fileName"></param>
		/// <param name="bucketId"></param>
		/// <param name="cancelToken"></param>
		/// <returns></returns>
		Task<B2File> StartLargeFile(string fileName, string contentType = "", string bucketId = "", Dictionary<string, string> fileInfo = null, CancellationToken cancelToken = default(CancellationToken));

		/// <summary>
		/// Starts a large file upload with file retention settings
		/// </summary>
		/// <param name="fileRetention"></param>
		/// <param name="fileName"></param>
		/// <param name="bucketId"></param>
		/// <param name="cancelToken"></param>
		/// <returns></returns>
		Task<B2File> StartLargeFile(string fileName, B2LargeFileRetention fileRetention, string contentType = "", string bucketId = "", Dictionary<string, string> fileInfo = null, CancellationToken cancelToken = default(CancellationToken));

		/// <summary>
		/// Upload one part of an already started large file upload.
		/// </summary>
		/// <param name="fileData"></param>
		/// <param name="cancelToken"></param>
		/// <returns></returns>
		Task<B2UploadPart> UploadPart(byte[] fileData, int partNumber, B2UploadPartUrl uploadPartUrl, CancellationToken cancelToken = default(CancellationToken));
		
		/// <summary>
		/// Copy a part from an existing file to a large file that is being uploaded
		/// </summary>
		/// <param name="sourceFileId">The ID of the source file to copy from</param>
		/// <param name="destinationLargeFileId">The ID of the large file that is being uploaded</param>
		/// <param name="destinationPartNumber">The part number to copy to (between 1 and 10000)</param>
		/// <param name="range">Optional byte range within the source file, formatted as "bytes=startByte-endByte"</param>
		/// <param name="cancelToken">Cancellation token</param>
		/// <returns>The part that was copied</returns>
		Task<B2LargeFilePart> CopyPart(string sourceFileId, string destinationLargeFileId, int destinationPartNumber, string range = "", CancellationToken cancelToken = default(CancellationToken));
	}
}
