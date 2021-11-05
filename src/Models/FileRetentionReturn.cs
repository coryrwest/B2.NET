using System;
using System.Collections.Generic;
using System.Text;

namespace B2Net.Models {
	public class FileRetentionReturn {
		public bool IsClientAuthorizedToRead { get; set; }
		public FileRetentionValue Value { get; set; }
	}

	public class FileRetentionValue {
		public string Mode { get; set; }
		public long RetainUntilTimestamp { get; set; }
	}
}
