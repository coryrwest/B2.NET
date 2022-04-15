using System;
using System.Collections.Generic;

namespace B2Net.Models {
	public class B2File {
		public string FileId { get; set; }
		public string FileName { get; set; }
		public string Action { get; set; }
		public long? UploadTimestamp { get; set; }

		public byte[] FileData { get; set; }

		// Uploaded File Response
		public long ContentLength { get; set; }
		public string ContentMd5 { get; set; }
		public string ContentSHA1 { get; set; }
		public string ContentType { get; set; }

		public Dictionary<string, string> FileInfo { get; set; }
		// End

		public DateTime UploadTimestampDate {
			get {
				if (UploadTimestamp != null) {
					var epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

					return epoch.AddMilliseconds(UploadTimestamp.Value);
				}
				else {
					return DateTime.Now;
				}
			}
		}
	}
}
