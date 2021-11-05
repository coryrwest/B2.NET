using B2Net.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace B2Net.Tests {
	[TestClass]
	public class BucketTests : BaseTest {
		private B2Client Client = null;
		private string BucketName = $"B2NETTestingBucket-{Path.GetRandomFileName().Replace(".", "").Substring(0, 6)}";

		[TestInitialize]
		public void Initialize() {
			Client = new B2Client(B2Client.Authorize(Options));
		}

		[TestMethod]
		public void GetBucketListTest() {
			var bucket = Client.Buckets.Create(BucketName, BucketTypes.allPrivate).Result;

			var list = Client.Buckets.GetList().Result;

			var deletedBucket = Client.Buckets.Delete(bucket.BucketId).Result;

			Assert.AreNotEqual(0, list.Count);
		}

		[TestMethod]
		public void CreateBucketTest() {
			var name = BucketName;
			var bucket = Client.Buckets.Create(name, BucketTypes.allPrivate).Result;

			// Clean up
			if (!string.IsNullOrEmpty(bucket.BucketId)) {
				Client.Buckets.Delete(bucket.BucketId).Wait();
			}

			Assert.AreEqual(name, bucket.BucketName);
		}

		[TestMethod]
		public void CreateBucket_WithFileLock_Test() {
			var name = BucketName;
			var bucket = Client.Buckets.Create(name, new B2BucketOptions() {
				BucketType = Models.BucketTypes.allPublic,
				FileLockEnabled = true
			}).Result;

			// Clean up
			if (!string.IsNullOrEmpty(bucket.BucketId)) {
				Client.Buckets.Delete(bucket.BucketId).Wait();
			}

			Assert.AreEqual(name, bucket.BucketName);
			Assert.IsTrue(bucket.FileLockConfiguration.Value.IsFileLockEnabled);
		}

		[TestMethod]
		[ExpectedException(typeof(AggregateException))]
		public void CreateBucketInvalidNameTest() {
			var name = "B2net-testing-bucket-%$";

			var bucket = Client.Buckets.Create(name, BucketTypes.allPrivate).Result;
		}

		[TestMethod]
		public void CreateBucketWithCacheControlTest() {
			var bucket = Client.Buckets.Create(BucketName, new B2BucketOptions() {
				CacheControl = 600
			}).Result;

			// Get bucket to check for info
			var bucketList = Client.Buckets.GetList().Result;

			// Clean up
			if (!string.IsNullOrEmpty(bucket.BucketId)) {
				Client.Buckets.Delete(bucket.BucketId).Wait();
			}

			B2Bucket savedBucket = bucketList.FirstOrDefault(b => b.BucketName == bucket.BucketName);

			Assert.IsNotNull(savedBucket, "Retreived bucket was null");
			Assert.IsNotNull(savedBucket.BucketInfo, "Bucekt info was null");
			Assert.IsTrue(savedBucket.BucketInfo.ContainsKey("cache-control"), "Bucket info did not contain Cache-Control");
			Assert.AreEqual("max-age=600", savedBucket.BucketInfo["cache-control"], "Cache-Control values were not equal.");
		}

		[TestMethod]
		public void CreateBucketWithLifecycleRulesTest() {
			var bucket = Client.Buckets.Create(BucketName, new B2BucketOptions() {
				LifecycleRules = new System.Collections.Generic.List<B2BucketLifecycleRule>() {
					new B2BucketLifecycleRule() {
						DaysFromHidingToDeleting = 30,
						DaysFromUploadingToHiding = null,
						FileNamePrefix = "testing"
					}
				}
			}).Result;

			// Get bucket to check for info
			var bucketList = Client.Buckets.GetList().Result;

			// Clean up
			if (!string.IsNullOrEmpty(bucket.BucketId)) {
				Client.Buckets.Delete(bucket.BucketId).Wait();
			}

			var savedBucket = bucketList.FirstOrDefault(b => b.BucketName == bucket.BucketName);

			Assert.IsNotNull(savedBucket, "Retreived bucket was null");
			Assert.IsNotNull(savedBucket.BucketInfo, "Bucekt info was null");
			Assert.AreEqual(savedBucket.LifecycleRules.Count, 1, "Lifecycle rules count was " + savedBucket.LifecycleRules.Count);
			Assert.AreEqual("testing", savedBucket.LifecycleRules.First().FileNamePrefix, "File name prefixes in the first lifecycle rule were not equal.");
			Assert.AreEqual(null, savedBucket.LifecycleRules.First().DaysFromUploadingToHiding, "The first lifecycle rule DaysFromUploadingToHiding was not null");
			Assert.AreEqual(30, savedBucket.LifecycleRules.First().DaysFromHidingToDeleting, "The first lifecycle rule DaysFromHidingToDeleting was not 30");
		}

		[TestMethod]
		public async Task UpdateBucketWithCacheControlTest() {
			var bucket = await Client.Buckets.Create(BucketName, new B2BucketOptions() { CacheControl = 600 });

			// Update bucket with new info
			bucket = await Client.Buckets.Update(new B2BucketOptions() { CacheControl = 300 }, bucket.BucketId);

			// Get bucket to check for info
			var bucketList = await Client.Buckets.GetList();

			// Clean up
			if (!string.IsNullOrEmpty(bucket.BucketId)) {
				await Client.Buckets.Delete(bucket.BucketId);
			}

			var savedBucket = bucketList.FirstOrDefault(b => b.BucketName == bucket.BucketName);

			Assert.IsNotNull(savedBucket, "Retreived bucket was null");
			Assert.IsNotNull(savedBucket.BucketInfo, "Bucekt info was null");
			Assert.IsTrue(savedBucket.BucketInfo.ContainsKey("cache-control"), "Bucket info did not contain Cache-Control");
			Assert.AreEqual("max-age=300", savedBucket.BucketInfo["cache-control"], "Cache-Control values were not equal.");
		}

		[TestMethod]
		public void UpdateBucketWithLifecycleRulesTest() {
			var bucket = Client.Buckets.Create(BucketName, new B2BucketOptions() {
				LifecycleRules = new System.Collections.Generic.List<B2BucketLifecycleRule>() {
					new B2BucketLifecycleRule() {
						DaysFromHidingToDeleting = 30,
						DaysFromUploadingToHiding = 15,
						FileNamePrefix = "testing"
					}
				}
			}).Result;

			// Update bucket with new info
			bucket = Client.Buckets.Update(new B2BucketOptions() {
				LifecycleRules = new System.Collections.Generic.List<B2BucketLifecycleRule>() {
					new B2BucketLifecycleRule() {
						DaysFromHidingToDeleting = 10,
						DaysFromUploadingToHiding = 10,
						FileNamePrefix = "tested"
					}
				}
			}, bucket.BucketId).Result;

			// Get bucket to check for info
			var bucketList = Client.Buckets.GetList().Result;

			// Clean up
			if (!string.IsNullOrEmpty(bucket.BucketId)) {
				Client.Buckets.Delete(bucket.BucketId).Wait();
			}

			var savedBucket = bucketList.FirstOrDefault(b => b.BucketName == bucket.BucketName);

			Assert.IsNotNull(savedBucket, "Retreived bucket was null");
			Assert.IsNotNull(savedBucket.BucketInfo, "Bucekt info was null");
			Assert.AreEqual(savedBucket.LifecycleRules.Count, 1, "Lifecycle rules count was " + savedBucket.LifecycleRules.Count);
			Assert.AreEqual("tested", savedBucket.LifecycleRules.First().FileNamePrefix, "File name prefixes in the first lifecycle rule were not equal.");
		}

		[TestMethod]
		public void DeleteBucketTest() {
			var name = BucketName;
			//Creat a bucket to delete
			var bucket = Client.Buckets.Create(name, BucketTypes.allPrivate).Result;

			if (!string.IsNullOrEmpty(bucket.BucketId)) {
				var deletedBucket = Client.Buckets.Delete(bucket.BucketId).Result;
				Assert.AreEqual(name, deletedBucket.BucketName);
			}
			else {
				Assert.Fail("The bucket was not deleted. The response did not contain a bucketid.");
			}
		}

		[TestMethod]
		public void UpdateBucketTest() {
			//Creat a bucket to delete
			var bucket = Client.Buckets.Create(BucketName, BucketTypes.allPrivate).Result;

			try {
				if (!string.IsNullOrEmpty(bucket.BucketId)) {
					var updatedBucket = Client.Buckets.Update(BucketTypes.allPublic, bucket.BucketId).Result;
					Assert.AreEqual(BucketTypes.allPublic.ToString(), updatedBucket.BucketType);
				}
				else {
					Assert.Fail("The bucket was not deleted. The response did not contain a bucketid.");
				}
			}
			catch (Exception ex) {
				Assert.Fail(ex.Message);
			}
			finally {
				Client.Buckets.Delete(bucket.BucketId).Wait();
			}
		}

		[TestMethod]
		public void BucketCORSRulesTest() {
			var bucket = Client.Buckets.Create(BucketName, new B2BucketOptions() {
				CORSRules = new List<B2CORSRule>() {
					new B2CORSRule() {
						CorsRuleName = "allowAnyHttps",
						AllowedHeaders = new []{ "x-bz-content-sha1", "x-bz-info-*" },
						AllowedOperations = new []{ "b2_upload_file" },
						AllowedOrigins = new [] { "https://*" }
					}
				}
			}).Result;

			try {
				var list = Client.Buckets.GetList().Result;
				Assert.AreNotEqual(0, list.Count);

				var corsBucket = list.First(x => x.BucketId == bucket.BucketId);

				Assert.AreEqual("allowAnyHttps", corsBucket.CORSRules.First().CorsRuleName, "CORS header was not saved or returned for bucket.");
			}
			finally {
				Client.Buckets.Delete(bucket.BucketId).Wait();
			}
		}

		[TestMethod]
		public void BucketCORSRuleUpdateTest() {
			var bucket = Client.Buckets.Create(BucketName, new B2BucketOptions() {
				CORSRules = new List<B2CORSRule>() {
					new B2CORSRule() {
						CorsRuleName = "allowAnyHttps",
						AllowedHeaders = new []{ "x-bz-content-sha1", "x-bz-info-*" },
						AllowedOperations = new []{ "b2_upload_file" },
						AllowedOrigins = new [] { "https://*" }
					}
				}
			}).Result;

			try {
				var updatedBucket = Client.Buckets.Update(new B2BucketOptions() {
					CORSRules = new List<B2CORSRule>() {
						new B2CORSRule() {
							CorsRuleName = "updatedRule",
							AllowedOperations = new []{ "b2_upload_part" },
							AllowedOrigins = new [] { "https://*" }
						}
					}
				}, bucket.Revision, bucket.BucketId).Result;

				var list = Client.Buckets.GetList().Result;
				Assert.AreNotEqual(0, list.Count);

				var corsBucket = list.First(x => x.BucketId == bucket.BucketId);

				Assert.AreEqual("updatedRule", corsBucket.CORSRules.First().CorsRuleName, "CORS header was not updated for bucket.");
				Assert.AreEqual("b2_upload_part", corsBucket.CORSRules.First().AllowedOperations.First(), "CORS header was not updated for bucket.");
			} finally {
				Client.Buckets.Delete(bucket.BucketId).Wait();
			}
		}

		//[TestMethod]
		//public async Task CleanUpAccount() {
		//	// Only use this test to clean up an account after tests run if buckets are left over.
		//	var list = await Client.Buckets.GetList();
		//	foreach (var b2Bucket in list.Where(x => x.BucketName.Contains("B2NETTestingBucket"))) {
		//		var files = await Client.Files.GetList(bucketId: b2Bucket.BucketId);
		//		if (files.Files.Count > 0) {
		//			files.Files.ForEach(async x => await Client.Files.Delete(x.FileId, x.FileName));
		//		}

		//		await Client.Buckets.Delete(b2Bucket.BucketId);
		//	}
		//}
	}
}
