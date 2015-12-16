using System;
using B2Net.Http;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace B2Net.Tests {
	[TestClass]
	public class BucketTests : BaseTest {
		[TestMethod]
		public void GetBucketListTest() {
			var client = new B2Client(Options);

			Options = client.Authorize().Result;

			var list = client.Buckets.GetBucketList().Result;

			Assert.AreNotEqual(0, list.Count);
		}

		[TestMethod]
		public void CreateBucketTest() {
			var name = "B2NETTestingBucket";
			var client = new B2Client(Options);

			Options = client.Authorize().Result;

			var bucket = client.Buckets.CreateBucket(name, BucketTypes.allPrivate).Result;

			// Clean up
			if (!string.IsNullOrEmpty(bucket.BucketId)) {
				client.Buckets.DeleteBucket(bucket.BucketId).Wait();
			}

			Assert.AreEqual(name, bucket.BucketName);
		}

		[TestMethod]
		public void DeleteBucketTest() {
			var name = "B2NETDeleteBucket";
			var client = new B2Client(Options);

			Options = client.Authorize().Result;

			//Creat a bucket to delete
			var bucket = client.Buckets.CreateBucket(name, BucketTypes.allPrivate).Result;

			if (!string.IsNullOrEmpty(bucket.BucketId)) {
				var deletedBucket = client.Buckets.DeleteBucket(bucket.BucketId).Result;
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
			var bucket = client.Buckets.CreateBucket(name, BucketTypes.allPrivate).Result;

			try {
				if (!string.IsNullOrEmpty(bucket.BucketId)) {
					var updatedBucket = client.Buckets.UpdateBucket(bucket.BucketId, BucketTypes.allPublic).Result;
					Assert.AreEqual(BucketTypes.allPublic.ToString(), updatedBucket.BucketType);
				} else {
					Assert.Fail("The bucket was not deleted. The response did not contain a bucketid.");
				}
			} catch (Exception ex) {
				Assert.Fail(ex.Message);
			} finally {
				client.Buckets.DeleteBucket(bucket.BucketId).Wait();
			}
		}
	}
}
