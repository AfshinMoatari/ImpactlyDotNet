using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using API.Constants;
using API.Models.Analytics;
using API.Models.Codes;
using API.Models.Notifications;
using API.Models.Projects;
using API.Models.Strategy;
using API.Repositories;
using API.Services;
using API.Views;

namespace API.Handlers
{
    public interface ISurveyHandler
    {
        public Task<(SurveyCode code, bool success)> SendSurvey(string projectId, string sender, ProjectPatient patient, 
            string strategyId, List<SurveyProperty> surveyProperties, string frequencyId);

        public Task<(SurveyCode code, bool success)> SendSurvey(string projectId, string sender, ProjectPatient patient, 
            string strategyId, string surveyCodeId, string NotificationId);

        public Task<SurveyCode> CreateSurveyCode(ProjectPatient patient, string strategyId,
            List<SurveyProperty> surveyProperties, string frequencyId);

        public void SetMessage(ISystemMessage message);
    }

    public class SurveyHandler : ISurveyHandler
    {
        private readonly ISMSHandler _smsHandler;
        private readonly IEmailHandler _emailHandler;
        private readonly ICodeContext _codeContext;
        private readonly IProjectContext _projectContext;
        private readonly NotificationService _notificationService;

        private ISystemMessage _message = new MessageDanish();
        
        public SurveyHandler(ISMSHandler smsHandler, IEmailHandler emailHandler, ICodeContext codeContext,
            NotificationService notificationService, IProjectContext projectContext)
        {
            _smsHandler = smsHandler;
            _emailHandler = emailHandler;
            _codeContext = codeContext;
            _notificationService = notificationService;
            _projectContext = projectContext;
        }

        public async Task<SurveyCode> CreateSurveyCode(ProjectPatient patient, string strategyId,
            List<SurveyProperty> surveyProperties, string frequencyId)
        {
            var surveyCode = new SurveyCode
            {
                Id = Regex.Replace(Convert.ToBase64String(Guid.NewGuid().ToByteArray()), "[/+=]", ""),
                ProjectId = patient.ParentId,
                StrategyId = strategyId,
                PatientId = patient.Id,
                FrequencyId = frequencyId,
                Properties = surveyProperties
            };

            await _codeContext.SurveyCodes.Create(surveyCode);

            return surveyCode;
        }

        public void SetMessage(ISystemMessage message)
        {
            this._message = message;
        }

        public async Task<(SurveyCode code, bool success)> SendSurvey(string projectId, string sender, 
            ProjectPatient patient, string strategyId, List<SurveyProperty> surveyProperties, string frequencyId)
        {
            if (!patient.IsActive) return (null, false);

            var code = await CreateSurveyCode(patient, strategyId, surveyProperties, frequencyId);
            if (EnvironmentMode.IsTest) return (code, true);

            var comms = await _projectContext.Communicaitons.ReadAll(patient.ParentId);
            var project = await _projectContext.Projects.Read(patient.ParentId);
            _message = project.TextLanguage switch
            {
                Languages.English => new MessageEnglish(),
                Languages.Danish => new MessageDanish(),
                _ => new MessageDanish()
            };
            var surveyMessage = comms
                .FirstOrDefault(t => t.MessageType == ProjectCommunication.CommunicationTypeSurvey);
            var messageText = surveyMessage == null
                ? _message.DefaultSurveyMessage().Replace("@Model.ProjectName", project.Name)
                : surveyMessage.MessageContent;
            var name = patient.Anonymity ? AnonymityMessage.HiddenPatientName : patient.Name;
            if (patient.PhoneNumber != null)
            {
                var url = $"{EnvironmentMode.ClientHost}/s/{code.Id}";
                var body = _message.SmsSurvey(name, messageText, url, project.Name);

                var phoneRes = await _smsHandler.SendSMS(sender, patient.PhoneNumber, body, projectId);
                if (phoneRes.HttpStatusCode == HttpStatusCode.OK)
                {
                    await CreateNotification(patient.Id, strategyId, projectId, code);
                    return (code, true);
                }
            }

            if (patient.Email == null) return (null, false);
            
            var emailRes = await _emailHandler.SendEmail(sender, patient.Email, _message.SurveyEmailTitle(),
                new SurveyEmail(_message)
                {
                    Title = _message.SurveyEmailLines()[0] + " " + sender,
                    ProjectName = sender,
                    UserName = name,
                    Message = messageText,
                    DownloadUrl = $"{EnvironmentMode.ClientHostForEmail}/s/{code.Id}",
                }, projectId);

            if (emailRes.HttpStatusCode != HttpStatusCode.OK) return (null, false);
            
            await CreateNotification(patient.Id, strategyId, projectId, code);
            return (code, true);
        }

