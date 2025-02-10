using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using API.Models.Auth;
using Impactly.Test.IntegrationTests.Models;
using Newtonsoft.Json;
using Xunit.Abstractions;

namespace Impactly.Test.IntegrationTests.Client
{
    public static class RestExtension
    {
        public static ITestOutputHelper OutputHelper; // When having problems, can be set outside and used here

        private static string AccessToken { get; set; }
        private static string RefreshToken { get; set; }

        public static void SetAuthorization(this HttpClient client, Authorization authorization)
        {
            AccessToken = authorization?.AccessToken ?? AccessToken;
            RefreshToken = authorization?.RefreshToken ?? RefreshToken;
        }

        public static Task<RestResponse<List<T>>> FetchAll<T>(this HttpClient client, HttpMethod method, string path) =>
            client.Fetch<List<T>>(method, path, null, null);

        public static Task<RestResponse<T>> Fetch<T>(this HttpClient client, HttpMethod method, string path) =>
            client.Fetch<T>(method, path, null, null);

        public static Task<RestResponse<T>> Fetch<T>(this HttpClient client, HttpMethod method, string path,
            object body) =>
            client.Fetch<T>(method, path, body, null);

        public static async Task<HttpResponseMessage> FetchResponse(this HttpClient client, HttpMethod method,
            string path, object body,
            Dictionary<string, string> headers)
        {
            var url = new Uri(client.BaseAddress + path[1..]);
            var serialisedBody = JsonConvert.SerializeObject(body);

            return await client.SendAsync(new HttpRequestMessage
            {
                Method = method,
                RequestUri = url,
                Content = new StringContent(serialisedBody, Encoding.Default, "application/json"),
                Headers =
                {
                    Authorization = client.GetAuthorization(headers),
                }
            });
        }

        public static async Task<RestResponse<T>> Fetch<T>(this HttpClient client, HttpMethod method, string path,
            object body,
            Dictionary<string, string> headers)
        {
            var response = await client.FetchResponse(method, path, body, headers);
            var responseContent = await response.Content.ReadAsStringAsync();

            try
            {
                var value = (T) JsonConvert.DeserializeObject(responseContent, typeof(T));
                return new RestResponse<T>
                {
                    StatusCode = response.StatusCode,
                    ReasonPhrase = response.ReasonPhrase,
                    Value = value,
                    // Error = error
                };
            }
            catch (Exception e)
            {
                return new RestResponse<T>
                {
                    StatusCode = response.StatusCode,
                    ReasonPhrase = response.ReasonPhrase,
                    Value = default
                };
            }
        }

        private static AuthenticationHeaderValue GetAuthorization(this HttpClient client,
            IReadOnlyDictionary<string, string> headers)
        {
            if (headers == null || !headers.ContainsKey("Authorization"))
                return new AuthenticationHeaderValue("Bearer", AccessToken);
            return new AuthenticationHeaderValue("Bearer", headers["Authorization"].Split(" ")[1]);
        }
    }
}