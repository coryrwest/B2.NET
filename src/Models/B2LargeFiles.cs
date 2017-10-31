using System;
using System.Collections.Generic;
using System.Text;

namespace B2Net.Models
{
    public class B2UploadPartUrl {
        public string FileId { get; set; }
        public string UploadUrl { get; set; }
        public string AuthorizationToken { get; set; }
    }

    public class B2UploadPart {
        public string FileId { get; set; }
        public int PartNumber { get; set; }
        public int Length { get; set; }
        public string SHA1 { get; set; }
    }
}
