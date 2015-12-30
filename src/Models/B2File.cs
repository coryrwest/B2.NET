using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace B2Net.Models {
	public class B2File {
		public string FileId { get; set; }
		public string FileName { get; set; }
		public string Action { get; set; }
		public float Size { get; set; }
		public string UploadTimestamp { get; set; }
		public byte[] FileData { get; set; }
		// Uploaded File Response
		public string ContentLength { get; set; }
		public string ContentSHA1 { get; set; }
		public string ContentType { get; set; }
		// TODO: Eventually support
		//public string FileInfo { get; set; }
		// End

		public DateTime UploadTimestampDate
		{
			get
			{
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
