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
		public async Task GetListTest() {
			var fileName = "B2Test.txt";
			var fileData = File.ReadAllBytes(Path.Combine(FilePath, fileName));
			var file = await Client.Files.Upload(fileData, fileName, TestBucket.BucketId);

			var list = await Client.Files.GetList(bucketId: TestBucket.BucketId);

			Assert.AreEqual(1, list.Files.Count, list.Files.Count + " files found.");
		}

		[TestMethod]
		public async Task GetListWithPrefixTest() {
			var fileName = "B2Test.txt";
			var fileNameWithFolder = "test/B2Test.txt";
			var fileData = File.ReadAllBytes(Path.Combine(FilePath, fileName));
			var file = await Client.Files.Upload(fileData, fileName, TestBucket.BucketId);
			var fileFolder = await Client.Files.Upload(fileData, fileNameWithFolder, TestBucket.BucketId);
			
			var list = Client.Files.GetListWithPrefixOrDemiliter(bucketId: TestBucket.BucketId, prefix: "test").Result.Files;

			Assert.AreEqual(1, list.Count, list.Count + " files found.");
		}

		[TestMethod]
		public async Task GetListWithPrefixAndDelimiterTest() {
			var fileName = "B2Test.txt";
			var fileNameWithFolder = "test/B2Test.txt";
			var fileData = File.ReadAllBytes(Path.Combine(FilePath, fileName));
			var file = await Client.Files.Upload(fileData, fileName, TestBucket.BucketId);
			var fileFolder =	await Client.Files.Upload(fileData, fileNameWithFolder, TestBucket.BucketId);
			
			var list = Client.Files.GetListWithPrefixOrDemiliter(bucketId: TestBucket.BucketId, prefix: "test", delimiter: "/").Result.Files;

			Assert.AreEqual(1, list.Count, list.Count + " files found.");
			Assert.AreEqual("test/", list.First().FileName, "File names to not match.");
		}

		[TestMethod]
		public async Task GetListWithDelimiterTest() {
			var fileName = "B2Test.txt";
			var fileNameWithFolder = "test/B2Test.txt";
			var fileNameWithFolder2 = "test2/B2Test.txt";
			var fileData = File.ReadAllBytes(Path.Combine(FilePath, fileName));
			var file = await Client.Files.Upload(fileData, fileNameWithFolder2, TestBucket.BucketId);
			var fileFolder = await Client.Files.Upload(fileData, fileNameWithFolder, TestBucket.BucketId);
			
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
		public async Task FileUploadTest() {
			var fileName = "B2Test.txt";
			var fileData = File.ReadAllBytes(Path.Combine(FilePath, fileName));
			string hash = Utilities.GetSHA1Hash(fileData);
			var file = await Client.Files.Upload(fileData, fileName, TestBucket.BucketId);
			
			// Since we did not pass a sha, hash will be prepended with unverified:
			Assert.AreEqual($"unverified:{hash}", file.ContentSHA1, "File hashes did not match.");
		}

		[TestMethod]
		public async Task FileUploadUsingUploadUrlTest() {
			var fileName = "B2Test.txt";
			var fileData = File.ReadAllBytes(Path.Combine(FilePath, fileName));
			string hash = Utilities.GetSHA1Hash(fileData);

			var uploadUrl = await Client.Files.GetUploadUrl(TestBucket.BucketId);

			var file = await Client.Files.Upload(fileData, fileName, uploadUrl, true, TestBucket.BucketId);

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
		public async Task FileUploadWithInfoTest() {
			var fileName = "B2Test.txt";
			var fileData = File.ReadAllBytes(Path.Combine(FilePath, fileName));
			string hash = Utilities.GetSHA1Hash(fileData);

			var fileInfo = new Dictionary<string, string>() {
				{"FileInfoTest", "1234"}
			};

			var file = await Client.Files.Upload(fileData, fileName, TestBucket.BucketId, fileInfo);
			
			// Since we did not pass a sha, hash will be prepended with unverified:
			Assert.AreEqual($"unverified:{hash}", file.ContentSHA1, "File hashes did not match.");
			Assert.AreEqual(1, file.FileInfo.Count, "File info count was off.");
		}

		[TestMethod]
		public async Task FileUploadStreamTest() {
			var fileName = "B2Test.txt";
			var bytes = File.ReadAllBytes(Path.Combine(FilePath, fileName));

			string hash = Utilities.GetSHA1Hash(bytes);
			var hashBytes = Encoding.UTF8.GetBytes(hash);

			var fileData = new MemoryStream(bytes.Concat(hashBytes).ToArray());

			var uploadUrl = await Client.Files.GetUploadUrl(TestBucket.BucketId);

			var file = await Client.Files.Upload(fileData, fileName, uploadUrl, "", false, TestBucket.BucketId);
			
			Assert.AreEqual(hash, file.ContentSHA1, "File hashes did not match.");
		}

		[TestMethod]
		public async Task FileUploadStreamNoSHATest() {
			var fileName = "B2Test.txt";
			var bytes = File.ReadAllBytes(Path.Combine(FilePath, fileName));

			var fileData = new MemoryStream(bytes);

			var uploadUrl = await Client.Files.GetUploadUrl(TestBucket.BucketId);

			var file = await Client.Files.Upload(fileData, fileName, uploadUrl, "", false, TestBucket.BucketId, null, true);

			Assert.IsTrue(file.ContentSHA1.StartsWith("unverified"), $"File was verified when it should not have been: {file.ContentSHA1}.");
		}

		[TestMethod]
		public async Task FileUploadStream_WithContext_Test() {
			var fileName = "B2Test.txt";
			var bytes = File.ReadAllBytes(Path.Combine(FilePath, fileName));

			string hash = Utilities.GetSHA1Hash(bytes);
			var hashBytes = Encoding.UTF8.GetBytes(hash);

			var fileData = new MemoryStream(bytes.Concat(hashBytes).ToArray());

			var uploadUrl = await Client.Files.GetUploadUrl(TestBucket.BucketId);

			var file = await Client.Files.Upload(fileData, new B2FileUploadContext() {
				FileName = fileName,
				B2UploadUrl = uploadUrl,
				ContentLanguage = "en-US",
				AdditionalFileInfo = new Dictionary<string, string>() { { "test-info", "1234" } }
			}, false);

			// Get file response for new headers
			var request = FileDownloadRequestGenerators.DownloadById(Options, file.FileId);
			var response = await HttpClient.SendAsync(request);

			Assert.AreEqual(hash, file.ContentSHA1, "File hashes did not match.");
			Assert.AreEqual(true, response.Headers.Contains("x-bz-info-test-info"));
			Assert.AreEqual("1234", response.Headers.GetValues("x-bz-info-test-info").First().ToString());
		}

		[TestMethod]
		public async Task FileDownloadNameTest() {
			var fileName = "B2Test.txt";
			var fileData = File.ReadAllBytes(Path.Combine(FilePath, fileName));
			string hash = Utilities.GetSHA1Hash(fileData);
			var file = await Client.Files.Upload(fileData, fileName, TestBucket.BucketId);
			
			// Since we did not pass a sha, hash will be prepended with unverified:
			Assert.AreEqual($"unverified:{hash}", file.ContentSHA1, "File hashes did not match.");

			// Test download
			var download = await Client.Files.DownloadByName(file.FileName, TestBucket.BucketName);
			var downloadHash = Utilities.GetSHA1Hash(download.FileData);

			Assert.AreEqual(hash, downloadHash);
		}

		[TestMethod]
		public async Task FileDownloadWithInfoTest() {
			var fileName = "B2Test.txt";
			var fileData = File.ReadAllBytes(Path.Combine(FilePath, fileName));
			string hash = Utilities.GetSHA1Hash(fileData);

			var fileInfo = new Dictionary<string, string>() {
				{"FileInfoTest", "1234"}
			};

			var file = await Client.Files.Upload(fileData, fileName, TestBucket.BucketId, fileInfo);

			// Since we did not pass a sha, hash will be prepended with unverified:
			Assert.AreEqual($"unverified:{hash}", file.ContentSHA1, "File hashes did not match.");

			// Test download
			var download = await Client.Files.DownloadById(file.FileId);
			var downloadHash = Utilities.GetSHA1Hash(download.FileData);

			Assert.AreEqual(hash, downloadHash);
			Assert.AreEqual(1, download.FileInfo.Count);
		}

		[TestMethod]
		public async Task FileDownloadIdTest() {
			var fileName = "B2Test.txt";
			var fileData = File.ReadAllBytes(Path.Combine(FilePath, fileName));
			string hash = Utilities.GetSHA1Hash(fileData);
			var file = await Client.Files.Upload(fileData, fileName, TestBucket.BucketId);

			// Since we did not pass a sha, hash will be prepended with unverified:
			Assert.AreEqual($"unverified:{hash}", file.ContentSHA1, "File hashes did not match.");

			// Test download
			var download = await Client.Files.DownloadById(file.FileId);
			var downloadHash = Utilities.GetSHA1Hash(download.FileData);

			Assert.AreEqual(hash, downloadHash);
		}

		[TestMethod]
		public async Task FileDownloadFolderTest() {
			var fileName = "B2Test.txt";
			var fileData = File.ReadAllBytes(Path.Combine(FilePath, fileName));
			string hash = Utilities.GetSHA1Hash(fileData);
			var file = await Client.Files.Upload(fileData, "B2Folder/Test/File.txt", TestBucket.BucketId);

			// Since we did not pass a sha, hash will be prepended with unverified:
			Assert.AreEqual($"unverified:{hash}", file.ContentSHA1, "File hashes did not match.");

			// Test download
			var download = await Client.Files.DownloadById(file.FileId);
			var downloadHash = Utilities.GetSHA1Hash(download.FileData);

			Assert.AreEqual(hash, downloadHash);
		}

		[TestMethod]
		public async Task FileDeleteTest() {
			var fileName = "B2Test.txt";
			var fileData = File.ReadAllBytes(Path.Combine(FilePath, fileName));
			string hash = Utilities.GetSHA1Hash(fileData);
			var file = await Client.Files.Upload(fileData, fileName, TestBucket.BucketId);

			// Since we did not pass a sha, hash will be prepended with unverified:
			Assert.AreEqual($"unverified:{hash}", file.ContentSHA1, "File hashes did not match.");

			// Clean up. We have to delete the file before we can delete the bucket
			var deletedFile = await Client.Files.Delete(file.FileId, file.FileName);

			Assert.AreEqual(file.FileId, deletedFile.FileId, "The deleted file id did not match.");
		}

		[TestMethod]
		public async Task ListVersionsTest() {
			var fileName = "B2Test.txt";
			var fileData = File.ReadAllBytes(Path.Combine(FilePath, fileName));
			string hash = Utilities.GetSHA1Hash(fileData);
			var file = await Client.Files.Upload(fileData, fileName, TestBucket.BucketId);


			// Since we did not pass a sha, hash will be prepended with unverified:
			Assert.AreEqual($"unverified:{hash}", file.ContentSHA1, "File hashes did not match.");

			var versions = await Client.Files.GetVersions(file.FileName, file.FileId, bucketId: TestBucket.BucketId);

			Assert.AreEqual(1, versions.Files.Count);
		}

		[TestMethod]
		public async Task GetInfoTest() {
			var fileName = "B2Test.txt";
			var fileData = File.ReadAllBytes(Path.Combine(FilePath, fileName));
			string hash = Utilities.GetSHA1Hash(fileData);

			var fileInfo = new Dictionary<string, string>();
			fileInfo.Add("FileInfoTest", "1234");

			var file = await Client.Files.Upload(fileData, fileName, TestBucket.BucketId, fileInfo);
			
			// Since we did not pass a sha, hash will be prepended with unverified:
			Assert.AreEqual($"unverified:{hash}", file.ContentSHA1, "File hashes did not match.");

			var info = await Client.Files.GetInfo(file.FileId);

			Assert.AreEqual(file.UploadTimestamp, info.UploadTimestamp);
			Assert.AreEqual(1, info.FileInfo.Count);
		}

		[TestMethod]
		public async Task GetDownloadAuthorizationTest() {
			var downloadAuth = await Client.Files.GetDownloadAuthorization("Test", 120, TestBucket.BucketId);

			Assert.AreEqual("Test", downloadAuth.FileNamePrefix, "File prefixes were not the same.");
		}

		[TestMethod]
		public async Task CopyFile() {
			var fileName = "B2Test.txt";
			var fileData = File.ReadAllBytes(Path.Combine(FilePath, fileName));
			var file = await Client.Files.Upload(fileData, fileName, TestBucket.BucketId);
			
			var copied = await Client.Files.Copy(file.FileId, "B2TestCopy.txt");

			Assert.AreEqual("copy", copied.Action, "Action was not as expected for the copy operation.");
			Assert.AreEqual(fileData.Length.ToString(), copied.ContentLength, "Length of the two files was not the same.");
		}

		[TestMethod]
		public async Task ReplaceFile() {
			var fileName = "B2Test.txt";
			var fileData = File.ReadAllBytes(Path.Combine(FilePath, fileName));
			var file = await Client.Files.Upload(fileData, fileName, TestBucket.BucketId);

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
			var file = await Client.Files.Upload(fileData, fileName, TestBucket.BucketId);
			
			var copied = await Client.Files.Copy(file.FileId, "B2TestCopy.txt", contentType: "b2/x-auto");
		}

		[TestMethod]
		[ExpectedException(typeof(CopyReplaceSetupException), "Replace did not fail when fields were missing.")]
		public async Task ReplaceFileWithMissingFields() {
			var fileName = "B2Test.txt";
			var fileData = File.ReadAllBytes(Path.Combine(FilePath, fileName));
			var file = await Client.Files.Upload(fileData, fileName, TestBucket.BucketId);
			
			var copied = await Client.Files.Copy(file.FileId, "B2TestCopy.txt", B2MetadataDirective.REPLACE);
		}

		[TestMethod]
		public async Task UpdateFileRetentionTest() {
			var fileName = "B2Test.txt";
			var fileData = File.ReadAllBytes(Path.Combine(FilePath, fileName));
			var uploadUrl = await Client.Files.GetUploadUrl(TestBucket.BucketId);
			// Get timestamp
			DateTimeOffset now = DateTimeOffset.UtcNow.AddDays(1);
#if NETFULL
			var UnixEpoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
			long unixTimeMilliseconds = (long) (DateTime.UtcNow - UnixEpoch).TotalMilliseconds;
#else
			long unixTimeMilliseconds = now.ToUnixTimeMilliseconds();
#endif
			// Upload test file
			var file = await Client.Files.Upload(fileData, new B2FileUploadContext() {
				FileName = fileName,
				B2UploadUrl = uploadUrl,
				BucketId = TestBucket.BucketId,
				RetentionMode = RetentionMode.governance,
				RetainUntilTimestamp = unixTimeMilliseconds
			});
			
			var response = await Client.Files.UpdateFileRetention(fileName, file.FileId, new B2DefaultRetention() {
				Mode = RetentionMode.governance,
				Period = new Period() {
					Duration = 2,
					Unit = RetentionUnit.days
				}
			});

			var fileInfo = await Client.Files.GetInfo(response.fileId);

			Assert.AreEqual(RetentionMode.governance.ToString(), fileInfo.FileRetention.Value.Mode, "Retention mode on the update did not match.");
			Assert.IsTrue(fileInfo.FileRetention.Value.RetainUntilTimestamp > unixTimeMilliseconds, "Retention timestamp was not updated.");
		}

		[ClassCleanup]
		public static async Task Cleanup() {
			// Recreate the client for static cleanup
			var options = new B2Options() {
				KeyId = TestConstants.KeyId,
				ApplicationKey = TestConstants.ApplicationKey
			};
			var client = new B2Client(options);
			var http = HttpClientFactory.CreateHttpClient(200);
			http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", options.AuthorizationToken);

			var buckets = await client.Buckets.GetList();
			var testingBuckets = buckets.Where(x => x.BucketName.Contains(BucketPrefix));

			// Loop all testing buckets and cleanup
			foreach (var testingBucket in testingBuckets) {
				// Loop the files and delete
				var list = await client.Files.GetList(bucketId: testingBucket.BucketId);
				foreach (B2File b2File in list.Files) {
					var deletedFile = await client.Files.Delete(b2File.FileId, b2File.FileName);
				}
				var deletedBucket = await client.Buckets.Delete(testingBucket.BucketId);
			}

		}
	}
}
