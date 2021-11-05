using System;
using System.Collections.Generic;

namespace B2Net.Models {
	public class B2File {
		public string FileId { get; set; }
		public string FileName { get; set; }
		public string Action { get; set; }
		/// <summary>
		/// DEPRECATED: Size will always be the same as ContentLength.
		/// </summary>
		public long Size => Convert.ToInt64(ContentLength);
		public string UploadTimestamp { get; set; }
		public byte[] FileData { get; set; }
		public FileRetentionReturn FileRetention { get; set; }
		public int FileRetentionRetainUntilTimestamp { get; set; }
		public LegalHold LegalHold { get; set; }
		public bool LegalHoldBool => LegalHold?.Value == "on";
		/// <summary>
		/// List of headers that exist in on the file that the client does not have permission to read.
		/// </summary>
		public string[] ClientUnauthorizedToRead { get; set; }

		// Uploaded File Response
		public string ContentLength { get; set; }
		public string ContentSHA1 { get; set; }
		public string ContentMD5 { get; set; }
		public string ContentType { get; set; }
		public Dictionary<string, string> FileInfo { get; set; }
		// End

		public DateTime UploadTimestampDate {
			get {
				if (!string.IsNullOrEmpty(UploadTimestamp)) {
					var epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
					return epoch.AddMilliseconds(double.Parse(UploadTimestamp));
				} else {
					return DateTime.Now;
				}
			}
		}
	}
}
