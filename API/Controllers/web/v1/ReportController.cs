using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using API.Constants;
using API.Handlers;
using API.Models;
using API.Models.Analytics;
using API.Models.Dump;
using API.Models.Reports;
using API.Models.Views.Report;
using API.Repositories;
using API.Services;
using MapsterMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

namespace API.Controllers.web.v1
{
    /// <summary>
    /// TODO This controller is not secure. Please update with token schema for request of data
    /// </summary>

    [ApiController]
    [Route("api/web/v1/reports")]
    public class ReportController : BaseController
    {
        private readonly IProjectContext _projectContext;
        private readonly ICodeContext _codeContext;
        private readonly IAnalyticsService _analyticsService;
        private readonly IReportService _reportService;
        private readonly ITimeMachineHandler _timeMachineHandler;
        private readonly IMapper _mapper;

        public ReportController(
            IMapper mapper,
            IProjectContext projectContext, 
            ICodeContext codeContext, 
            IAnalyticsService analyticsService, 
            IReportService reportService, 
            ITimeMachineHandler timeMachineHandler)
        {
            _projectContext = projectContext;
            _codeContext = codeContext;
            _analyticsService = analyticsService;
            _reportService = reportService;
            _timeMachineHandler = timeMachineHandler;
            _mapper = mapper;
        }

        //GETS
        [AllowAnonymous, HttpGet("code/{codeId}")]
        public async Task<ActionResult<Report>> ReadReportFromCode([FromRoute] string codeId)
        {
            var message = GetMessage();
            var code = await _codeContext.ReportCodes.Read(codeId);
            if (code == null) return NotFound(message.ErrorNotFoundReportCode());

            var report = await _projectContext.Reports.Read(code.ProjectId, code.ReportId);
            if (report == null) return NotFound(message.ErrorNotFoundReport());

            return Ok(report);
        }

        //POSTS
        [HttpPost("modules/registrations/incidents"), Authorize]
        public async Task<ActionResult<List<ProjectReportResponse>>> GetEventRegs(
            [FromBody] IncidentReportModuleConfigViewModel incidentReportModuleConfigView
        )
        {
            var reportModuleConfig = new ReportModuleConfig();
            try
            {
                reportModuleConfig =  _mapper.Map<ReportModuleConfig>(incidentReportModuleConfigView);
            }
            catch (Exception e)
            {
                throw new ArgumentException($"Failed to map incidentReportModuleConfigView: {incidentReportModuleConfigView}", e);
            }

            var message = GetMessage();
            var setRequestedCulture = await _timeMachineHandler.SetCultureInfoByProjectId(Request, reportModuleConfig.ProjectId);
            if (setRequestedCulture.IsNullOrEmpty()) return ErrorResponse.Forbidden(message.ErrorNotFoundUser());
            var chartData = await _reportService.GetIncidentRegChartData(reportModuleConfig.EffectId, reportModuleConfig.ProjectId, reportModuleConfig.StrategyId, reportModuleConfig.Tags, reportModuleConfig.TimeUnit, reportModuleConfig.TimePreset, reportModuleConfig.Start, reportModuleConfig.End, message, reportModuleConfig.isEmpty);
            var reportIncidentsResponse = new ProjectReportResponse
            {
                PopulationSize = chartData.PopulationSizes,
                ChartDatas = chartData.ChartValues,
                SampleSizes = chartData.SampleSizes
            };

            return Ok(reportIncidentsResponse);
        }

