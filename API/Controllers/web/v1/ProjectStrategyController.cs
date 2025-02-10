using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using API.Constants;
using API.Handlers;
using API.Models;
using API.Models.Analytics;
using API.Models.Projects;
using API.Models.Strategy;
using API.Models.Views.Strategy;
using API.Services;
using MapsterMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers.web.v1
{
    [Authorize(PolicyNames.ProjectAccess)]
    [ApiController]
    [Route("api/web/v1/projects/{projectId}/strategies")]
    public class ProjectStrategyController : BaseController
    {
        private readonly IStrategyService _strategyService;
        private readonly IAnalyticsService _analyticsService;
        private readonly IPatientService _patientService;
        private readonly IAnonymityHandler _anonymityHandler;
        private readonly ISendoutService _sendoutService;
        private readonly IMapper _mapper;

        public ProjectStrategyController(
            IStrategyService strategyService,
            IPatientService patientService,
            IAnonymityHandler anonymityHandler,
            IAnalyticsService analyticsService,
            ISendoutService sendoutService,
            IMapper mapper)
        {
            _strategyService = strategyService;
            _analyticsService = analyticsService;
            _patientService = patientService;
            _anonymityHandler = anonymityHandler;
            _sendoutService = sendoutService;
            _mapper = mapper;
        }

        //POSTS
        [HttpPost, Authorize(Permissions.Strategy.Write)]
        public async Task<ActionResult<Strategy>> Create(
        [FromRoute] string projectId,
        [FromBody] StrategyViewModel strategyView)
        {
            var strategy = new Strategy();
            try
            {
                strategy = _mapper.Map<Strategy>(strategyView);
            }
            catch (Exception e)
            {
                throw new ArgumentException($"Failed to map strategyView: {strategyView}", e);
            }

            var result = await _strategyService.CreateStrategy(projectId, strategy);
            return Ok(result);
        }

        [HttpPost("{strategyId}/batchsendouts/create/{jobType}"), Authorize(Permissions.Strategy.Write)]
        public async Task<ActionResult<BatchSendoutFrequency>> CreateSendouts(
        [FromRoute] string projectId,
        [FromBody] BatchSendoutFrequencyViewModel batchSendoutFrequencyView,
        [FromRoute] string jobType)
        {
            var batchSendoutFrequency = new BatchSendoutFrequency();
            try
            {
                batchSendoutFrequency = _mapper.Map<BatchSendoutFrequency>(batchSendoutFrequencyView);
            }
            catch (Exception e)
            {
                throw new ArgumentException($"Failed to map batchSendoutFrequencyView: {batchSendoutFrequencyView}", e);
            }

            var result = await _sendoutService.CreateSendouts(projectId, jobType, batchSendoutFrequency);
            return Ok(result);
        }

        [HttpPost("{strategyId}/effects/create"), Authorize(Permissions.Strategy.Write)]
        public async Task<ActionResult<List<StrategyEffect>>> CreateEffects(
        [FromRoute] string strategyId,
        [FromBody] List<StrategyEffectViewModel> StrategyEffectsView)
        {
            var strategyEffects = new List<StrategyEffect>();
            try
            {
                strategyEffects = _mapper.Map<List<StrategyEffectViewModel>, List<StrategyEffect>>(StrategyEffectsView);
            }
            catch (Exception e)
            {
                throw new ArgumentException($"Failed to map StrategyEffectsView: {StrategyEffectsView}", e);
            }

            var result = await _strategyService.CreateEffects(strategyId, strategyEffects);
            return Ok(result);
        }

        [HttpPost("{strategyId}/registrations/create"), Authorize(Permissions.Strategy.Write)]
        public async Task<ActionResult<List<Registration>>> CreateRegisterations(
        [FromRoute] string strategyId,
        [FromBody] List<RegistrationViewModel> registrationsView)
        {
            var batchRegistrations = new List<Registration>();
            try
            {
                batchRegistrations = _mapper.Map<List<RegistrationViewModel>, List<Registration>>(registrationsView);
            }
            catch (Exception e)
            {
                throw new ArgumentException($"Failed to map registrationsView: {registrationsView}", e);
            }

            var result = await _analyticsService.CreateRegisterations(strategyId, batchRegistrations);
            return Ok(result);
        }

        [HttpPost("{strategyId}/surveys/create"), Authorize(Permissions.Strategy.Write)]
        public async Task<ActionResult<List<SurveyProperty>>> CreateStrategySurveys(
        [FromRoute] string projectId,
        [FromRoute] string strategyId,
        [FromBody] List<SurveyPropertyViewModel> SurveyPropertiesView)
        {
            var surveys = new List<SurveyProperty>();
            try
            {
                surveys = _mapper.Map<List<SurveyPropertyViewModel>, List<SurveyProperty>>(SurveyPropertiesView);
            }
            catch (Exception e)
            {
                throw new ArgumentException($"Failed to map SurveyPropertiesView: {SurveyPropertiesView}", e);
            }

            var result = await _strategyService.CreateStrategySurveys(projectId, strategyId, surveys);
            return Ok(result);
        }

        //GETS
        [HttpGet, Authorize(Permissions.Strategy.Read)]
        public async Task<ActionResult<List<Strategy>>> ReadAll(
        [FromRoute] string projectId)
        {
            var result = await _strategyService.GetStrategies(projectId);
            return Ok(result ?? new List<Strategy>());
        }

        [HttpGet("{strategyId}"), Authorize(Permissions.Strategy.Read)]
        public async Task<ActionResult<Strategy>> GetStrategy(
        [FromRoute] string projectId,
        [FromRoute] string strategyId)
        {
            var message = GetMessage();
            var result = await _strategyService.GetStrategy(projectId, strategyId);
            if (result == null) return ErrorResponse.NotFound(message.ErrorNotFoundStrategy());
            foreach (var patient in result.Patients)
            {
                var roleId = CurrentRoleId();
                if (roleId != RoleSeed.AdministratorRoleId && patient.Anonymity)
                {
                    patient.Name = AnonymityMessage.HiddingMessage;
                }
            }
            return Ok(result);
        }

        [HttpGet("{strategyId}/patients"), Authorize(Permissions.Strategy.Read)]
        public async Task<ActionResult<IEnumerable<ProjectPatient>>> GetStrategyPatients(
        [FromRoute] string strategyId)
        {
            var result = await _patientService.GetStrategyPatients(strategyId);
            var roleId = CurrentRoleId();
            if (roleId != RoleSeed.AdministratorRoleId)
            {
                return Ok(_anonymityHandler.HidePatients(result));
            }
            return Ok(result);
        }

        [HttpGet("{effectId}/registrations"), Authorize(Permissions.Strategy.Read)]
        public async Task<IEnumerable<Registration>> GetStrategyRegistrations(
        [FromRoute] string projectId,
        [FromRoute] string effectId)
        {
            return await _analyticsService.GetRegistrationsByEffectId(projectId, effectId);
        }

        [HttpGet("{strategyId}/surveys"), Authorize(Permissions.Strategy.Read)]
        public async Task<IEnumerable<Survey>> GetStrategySurveys(
        [FromRoute] string projectId,
        [FromRoute] string strategyId)
        {
            return await _strategyService.GetStrategySurveys(projectId, strategyId);
        }

        [HttpGet("{strategyId}/frequency/{frequencyId}"), Authorize(Permissions.Strategy.Read)]
        public async Task<ActionResult<BatchSendoutFrequency>> GetStrategyFrequency(
        [FromRoute] string strategyId,
        [FromRoute] string frequencyId)
        {
            return await _strategyService.GetStrategyFrequency(strategyId, frequencyId);
        }

        [HttpGet("{strategyId}/registrations/{category}")]
        public async Task<ActionResult<List<RegistrationStatus>>> GetStrategyRegistered(
        [FromRoute] string projectId,
        [FromRoute] string category,
        [FromRoute] string strategyId
        )
        {
            var result = await _strategyService.GetRegisteredStatus(projectId, category, strategyId);
            return Ok(result);
        }

        //PUTS
        [HttpPut("{strategyId}/batchsendouts"), Authorize(Permissions.Strategy.Write)]
        public async Task<ActionResult<BatchSendoutFrequency>> UpdateFrequentBatchSendout(
        [FromRoute] string projectId,
        [FromBody] BatchSendoutFrequencyViewModel batchSendoutFrequencyView)
        {
            var batchSendoutFrequency = new BatchSendoutFrequency();
            try
            {
                batchSendoutFrequency = _mapper.Map<BatchSendoutFrequency>(batchSendoutFrequencyView);
            }
            catch (Exception e)
            {
                throw new ArgumentException($"Failed to map batchSendoutFrequencyView: {batchSendoutFrequencyView}", e);
            }

            var result = await _strategyService.UpdateBatchSendout(projectId, batchSendoutFrequency);
            return Ok(result);
        }

        [HttpPut("{strategyId}/effects"), Authorize(Permissions.Strategy.Write)]
        public async Task<ActionResult<List<StrategyEffect>>> UpdateEffects(
        [FromRoute] string projectId,
        [FromRoute] string strategyId,
        [FromBody] List<StrategyEffectViewModel> StrategyEffectsView)
        {
            var strategyEffects = new List<StrategyEffect>();
            try
            {
                strategyEffects = _mapper.Map<List<StrategyEffectViewModel>, List<StrategyEffect>>(StrategyEffectsView);
            }
            catch (Exception e)
            {
                throw new ArgumentException($"Failed to map StrategyEffectsView: {StrategyEffectsView}", e);
            }

            var result = await _strategyService.UpdateEffects(projectId, strategyId, strategyEffects);
            return Ok(result);
        }

        [HttpPut("{strategyId}"), Authorize(Permissions.Strategy.Write)]
        public async Task<ActionResult<Strategy>> UpdateStrategy(
        [FromRoute] string projectId,
        [FromRoute] string strategyId,
        [FromBody] StrategyViewModel strategyView)
        {
            var strategy = new Strategy();
            try
            {
                strategy = _mapper.Map<Strategy>(strategyView);
            }
            catch (Exception e)
            {
                throw new ArgumentException($"Failed to map strategyView: {strategyView}", e);
            }
            var result = await _strategyService.UpdateStrategy(projectId, strategyId, strategy);
            return Ok(result);
        }

        //DELETES
        [HttpDelete("{strategyId}/batchsendouts/delete/{frequencyId}"), Authorize(Permissions.Strategy.Write)]
        public async Task<ActionResult<BatchSendoutFrequency>> DeleteBatchSendout(
        [FromRoute] string strategyId,
        [FromRoute] string frequencyId)
        {
            var result = await _strategyService.DeleteBatchSendout(strategyId, frequencyId);
            return Ok(result);
        }

        [HttpDelete("{strategyId}/effects/delete"), Authorize(Permissions.Strategy.Write)]
        public async Task<ActionResult<List<Registration>>> DeleteEffects(
        [FromRoute] string projectId,
        [FromRoute] string strategyId,
        [FromBody] List<string> effectsId)
        {
            var result = await _strategyService.DeleteEffects(projectId, strategyId, effectsId);
            return Ok(result);
        }

        [HttpDelete("{strategyId}"), Authorize(Permissions.Strategy.Write)]
        public async Task<ActionResult> DeleteStrategy(
        [FromRoute] string projectId,
        [FromRoute] string strategyId)
        {
            var result = await _strategyService.DeleteStrategy(projectId, strategyId);
            return Ok(result);
        }
    }
}