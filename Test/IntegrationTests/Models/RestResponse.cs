using System.Net;
using API.Models;

namespace Impactly.Test.IntegrationTests.Models
{
    public class RestResponse<T>
    {
        public HttpStatusCode StatusCode { get; set; }
        public string ReasonPhrase { get; set; }
        public bool IsSuccessStatusCode => (int) StatusCode >= 200 && (int) StatusCode <= 299;
        public T Value;
        public ErrorResponse Error { get; set; }
    }
}