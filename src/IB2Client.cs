using System.Threading;
using System.Threading.Tasks;
using B2Net.Models;

namespace B2Net
{
  public interface IB2Client
  {
	string AllowedBucketId { get; }
	string AllowedBucketName { get; }

	IBuckets Buckets { get; }
    IFiles Files { get; }
    ILargeFiles LargeFiles { get; }

    Task<B2Options> Authorize(CancellationToken cancelToken = default(CancellationToken));
  }
}