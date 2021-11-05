using System;
using System.Collections.Generic;
using System.Text;

namespace B2Net.Models {
	public class B2FileUploadContext {
		public string FileName { get; set; }
		public string BucketId { get; set; }
		public Dictionary<string, string> AdditionalFileInfo { get; set; }
		public B2UploadUrl B2UploadUrl { get; set; }
		public bool AutoRetry { get; set; }

		public string ContentType { get; set; }
		public string ContentSHA { get; set; }
		/// <summary>
		/// Follow the convention specified here: https://www.backblaze.com/b2/docs/b2_upload_file.html
		/// The value should be a base 10 number which represents a UTC time when the original source file was last modified. It is a base 10 number of milliseconds since midnight, January 1, 1970 UTC.
		/// </summary>
		public long SrcLastModifiedMillis { get; set; }
		/// <summary>
		/// Follow the grammar specified in RFC 2616 https://datatracker.ietf.org/doc/html/rfc2616#section-19.5.1
		/// </summary>
		public string ContentDisposition { get; set; }
		/// <summary>
		/// Follow the grammar specified in RFC 2616 https://datatracker.ietf.org/doc/html/rfc2616#section-19.5.1
		/// </summary>
		public string ContentLanguage { get; set; }
		/// <summary>
		/// Follow the grammar specified in RFC 2616 https://datatracker.ietf.org/doc/html/rfc2616#section-19.5.1
		/// </summary>
		public DateTime? Expires { get; set; }
		/// <summary>
		/// Follow the grammar specified in RFC 2616 https://datatracker.ietf.org/doc/html/rfc2616#section-19.5.1
		/// </summary>
		public string CacheControl { get; set; }
		/// <summary>
		/// Follow the grammar specified in RFC 2616 https://datatracker.ietf.org/doc/html/rfc2616#section-19.5.1
		/// </summary>
		public ContentEncoding? ContentEncoding { get; set; }
		public bool LegalHold { get; set; }
		public RetentionMode? RetentionMode { get; set; }
		/// <summary>
		///  This header value must be specified as a base 10 number of milliseconds since midnight, January 1, 1970 UTC.
		/// </summary>
		public long RetainUntilTimestamp { get; set; }
	}
}
