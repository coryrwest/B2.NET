using System.Threading;
using System.Threading.Tasks;
using B2Net.Models;

namespace B2Net {
	public interface IB2Client {
		IBuckets Buckets { get; }
		IFiles Files { get; }
		ILargeFiles LargeFiles { get; }
		B2Capabilities Capabilities { get; }

		Task<B2Options> Authorize(CancellationToken cancelToken = default(CancellationToken));
	}
}
