using System.Collections.Generic;

namespace B2Net.Models {
	public class B2Bucket {
		public string BucketId { get; set; }
		public string BucketName { get; set; }
		public string BucketType { get; set; }
		public Dictionary<string, string> BucketInfo { get; set; }
		public List<B2BucketLifecycleRule> LifecycleRules { get; set; }
		public List<B2CORSRule> CORSRules { get; set; }
		public int Revision { get; set; }
		public FileLockConfiguration FileLockConfiguration { get; set; }
	}

	public class B2BucketLifecycleRule {
		public int? DaysFromHidingToDeleting { get; set; }
		public int? DaysFromUploadingToHiding { get; set; }
		public string FileNamePrefix { get; set; }
	}

	public class B2BucketListDeserializeModel {
		public List<B2Bucket> Buckets { get; set; }
	}
	public class FileLockValue {
		public B2DefaultRetention DefaultRetention { get; set; }
		public bool IsFileLockEnabled { get; set; }
	}
	public class FileLockConfiguration {
		public bool IsClientAuthorizedToRead { get; set; }
		public FileLockValue Value { get; set; }
	}

}
