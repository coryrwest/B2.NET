﻿using System;
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
        public int ContentLength { get; set; }
        public string ContentSha1 { get; set; }
    }

    public class B2LargeFileParts {
        public int NextPartNumber { get; set; }
        public List<B2LargeFilePart> Parts { get; set; }
    }

    public class B2LargeFilePart {
        public string FileId { get; set; }
        public int PartNumber { get; set; }
        public string ContentLength { get; set; }
        public string ContentSha1 { get; set; }
        public string UploadTimestamp { get; set; }
    }

    public class B2CancelledFile {
        public string FileId { get; set; }
        public string AccountId { get; set; }
        public string BucketId { get; set; }
        public string FileName { get; set; }
    }

    public class B2IncompleteLargeFiles {
        public string NextFileId { get; set; }
        public List<B2File> Files { get; set; }
    }
}
