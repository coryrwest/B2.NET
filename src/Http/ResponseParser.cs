using Newtonsoft.Json;
using System.Net.Http;
using System.Threading.Tasks;

namespace B2Net.Http {
	public static class ResponseParser {
		public static async Task<T> ParseResponse<T>(HttpResponseMessage response, string callingApi = "") {
			var jsonResponse = await response.Content.ReadAsStringAsync();

			await Utilities.CheckForErrors(response, callingApi);

			var obj = JsonConvert.DeserializeObject<T>(jsonResponse, new JsonSerializerSettings() {
				NullValueHandling = NullValueHandling.Ignore
			});
			return obj;
		}
	}
}