        [HttpPost("modules/registrations/numeric"), Authorize]
        public async Task<ActionResult<List<ProjectReportResponse>>> GetNumericalRegs(
            [FromBody] NumericReportModuleConfigViewModel numericReportModuleConfigView
        )
        {
            var reportModuleConfig = new ReportModuleConfig();
            try
            {
                reportModuleConfig = _mapper.Map<ReportModuleConfig>(numericReportModuleConfigView);
            }
            catch (Exception e)
            {
                throw new ArgumentException($"Failed to map numericReportModuleConfigView: {numericReportModuleConfigView}", e);
            }

            var message = GetMessage();
            var setRequestedCulture = await _timeMachineHandler.SetCultureInfoByProjectId(Request, reportModuleConfig.ProjectId);
            if (setRequestedCulture.IsNullOrEmpty()) return ErrorResponse.Forbidden(message.ErrorNotFoundUser());
            var chartData = await _reportService.GetNumericRegChartData(reportModuleConfig.EffectId, reportModuleConfig.ProjectId, reportModuleConfig.StrategyId, reportModuleConfig.Tags, reportModuleConfig.TimeUnit, reportModuleConfig.TimePreset, reportModuleConfig.Start, reportModuleConfig.End, message, reportModuleConfig.isEmpty);

            var reportNumericResponse = new ProjectReportResponse
            {
                PopulationSize = chartData.PopulationSizes,
                ChartDatas = chartData.ChartValues,
                SampleSizes = chartData.SampleSizes
            };

            return Ok(reportNumericResponse);
        }

        [HttpPost("modules/registrations/status"), Authorize]
        public async Task<ActionResult<ProjectReportResponse>> GetStatusRegs(
        [FromBody] StatusReportModuleConfigViewModel statusReportModuleConfigView
        )
        {
            var reportModuleConfig = new ReportModuleConfig();
            try
            {
                reportModuleConfig = _mapper.Map<ReportModuleConfig>(statusReportModuleConfigView);
            }
            catch (Exception e)
            {
                throw new ArgumentException($"Failed to map statusReportModuleConfigView: {statusReportModuleConfigView}", e);
            }

            var chartData = await _reportService.GetStatusRegChartData(reportModuleConfig.ProjectId, reportModuleConfig.StrategyId, reportModuleConfig.Category, reportModuleConfig.endDates, reportModuleConfig.pointSystemType, reportModuleConfig.Tags);

            var reportCustomSurveyResponse = new ProjectReportResponse
            {
                PopulationSize = chartData.PopulationSizes,
                ChartDatas = chartData.ChartValues,
                SampleSizes = chartData.SampleSizes
            };

            return Ok(reportCustomSurveyResponse);
        }

        [HttpPost("modules/surveys/validated"), Authorize]
        public async Task<ActionResult<ProjectReportResponse>> GetValidated(
            [FromBody] ValidatedReportModuleConfigViewModel validatedReportModuleConfigView
        )
        {
            var reportModuleConfig = new ReportModuleConfig();
            try
            {
                reportModuleConfig = _mapper.Map<ReportModuleConfig>(validatedReportModuleConfigView);
            }
            catch (Exception e)
            {
                throw new ArgumentException($"Failed to map validatedReportModuleConfigView: {validatedReportModuleConfigView}", e);
            }

            if (reportModuleConfig.IsExcludeOnlyOneAnswer)
            {
                if (reportModuleConfig.Filters.IsNullOrEmpty())
                {
                    reportModuleConfig.Filters = new List<string>();
                }
                reportModuleConfig.Filters.Add(ReportModuleConfig.FilterExcludeOnlyOneAnswer);
            }

            var population = await _analyticsService.GetPopulationByStrategyIdAndTags(reportModuleConfig.StrategyId,
                reportModuleConfig.Tags, reportModuleConfig.Filters);
            var projectPatients = population.ToList();
            var chartData = await _reportService.GetValidatedSurveyChartData(reportModuleConfig, projectPatients);
            
            var reportCustomSurveyResponse = new ProjectReportResponse
            {
                PopulationSize = chartData.PopulationSizes,
                ChartDatas = chartData.ChartValues,
                SampleSizes = chartData.SampleSizes,
            };

            return Ok(reportCustomSurveyResponse);
        }