        public async Task<(SurveyCode code, bool success)> SendSurvey(string projectId, string sender, 
            ProjectPatient patient, string strategyId, string surveyCodeId, string notificationId)
        {
            if (!patient.IsActive) return (null, false);
            
            var code = await _codeContext.SurveyCodes.Read(surveyCodeId);
            if (EnvironmentMode.IsTest) return (code, true);

            var comms = await _projectContext.Communicaitons.ReadAll(patient.ParentId);
            var project = await _projectContext.Projects.Read(patient.ParentId);
            _message = project.TextLanguage switch
            {
                Languages.English => new MessageEnglish(),
                Languages.Danish => new MessageDanish(),
                _ => new MessageDanish()
            };
            var surveyMessage = comms
                .FirstOrDefault(t => t.MessageType == ProjectCommunication.CommunicationTypeSurvey);
            var messageText = surveyMessage == null
                ? _message.DefaultSurveyMessage().Replace("@Model.ProjectName", project.Name)
                : surveyMessage.MessageContent;
            var name = patient.Anonymity ? AnonymityMessage.HiddenPatientName : patient.Name;
            if (patient.PhoneNumber != null)
            {
                var url = $"{EnvironmentMode.ClientHost}/s/{surveyCodeId}";
                var body = _message.SmsSurvey(name, messageText, url, project.Name);

                var phoneRes = await _smsHandler.SendSMS(sender, patient.PhoneNumber, body, projectId);
                if (phoneRes.HttpStatusCode == HttpStatusCode.OK)
                {
                    await UpdateNotification(notificationId);
                    return (code, true);
                }
            }

            if (patient.Email == null) return (null, false);
            
            var emailRes = await _emailHandler.SendEmail(sender, patient.Email, _message.SurveyEmailTitle(),
                new SurveyEmail(_message)
                {
                    Title = _message.SurveyEmailLines()[0] + " " + sender,
                    ProjectName = sender,
                    UserName = name,
                    Message = messageText,
                    DownloadUrl = $"{EnvironmentMode.ClientHostForEmail}/s/{code.Id}",
                }, projectId);

            if (emailRes.HttpStatusCode == HttpStatusCode.OK)
            {
                await UpdateNotification(notificationId);
                return (code, true);
            }

            return (null, false);
        }

        private async Task UpdateNotification(string notificationId)
        {
            var curNotifications = (await _notificationService.GetNotifications(NotificationType.Survey))
                .Find(x => x.Id == notificationId);
            if (curNotifications != null)
            {
                curNotifications.SendOutDate = DateTime.Now;
                await _notificationService.UpdateNotification(curNotifications);
            }
        }

        private async Task CreateNotification(string patientId,
            string strategyId, string projectId, SurveyCode code)
        {
            var notificationType = NotificationType.Survey;

            var notification = new Notification()
            {
                Message = $"project {projectId}, to user {patientId}",
                NotificationType = notificationType,
                PatientId = patientId,
                SendOutDate = DateTime.Now,
                StrategyId = strategyId,
                Id = Guid.NewGuid().ToString(),
                SurveyCode = code.Id,
                ProjectId = projectId,
            };
            await _notificationService.SaveNotification(notification);
        }

        public static double CalculateScore(EntryBatch answer)
        {
            if (answer.SurveyId == "who5") return Sum(answer) * 4;
            if (answer.SurveyId == "swemwbs")
            {
                double score = 0;
                double values = 0;
                foreach (var entry in answer.Entries)
                {
                    values += entry.Value;
                }

                switch (values)
                {
                    case 7:
                        score = 7.00;
                        break;
                    case 8:
                        score = 9.51;
                        break;
                    case 9:
                        score = 11.25;
                        break;
                    case 10:
                        score = 12.40;
                        break;
                    case 11:
                        score = 13.33;
                        break;
                    case 12:
                        score = 14.08;
                        break;
                    case 13:
                        score = 14.75;
                        break;
                    case 14:
                        score = 15.32;
                        break;
                    case 15:
                        score = 15.84;
                        break;
                    case 16:
                        score = 16.36;
                        break;
                    case 17:
                        score = 16.88;
                        break;
                    case 18:
                        score = 17.43;
                        break;
                    case 19:
                        score = 17.98;
                        break;
                    case 20:
                        score = 18.59;
                        break;
                    case 21:
                        score = 19.25;
                        break;
                    case 22:
                        score = 19.98;
                        break;
                    case 23:
                        score = 20.73;
                        break;
                    case 24:
                        score = 21.54;
                        break;
                    case 25:
                        score = 22.35;
                        break;
                    case 26:
                        score = 23.21;
                        break;
                    case 27:
                        score = 24.11;
                        break;
                    case 28:
                        score = 25.03;
                        break;
                    case 29:
                        score = 26.02;
                        break;
                    case 30:
                        score = 27.03;
                        break;
                    case 31:
                        score = 28.13;
                        break;
                    case 32:
                        score = 29.31;
                        break;
                    case 33:
                        score = 30.70;
                        break;
                    case 34:
                        score = 32.55;
                        break;
                    case 35:
                        score = 35.00;
                        break;
                    default:
                        throw new Exception("something went wrong values doesnt match possible score");
                }

                return score;
            }

            if (answer.SurveyId == "eq-5d-5l")
            {
                var entry = answer.Entries.ToArray();
                double value = 1 - ((entry[0].Value - 1) - (entry[1].Value - 1) - (entry[2].Value - 1) -
                               (entry[3].Value - 1) - (entry[4].Value - 1));
                return Math.Round(value, 2);
            }

            return Sum(answer);
        }

        private static double Sum(EntryBatch answer)
        {
            return answer.Entries.Sum(entry => entry.Value);
        }


        public static double CalculateAverageScore(EntryBatch answer)
        {
            try
            {
                return answer.Entries.Average(e => e.Value);
            }
            catch (Exception e)
            {
                //Console.WriteLine(e);
                return 0;
            }
            
        }
    }
}