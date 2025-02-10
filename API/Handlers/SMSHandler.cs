using System.Collections.Generic;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Amazon.SimpleNotificationService;
using Amazon.SimpleNotificationService.Model;
using API.Constants;
using API.Models.Logs;

namespace API.Handlers
{
    public interface ISMSHandler
    {
        public Task<PublishResponse> SendSMS(string sender, string receiver, string body, string projectId);
    }

    public class SMSHandler : ISMSHandler
    {
        private readonly IAmazonSimpleNotificationService _smsService;

        private readonly ILogHandler _logHandler;

        Regex regex = new Regex(@"\+45");

        public SMSHandler(IAmazonSimpleNotificationService smsService, ILogHandler logHandler)
        {
            _smsService = smsService;
            _logHandler = logHandler;
        }

        public async Task<PublishResponse> SendSMS(string sender, string receiver, string body, string projectId)
        {
            var match = regex.Match(receiver);
            var phoneNumber = match.Success ? receiver : $"+45{receiver}";

            var req = new PublishRequest
            {
                PhoneNumber = phoneNumber,
                Message = body,
                MessageAttributes =
                    new Dictionary<string, MessageAttributeValue>
                    {
                        {
                            "AWS.SNS.SMS.SenderID",
                            new MessageAttributeValue
                            {
                                DataType = "String",
                                StringValue = "Impactly" // TODO: Create form to set senderId 1-11 alphanumeric
                            }
                        }
                    }
            };

            var response = await _smsService.PublishAsync(req);

            var log = new Log()
            {
                Sender = sender,
                Receivers = new List<string> { phoneNumber },
                Body = body,
                Status = response.HttpStatusCode.ToString(),
                Type = Log.LogtypeSms,
                ProjectId = projectId
            };
            await _logHandler.AddLog(log);
            return response;
        }
    }
}