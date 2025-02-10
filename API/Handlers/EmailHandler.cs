using System.Collections.Generic;
using System.Threading.Tasks;
using Amazon.SimpleEmailV2;
using Amazon.SimpleEmailV2.Model;
using API.Constants;
using API.Lib;
using API.Views;
using System;
using System.Net;
using API.Models.Logs;
using Microsoft.AspNetCore.Mvc;
using System.Linq;

namespace API.Handlers
{
    // TODO move render
    public interface IEmailHandler
    {
        Task<SendEmailResponse> SendEmail(string sender, string receiver, string subject, BaseEmail model, string projectId);
        Task<SendEmailResponse> SendEmail(string sender, List<string> receiver, string subject, string body, BaseEmail model, string projectId);
    }

    public class EmailHandler : IEmailHandler
    {
        private const string SenderAddress = "{{}} <noreply@impactly.dk>";
        private readonly IAmazonSimpleEmailServiceV2 _emailService;
        private static readonly string ClientUrl = EnvironmentMode.ClientHost;
        private static readonly bool IsDisabled = EnvironmentMode.IsTest;

        private readonly ILogHandler _logHandler;
        
        private readonly IRazorViewToStringRenderer _razorViewToStringRenderer;

        public EmailHandler(IAmazonSimpleEmailServiceV2 emailService,
            IRazorViewToStringRenderer razorViewToStringRenderer, ILogHandler logHandler)
        {
            _emailService = emailService;
            _razorViewToStringRenderer = razorViewToStringRenderer;
            _logHandler = logHandler;
        }

        private async Task<string> ParseView(string view, BaseEmail model)
        {
            var viewModelPath = $"Views/{view}.cshtml";
            var res = IsDisabled
                ? "<html><body>TEST</body></html>"
                : await _razorViewToStringRenderer.RenderViewToStringAsync(viewModelPath, model);
            return res;
        }

        public async Task<SendEmailResponse> SendEmail(string sender, string receiver, string subject, string body, BaseEmail model, string projectId) =>
            await SendEmail(sender, new List<string> {receiver}, subject, body, model, projectId);
        
        public async Task<SendEmailResponse> SendEmail(string sender, string receiver, string subject, BaseEmail model, string projectId)
        {
            var htmlBody = await ParseView(model.GetThisClassName(), model);
            return await SendEmail(sender, new List<string> {receiver}, IsDisabled ? "Test mail" : subject, htmlBody, model, projectId); ;
        }

        private async Task<(string sanitized, bool wasSanitized)> TryIDNEmail(string email, string projectId)
        {
            try 
            {
                // Split email into local part and domain
                var parts = email.Split('@');
                if (parts.Length != 2)
                {
                    throw new FormatException("Invalid email format");
                }

                var localPart = parts[0];
                var domain = parts[1];

                // Check if local part contains special characters
                if (localPart.Any(c => c > 127)) // ASCII check
                {
                    // If we have special characters, sanitize them
                    var sanitized = email
                        .Replace("æ", "ae")
                        .Replace("ø", "o")
                        .Replace("å", "a")
                        .Replace("Æ", "AE")
                        .Replace("Ø", "O")
                        .Replace("Å", "A");

                    var sanitizeLog = new Log
                    {
                        Type = $"{Log.LogtypeEmail} - Sanitization",
                        ParentId = Log.LogtypeEmail,
                        Body = $"Email local part contains special characters: {email} -> {sanitized}",
                        Subject = "Email Sanitization",
                        Status = "Sanitized",
                        Sender = "sanitizeLog",
                        Receivers = new List<string> { sanitized },
                        ProjectId = projectId
                    };
                    await _logHandler.AddLog(sanitizeLog);
                    
                    return (sanitized, true);
                }

                // If no special characters, use as-is
                return (email, false);
            }
            catch (Exception ex)
            {
                // Log the specific validation error
                var errorLog = new Log
                {
                    Type = $"{Log.LogtypeEmail} - Validation",
                    ParentId = Log.LogtypeEmail,
                    Body = $"Email validation error: {email}, Error: {ex.Message}",
                    Subject = "Email Validation",
                    Status = "ValidationFailed",
                    Sender = "errorLog",
                    Receivers = new List<string> { email },
                    ProjectId = projectId
                };
                await _logHandler.AddLog(errorLog);

                // Return the result from the first check
                return (email.Replace("æ", "ae")
                            .Replace("ø", "o")
                            .Replace("å", "a")
                            .Replace("Æ", "AE")
                            .Replace("Ø", "O")
                            .Replace("Å", "A"), 
                        true);
            }
        }

        public async Task<SendEmailResponse> SendEmail(string sender, List<string> receivers, string subject,
            string body, BaseEmail model, string projectId)
        {
            try 
            {
                // Store original emails for logging
                var originalReceivers = receivers.ToList();
                
                // Process each email
                var processedEmails = new List<string>();
                foreach (var email in receivers)
                {
                    var (processed, wasSanitized) = await TryIDNEmail(email, projectId);
                    processedEmails.Add(processed);
                }

                var base64Name = System.Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(sender));
                
                var response = await _emailService.SendEmailAsync(new SendEmailRequest
                {
                    FromEmailAddress = SenderAddress.Replace("{{}}", $"=?UTF-8?B?{base64Name}?="),
                    Destination = new Destination
                    {
                        ToAddresses = processedEmails
                    },
                    Content = new EmailContent
                    {
                        Simple = new Message
                        {
                            Subject = new Content
                            {
                                Charset = "UTF-8",
                                Data = subject
                            },
                            Body = new Body
                            {
                                Html = new Content
                                {
                                    Charset = "UTF-8",
                                    Data = body
                                }
                            }
                        }
                    }
                });

                // Log successful send with original emails
                var log = new Log()
                {
                    Type = Log.LogtypeEmail,
                    ParentId = Log.LogtypeEmail,
                    Body = model.Message + " : " + model.DownloadUrl,
                    Sender = sender,
                    Receivers = originalReceivers, // Keep original emails in logs
                    Subject = subject,
                    Status = response.HttpStatusCode.ToString(),
                    ProjectId = projectId,
                };
                await _logHandler.AddLog(log);
                return response;
            }
            catch (Exception e)
            {
                // Log any other email sending errors
                var errorLog = new Log
                {
                    Type = Log.LogtypeEmail,
                    ParentId = Log.LogtypeEmail,
                    Body = $"Email sending failed: {e.Message}",
                    Sender = sender,
                    Receivers = receivers,
                    Subject = subject,
                    Status = HttpStatusCode.InternalServerError.ToString(),
                    ProjectId = projectId,
                };
                await _logHandler.AddLog(errorLog);
                throw;
            }
        }
    }
}