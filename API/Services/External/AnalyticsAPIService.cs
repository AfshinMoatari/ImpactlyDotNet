using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using API.Models.Views.AnalyticsReport.SROI;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace API.Services.External
{
    public interface IAnalyticsAPIService
    {
        /// <summary>
        /// Call analytics API endpoint to retrieve the result
        /// </summary>
        /// <returns>An object with related key value paires conating figures for generator.</returns>
        public Task<SROIAnalyticsResponseViewModel> GenerateSROIData(SROIAnalyticsAPIViewModel requestData);
    }

    /// <summary>
    /// Service class for managing analytics API calls.
    /// </summary>
    public class AnalyticsAPIService : IAnalyticsAPIService
    {
        private readonly HttpClient _httpClient;

        /// <summary>
        /// Initializes a new instance of the <see cref="AnalyticsAPIService"/> class.
        /// </summary>
        /// <param name="httpClient">The _httpClient.</param>
        public AnalyticsAPIService(HttpClient httpClient)
        {
            _httpClient = httpClient;
            ConfigureClient();
        }

        //public async Task<object> GenerateSROIData(SROIAnalyticsAPIViewModel requestData)
        //{
        //    string jsonData = JsonConvert.SerializeObject(requestData);
        //    var content = new StringContent(jsonData, Encoding.UTF8, "application/json");

        //    var httpResponseMessage = await _httpClient.PostAsync("sroi/investment/report/", content);
        //    httpResponseMessage.EnsureSuccessStatusCode();
        //    string responseBody = await httpResponseMessage.Content.ReadAsStringAsync();
        //    return responseBody ?? null;
        //}
        public class RootObject
        {
            public int Code { get; set; }
            public string Msg { get; set; }
            public SROIAnalyticsResponseViewModel Results { get; set; }
            public int Count { get; set; }
        }
        public async Task<SROIAnalyticsResponseViewModel> GenerateSROIData(SROIAnalyticsAPIViewModel requestData)
        {
            //string jsonData = JsonConvert.SerializeObject(requestData);
            var jsonData = JsonConvert.SerializeObject(requestData, Formatting.Indented, new JsonSerializerSettings
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver()
            });
            var content = new StringContent(jsonData, Encoding.UTF8, "application/json");
            var httpResponseMessage = await _httpClient.PostAsync("sroi/investment/report/", content);

            if (httpResponseMessage.IsSuccessStatusCode)
            {
                string responseBody = await httpResponseMessage.Content.ReadAsStringAsync();
                var rootObject = JsonConvert.DeserializeObject<RootObject>(responseBody);
                SROIAnalyticsResponseViewModel responseModel = rootObject.Results;
                return responseModel;
            }
            else
            {
                string reasonPhrase = httpResponseMessage.ReasonPhrase;

                throw new HttpRequestException($"Request failed with status code {(int)httpResponseMessage.StatusCode}: {reasonPhrase}");
            }
        }

        private void ConfigureClient()
        {
            _httpClient.BaseAddress = new Uri("http://analytics.services.impactly.dk:8080/");
        }
    }
}