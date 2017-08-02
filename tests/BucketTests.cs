using System;
using B2Net.Http;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using B2Net.Models;
using System.Linq;

namespace B2Net.Tests {
	[TestClass]
	public class BucketTests : BaseTest {
		[TestMethod]
		public void GetBucketListTest() {
			var client = new B2Client(Options);

			Options = client.Authorize().Result;

			var bucket = client.Buckets.Create("B2NETTestingBucket", BucketTypes.allPrivate).Result;

			var list = client.Buckets.GetList().Result;

			var deletedBucket = client.Buckets.Delete(bucket.BucketId).Result;

			Assert.AreNotEqual(0, list.Count);
		}

		[TestMethod]
		public void CreateBucketTest() {
			var name = "B2NETTestingBucket";
			var client = new B2Client(Options);

			Options = client.Authorize().Result;

			var bucket = client.Buckets.Create(name, BucketTypes.allPrivate).Result;

			// Clean up
			if (!string.IsNullOrEmpty(bucket.BucketId)) {
				client.Buckets.Delete(bucket.BucketId).Wait();
			}

			Assert.AreEqual(name, bucket.BucketName);
		}
        
        [TestMethod]
        public void CreateBucketWithInfoTest()
        {
            var name = "B2NETTestingBucket";
            var client = new B2Client(Options);

            Options = client.Authorize().Result;

            var bucket = client.Buckets.Create(name, new B2BucketOptions() {
                CacheControl = 600
            }).Result;

            // Get bucket to check for info
            var bucketList = client.Buckets.GetList().Result;

            // Clean up
            if (!string.IsNullOrEmpty(bucket.BucketId))
            {
                client.Buckets.Delete(bucket.BucketId).Wait();
            }

            var savedBucket = bucketList.FirstOrDefault(b => b.BucketName == bucket.BucketName);

            Assert.IsNotNull(savedBucket, "Retreived bucket was null");
            Assert.IsNotNull(savedBucket.BucketInfo, "Bucekt info was null");
            Assert.IsTrue(savedBucket.BucketInfo.ContainsKey("Cache-Control"), "Bucket info did not contain Cache-Control");
            Assert.AreEqual("max-age=600", savedBucket.BucketInfo["Cache-Control"], "Cache-Control values were not equal.");
        }

        [TestMethod]
		public void DeleteBucketTest() {
			var name = "B2NETDeleteBucket";
			var client = new B2Client(Options);

			Options = client.Authorize().Result;

			//Creat a bucket to delete
			var bucket = client.Buckets.Create(name, BucketTypes.allPrivate).Result;

			if (!string.IsNullOrEmpty(bucket.BucketId)) {
				var deletedBucket = client.Buckets.Delete(bucket.BucketId).Result;
				Assert.AreEqual(name, deletedBucket.BucketName);
			} else {
				Assert.Fail("The bucket was not deleted. The response did not contain a bucketid.");
			}
		}

		[TestMethod]
		public void UpdateBucketTest() {
			var name = "B2NETUpdateBucket";
			var client = new B2Client(Options);

			Options = client.Authorize().Result;

			//Creat a bucket to delete
			var bucket = client.Buckets.Create(name, BucketTypes.allPrivate).Result;

			try {
				if (!string.IsNullOrEmpty(bucket.BucketId)) {
					var updatedBucket = client.Buckets.Update(BucketTypes.allPublic, bucket.BucketId).Result;
					Assert.AreEqual(BucketTypes.allPublic.ToString(), updatedBucket.BucketType);
				} else {
					Assert.Fail("The bucket was not deleted. The response did not contain a bucketid.");
				}
			} catch (Exception ex) {
				Assert.Fail(ex.Message);
			} finally {
				client.Buckets.Delete(bucket.BucketId).Wait();
			}
		}
	}
}
