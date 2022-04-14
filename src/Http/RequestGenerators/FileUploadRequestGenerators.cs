﻿using System;
using System.Net.Http;
using System.Net.Http.Headers;
using B2Net.Http.RequestGenerators;
using B2Net.Models;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;


namespace B2Net.Http {
	public static class FileUploadRequestGenerators {
		private static class Endpoints {
			public const string GetUploadUrl = "b2_get_upload_url";
		}

		/// <summary>
		/// Upload a file to B2. This method will calculate the SHA1 checksum before sending any data.
		/// </summary>
		/// <param name="options"></param>
		/// <param name="uploadUrl"></param>
		/// <param name="fileData"></param>
		/// <param name="fileName"></param>
		/// <param name="fileInfo"></param>
		/// <returns></returns>
		public static HttpRequestMessage Upload(string authorizationToken, string uploadUrl, byte[] fileData,
			string fileName,
			Dictionary<string, string> fileInfo, string contentType = "") {
			var uri = new Uri(uploadUrl);
			var request = new HttpRequestMessage() {
				Method = HttpMethod.Post,
				RequestUri = uri,
				Content = new ByteArrayContent(fileData)
			};

			// Get the file checksum
			string hash = Utilities.GetSHA1Hash(fileData);

			// Add headers
			request.Headers.TryAddWithoutValidation("Authorization", authorizationToken);
			request.Headers.Add("X-Bz-File-Name", fileName.b2UrlEncode());
			request.Headers.Add("X-Bz-Content-Sha1", hash);
			// File Info headers
			if (fileInfo != null && fileInfo.Count > 0) {
				foreach (var info in fileInfo.Take(10)) {
					request.Headers.Add($"X-Bz-Info-{info.Key}", info.Value);
				}
			}
			// TODO last modified
			//request.Headers.Add("X-Bz-src_last_modified_millis", hash);

			request.Content.Headers.ContentType =
				new MediaTypeHeaderValue(string.IsNullOrWhiteSpace(contentType) ? "b2/x-auto" : contentType);
			request.Content.Headers.ContentLength = fileData.Length;

			return request;
		}

		/// <summary>
		/// Upload a file to B2 using a stream. NOTE: You MUST provide the SHA1 at the end of your stream. This method will NOT do it for you.
		/// </summary>
		/// <param name="options"></param>
		/// <param name="uploadUrl"></param>
		/// <param name="fileData"></param>
		/// <param name="fileName"></param>
		/// <param name="fileInfo"></param>
		/// <returns></returns>
		public static HttpRequestMessage Upload(string authorizationToken, string uploadUrl, Stream fileDataWithSHA,
			string fileName, Dictionary<string, string> fileInfo, string contentType = "", bool dontSHA = false) {
			var uri = new Uri(uploadUrl);
			var request = new HttpRequestMessage() {
				Method = HttpMethod.Post,
				RequestUri = uri,
				Content = new StreamContent(fileDataWithSHA)
			};

			// Add headers
			request.Headers.TryAddWithoutValidation("Authorization", authorizationToken);
			request.Headers.Add("X-Bz-File-Name", fileName.b2UrlEncode());
			// Stream puts the SHA1 at the end of the content
			request.Headers.Add("X-Bz-Content-Sha1", dontSHA ? "do_not_verify" : "hex_digits_at_end");
			// File Info headers
			if (fileInfo != null && fileInfo.Count > 0) {
				foreach (var info in fileInfo.Take(10)) {
					request.Headers.Add($"X-Bz-Info-{info.Key}", info.Value);
				}
			}
			// TODO last modified
			//request.Headers.Add("X-Bz-src_last_modified_millis", hash);

			request.Content.Headers.ContentType =
				new MediaTypeHeaderValue(string.IsNullOrWhiteSpace(contentType) ? "b2/x-auto" : contentType);
			// SHA will be in Stream already
			request.Content.Headers.ContentLength = fileDataWithSHA.Length;

			return request;
		}

		public static HttpRequestMessage WithGetUploadUrlBody(this HttpRequestMessage request, string bucketId) {
			var json = Utilities.Serialize(new { bucketId });
			request = request.WithJson(json);

			return request;
		}

		public static HttpRequestMessage ToFileUploadEndpoint(this HttpRequestMessage request, string apiUrl) {
			request = request.ToEndpoint(HttpMethod.Post, apiUrl, Endpoints.GetUploadUrl);

			return request;
		}
	}
}
