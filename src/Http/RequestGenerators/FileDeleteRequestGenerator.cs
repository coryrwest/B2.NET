using System.Net.Http;
using B2Net.Http.RequestGenerators;
using B2Net.Models;
using Newtonsoft.Json;

namespace B2Net.Http {
	public static class FileDeleteRequestGenerator {
		private static class Endpoints {
			public const string Delete = "b2_delete_file_version";
		}

		public static HttpRequestMessage Delete(B2Options options, string fileId, string fileName) {
			return BaseRequestGenerator.PostRequest(Endpoints.Delete,
				"{\"fileId\":\"" + fileId + "\", \"fileName\":" + JsonConvert.ToString(fileName) + "}", options);
		}
	}
}
