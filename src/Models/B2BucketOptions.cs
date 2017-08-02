using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace B2Net.Models
{
    public class B2BucketOptions
    {
        public BucketTypes BucketType { get; set; } = BucketTypes.allPrivate;
        public int CacheControl { get; set; }
    }
}
