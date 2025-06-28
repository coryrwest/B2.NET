using System.Text.Json;
using System.Net.Http;
using System.Threading.Tasks;

namespace B2Net.Http {
	public static class ResponseParser {
		public static async Task<T> ParseResponse<T>(HttpResponseMessage response, string callingApi = "") {
			var jsonResponse = await response.Content.ReadAsStringAsync();

			await Utilities.CheckForErrors(response, callingApi);

			var options = new JsonSerializerOptions {
				DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
			};
			var obj = JsonSerializer.Deserialize<T>(jsonResponse, options);
			return obj;
		}
	}
}
