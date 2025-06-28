using System;
using B2Net.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

namespace B2Net.Tests {
	[TestClass]
	public class PublicFileTests : BaseTest {
		private B2Bucket TestBucket = new B2Bucket();
		private B2Client Client = null;
		private List<B2File> FilesToDelete = new List<B2File>();
		private static string BucketPrefix = "B2NETTestingBucketPublic";
		private static string BucketName = $"{BucketPrefix}-{Path.GetRandomFileName().Replace(".", "").Substring(0, 6)}";

#if NETFULL
		private string FilePath => Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, "../../../");
#else
		private string FilePath => Path.Combine(System.AppContext.BaseDirectory, "../../../");
#endif

		[TestInitialize]
		public async Task Initialize() {
			Client = new B2Client(Options);
			Options = await Client.Authorize();

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
				TestBucket = await Client.Buckets.Create(BucketName, BucketTypes.allPublic);
			}
		}

		[TestMethod]
		public async Task FileGetFriendlyUrlTest() {
			var fileName = "B2Test.txt";
			var fileData = File.ReadAllBytes(Path.Combine(FilePath, fileName));
			string hash = Utilities.GetSHA1Hash(fileData);
			var file = await Client.Files.Upload(fileData, fileName, TestBucket.BucketId);
			// Clean up.
			FilesToDelete.Add(file);

			// Since we did not pass a sha, hash will be prepended with unverified:
			Assert.AreEqual($"unverified:{hash}", file.ContentSHA1, "File hashes did not match.");

			// Get url
			var friendlyUrl = Client.Files.GetFriendlyDownloadUrl(fileName, TestBucket.BucketName);

			// Test download
			var client = new HttpClient();
			var friendFile = await client.GetAsync(friendlyUrl);
			var ffileData = await friendFile.Content.ReadAsByteArrayAsync();
			var downloadHash = Utilities.GetSHA1Hash(ffileData);

			Assert.AreEqual(hash, downloadHash);
		}

		[TestCleanup]
		public async Task Cleanup() {
			var fileFailure = false;
			foreach (B2File b2File in FilesToDelete) {
				try {
					var deletedFile = await Client.Files.Delete(b2File.FileId, b2File.FileName);
				}
				catch (Exception e) {
					fileFailure = true;
				}
			}

			if (fileFailure) {
				Assert.Inconclusive("Cleanup failed");
			}
			else {
				try {
					var deletedBucket = await Client.Buckets.Delete(TestBucket.BucketId);
				}
				catch (Exception e) {
					Assert.Inconclusive("Cleanup failed");
				}
			}
		}
	}
}
