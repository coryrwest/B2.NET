using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace B2Net.Models {
	public class B2Bucket {
		public string BucketId { get; set; }
		public string BucketName { get; set; }
		public string BucketType { get; set; }
	}

	public class B2BucketListDeserializeModel {
		public List<B2Bucket> Buckets { get; set; }
	}
}
