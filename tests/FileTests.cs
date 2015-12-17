using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using B2Net.Http;
using B2Net.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace B2Net.Tests {
	[TestClass]
	public class FileTests : BaseTest {
		B2Bucket TestBucket = new B2Bucket();
		B2Client Client = null;

		[TestInitialize]
		public void Initialize() {
			Client = new B2Client(Options);
			Options = Client.Authorize().Result;
			TestBucket = Client.Buckets.Create("B2NETTestingBucket", BucketTypes.allPrivate).Result;
		}

		[TestMethod]
		public void GetListTest() {
			var fileName = "B2Test.txt";
			var fileData = File.ReadAllBytes(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, fileName));
			var file = Client.Files.Upload(fileData, fileName, TestBucket.BucketId).Result;

			var list = Client.Files.GetList(TestBucket.BucketId).Result.Files;

			// Delete file
			//var deletedFile = client.Files.Delete().Result;
			
			Assert.AreEqual(1, list.Count, list.Count + " files found.");
		}

		[TestMethod]
		public void HideFileTest() {
		}

		[TestMethod]
		public void FileUploadTest() {
			var fileName = "B2Test.txt";
			var fileData = File.ReadAllBytes(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, fileName));
			string hash = Utilities.GetSHA1Hash(fileData);
			var file = Client.Files.Upload(fileData, fileName, TestBucket.BucketId).Result;

			Assert.AreEqual(hash, file.ContentSHA1);

			// Clean up. We have to delete the file before we can delete the bucket
		}

		[TestMethod]
		public void ListVersionsTest() {
		}

		[TestMethod]
		public void GetInfoTest() {
		}

		[TestCleanup]
		public void Cleanup() {
			var deletedBucket = Client.Buckets.Delete(TestBucket.BucketId).Result;
		}
	}
}
