using System;
using System.Linq;
using System.Threading.Tasks;
using API.Models;
using API.Models.Analytics;
using API.Models.Codes;
using API.Models.Notifications;
using API.Models.Strategy;
using API.Repositories;
using API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers.web.v1
{
    /// <summary>
    /// Represents a controller for managing surveys.
    /// </summary>
    [AllowAnonymous]
    [ApiController]
    [Route("api/web/v1/surveys")]
    public class 
        SurveyController : BaseController
    {
        private readonly IStrategyContext _strategyContext;
        private readonly IPatientContext _patientContext;
        private readonly ICodeContext _codeContext;
        private readonly IAnalyticsContext _analytics;
        private readonly NotificationService _notificationService;

        /// <summary>
        /// Initializes a new instance of the <see cref="SurveyController"/> class.
        /// </summary>
        /// <param name="strategyContext">The strategy context.</param>
        /// <param name="patientContext">The patient context.</param>
        /// <param name="codeContext">The code context.</param>
        /// <param name="analytics">The analytics context.</param>
        /// <param name="notificationService">The notification service.</param>
        public SurveyController(IStrategyContext strategyContext, IPatientContext patientContext,
            ICodeContext codeContext, IAnalyticsContext analytics, 
            NotificationService notificationService)
        {
            _strategyContext = strategyContext;
            _patientContext = patientContext;
            _codeContext = codeContext;
            _analytics = analytics;
            _notificationService = notificationService;
        }

        /// <summary>
        /// Submits answers for a survey.
        /// </summary>
        /// <param name="request">The batch of survey answers.</param>
        /// <returns>The result of the answer submission.</returns>
        [HttpPost("answer")]
        public async Task<ActionResult<EntryBatch>> Answer([FromBody] EntryBatch request)
        {
            var message = GetMessage();
            var patient = await _patientContext.ReadPatient(request.ProjectId, request.PatientId);
            if (patient == null) return ErrorResponse.NotFound(message.ErrorNotFoundCitizen());
            request.Tags = patient.Tags.Select(t => t.Name).ToList();

            request.StrategyId = patient.StrategyId;
            request.Entries = request.Entries.Select(e =>
            {
                e.Tags = request.Tags;
                e.AnsweredAt = request.AnsweredAt;
                e.ProjectId = request.ProjectId;
                e.PatientId = request.PatientId;
                e.StrategyId = patient.StrategyId;
                e.SurveyId = request.SurveyId;
                return e;
            });

            var response = await _analytics.CreateEntries(request);

            // if (EnvironmentMode.IsDevelopment || EnvironmentMode.IsTest)
            // {
            //     await _elasticClient.IndexDocumentAsync(response);
            //     foreach (var entry in request.Entries)
            //     {
            //         await _elasticClient.IndexDocumentAsync(entry);
            //     }
            // }

            var notifications = await _notificationService.GetNotifications(NotificationType.Survey);
            foreach (var notification in notifications.Where(notification => notification.SurveyCode == request.CodeId &&
                                                                             notification.StrategyId == patient.StrategyId &&
                                                                             notification.PatientId == request.PatientId))
            {
                notification.AnsweredAt = DateTime.Now;
                await _notificationService.UpdateNotification(notification);
            }

            patient.LastAnswered = response.CreatedAt;
            patient.SubmissionDate = response.AnsweredAt;

            await _patientContext.ProjectPatients.Update(request.ProjectId, patient);
            return Ok(response);
        }

        /// <summary>
        /// Gets a survey code and its associated surveys.
        /// </summary>
        /// <param name="codeId">The ID of the survey code.</param>
        /// <returns>The survey code and its associated surveys.</returns>
        [HttpGet("code/{codeId}")]
        public async Task<ActionResult<SurveyCode>> GetSurveyCode([FromRoute] string codeId)
        {
            var message = GetMessage();
            var surveyCode = await _codeContext.SurveyCodes.Read(codeId);
            if (surveyCode == null) return NotFound(message.ErrorNotFoundSurveyCode());

            var surveys = (await _strategyContext.ReadSurveysFromProperty(surveyCode.Properties))
                .Where(CheckLanguageIfValidated).ToList();
            surveys = surveys.FindAll((s) => s != null);
            if (!surveys.Any()) return NotFound(message.ErrorNotFoundSurvey());
            surveyCode.Surveys = surveys.OrderBy(s=>s.Index).ToList();
            return Ok(surveyCode);
        }

        //please rename me!
        [HttpGet]
        public async Task<ActionResult<Survey[]>> GetTemplateSurveys()
        {
            var surveys = (await _strategyContext.ReadTemplateSurveys()).Where(s=>CheckLanguage(s.TextLanguage));
            return Ok(surveys.ToArray());
        }

        private bool CheckLanguageIfValidated(Survey s)
        {
            return s.ParentId is not "TEMPLATE" || CheckLanguage(s.TextLanguage);
        }


    }
}