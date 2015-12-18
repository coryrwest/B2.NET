using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using B2Net.Models;
using Newtonsoft.Json;

namespace B2Net.Http {
	public static class ResponseParser {
		public static async Task<T> ParseResponse<T>(HttpResponseMessage response) {
			var jsonResponse = await response.Content.ReadAsStringAsync();

			Utilities.CheckForErrors(response);

			return JsonConvert.DeserializeObject<T>(jsonResponse);
		}
	}
}
