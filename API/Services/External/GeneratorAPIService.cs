using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using API.Models.Views.AnalyticsReport.SROI;

namespace API.Services.External
{
    public interface IGeneratorAPIService
    {
        /// <summary>
        /// Call generator API endpoints to generate the documents
        /// </summary>
        /// <returns>The name of sucessfully generated document on the S3.</returns>
        public Task<string> GenerateSROIPDF(SROIGeneratorAPIRequestViewModel requestDate, string env);
    }

    /// <summary>
    /// Service class for managing generator API calls.
    /// </summary>
    public class GeneratorAPIService : IGeneratorAPIService
    {
        private readonly HttpClient _httpClient;

        /// <summary>
        /// Initializes a new instance of the <see cref="GeneratorAPIService"/> class.
        /// </summary>
        /// <param name="httpClient">The _httpClient.</param>
        public GeneratorAPIService(HttpClient httpClient)
        {
            _httpClient = httpClient;
            ConfigureClient();
        }

        public async Task<string> GenerateSROIPDF(SROIGeneratorAPIRequestViewModel requestData, string env)
        {
            _httpClient.DefaultRequestHeaders.Add("ENV", env);

            string jsonData = JsonSerializer.Serialize(requestData);
            var content = new StringContent(jsonData, Encoding.UTF8, "application/json");

            var httpResponseMessage = await _httpClient.PostAsync("report/sroi/", content);
            httpResponseMessage.EnsureSuccessStatusCode();
            string responseBody = await httpResponseMessage.Content.ReadAsStringAsync();
            return responseBody ?? null;
        }

        private void ConfigureClient()
        {
            _httpClient.BaseAddress = new Uri("http://generator.services.impactly.dk:8080/");
            //_httpClient.BaseAddress = new Uri("http://localhost:32768/");
        }
    }
}