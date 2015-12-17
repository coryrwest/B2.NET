using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using B2Net.Http;
using B2Net.Models;
using Newtonsoft.Json;

namespace B2Net {
	public class Files {
		private B2Options _options;

		public Files(B2Options options) {
			_options = options;
		}

		/// <summary>
		/// Lists the names of all files in a bucket, starting at a given name.
		/// </summary>
		/// <param name="bucketId"></param>
		/// <param name="startFileName"></param>
		/// <param name="maxFileCount"></param>
		/// <param name="cancelToken"></param>
		/// <returns></returns>
		public async Task<B2FileList> GetList(string bucketId = "", string startFileName = "", int maxFileCount = 100, CancellationToken cancelToken = default(CancellationToken)) {
			// Check for a persistant bucket
			if (!_options.PersistBucket && string.IsNullOrEmpty(bucketId)) {
				throw new ArgumentNullException(nameof(bucketId));
			}

			var client = HttpClientFactory.CreateHttpClient();

			// Are we persisting buckets? If so use the one from settings
			string operationalBucketId = _options.PersistBucket ? _options.BucketId : bucketId;

			var requestMessage = FileMetaDataRequestGenerators.GetList(_options, operationalBucketId, startFileName, maxFileCount);
			var response = await client.SendAsync(requestMessage, cancelToken);

			var jsonResponse = await response.Content.ReadAsStringAsync();
			if (response.IsSuccessStatusCode) {
				var fileList = JsonConvert.DeserializeObject<B2FileList>(jsonResponse);
				return fileList;
			} else {
				throw new AuthorizationException(jsonResponse);
			}
		}
	}
}
