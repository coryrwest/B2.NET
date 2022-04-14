using B2Net.Models;
using System;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace B2Net {
	public static class Utilities {
		private static readonly JsonSerializerOptions FormattedJsonSerializerOptions = new JsonSerializerOptions() {
			WriteIndented = true,
			PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
			PropertyNameCaseInsensitive = true,
			DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
		};

		public static string Serialize<T>(T obj) {
			//the branch origin used: JsonConvert
			return JsonSerializer.Serialize(obj);
		}

		public static string SerializeFormatted<T>(T obj) {
			//the branch origin used: JsonConvert.Serialize(data, Formatting.Indented, new JsonSerializerSettings() {ContractResolver = new CamelCasePropertyNamesContractResolver()})
			return JsonSerializer.Serialize(obj, FormattedJsonSerializerOptions);
		}

		public static T Deserialize<T>(string jsonResponse) {
			//the branch origin used: JsonConvert (and NullValueHandling.Ignore)
			return System.Text.Json.JsonSerializer.Deserialize<T>(jsonResponse, FormattedJsonSerializerOptions);
		}


		/// <summary>
		/// Create the B2 Authorization header. Base64 encoded accountId:applicationKey.
		/// </summary>
		public static string CreateAuthorizationHeader(string accountId, string applicationKey) {
			var authHeader = "Basic ";
			var credentials = Convert.ToBase64String(Encoding.UTF8.GetBytes(accountId + ":" + applicationKey));

			return authHeader + credentials;
		}

		public static async Task CheckForErrors(HttpResponseMessage response, string callingApi = "") {
			if (!response.IsSuccessStatusCode) {
				// Should retry
				bool retry = response.StatusCode == (HttpStatusCode) 429 ||
				             response.StatusCode == HttpStatusCode.RequestTimeout ||
				             response.StatusCode == HttpStatusCode.ServiceUnavailable;

				string content = await response.Content.ReadAsStringAsync();

				B2Error b2Error;
				try {
					b2Error = Utilities.Deserialize<B2Error>(content);
				}
				catch (Exception ex) {
					throw new Exception(
						"Seralization of the response failed. See inner exception for response contents and serialization error.",
						ex);
				}

				if (b2Error != null) {
					// If calling API is supplied, append to the error message
					if (!string.IsNullOrEmpty(callingApi) && b2Error.Status == 401) {
						b2Error.Message =
							$"Unauthorized error when operating on {callingApi}. Are you sure the key you are using has access? {b2Error.Message}";
					}

					throw new B2Exception(b2Error.Code, b2Error.Status, b2Error.Message, retry);
				}
			}
		}

		public static string GetSHA1Hash(byte[] fileData) {
			using (var sha1 = SHA1.Create()) {
				return HexStringFromBytes(sha1.ComputeHash(fileData));
			}
		}

		public static string DetermineBucketId(B2Config options, string bucketId) {
			// Check for a persistant bucket
			if (!options.PersistBucket && string.IsNullOrEmpty(bucketId)) {
				throw new ArgumentNullException(nameof(bucketId),
					"You must either Persist a Bucket or provide a BucketId in the method call.");
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
			public int Status { get; set; }
		}
	}

	public static class B2StringExtension {
		public static string b2UrlEncode(this string str) {
			if (str == "/") {
				return str;
			}

			// Decode / back to un-encoded value
			return Uri.EscapeDataString(str).Replace("%2F", "/");
		}

		public static string b2UrlDecode(this string str) {
			if (str == "+") {
				return " ";
			}

			return Uri.UnescapeDataString(str);
		}
	}
}
