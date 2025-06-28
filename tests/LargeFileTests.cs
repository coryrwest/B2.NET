using B2Net.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace B2Net.Tests {
	[TestClass]
	public class LargeFileTests : BaseTest {
		private B2Bucket TestBucket = new B2Bucket();
		private B2Client Client = null;
		private List<B2File> FilesToDelete = new List<B2File>();
		private string BucketName = "";

#if NETFULL
		private string FilePath => Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, "../../../");
#else
        private string FilePath => Path.Combine(System.AppContext.BaseDirectory, "../../../");
#endif

		[TestInitialize]
		public async Task Initialize() {
			Client = new B2Client(Options);
			BucketName = $"B2NETTestingBucket-{Path.GetRandomFileName().Replace(".", "").Substring(0, 6)}";
			var buckets = await Client.Buckets.GetList();
			B2Bucket existingBucket = null;
			foreach (B2Bucket b2Bucket in buckets) {
				if (b2Bucket.BucketName == BucketName) {
					existingBucket = b2Bucket;
				}
			}

			if (existingBucket != null) {
				TestBucket = existingBucket;
			}
			else {
				TestBucket = await Client.Buckets.Create(BucketName, new B2BucketOptions() {FileLockEnabled = true, BucketType = Models.BucketTypes.allPrivate });
			}
		}

		// THIS TEST DOES NOT PROPERLY CLEAN UP after an exception.
		[TestMethod]
		public async System.Threading.Tasks.Task LargeFileUploadTest() {
			var fileName = "B2LargeFileTest.txt";
			FileStream fileStream = File.OpenRead(Path.Combine(FilePath, fileName));
			byte[] c = null;
            List<byte[]> parts = new List<byte[]>();
		    var shas = new List<string>();
			long fileSize = fileStream.Length;
			long totalBytesParted = 0;
			long minPartSize = 1024 * (5 * 1024);

			while (totalBytesParted < fileSize) {
				var partSize = minPartSize;
				// If last part is less than min part size, get that length
			    if (fileSize - totalBytesParted < minPartSize) {
				    partSize = fileSize - totalBytesParted;
			    }

			    c = new byte[partSize];
				fileStream.Seek(totalBytesParted, SeekOrigin.Begin);
				fileStream.Read(c, 0, c.Length);

				parts.Add(c);
				totalBytesParted += partSize;
			}

		    foreach (var part in parts) {
		        string hash = Utilities.GetSHA1Hash(part);
                shas.Add(hash);
            }

		    B2File start = null;
		    B2File finish = null;
            try {
		        start = await Client.LargeFiles.StartLargeFile(fileName, "", TestBucket.BucketId);

		        for (int i = 0; i < parts.Count; i++) {
		            var uploadUrl = await Client.LargeFiles.GetUploadPartUrl(start.FileId);
		            var part = await Client.LargeFiles.UploadPart(parts[i], i + 1, uploadUrl);
		        }

		        finish = await Client.LargeFiles.FinishLargeFile(start.FileId, shas.ToArray());
		    }
		    catch (Exception e) {
			    await Client.LargeFiles.CancelLargeFile(start.FileId);
		        Console.WriteLine(e);
		        throw;
		    }

			// Clean up.
			FilesToDelete.Add(start);


			Assert.AreEqual(start.FileId, finish.FileId, "File Ids did not match.");
		}

		// THIS TEST DOES NOT PROPERLY CLEAN UP after an exception.
		[TestMethod]
		public async System.Threading.Tasks.Task LargeFileUploadTest_WithRetention() {
			var fileName = "B2LargeFileTest.txt";
			FileStream fileStream = File.OpenRead(Path.Combine(FilePath, fileName));
			byte[] c = null;
			List<byte[]> parts = new List<byte[]>();
			var shas = new List<string>();
			long fileSize = fileStream.Length;
			long totalBytesParted = 0;
			long minPartSize = 1024 * (5 * 1024);

			while (totalBytesParted < fileSize) {
				var partSize = minPartSize;
				// If last part is less than min part size, get that length
				if (fileSize - totalBytesParted < minPartSize) {
					partSize = fileSize - totalBytesParted;
				}

				c = new byte[partSize];
				fileStream.Seek(totalBytesParted, SeekOrigin.Begin);
				fileStream.Read(c, 0, c.Length);

				parts.Add(c);
				totalBytesParted += partSize;
			}

			foreach (var part in parts) {
				string hash = Utilities.GetSHA1Hash(part);
				shas.Add(hash);
			}

			B2File start = null;
			B2File finish = null;

			// Get timestamp
			DateTimeOffset now = DateTimeOffset.UtcNow.AddDays(1);
#if NETFULL
			var UnixEpoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
			long unixTimeMilliseconds = (long) (DateTime.UtcNow - UnixEpoch).TotalMilliseconds;
#else
			long unixTimeMilliseconds = now.ToUnixTimeMilliseconds();
#endif

			try {
				start = await Client.LargeFiles.StartLargeFile(fileName, new B2LargeFileRetention() {
					Mode = RetentionMode.governance,
					RetainUntilTimestamp = unixTimeMilliseconds
				}, "", TestBucket.BucketId);

				for (int i = 0; i < parts.Count; i++) {
					var uploadUrl = await Client.LargeFiles.GetUploadPartUrl(start.FileId);
					var part = await Client.LargeFiles.UploadPart(parts[i], i + 1, uploadUrl);
				}

				finish = await Client.LargeFiles.FinishLargeFile(start.FileId, shas.ToArray());
			}
			catch (Exception e) {
				await Client.LargeFiles.CancelLargeFile(start.FileId);
				Console.WriteLine(e);
				throw;
			}

			// Clean up.
			FilesToDelete.Add(start);


			Assert.AreEqual(start.FileId, finish.FileId, "File Ids did not match.");
			Assert.AreEqual(RetentionMode.governance.ToString(), start.FileRetention.Value.Mode, "File retention mode was not returned correctly.");
			Assert.AreEqual(unixTimeMilliseconds, start.FileRetention.Value.RetainUntilTimestamp, "File retention timestamp was not returned correctly.");
		}


		[TestMethod]
		public async Task LargeFileUploadIncompleteGetPartsTest() {
			var fileName = "B2LargeFileTest.txt";
			FileStream fileStream = File.OpenRead(Path.Combine(FilePath, fileName));
			var stream = new StreamReader(fileStream);
			char[] c = null;
			List<byte[]> parts = new List<byte[]>();
			var shas = new List<string>();

			var listParts = new B2LargeFileParts();

			while (stream.Peek() >= 0) {
				c = new char[1024 * (5 * 1024)];
				stream.Read(c, 0, c.Length);

				parts.Add(Encoding.UTF8.GetBytes(c));
			}

			foreach (var part in parts.Take(2)) {
				string hash = Utilities.GetSHA1Hash(part);
				shas.Add(hash);
			}

			B2File start = null;
			B2File finish = null;
			try {
				start = await Client.LargeFiles.StartLargeFile(fileName, "", TestBucket.BucketId);

				for (int i = 0; i < 2; i++) {
					var uploadUrl = await Client.LargeFiles.GetUploadPartUrl(start.FileId);
					var part = await Client.LargeFiles.UploadPart(parts[i], i + 1, uploadUrl);
				}

				// Now we can list parts and get a result
				listParts = await Client.LargeFiles.ListPartsForIncompleteFile(start.FileId, 1, 100);
			}
			catch (Exception e) {
				Console.WriteLine(e);
				throw;
			}
			finally {
				// Clean up.
				FilesToDelete.Add(start);
			}

			Assert.AreEqual(2, listParts.Parts.Count, "List of parts did not return expected amount of parts.");
		}

		[TestMethod]
		public async Task LargeFileCancelTest() {
			var fileName = "B2LargeFileTest.txt";
			FileStream fileStream = File.OpenRead(Path.Combine(FilePath, fileName));
			var stream = new StreamReader(fileStream);
			char[] c = null;
			List<byte[]> parts = new List<byte[]>();
			var shas = new List<string>();

			var cancelledFile = new B2CancelledFile();

			while (stream.Peek() >= 0) {
				c = new char[1024 * (5 * 1024)];
				stream.Read(c, 0, c.Length);

				parts.Add(Encoding.UTF8.GetBytes(c));
			}

			foreach (var part in parts.Take(2)) {
				string hash = Utilities.GetSHA1Hash(part);
				shas.Add(hash);
			}

			B2File start = null;
			B2File finish = null;
			try {
				start = await Client.LargeFiles.StartLargeFile(fileName, "", TestBucket.BucketId);

				for (int i = 0; i < 2; i++) {
					var uploadUrl = await Client.LargeFiles.GetUploadPartUrl(start.FileId);
					var part = await Client.LargeFiles.UploadPart(parts[i], i + 1, uploadUrl);
				}

				// Now we can list parts and get a result
				cancelledFile = await Client.LargeFiles.CancelLargeFile(start.FileId);
			}
			catch (Exception e) {
				Console.WriteLine(e);
				throw;
			}

			Assert.AreEqual(start.FileId, cancelledFile.FileId, "Started file and Cancelled file do not have the same id.");
		}

		[TestMethod]
		public async Task LargeFileIncompleteListTest() {
			var fileName = "B2LargeFileTest.txt";
			FileStream fileStream = File.OpenRead(Path.Combine(FilePath, fileName));
			var stream = new StreamReader(fileStream);
			char[] c = null;
			List<byte[]> parts = new List<byte[]>();
			var shas = new List<string>();

			var fileList = new B2IncompleteLargeFiles();

			while (stream.Peek() >= 0) {
				c = new char[1024 * (5 * 1024)];
				stream.Read(c, 0, c.Length);

				parts.Add(Encoding.UTF8.GetBytes(c));
			}

			foreach (var part in parts.Take(2)) {
				string hash = Utilities.GetSHA1Hash(part);
				shas.Add(hash);
			}

			B2File start = null;
			B2File finish = null;
			try {
				start = await Client.LargeFiles.StartLargeFile(fileName, "", TestBucket.BucketId);

				for (int i = 0; i < 2; i++) {
					var uploadUrl = await Client.LargeFiles.GetUploadPartUrl(start.FileId);
					var part = await Client.LargeFiles.UploadPart(parts[i], i + 1, uploadUrl);
				}

				// Now we can list parts and get a result
				fileList = await Client.LargeFiles.ListIncompleteFiles(TestBucket.BucketId);
			}
			catch (Exception e) {
				Console.WriteLine(e);
				throw;
			}
			finally {
				var cancelledFile = await Client.LargeFiles.CancelLargeFile(start.FileId);
			}

			Assert.AreEqual(1, fileList.Files.Count, "Incomplete file list count does not match what we expected.");
		}

		[TestMethod]
		public async System.Threading.Tasks.Task LargeFileCopyPartTest() {
			var fileName = "B2LargeFileTest.txt";
			FileStream fileStream = File.OpenRead(Path.Combine(FilePath, fileName));
			byte[] c = null;
			List<byte[]> parts = new List<byte[]>();
			var shas = new List<string>();
			long fileSize = fileStream.Length;
			long totalBytesParted = 0;
			long minPartSize = 1024 * (5 * 1024);

			while (totalBytesParted < fileSize) {
				var partSize = minPartSize;
				// If last part is less than min part size, get that length
				if (fileSize - totalBytesParted < minPartSize) {
					partSize = fileSize - totalBytesParted;
				}

				c = new byte[partSize];
				fileStream.Seek(totalBytesParted, SeekOrigin.Begin);
				fileStream.Read(c, 0, c.Length);

				parts.Add(c);
				totalBytesParted += partSize;
			}

			foreach (var part in parts) {
				string hash = Utilities.GetSHA1Hash(part);
				shas.Add(hash);
			}

			B2File start = null;
			B2File finish = null;
			var uploadedParts = new List<B2UploadPart>();
			try {
				start = await Client.LargeFiles.StartLargeFile(fileName, "", TestBucket.BucketId);

				for (int i = 0; i < parts.Count; i++) {
					var uploadUrl = await Client.LargeFiles.GetUploadPartUrl(start.FileId);
					uploadedParts.Add(await Client.LargeFiles.UploadPart(parts[i], i + 1, uploadUrl));
				}

				finish = await Client.LargeFiles.FinishLargeFile(start.FileId, shas.ToArray());
			} catch (Exception e) {
				await Client.LargeFiles.CancelLargeFile(start.FileId);
				Console.WriteLine(e);
				throw;
			}

			// Clean up at the end
			FilesToDelete.Add(finish);

			// Now we can copy the parts
			var copyFileName = "B2LargeFileCopyTest.txt";
			var startCopy = await Client.LargeFiles.StartLargeFile(copyFileName, "", TestBucket.BucketId);
			var copyParts = new List<B2LargeFilePart>();
			
			try {
				// Copy each part with proper range
				for (int i = 0; i < parts.Count; i++) {
					long startByte = i * minPartSize;
					long endByte = Math.Min((i + 1) * minPartSize - 1, fileSize - 1);
					string range = $"bytes={startByte}-{endByte}";
					var response = await Client.LargeFiles.CopyPart(finish.FileId, startCopy.FileId, i + 1, range);
					copyParts.Add(response);
				}
				
				// Get the SHA1s of the copied parts
				var copyShas = copyParts.Select(p => p.ContentSha1).ToArray();
				var finishedCopy = await Client.LargeFiles.FinishLargeFile(startCopy.FileId, copyShas);

				FilesToDelete.Add(finishedCopy);

				Assert.AreEqual(finish.ContentLength, finishedCopy.ContentLength, "File sizes did not match.");
			} catch (Exception e) {
				await Client.LargeFiles.CancelLargeFile(startCopy.FileId);
				Console.WriteLine(e);
				throw;
			}
		}

		[TestCleanup]
		public async Task Cleanup() {
			foreach (B2File b2File in FilesToDelete) {
				var deletedFile = await Client.Files.Delete(b2File.FileId, b2File.FileName);
			}
			var deletedBucket = await Client.Buckets.Delete(TestBucket.BucketId);
		}
	}
}