        [HttpPost("modules/surveys/custom"), Authorize]
        public async Task<ActionResult<ProjectReportResponse>> GetCustom(
            [FromBody] CustomReportModuleConfigViewModel customReportModuleConfigView
        )
        {
            var reportModuleConfig = new ReportModuleConfig();
            try
            {
                reportModuleConfig = _mapper.Map<ReportModuleConfig>(customReportModuleConfigView);
            }
            catch (Exception e)
            {
                throw new ArgumentException($"Failed to map customReportModuleConfigView: {customReportModuleConfigView}", e);
            }

            if (reportModuleConfig.IsExcludeOnlyOneAnswer)
            {
                if (reportModuleConfig.Filters.IsNullOrEmpty())
                {
                    reportModuleConfig.Filters = new List<string>();
                }
                reportModuleConfig.Filters.Add(ReportModuleConfig.FilterExcludeOnlyOneAnswer);
            }

            var population = await _analyticsService.GetPopulationByStrategyIdAndTags(reportModuleConfig.StrategyId,
                reportModuleConfig.Tags, reportModuleConfig.Filters);
            var projectPatients = population.ToList();

            
            var chartData = await _reportService.GetCustomSurveyChartData(reportModuleConfig, projectPatients);
            
            var reportCustomSurveyResponse = new ProjectReportResponse
            {
                PopulationSize = chartData.PopulationSizes,
                ChartDatas = chartData.ChartValues,
                SampleSizes = chartData.SampleSizes,
            };

            return Ok(reportCustomSurveyResponse);
        }


        [HttpPost("modules/surveys/multiple"), Authorize]
        public async Task<ActionResult<ProjectReportResponse>> GetCustomMultipleQuestions(
            [FromBody] CustomReportModuleConfigViewModel customReportModuleConfigViewModel)
        {
            var reportModuleConfig = new ReportModuleConfig();
            try
            {
                reportModuleConfig = _mapper.Map<ReportModuleConfig>(customReportModuleConfigViewModel);
            }
            catch (Exception e)
            {
                throw new ArgumentException($"Failed to map customReportModuleConfigView: {customReportModuleConfigViewModel}", e);
            }

            if (reportModuleConfig.IsExcludeOnlyOneAnswer)
            {
                if (reportModuleConfig.Filters.IsNullOrEmpty())
                {
                    reportModuleConfig.Filters = new List<string>();
                }
                reportModuleConfig.Filters.Add(ReportModuleConfig.FilterExcludeOnlyOneAnswer);
            }

            var population = await _analyticsService.GetPopulationByStrategyIdAndTags(reportModuleConfig.StrategyId,
                reportModuleConfig.Tags, reportModuleConfig.Filters);
            var projectPatients = population.ToList();
            var chartData = await _reportService.GetSurveyChartMultipleQuestions(reportModuleConfig, projectPatients);

            var reportCustomSurveyResponse = new ProjectReportResponse
            {
                PopulationSize = chartData.PopulationSizes,
                ChartDatas = chartData.ChartValues,
                SampleSizes = chartData.SampleSizes,
            };

            return Ok(reportCustomSurveyResponse);
        }


        [HttpPost("modules/surveys/multiple/stats"), Authorize]
        public async Task<ActionResult<IEnumerable<ProjectReportStats>>> GetCustomMultipleStats(
            [FromBody] CustomReportModuleConfigViewModel customReportModuleConfigViewModel)
        {
            var reportModuleConfig = new ReportModuleConfig();
            try
            {
                reportModuleConfig = _mapper.Map<ReportModuleConfig>(customReportModuleConfigViewModel);
            }
            catch (Exception e)
            {
                throw new ArgumentException($"Failed to map customReportModuleConfigView: {customReportModuleConfigViewModel}", e);
            }
            var population = await _analyticsService.GetPopulationByStrategyIdAndTags(reportModuleConfig.StrategyId,
                reportModuleConfig.Tags, reportModuleConfig.Filters);
            var projectPatients = population.ToList();

            var rs = await _reportService.GetSurveyMultipleStats(reportModuleConfig, projectPatients);
            return Ok(rs);
        }
        
    }
    

}