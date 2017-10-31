using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Runtime.Serialization;
using System.Text;
using B2Net.Models;
using Newtonsoft.Json;

namespace B2Net.Http.RequestGenerators
{
    public class LargeFileRequestGenerators
    {
        private static class Endpoints {
            public const string Start = "b2_start_large_file";
            public const string GetPartUrl = "b2_get_upload_part_url";
            public const string Upload = "b2_upload_part";
            public const string Finish = "b2_finish_large_file";
        }

        public static HttpRequestMessage Start(B2Options options, string bucketId, string fileName, string contentType, Dictionary<string, string> fileInfo = null) {
            var uri = new Uri(options.ApiUrl + "/b2api/" + Constants.Version + "/" + Endpoints.Start);
            var content = "{\"bucketId\":\"" + bucketId + "\",\"fileName\":\"" + fileName +
                                            "\",\"contentType\":\"" + (string.IsNullOrEmpty(contentType) ? "b2/x-auto" : contentType) + "\"}";
            var request = new HttpRequestMessage() {
                Method = HttpMethod.Post,
                RequestUri = uri,
                Content = new StringContent(content),
            };

            request.Headers.Add("Authorization", options.AuthorizationToken);
            // File Info headers
            if (fileInfo != null && fileInfo.Count > 0) {
                foreach (var info in fileInfo.Take(10)) {
                    request.Headers.Add($"X-Bz-Info-{info.Key}", info.Value);
                }
            }
            request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
            request.Content.Headers.ContentLength = content.Length;

            return request;
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
        public static HttpRequestMessage Upload(B2Options options, byte[] fileData, int partNumber, B2UploadPartUrl uploadPartUrl) {
            if (partNumber < 1 || partNumber > 10000) {
                throw new Exception("Part number must be between 1 and 10,000");
            }

            var uri = new Uri(uploadPartUrl.UploadUrl);
            var request = new HttpRequestMessage() {
                Method = HttpMethod.Post,
                RequestUri = uri,
                Content = new ByteArrayContent(fileData)
            };

            // Get the file checksum
            string hash = Utilities.GetSHA1Hash(fileData);

            // Add headers
            request.Headers.Add("Authorization", uploadPartUrl.AuthorizationToken);
            request.Headers.Add("X-Bz-Part-Number", partNumber.ToString());
            request.Headers.Add("X-Bz-Content-Sha1", hash);
            request.Content.Headers.ContentLength = fileData.Length;

            return request;
        }

        public static HttpRequestMessage GetUploadPartUrl(B2Options options, string fileId) {
            return BaseRequestGenerator.PostRequest(Endpoints.GetPartUrl, "{\"fileId\":\"" + fileId + "\"}", options);
        }

        public static HttpRequestMessage Finish(B2Options options, string fileId, string[] partSHA1Array) {
            var uri = new Uri(options.ApiUrl + "/b2api/" + Constants.Version + "/" + Endpoints.Finish);
            var content = JsonConvert.SerializeObject(new { fileId, partSha1Array  = partSHA1Array });
            var request = new HttpRequestMessage() {
                Method = HttpMethod.Post,
                RequestUri = uri,
                Content = new StringContent(content),
            };

            request.Headers.Add("Authorization", options.AuthorizationToken);
            request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
            request.Content.Headers.ContentLength = content.Length;

            return request;
        }
    }
}
