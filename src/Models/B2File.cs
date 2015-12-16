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
		public DateTime UploadTimestamp { get; set; }
	}
}
