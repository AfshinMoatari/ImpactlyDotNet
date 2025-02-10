using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using API.Models;
using Xunit;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace Impactly.Test.IntegrationTests.Client
{
    public static class CrudExtension
    {
        public static readonly ITestOutputHelper OutputHelper; // When having problems, can be set outside and used here
        
        public static async Task<T> Post<T>(this HttpClient client, string path, T request)
            where T : class

        {
            var response = await client.Fetch<T>(HttpMethod.Post, path, request).ConfigureAwait(false);
            var responseBody = response.Value;
            return responseBody;
        }

        public static async Task<T> CreateElementOf<T>(this HttpClient client, string path,
            object request)
            where T : class, ICrudPropModel
        {
            var response = await client.Fetch<T>(HttpMethod.Post, path, request);
            var responseBody = response.Value;

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.NotNull(responseBody);
            return responseBody;
        }

        public static async Task<T> Read<T>(this HttpClient client, string path)
            where T : class, ICrudPropModel

        {
            var response = await client.Fetch<T>(HttpMethod.Get, path);
            var responseBody = response.Value;

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.NotNull(responseBody);
            return responseBody;
        }

        public static async Task ReadNotFound(this HttpClient client, string path)
        {
            var response = await client.Fetch<object>(HttpMethod.Get, path);
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        public static async Task<List<T>> ReadAll<T>(this HttpClient client, string path)
        {
            var response = await client.FetchAll<T>(HttpMethod.Get, path);
            var responseBody = response.Value;
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.NotNull(responseBody);
            return responseBody;
        }

        public static async Task<T> Update<T>(this HttpClient client, string path, T request)
            where T : class, ICrudPropModel
        {
            var response = await client.Fetch<T>(HttpMethod.Put, path, request);
            var responseBody = response.Value;
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.NotNull(responseBody);
            Assert.NotEmpty(responseBody.Id);
            Assert.Equal(request.Id, responseBody.Id);
            return responseBody;
        }

        public static async Task<T> Update<T>(this HttpClient client, string path)
            where T : class, ICrudPropModel
        {
            var response = await client.Fetch<T>(HttpMethod.Put, path);
            var responseBody = response.Value;
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.NotNull(responseBody);
            return responseBody;
        }

        public static async Task<T> Update<T, TS>(this HttpClient client, string path, List<TS> request)
            where T : class, ICrudPropModel
        {
            var response = await client.Fetch<T>(HttpMethod.Put, path, request);
            var responseBody = response.Value;
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.NotNull(responseBody);
            return responseBody;
        }

        public static async Task Delete(this HttpClient client, string path)
        {
            var response = await client.Fetch<object>(HttpMethod.Delete, path, null);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        public static async Task<T> DeleteElementOf<T>(this HttpClient client, string path)
            where T : class, ICrudPropModel
        {
            var response = await client.Fetch<T>(HttpMethod.Delete, path, null);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var responseBody = response.Value;

            return responseBody;
        }
    }
}