using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using B2Net.Http;
using B2Net.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace B2Net.Tests {
	[TestClass]
	public class LargeFileTests : BaseTest {
		private B2Bucket TestBucket = new B2Bucket();
		private B2Client Client = null;
		private List<B2File> FilesToDelete = new List<B2File>();

#if NETFULL
	    private string FilePath => Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, "../../../");
#else
        private string FilePath => Path.Combine(System.AppContext.BaseDirectory, "../../../");
#endif

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
        
        // THIS TEST DOES NOT PROPERLY CLEAN UP after an exception.
		[TestMethod]
		public void LargeFileUploadTest() {
			var fileName = "B2LargeFileTest.txt";
			FileStream fileStream = File.OpenRead(Path.Combine(FilePath, fileName));
            var stream = new StreamReader(fileStream);
		    char[] c = null;
            List<byte[]> parts = new List<byte[]>();
		    var shas = new List<string>();

		    while (stream.Peek() >= 0) {
		        c = new char[1024 * (5 * 1024)];
		        stream.Read(c, 0, c.Length);

		        parts.Add(Encoding.UTF8.GetBytes(c));
            }

		    foreach (var part in parts) {
		        string hash = Utilities.GetSHA1Hash(part);
                shas.Add(hash);
            }

		    var start = Client.LargeFiles.StartLargeFile(fileName, "", TestBucket.BucketId).Result;

		    for (int i = 0; i < parts.Count; i++) {
		        var uploadUrl = Client.LargeFiles.GetUploadPartUrl(start.FileId).Result;
		        var part = Client.LargeFiles.UploadPart(parts[i], i + 1, uploadUrl).Result;
		    }

		    var finish = Client.LargeFiles.FinishLargeFile(start.FileId, shas.ToArray()).Result;

			// Clean up.
			FilesToDelete.Add(start);
            
			Assert.AreEqual(start.FileId, finish.FileId, "File Ids did not match.");
		}

		[TestCleanup]
		public void Cleanup() {
			foreach (B2File b2File in FilesToDelete) {
                // 2 versions are sometimes created with the Large File API
				var deletedFile = Client.Files.Delete(b2File.FileId, b2File.FileName).Result;
			    try { deletedFile = Client.Files.Delete(b2File.FileId, b2File.FileName).Result; }
			    catch (B2Exception e) when (e.Message.Contains("File not present")) {}
            }
			var deletedBucket = Client.Buckets.Delete(TestBucket.BucketId).Result;
		}
	}
}
