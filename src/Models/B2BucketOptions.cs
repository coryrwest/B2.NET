﻿using System.Collections.Generic;

namespace B2Net.Models {
	public class B2BucketOptions {
		public BucketTypes BucketType { get; set; } = BucketTypes.allPrivate;
		public int CacheControl { get; set; }
		public List<B2BucketLifecycleRule> LifecycleRules { get; set; }
		public List<B2CORSRule> CORSRules { get; set; }
		/// <summary>
		/// Only used on Create Bucket
		/// </summary>
		public bool FileLockEnabled { get; set; }
		/// <summary>
		/// Only used on Update Bucket
		/// </summary>
		public B2DefaultRetention DefaultRetention { get; set; }
	}
}
