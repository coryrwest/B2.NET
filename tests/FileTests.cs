using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Configuration;
using System.Text;
using System.Threading.Tasks;
using B2Net.Http;
using B2Net.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace B2Net.Tests {
	[TestClass]
	public class FileTests : BaseTest {
		private B2Bucket TestBucket = new B2Bucket();
		private B2Client Client = null;
		private List<B2File> FilesToDelete = new List<B2File>();

		[TestInitialize]
		public void Initialize() {
			Client = new B2Client(Options);
			Options = Client.Authorize().Result;

			var buckets = Client.Buckets.GetList().Result;
			B2Bucket existingBucket = null;
			foreach (B2Bucket b2Bucket in buckets) {
				if (b2Bucket.BucketName == "B2NETTestingBucket") {
					existingBucket = b2Bucket;
				}
			}

			if (existingBucket != null) {
				TestBucket = existingBucket;
			} else {
				TestBucket = Client.Buckets.Create("B2NETTestingBucket", BucketTypes.allPrivate).Result;
			}
		}

		[TestMethod]
		public void GetListTest() {
			var fileName = "B2Test.txt";
			var fileData = File.ReadAllBytes(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, fileName));
			var file = Client.Files.Upload(fileData, fileName, TestBucket.BucketId).Result;
			// Clean up.
			FilesToDelete.Add(file);

			var list = Client.Files.GetList(bucketId: TestBucket.BucketId).Result.Files;

			Assert.AreEqual(1, list.Count, list.Count + " files found.");
		}

		//[TestMethod]
		//public void EmptyBucket() {
		//	var list = Client.Files.GetList(bucketId: TestBucket.BucketId).Result.Files;

		//	foreach (B2File b2File in list) {
		//		var deletedFile = Client.Files.Delete(b2File.FileId, b2File.FileName).Result;
		//	}
		//}

		//[TestMethod]
		//public void HideFileTest() {
		//	var fileName = "B2Test.txt";
		//	var fileData = File.ReadAllBytes(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, fileName));
		//	string hash = Utilities.GetSHA1Hash(fileData);
		//	var file = Client.Files.Upload(fileData, fileName, TestBucket.BucketId).Result;
		//	// Clean up.
		//	FilesToDelete.Add(file);

		//	Assert.AreEqual(hash, file.ContentSHA1, "File hashes did not match.");

		//          var hiddenFile = Client.Files.Hide(file.FileName, TestBucket.BucketId).Result;

		//          Assert.IsTrue(hiddenFile.Action == "hide");

		//          // Unhide the file so we can delete it later
		//          hiddenFile = Client.Files.Hide(file.FileName, TestBucket.BucketId).Result;
		//      }

		[TestMethod]
		public void FileUploadTest() {
			var fileName = "B2Test.txt";
			var fileData = File.ReadAllBytes(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, fileName));
			string hash = Utilities.GetSHA1Hash(fileData);
			var file = Client.Files.Upload(fileData, fileName, TestBucket.BucketId).Result;

			// Clean up.
			FilesToDelete.Add(file);

			Assert.AreEqual(hash, file.ContentSHA1, "File hashes did not match.");
		}

		//[TestMethod]
		//public void FileUploadEncodingTest() {
		//	var fileName = "B2 Test File.txt";
		//	var fileData = File.ReadAllBytes(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, fileName));
		//	string hash = Utilities.GetSHA1Hash(fileData);
		//	var file = Client.Files.Upload(fileData, fileName, TestBucket.BucketId).Result;

		//	// Clean up.
		//	FilesToDelete.Add(file);

		//	Assert.AreEqual(hash, file.ContentSHA1, "File hashes did not match.");
		//}

		[TestMethod]
        public void FileUploadWithInfoTest()
        {
            var fileName = "B2Test.txt";
            var fileData = File.ReadAllBytes(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, fileName));
            string hash = Utilities.GetSHA1Hash(fileData);

            var fileInfo = new Dictionary<string, string>();
            fileInfo.Add("FileInfoTest", "1234");

            var file = Client.Files.Upload(fileData, fileName, TestBucket.BucketId, fileInfo).Result;

            // Clean up.
            FilesToDelete.Add(file);

            Assert.AreEqual(hash, file.ContentSHA1, "File hashes did not match.");
            Assert.AreEqual(1, file.FileInfo.Count, "File info count was off.");
        }

        [TestMethod]
        public void FileDownloadNameTest()
        {
            var fileName = "B2Test.txt";
            var fileData = File.ReadAllBytes(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, fileName));
            string hash = Utilities.GetSHA1Hash(fileData);
            var file = Client.Files.Upload(fileData, fileName, TestBucket.BucketId).Result;
            // Clean up.
            FilesToDelete.Add(file);

            Assert.AreEqual(hash, file.ContentSHA1, "File hashes did not match.");

            // Test download
            var download = Client.Files.DownloadByName(file.FileName, TestBucket.BucketName).Result;
            var downloadHash = Utilities.GetSHA1Hash(download.FileData);

            Assert.AreEqual(hash, downloadHash);
        }

        [TestMethod]
        public void FileDownloadWithInfoTest()
        {
            var fileName = "B2Test.txt";
            var fileData = File.ReadAllBytes(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, fileName));
            string hash = Utilities.GetSHA1Hash(fileData);

            var fileInfo = new Dictionary<string, string>();
            fileInfo.Add("FileInfoTest", "1234");

            var file = Client.Files.Upload(fileData, fileName, TestBucket.BucketId, fileInfo).Result;
            // Clean up.
            FilesToDelete.Add(file);

            Assert.AreEqual(hash, file.ContentSHA1, "File hashes did not match.");

            // Test download
            var download = Client.Files.DownloadById(file.FileId).Result;
            var downloadHash = Utilities.GetSHA1Hash(download.FileData);

            Assert.AreEqual(hash, downloadHash);
            Assert.AreEqual(1, download.FileInfo.Count);
        }

        [TestMethod]
		public void FileDownloadIdTest() {
			var fileName = "B2Test.txt";
			var fileData = File.ReadAllBytes(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, fileName));
			string hash = Utilities.GetSHA1Hash(fileData);
			var file = Client.Files.Upload(fileData, fileName, TestBucket.BucketId).Result;
			// Clean up.
			FilesToDelete.Add(file);

			Assert.AreEqual(hash, file.ContentSHA1, "File hashes did not match.");

			// Test download
			var download = Client.Files.DownloadById(file.FileId).Result;
			var downloadHash = Utilities.GetSHA1Hash(download.FileData);

			Assert.AreEqual(hash, downloadHash);
		}

		[TestMethod]
		public void FileDeleteTest() {
			var fileName = "B2Test.txt";
			var fileData = File.ReadAllBytes(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, fileName));
			string hash = Utilities.GetSHA1Hash(fileData);
			var file = Client.Files.Upload(fileData, fileName, TestBucket.BucketId).Result;

			Assert.AreEqual(hash, file.ContentSHA1, "File hashes did not match.");

			// Clean up. We have to delete the file before we can delete the bucket
			var deletedFile = Client.Files.Delete(file.FileId, file.FileName).Result;

			Assert.AreEqual(file.FileId, deletedFile.FileId, "The deleted file id did not match.");
		}

		[TestMethod]
		public void ListVersionsTest() {
			var fileName = "B2Test.txt";
			var fileData = File.ReadAllBytes(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, fileName));
			string hash = Utilities.GetSHA1Hash(fileData);
			var file = Client.Files.Upload(fileData, fileName, TestBucket.BucketId).Result;
			// Clean up.
			FilesToDelete.Add(file);

			Assert.AreEqual(hash, file.ContentSHA1, "File hashes did not match.");

			var versions = Client.Files.GetVersions(file.FileName, file.FileId, bucketId: TestBucket.BucketId).Result;

			Assert.AreEqual(1, versions.Files.Count);
		}

		[TestMethod]
		public void GetInfoTest() {
			var fileName = "B2Test.txt";
			var fileData = File.ReadAllBytes(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, fileName));
			string hash = Utilities.GetSHA1Hash(fileData);

            var fileInfo = new Dictionary<string, string>();
            fileInfo.Add("FileInfoTest", "1234");

            var file = Client.Files.Upload(fileData, fileName, TestBucket.BucketId, fileInfo).Result;
			// Clean up.
			FilesToDelete.Add(file);

			Assert.AreEqual(hash, file.ContentSHA1, "File hashes did not match.");

			var info = Client.Files.GetInfo(file.FileId).Result;

            Assert.AreEqual(file.UploadTimestamp, info.UploadTimestamp);
            Assert.AreEqual(1, info.FileInfo.Count);
        }

		[TestCleanup]
		public void Cleanup() {
			foreach (B2File b2File in FilesToDelete) {
				var deletedFile = Client.Files.Delete(b2File.FileId, b2File.FileName).Result;
			}
			var deletedBucket = Client.Buckets.Delete(TestBucket.BucketId).Result;
		}
	}
}
