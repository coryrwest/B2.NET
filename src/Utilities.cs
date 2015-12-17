using System;
using System.Diagnostics;
using System.Net.Http;
using System.Text;
using B2Net.Models;
using Newtonsoft.Json;

namespace B2Net {
	public static class Utilities {
		/// <summary>
		/// Create the B2 Authorization header. Base64 encoded accountId:applicationKey.
		/// </summary>
		public static string CreateAuthorizationHeader(string accountId, string applicationKey) {
			var authHeader = "Basic ";
			var credentials = Convert.ToBase64String(Encoding.UTF8.GetBytes(accountId + ":" + applicationKey));
			return authHeader + credentials;
		}
		
		public static void CheckForErrors(HttpResponseMessage response) {
			if (!response.IsSuccessStatusCode) {
				string content = response.Content.ReadAsStringAsync().Result;

				try {
					var b2Error = JsonConvert.DeserializeObject<B2Error>(content);
					throw new Exception($"Status: {b2Error.Status}, Code: {b2Error.Code}, Message: {b2Error.Message}");
				} catch (Exception ex) {
					var inner = new Exception("Content: " + content, ex);
					throw new Exception("Seralization of the response failed. See inner exception for response contents and serialization error.", inner);
				}
			}
		}

		internal class B2Error {
				public string Code { get; set; }
				public string Message { get; set; }
				public string Status { get; set; }
		}
	}
}
