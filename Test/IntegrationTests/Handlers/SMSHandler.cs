using System.Net;
using System.Threading.Tasks;
using API.Handlers;
using Impactly.Test.Utils;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Impactly.Test.IntegrationTests.Handlers
{
    [Collection("Integration Test")]
    public class SMSHandler
    {
        private readonly ISMSHandler _smsHandler;

        public SMSHandler(TestFixture fixture)
        {
            _smsHandler = fixture.Server.Services.GetRequiredService<ISMSHandler>();
        }


        public async Task SendSMSWithoutCountryCode()
        {
            var res = await _smsHandler.SendSMS("Impactly", "20729960", "Impactly SMS test w.o. country code", "");
            Assert.NotEmpty(res.MessageId);
            Assert.Equal(HttpStatusCode.OK, res.HttpStatusCode);
        }
        
        public async Task SendSMSWithCountryCode()
        {
            var res = await _smsHandler.SendSMS("Impactly", "+4520729960", "Impactly SMS test w. country code", "");
            Assert.NotEmpty(res.MessageId);
            Assert.Equal(HttpStatusCode.OK, res.HttpStatusCode);
        }
    }
}