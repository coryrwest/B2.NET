using System;
using System.IO;
using B2Net.Http;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using B2Net.Models;
using System.Linq;

namespace B2Net.Tests {
	[TestClass]
	public class BucketTests : BaseTest {
	    private B2Client Client = null;
	    private string BucketName = "";

        [TestInitialize]
	    public void Initialize() {
	        Client = new B2Client(B2Client.Authorize(Options));
            BucketName = $"B2NETTestingBucket-{Path.GetRandomFileName().Replace(".", "").Substring(0, 6)}";
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
	        var bucket = Client.Buckets.Create(BucketName, BucketTypes.allPrivate).Result;

	        // Clean up
	        if (!string.IsNullOrEmpty(bucket.BucketId)) {
	            Client.Buckets.Delete(bucket.BucketId).Wait();
	        }

	        Assert.AreEqual(BucketName, bucket.BucketName);
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
            if (!string.IsNullOrEmpty(bucket.BucketId))
            {
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
        public void UpdateBucketWithCacheControlTest() {
            var bucket = Client.Buckets.Create(BucketName, new B2BucketOptions() { CacheControl = 600 }).Result;

            // Update bucket with new info
            bucket = Client.Buckets.Update(new B2BucketOptions() { CacheControl = 300 }, bucket.BucketId).Result;

            // Get bucket to check for info
            var bucketList = Client.Buckets.GetList().Result;

            // Clean up
            if (!string.IsNullOrEmpty(bucket.BucketId)) {
                Client.Buckets.Delete(bucket.BucketId).Wait();
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
			//Creat a bucket to delete
			var bucket = Client.Buckets.Create(BucketName, BucketTypes.allPrivate).Result;

			if (!string.IsNullOrEmpty(bucket.BucketId)) {
				var deletedBucket = Client.Buckets.Delete(bucket.BucketId).Result;
				Assert.AreEqual(BucketName, deletedBucket.BucketName);
			} else {
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
				} else {
					Assert.Fail("The bucket was not deleted. The response did not contain a bucketid.");
				}
			} catch (Exception ex) {
				Assert.Fail(ex.Message);
			} finally {
			    Client.Buckets.Delete(bucket.BucketId).Wait();
			}
		}
	}
}
