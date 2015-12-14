using System;
using System.Text;

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

		//public static void CheckForErrors(HttpResponseMessage response) {
		//	var statusCode = response.StatusCode;
		//	string content = null;

		//	if (!response.IsSuccessStatusCode) {
		//		// What to do with it.
		//	}
		//}
	}
}
