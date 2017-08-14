using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace B2Net.Http {
	public static class ResponseParser {
		public static async Task<T> ParseResponse<T>(HttpResponseMessage response) {
			var jsonResponse = await response.Content.ReadAsStringAsync();

			Utilities.CheckForErrors(response);

			var obj = JsonConvert.DeserializeObject<T>(jsonResponse, new JsonSerializerSettings() {
                NullValueHandling = NullValueHandling.Ignore
            });
            return obj;
		}
	}
}
