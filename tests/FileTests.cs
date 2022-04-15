using System;
using B2Net.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using B2Net.Http;

namespace B2Net.Tests {
	[TestClass]
	public class FileTests : BaseTest {
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
			Client = CreateB2ClientWithNormalKey();
			BucketName = $"B2NETTestingBucket-NonRandom";

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
				TestBucket = await Client.Buckets.Create(BucketName, BucketTypes.allPrivate);
			}
		}

		[TestMethod]
		public async Task GetListTest() {
			var fileName = "B2Test.txt";
			var fileData = File.ReadAllBytes(Path.Combine(FilePath, fileName));
			var file = await Client.Files.Upload(fileData, fileName, TestBucket.BucketId);
			// Clean up.
			FilesToDelete.Add(file);

			var list = (await Client.Files.GetList(bucketId: TestBucket.BucketId)).Files;

			Assert.AreEqual(1, list.Count, list.Count + " files found.");
		}

		[TestMethod]
		public async Task GetLongListTest() {
			var fileName = "B2Test.txt";
			var fileData = await File.ReadAllBytesAsync(Path.Combine(FilePath, fileName));
			var file = await Client.Files.Upload(fileData, fileName, TestBucket.BucketId);
			// Clean up.
			FilesToDelete.Add(file);

			var list = new List<B2File>();
			var enumerable = Client.Files.GetLongList(bucketId: TestBucket.BucketId);
			await foreach (var item in enumerable) {
				list.Add(item);
			}


			Assert.AreEqual(1, list.Count, list.Count + " files found.");
		}

		[TestMethod]
		public void GetListWithPrefixTest() {
			var fileName = "B2Test.txt";
			var fileNameWithFolder = "test/B2Test.txt";
			var fileData = File.ReadAllBytes(Path.Combine(FilePath, fileName));
			var file = Client.Files.Upload(fileData, fileName, TestBucket.BucketId).Result;
			var fileFolder = Client.Files.Upload(fileData, fileNameWithFolder, TestBucket.BucketId).Result;
			// Clean up.
			FilesToDelete.Add(file);
			FilesToDelete.Add(fileFolder);

			var list = Client.Files.GetListWithPrefixOrDemiliter(bucketId: TestBucket.BucketId, prefix: "test").Result
				.Files;

			Assert.AreEqual(1, list.Count, list.Count + " files found.");
		}

		[TestMethod]
		public void GetListWithPrefixAndDelimiterTest() {
			var fileName = "B2Test.txt";
			var fileNameWithFolder = "test/B2Test.txt";
			var fileData = File.ReadAllBytes(Path.Combine(FilePath, fileName));
			var file = Client.Files.Upload(fileData, fileName, TestBucket.BucketId).Result;
			var fileFolder = Client.Files.Upload(fileData, fileNameWithFolder, TestBucket.BucketId).Result;
			// Clean up.
			FilesToDelete.Add(file);
			FilesToDelete.Add(fileFolder);

			var list = Client.Files
				.GetListWithPrefixOrDemiliter(bucketId: TestBucket.BucketId, prefix: "test", delimiter: "/").Result
				.Files;

			Assert.AreEqual(1, list.Count, list.Count + " files found.");
			Assert.AreEqual("test/", list.First().FileName, "File names to not match.");
		}

		[TestMethod]
		public void GetListWithDelimiterTest() {
			var fileName = "B2Test.txt";
			var fileNameWithFolder = "test/B2Test.txt";
			var fileNameWithFolder2 = "test2/B2Test.txt";
			var fileData = File.ReadAllBytes(Path.Combine(FilePath, fileName));
			var file = Client.Files.Upload(fileData, fileNameWithFolder2, TestBucket.BucketId).Result;
			var fileFolder = Client.Files.Upload(fileData, fileNameWithFolder, TestBucket.BucketId).Result;
			// Clean up.
			FilesToDelete.Add(file);
			FilesToDelete.Add(fileFolder);

			var list = Client.Files.GetListWithPrefixOrDemiliter(bucketId: TestBucket.BucketId, delimiter: "/").Result
				.Files;

			Assert.AreEqual(2, list.Count, list.Count + " files found.");
			Assert.IsTrue(list.All(f => f.Action == "folder"), "Not all list items were folders.");
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
			var fileData = File.ReadAllBytes(Path.Combine(FilePath, fileName));
			string hash = Utilities.GetSHA1Hash(fileData);
			var file = Client.Files.Upload(fileData, fileName, TestBucket.BucketId).Result;

			// Clean up.
			FilesToDelete.Add(file);

			Assert.AreEqual(hash, file.ContentSHA1, "File hashes did not match.");
		}

		[TestMethod]
		public void FileUploadUsingUploadUrlTest() {
			var fileName = "B2Test.txt";
			var fileData = File.ReadAllBytes(Path.Combine(FilePath, fileName));
			string hash = Utilities.GetSHA1Hash(fileData);

			var uploadUrl = Client.Files.GetUploadUrl(TestBucket.BucketId).Result;

			var file = Client.Files.Upload(fileData, fileName, uploadUrl, true, TestBucket.BucketId).Result;

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
		public void FileUploadWithInfoTest() {
			var fileName = "B2Test.txt";
			var fileData = File.ReadAllBytes(Path.Combine(FilePath, fileName));
			string hash = Utilities.GetSHA1Hash(fileData);

			var fileInfo = new Dictionary<string, string>() {
				{ "FileInfoTest", "1234" }
			};

			var file = Client.Files.Upload(fileData, fileName, TestBucket.BucketId, fileInfo).Result;

			// Clean up.
			FilesToDelete.Add(file);

			Assert.AreEqual(hash, file.ContentSHA1, "File hashes did not match.");
			Assert.AreEqual(1, file.FileInfo.Count, "File info count was off.");
		}

		[TestMethod]
		public void FileUploadStreamTest() {
			var fileName = "B2Test.txt";
			var bytes = File.ReadAllBytes(Path.Combine(FilePath, fileName));

			string hash = Utilities.GetSHA1Hash(bytes);
			var hashBytes = Encoding.UTF8.GetBytes(hash);

			var fileData = new MemoryStream(bytes.Concat(hashBytes).ToArray());

			var uploadUrl = Client.Files.GetUploadUrl(TestBucket.BucketId).Result;

			var file = Client.Files.Upload(fileData, fileName, uploadUrl, "", false, TestBucket.BucketId).Result;

			// Clean up.
			FilesToDelete.Add(file);

			Assert.AreEqual(hash, file.ContentSHA1, "File hashes did not match.");
		}

		[TestMethod]
		public void FileUploadStreamNoSHATest() {
			var fileName = "B2Test.txt";
			var bytes = File.ReadAllBytes(Path.Combine(FilePath, fileName));

			var fileData = new MemoryStream(bytes);

			var uploadUrl = Client.Files.GetUploadUrl(TestBucket.BucketId).Result;

			var file = Client.Files.Upload(fileData, fileName, uploadUrl, "", false, TestBucket.BucketId, null, true)
				.Result;

			// Clean up.
			FilesToDelete.Add(file);

			Assert.IsTrue(file.ContentSHA1.StartsWith("unverified"),
				$"File was verified when it should not have been: {file.ContentSHA1}.");
		}

		[TestMethod]
		public void FileDownloadNameTest() {
			var fileName = "B2Test.txt";
			var fileData = File.ReadAllBytes(Path.Combine(FilePath, fileName));
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
		public void FileDownloadWithInfoTest() {
			var fileName = "B2Test.txt";
			var fileData = File.ReadAllBytes(Path.Combine(FilePath, fileName));
			string hash = Utilities.GetSHA1Hash(fileData);

			var fileInfo = new Dictionary<string, string>() {
				{ "FileInfoTest", "1234" }
			};

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
			var fileData = File.ReadAllBytes(Path.Combine(FilePath, fileName));
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
		public void FileDownloadFolderTest() {
			var fileName = "B2Test.txt";
			var fileData = File.ReadAllBytes(Path.Combine(FilePath, fileName));
			string hash = Utilities.GetSHA1Hash(fileData);
			var file = Client.Files.Upload(fileData, "B2Folder/Test/File.txt", TestBucket.BucketId).Result;
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
			var fileData = File.ReadAllBytes(Path.Combine(FilePath, fileName));
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
			var fileData = File.ReadAllBytes(Path.Combine(FilePath, fileName));
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
			var fileData = File.ReadAllBytes(Path.Combine(FilePath, fileName));
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

		[TestMethod]
		public void GetDownloadAuthorizationTest() {
			var downloadAuth = Client.Files.GetDownloadAuthorization("Test", 120, TestBucket.BucketId).Result;

			Assert.AreEqual("Test", downloadAuth.FileNamePrefix, "File prefixes were not the same.");
		}

		[TestMethod]
		public async Task CopyFile() {
			var fileName = "B2Test.txt";
			var fileData = File.ReadAllBytes(Path.Combine(FilePath, fileName));
			var file = Client.Files.Upload(fileData, fileName, TestBucket.BucketId).Result;
			// Clean up.
			FilesToDelete.Add(file);

			var copied = await Client.Files.Copy(file.FileId, "B2TestCopy.txt");
			// Clean up.
			FilesToDelete.Add(copied);

			Assert.AreEqual("copy", copied.Action, "Action was not as expected for the copy operation.");
			Assert.AreEqual(fileData.Length, copied.ContentLength,
				"Length of the two files was not the same.");
		}

		[TestMethod]
		public async Task ReplaceFile() {
			var fileName = "B2Test.txt";
			var fileData = File.ReadAllBytes(Path.Combine(FilePath, fileName));
			var file = Client.Files.Upload(fileData, fileName, TestBucket.BucketId).Result;
			// Clean up.
			FilesToDelete.Add(file);

			var copied = await Client.Files.Copy(file.FileId, "B2TestCopy.txt", B2MetadataDirective.REPLACE,
				"text/plain", new Dictionary<string, string>() {
					{ "FileInfoTest", "1234" }
				});
			// Clean up.
			FilesToDelete.Add(copied);

			Assert.IsTrue(copied.FileInfo.ContainsKey("fileinfotest"),
				"FileInfo was not as expected for the replace operation.");
			Assert.AreEqual(fileData.Length, copied.ContentLength,
				"Length of the two files was not the same.");
		}

		[TestMethod]
		[ExpectedException(typeof(CopyReplaceSetupException),
			"Copy did not fail when disallowed fields were provided.")]
		public async Task CopyFileWithDisallowedFields() {
			var fileName = "B2Test.txt";
			var fileData = File.ReadAllBytes(Path.Combine(FilePath, fileName));
			var file = Client.Files.Upload(fileData, fileName, TestBucket.BucketId).Result;

			// Clean up.
			FilesToDelete.Add(file);

			var copied = await Client.Files.Copy(file.FileId, "B2TestCopy.txt", contentType: "b2/x-auto");
		}

		[TestMethod]
		[ExpectedException(typeof(CopyReplaceSetupException), "Replace did not fail when fields were missing.")]
		public async Task ReplaceFileWithMissingFields() {
			var fileName = "B2Test.txt";
			var fileData = File.ReadAllBytes(Path.Combine(FilePath, fileName));
			var file = Client.Files.Upload(fileData, fileName, TestBucket.BucketId).Result;
			// Clean up.
			FilesToDelete.Add(file);

			var copied = await Client.Files.Copy(file.FileId, "B2TestCopy.txt", B2MetadataDirective.REPLACE);
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
