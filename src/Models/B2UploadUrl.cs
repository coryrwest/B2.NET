using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace B2Net.Models {
    public class B2UploadUrl {
        public string BucketId { get; set; }
        public string UploadUrl { get; set; }
        public string AuthorizationToken { get; set; }
    }
}
