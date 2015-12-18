using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace B2Net.Models {
	public class B2FileList {
		public string NextFileName { get; set; }
		public string NextFileId { get; set; }
		public List<B2File> Files { get; set; }
	}
}
