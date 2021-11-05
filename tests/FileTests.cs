using System;
using B2Net.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using B2Net.Http;

namespace B2Net.Tests {
	[TestClass]
	public class FileTests : BaseTest {
		private B2Bucket TestBucket = new B2Bucket();
		private B2Client Client = null;
		private static string BucketPrefix = "B2NETTestingBucket";
		private static string BucketName = $"{BucketPrefix}-{Path.GetRandomFileName().Replace(".", "").Substring(0, 6)}";
		private HttpClient HttpClient;

#if NETFULL
		private string FilePath => Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, "../../../");
#else
		private string FilePath => Path.Combine(System.AppContext.BaseDirectory, "../../../");
#endif

		// Initialize cannot be static so we have to use Test instead of ClassInitialize
		[TestInitialize]
		public void Initialize() {
			Client = new B2Client(Options);
			HttpClient = HttpClientFactory.CreateHttpClient(200);
			HttpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", Options.AuthorizationToken);

			var buckets = Client.Buckets.GetList().Result;
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
				TestBucket = Client.Buckets.Create(BucketName, BucketTypes.allPrivate).Result;
			}
		}

		[TestMethod]
		public void GetListTest() {
			var fileName = "B2Test.txt";
			var fileData = File.ReadAllBytes(Path.Combine(FilePath, fileName));
			var file = Client.Files.Upload(fileData, fileName, TestBucket.BucketId).Result;

			var list = Client.Files.GetList(bucketId: TestBucket.BucketId).Result.Files;

			Assert.AreEqual(1, list.Count, list.Count + " files found.");
		}

		[TestMethod]
		public void GetListWithPrefixTest() {
			var fileName = "B2Test.txt";
			var fileNameWithFolder = "test/B2Test.txt";
			var fileData = File.ReadAllBytes(Path.Combine(FilePath, fileName));
			var file = Client.Files.Upload(fileData, fileName, TestBucket.BucketId).Result;
			var fileFolder = Client.Files.Upload(fileData, fileNameWithFolder, TestBucket.BucketId).Result;
			
			var list = Client.Files.GetListWithPrefixOrDemiliter(bucketId: TestBucket.BucketId, prefix: "test").Result.Files;

			Assert.AreEqual(1, list.Count, list.Count + " files found.");
		}

		[TestMethod]
		public void GetListWithPrefixAndDelimiterTest() {
			var fileName = "B2Test.txt";
			var fileNameWithFolder = "test/B2Test.txt";
			var fileData = File.ReadAllBytes(Path.Combine(FilePath, fileName));
			var file = Client.Files.Upload(fileData, fileName, TestBucket.BucketId).Result;
			var fileFolder = Client.Files.Upload(fileData, fileNameWithFolder, TestBucket.BucketId).Result;
			
			var list = Client.Files.GetListWithPrefixOrDemiliter(bucketId: TestBucket.BucketId, prefix: "test", delimiter: "/").Result.Files;

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
			
			var list = Client.Files.GetListWithPrefixOrDemiliter(bucketId: TestBucket.BucketId, delimiter: "/").Result.Files;

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
		//	
		//	

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
			
			// Since we did not pass a sha, hash will be prepended with unverified:
			Assert.AreEqual($"unverified:{hash}", file.ContentSHA1, "File hashes did not match.");
		}

		[TestMethod]
		public void FileUploadUsingUploadUrlTest() {
			var fileName = "B2Test.txt";
			var fileData = File.ReadAllBytes(Path.Combine(FilePath, fileName));
			string hash = Utilities.GetSHA1Hash(fileData);

			var uploadUrl = Client.Files.GetUploadUrl(TestBucket.BucketId).Result;

			var file = Client.Files.Upload(fileData, fileName, uploadUrl, true, TestBucket.BucketId).Result;

			// Since we did not pass a sha, hash will be prepended with unverified:
			Assert.AreEqual($"unverified:{hash}", file.ContentSHA1, "File hashes did not match.");
		}

		//[TestMethod]
		//public void FileUploadEncodingTest() {
		//	var fileName = "B2 Test File.txt";
		//	var fileData = File.ReadAllBytes(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, fileName));
		//	string hash = Utilities.GetSHA1Hash(fileData);
		//	var file = Client.Files.Upload(fileData, fileName, TestBucket.BucketId).Result;

		//	
		//	

		//	Assert.AreEqual(hash, file.ContentSHA1, "File hashes did not match.");
		//}

		[TestMethod]
		public void FileUploadWithInfoTest() {
			var fileName = "B2Test.txt";
			var fileData = File.ReadAllBytes(Path.Combine(FilePath, fileName));
			string hash = Utilities.GetSHA1Hash(fileData);

			var fileInfo = new Dictionary<string, string>() {
				{"FileInfoTest", "1234"}
			};

			var file = Client.Files.Upload(fileData, fileName, TestBucket.BucketId, fileInfo).Result;
			
			// Since we did not pass a sha, hash will be prepended with unverified:
			Assert.AreEqual($"unverified:{hash}", file.ContentSHA1, "File hashes did not match.");
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
			
			Assert.AreEqual(hash, file.ContentSHA1, "File hashes did not match.");
		}

		[TestMethod]
		public void FileUploadStreamNoSHATest() {
			var fileName = "B2Test.txt";
			var bytes = File.ReadAllBytes(Path.Combine(FilePath, fileName));

			var fileData = new MemoryStream(bytes);

			var uploadUrl = Client.Files.GetUploadUrl(TestBucket.BucketId).Result;

			var file = Client.Files.Upload(fileData, fileName, uploadUrl, "", false, TestBucket.BucketId, null, true).Result;

			Assert.IsTrue(file.ContentSHA1.StartsWith("unverified"), $"File was verified when it should not have been: {file.ContentSHA1}.");
		}

		[TestMethod]
		public async Task FileUploadStream_WithContext_Test() {
			var fileName = "B2Test.txt";
			var bytes = File.ReadAllBytes(Path.Combine(FilePath, fileName));

			string hash = Utilities.GetSHA1Hash(bytes);
			var hashBytes = Encoding.UTF8.GetBytes(hash);

			var fileData = new MemoryStream(bytes.Concat(hashBytes).ToArray());

			var uploadUrl = Client.Files.GetUploadUrl(TestBucket.BucketId).Result;

			var file = Client.Files.Upload(fileData, new B2FileUploadContext() {
				FileName = fileName,
				B2UploadUrl = uploadUrl,
				ContentLanguage = "en-US",
				AdditionalFileInfo = new Dictionary<string, string>() { { "test-info", "1234" } }
			}, false).Result;

			// Get file response for new headers
			var request = FileDownloadRequestGenerators.DownloadById(Options, file.FileId);
			var response = await HttpClient.SendAsync(request);

			Assert.AreEqual(hash, file.ContentSHA1, "File hashes did not match.");
			Assert.AreEqual(true, response.Headers.Contains("x-bz-info-test-info"));
			Assert.AreEqual("1234", response.Headers.GetValues("x-bz-info-test-info").First().ToString());
		}

		[TestMethod]
		public void FileDownloadNameTest() {
			var fileName = "B2Test.txt";
			var fileData = File.ReadAllBytes(Path.Combine(FilePath, fileName));
			string hash = Utilities.GetSHA1Hash(fileData);
			var file = Client.Files.Upload(fileData, fileName, TestBucket.BucketId).Result;
			
			// Since we did not pass a sha, hash will be prepended with unverified:
			Assert.AreEqual($"unverified:{hash}", file.ContentSHA1, "File hashes did not match.");

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
				{"FileInfoTest", "1234"}
			};

			var file = Client.Files.Upload(fileData, fileName, TestBucket.BucketId, fileInfo).Result;

			// Since we did not pass a sha, hash will be prepended with unverified:
			Assert.AreEqual($"unverified:{hash}", file.ContentSHA1, "File hashes did not match.");

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

			// Since we did not pass a sha, hash will be prepended with unverified:
			Assert.AreEqual($"unverified:{hash}", file.ContentSHA1, "File hashes did not match.");

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

			// Since we did not pass a sha, hash will be prepended with unverified:
			Assert.AreEqual($"unverified:{hash}", file.ContentSHA1, "File hashes did not match.");

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

			// Since we did not pass a sha, hash will be prepended with unverified:
			Assert.AreEqual($"unverified:{hash}", file.ContentSHA1, "File hashes did not match.");

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


			// Since we did not pass a sha, hash will be prepended with unverified:
			Assert.AreEqual($"unverified:{hash}", file.ContentSHA1, "File hashes did not match.");

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
			
			// Since we did not pass a sha, hash will be prepended with unverified:
			Assert.AreEqual($"unverified:{hash}", file.ContentSHA1, "File hashes did not match.");

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
			
			var copied = await Client.Files.Copy(file.FileId, "B2TestCopy.txt");

			Assert.AreEqual("copy", copied.Action, "Action was not as expected for the copy operation.");
			Assert.AreEqual(fileData.Length.ToString(), copied.ContentLength, "Length of the two files was not the same.");
		}

		[TestMethod]
		public async Task ReplaceFile() {
			var fileName = "B2Test.txt";
			var fileData = File.ReadAllBytes(Path.Combine(FilePath, fileName));
			var file = Client.Files.Upload(fileData, fileName, TestBucket.BucketId).Result;

			var copied = await Client.Files.Copy(file.FileId, "B2TestCopy.txt", B2MetadataDirective.REPLACE, "text/plain", new Dictionary<string, string>() {
				{"FileInfoTest", "1234"}
			});

			Assert.IsTrue(copied.FileInfo.ContainsKey("fileinfotest"), "FileInfo was not as expected for the replace operation.");
			Assert.AreEqual(fileData.Length.ToString(), copied.ContentLength, "Length of the two files was not the same.");
		}

		[TestMethod]
		[ExpectedException(typeof(CopyReplaceSetupException), "Copy did not fail when disallowed fields were provided.")]
		public async Task CopyFileWithDisallowedFields() {
			var fileName = "B2Test.txt";
			var fileData = File.ReadAllBytes(Path.Combine(FilePath, fileName));
			var file = Client.Files.Upload(fileData, fileName, TestBucket.BucketId).Result;
			
			var copied = await Client.Files.Copy(file.FileId, "B2TestCopy.txt", contentType: "b2/x-auto");
		}

		[TestMethod]
		[ExpectedException(typeof(CopyReplaceSetupException), "Replace did not fail when fields were missing.")]
		public async Task ReplaceFileWithMissingFields() {
			var fileName = "B2Test.txt";
			var fileData = File.ReadAllBytes(Path.Combine(FilePath, fileName));
			var file = Client.Files.Upload(fileData, fileName, TestBucket.BucketId).Result;
			
			var copied = await Client.Files.Copy(file.FileId, "B2TestCopy.txt", B2MetadataDirective.REPLACE);
		}

		[TestMethod]
		public async Task UpdateFileRetentionTest() {
			var fileName = "B2Test.txt";
			var fileData = File.ReadAllBytes(Path.Combine(FilePath, fileName));
			var uploadUrl = Client.Files.GetUploadUrl(TestBucket.BucketId).Result;
			// Get timestamp
			DateTimeOffset now = DateTimeOffset.UtcNow.AddDays(1);
#if NETFULL
			var UnixEpoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
			long unixTimeMilliseconds = (long) (DateTime.UtcNow - UnixEpoch).TotalMilliseconds;
#else
			long unixTimeMilliseconds = now.ToUnixTimeMilliseconds();
#endif
			// Upload test file
			var file = Client.Files.Upload(fileData, new B2FileUploadContext() {
				FileName = fileName,
				B2UploadUrl = uploadUrl,
				BucketId = TestBucket.BucketId,
				RetentionMode = RetentionMode.governance,
				RetainUntilTimestamp = unixTimeMilliseconds
			}).Result;
			
			var response = Client.Files.UpdateFileRetention(fileName, file.FileId, new B2DefaultRetention() {
				Mode = RetentionMode.governance,
				Period = new Period() {
					Duration = 2,
					Unit = RetentionUnit.days
				}
			}).Result;

			var fileInfo = Client.Files.GetInfo(response.fileId).Result;

			Assert.AreEqual(RetentionMode.governance, fileInfo.FileRetention.Value.Mode, "Retention mode on the update did not match.");
			Assert.IsTrue(fileInfo.FileRetention.Value.RetainUntilTimestamp > unixTimeMilliseconds, "Retention timestamp was not updated.");
		}

		[ClassCleanup]
		public static void Cleanup() {
			// Recreate the client for static cleanup
			var options = new B2Options() {
				KeyId = TestConstants.KeyId,
				ApplicationKey = TestConstants.ApplicationKey
			};
			var client = new B2Client(options);
			var http = HttpClientFactory.CreateHttpClient(200);
			http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", options.AuthorizationToken);

			var buckets = client.Buckets.GetList().Result;
			var testingBuckets = buckets.Where(x => x.BucketName.Contains(BucketPrefix));

			// Loop all testing buckets and cleanup
			foreach (var testingBucket in testingBuckets) {
				// Loop the files and delete
				var list = client.Files.GetList(bucketId: testingBucket.BucketId).Result.Files;
				foreach (B2File b2File in list) {
					var deletedFile = client.Files.Delete(b2File.FileId, b2File.FileName).Result;
				}
				var deletedBucket = client.Buckets.Delete(testingBucket.BucketId).Result;
			}

		}
	}
}
