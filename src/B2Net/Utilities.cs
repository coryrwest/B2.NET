using System;
using System.Net.Http;
using System.Security.Cryptography;
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

				B2Error b2Error;
				try {
					b2Error = JsonConvert.DeserializeObject<B2Error>(content);
				} catch (Exception ex) {
					throw new Exception("Seralization of the response failed. See inner exception for response contents and serialization error.", ex);
				}
				if (b2Error != null) {
					throw new Exception($"Status: {b2Error.Status}, Code: {b2Error.Code}, Message: {b2Error.Message}");
				}
			}
		}

		public static string GetSHA1Hash(byte[] fileData) {
			using (var sha1 = SHA1.Create()) {
				return HexStringFromBytes(sha1.ComputeHash(fileData));
			}
		}

		public static string DetermineBucketId(B2Options options, string bucketId) {
			// Check for a persistant bucket
			if (!options.PersistBucket && string.IsNullOrEmpty(bucketId)) {
				throw new ArgumentNullException(nameof(bucketId));
			}
			
			// Are we persisting buckets? If so use the one from settings
			return options.PersistBucket ? options.BucketId : bucketId;
		}

		private static string HexStringFromBytes(byte[] bytes) {
			var sb = new StringBuilder();
			foreach (byte b in bytes) {
				var hex = b.ToString("x2");
				sb.Append(hex);
			}
			return sb.ToString();
		}

		internal class B2Error {
				public string Code { get; set; }
				public string Message { get; set; }
				public string Status { get; set; }
		}
	}

	public static class B2StringExtension {
		public static string b2UrlEncode(this string str) {
			if (str == "/") {
				return str;
			}
			return Uri.EscapeDataString(str);
		}

		public static string b2UrlDecode(this string str) {
			if (str == "+") {
				return " ";
			}
			return Uri.UnescapeDataString(str);
		}
	}
}
