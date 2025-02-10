using Amazon.DynamoDBv2.Model;
using API.Constants;
using API.Mapping;
using API.Models.AnalyticsReport;
using API.Models.AnalyticsReport.SROI;
using API.Models.Projects;
using API.Models.Reports;
using API.Models.Strategy;
using API.Models.Views.AnalyticsReport.SROI;
using API.Models.Views.Report;
using API.Services;
using Mapster;
using MapsterMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace API.Controllers.web.v1
{
    [Authorize(PolicyNames.ProjectAccess)]
    [ApiController]
    [Route("api/web/v1/projects/{projectId}/analytics")]
    public class
        ProjectAnalyticsReportController : BaseController
    {
        private readonly IAnalyticsReportService _analyticsReportService;
        private readonly IProjectService _projectService;
        private readonly IMapper _mapper;

        public ProjectAnalyticsReportController(
            IAnalyticsReportService analyticsReportService, IProjectService projectService, IMapper mapper)
        {
            _analyticsReportService = analyticsReportService;
            _projectService = projectService;
            _mapper = mapper;
        }

        //POSTS
        [HttpPost("v2/create")]
        public async Task<ActionResult<AnalyticsReport>> CreateV2(
            [FromRoute] string projectId,
            [FromBody] SROIReportConfigV2 request)
        {
            request.Id = Guid.NewGuid().ToString();

            var SROIAnalyticsAPIView = new SROIAnalyticsAPIViewModel();
            try
            {
                var mappingConfig = new AnalyticReportAPIMappingConfigV2(request.General.ReportLanguage);
                TypeAdapterConfig.GlobalSettings.Apply(mappingConfig);

                SROIAnalyticsAPIView = request.Adapt<SROIAnalyticsAPIViewModel>();

            }
            catch (Exception e)
            {
                throw new ArgumentException($"Failed to map requested SROIReportConfig to SROIAnalyticsAPIViewModel: {request}", e);
            }
            var SROIAnalysedData = await _analyticsReportService.GenerateSROIData(SROIAnalyticsAPIView);

            var SROIAnalysedDataGeneratorAPIRequestView = _mapper.Map<SROIGeneratorAPIRequestViewModel>(SROIAnalysedData);
            var SROIRequestedGeneratorAPIRequestView = _mapper.Map<SROIGeneratorAPIRequestViewModel>(request);

            SROIAnalysedDataGeneratorAPIRequestView.Id = SROIRequestedGeneratorAPIRequestView.Id;
            SROIAnalysedDataGeneratorAPIRequestView.Language = request.General.ReportLanguage;
            SROIAnalysedDataGeneratorAPIRequestView.SROIPage1.ReportName = SROIRequestedGeneratorAPIRequestView.SROIPage1.ReportName;
            SROIAnalysedDataGeneratorAPIRequestView.Currency = SROIRequestedGeneratorAPIRequestView.Currency;
            SROIAnalysedDataGeneratorAPIRequestView.SROIPage2 = SROIRequestedGeneratorAPIRequestView.SROIPage2;
            SROIAnalysedDataGeneratorAPIRequestView.SROIPage1.ExecutiveSummary = SROIRequestedGeneratorAPIRequestView.SROIPage1.ExecutiveSummary;
            SROIAnalysedDataGeneratorAPIRequestView.SROIPage1.Logo = SROIRequestedGeneratorAPIRequestView.SROIPage1.Logo;
            SROIAnalysedDataGeneratorAPIRequestView.SROIPage4.InputSummary.InvestmentAmount = SROIRequestedGeneratorAPIRequestView.SROIPage4.InputSummary.InvestmentAmount;
            SROIAnalysedDataGeneratorAPIRequestView.SROIPage4.InputSummary.TotalCost = SROIRequestedGeneratorAPIRequestView.SROIPage4.InputSummary.TotalCost;
            SROIAnalysedDataGeneratorAPIRequestView.SROIPage4.InputSummary.FundingSources = SROIRequestedGeneratorAPIRequestView.SROIPage4.InputSummary.FundingSources;

            SROIAnalysedDataGeneratorAPIRequestView.SROIPage5 = new SROIPage5Model(SROIRequestedGeneratorAPIRequestView.SROIPage5.Description);

            var fileName = await _analyticsReportService.GenerateSROIPDF(SROIAnalysedDataGeneratorAPIRequestView, projectId);


            var SROI = new AnalyticsReport()
            {
                Name = request.General.ReportName,
                Id = request.Id,
                ParentId = projectId,
                Type = AnalyticsReportTypeEnum.SROI.ToString().ToUpper(),
                ReportConfig = request
            };


            var savedSROIMeta = await _analyticsReportService.SaveAnalyticsReport(SROI);
            var SROIDownloadURL = _analyticsReportService.GenerateAnalyticsReportDownloadURL(request.Id, AnalyticsReportTypeEnum.SROI);

            savedSROIMeta.DownloadURL = SROIDownloadURL;

            return Ok(savedSROIMeta);
        }


        [HttpPost("{reportId}")]
        public async Task<ActionResult<AnalyticsReport>> Copy(
          [FromRoute] string projectId,
          [FromRoute] string reportId)
        {
            var report = await _analyticsReportService.GetById(reportId, projectId);
            report.Id = Guid.NewGuid().ToString();
            var projectLanguage = await _projectService.GetProjectLanguageById(projectId);

            var SROIAnalyticsAPIView = new SROIAnalyticsAPIViewModel();
            try
            {
                var mappingConfig = new AnalyticReportAPIMappingConfigV2(projectLanguage);
                TypeAdapterConfig.GlobalSettings.Apply(mappingConfig);

                SROIAnalyticsAPIView = report.ReportConfig.Adapt<SROIAnalyticsAPIViewModel>();
            }
            catch (Exception e)
            {
                throw new ArgumentException($"Failed to map requested SROIReportConfig to SROIAnalyticsAPIViewModel: {report.ReportConfig}", e);
            }

            var SROIAnalysedData = await _analyticsReportService.GenerateSROIData(SROIAnalyticsAPIView);

            var SROIAnalysedDataGeneratorAPIRequestView = _mapper.Map<SROIGeneratorAPIRequestViewModel>(SROIAnalysedData);
            var SROIRequestedGeneratorAPIRequestView = _mapper.Map<SROIGeneratorAPIRequestViewModel>(report.ReportConfig);

            SROIAnalysedDataGeneratorAPIRequestView.Id = SROIRequestedGeneratorAPIRequestView.Id;
            SROIAnalysedDataGeneratorAPIRequestView.Language = projectLanguage;
            SROIAnalysedDataGeneratorAPIRequestView.SROIPage1.ReportName = SROIRequestedGeneratorAPIRequestView.SROIPage1.ReportName;
            SROIAnalysedDataGeneratorAPIRequestView.Currency = SROIRequestedGeneratorAPIRequestView.Currency;
            SROIAnalysedDataGeneratorAPIRequestView.SROIPage2 = SROIRequestedGeneratorAPIRequestView.SROIPage2;
            SROIAnalysedDataGeneratorAPIRequestView.SROIPage1.ExecutiveSummary = SROIRequestedGeneratorAPIRequestView.SROIPage1.ExecutiveSummary;
            SROIAnalysedDataGeneratorAPIRequestView.SROIPage1.Logo = SROIRequestedGeneratorAPIRequestView.SROIPage1.Logo;
            SROIAnalysedDataGeneratorAPIRequestView.SROIPage4.InputSummary.InvestmentAmount = SROIRequestedGeneratorAPIRequestView.SROIPage4.InputSummary.InvestmentAmount;
            SROIAnalysedDataGeneratorAPIRequestView.SROIPage4.InputSummary.TotalCost = SROIRequestedGeneratorAPIRequestView.SROIPage4.InputSummary.TotalCost;
            SROIAnalysedDataGeneratorAPIRequestView.SROIPage4.InputSummary.FundingSources = SROIRequestedGeneratorAPIRequestView.SROIPage4.InputSummary.FundingSources;


            var fileName = await _analyticsReportService.GenerateSROIPDF(SROIAnalysedDataGeneratorAPIRequestView, projectId);

            var SROI = new AnalyticsReport()
            {
                Id = report.Id,
                ParentId = report.ParentId,
                Name = $"{report.Name} - Copy", 
                ReportConfig = report.ReportConfig
            };

            var savedSROIMeta = await _analyticsReportService.SaveAnalyticsReport(SROI);
            var SROIDownloadURL = _analyticsReportService.GenerateAnalyticsReportDownloadURL(report.Id, AnalyticsReportTypeEnum.SROI);

            savedSROIMeta.DownloadURL = SROIDownloadURL;

            return Ok(savedSROIMeta);
        }

        //GETS
        [HttpGet]
        public async Task<ActionResult<List<AnalyticsReport>>> ReadAll([FromRoute] string projectId)
        {
            var analyticsReport = await _analyticsReportService.GetAll(projectId);
            return Ok(analyticsReport.ToList());
        }

        [HttpGet("{reportId}")]
        public async Task<ActionResult<AnalyticsReport>> ReadSROI(
            [FromRoute] string reportId, 
            [FromRoute] string projectId)
        {
            var message = GetMessage();

            var SROIreport = await _analyticsReportService.GetById(reportId, projectId);
            if (SROIreport == null) return NotFound(message.ErrorNotFoundReport());

            var SROIDownloadURL = _analyticsReportService.GenerateAnalyticsReportDownloadURL(reportId, AnalyticsReportTypeEnum.SROI);
            SROIreport.DownloadURL = SROIDownloadURL;

            return Ok(SROIreport);
        }


        [HttpGet("{reportId}/config")]
        public async Task<ActionResult<SROIReportConfigV2>> ReadReportConfigByReportId(
            [FromRoute] string reportId,
            [FromRoute] string projectId)
        {
            var message = GetMessage();

            var SROIreport = await _analyticsReportService.GetById(reportId, projectId);
            if (SROIreport == null) return NotFound(message.ErrorNotFoundReport());
       
            return Ok((SROIReportConfigV2)SROIreport.ReportConfig);
        }

        //DELS
        [HttpDelete("{reportId}")]
        public async Task<ActionResult<ProjectPatient>> DeleteSROI(
            [FromRoute] string reportId, 
            [FromRoute] string projectId)
        {
            await _analyticsReportService.DeleteById(projectId, reportId, AnalyticsReportTypeEnum.SROI);
            return Ok(reportId);
        }

        //PUTS
        [HttpPut("v2/edit/{reportId}")]
        public async Task<ActionResult<BatchSendoutFrequency>> UpdateSROI(
            [FromRoute] string projectId,
            [FromBody] SROIReportConfigV2 request)
        {
            var oldReportId = request.Id;
            request.Id = Guid.NewGuid().ToString();

            var SROIAnalyticsAPIView = new SROIAnalyticsAPIViewModel();
            try
            {
                var mappingConfig = new AnalyticReportAPIMappingConfigV2(request.General.ReportLanguage);
                TypeAdapterConfig.GlobalSettings.Apply(mappingConfig);

                SROIAnalyticsAPIView = request.Adapt<SROIAnalyticsAPIViewModel>();

            }
            catch (Exception e)
            {
                throw new ArgumentException($"Failed to map requested SROIReportConfig to SROIAnalyticsAPIViewModel: {request}", e);
            }


            var SROIAnalysedData = await _analyticsReportService.GenerateSROIData(SROIAnalyticsAPIView);

            var SROIAnalysedDataGeneratorAPIRequestView = _mapper.Map<SROIGeneratorAPIRequestViewModel>(SROIAnalysedData);
            var SROIRequestedGeneratorAPIRequestView = _mapper.Map<SROIGeneratorAPIRequestViewModel>(request);

            SROIAnalysedDataGeneratorAPIRequestView.Id = SROIRequestedGeneratorAPIRequestView.Id;
            SROIAnalysedDataGeneratorAPIRequestView.Language = request.General.ReportLanguage;
            SROIAnalysedDataGeneratorAPIRequestView.SROIPage1.ReportName = SROIRequestedGeneratorAPIRequestView.SROIPage1.ReportName;
            SROIAnalysedDataGeneratorAPIRequestView.Currency = SROIRequestedGeneratorAPIRequestView.Currency;
            SROIAnalysedDataGeneratorAPIRequestView.SROIPage2 = SROIRequestedGeneratorAPIRequestView.SROIPage2;
            SROIAnalysedDataGeneratorAPIRequestView.SROIPage1.ExecutiveSummary = SROIRequestedGeneratorAPIRequestView.SROIPage1.ExecutiveSummary;
            SROIAnalysedDataGeneratorAPIRequestView.SROIPage1.Logo = SROIRequestedGeneratorAPIRequestView.SROIPage1.Logo;
            SROIAnalysedDataGeneratorAPIRequestView.SROIPage4.InputSummary.InvestmentAmount = SROIRequestedGeneratorAPIRequestView.SROIPage4.InputSummary.InvestmentAmount;
            SROIAnalysedDataGeneratorAPIRequestView.SROIPage4.InputSummary.TotalCost = SROIRequestedGeneratorAPIRequestView.SROIPage4.InputSummary.TotalCost;
            SROIAnalysedDataGeneratorAPIRequestView.SROIPage4.InputSummary.FundingSources = SROIRequestedGeneratorAPIRequestView.SROIPage4.InputSummary.FundingSources;

            SROIAnalysedDataGeneratorAPIRequestView.SROIPage5 = new SROIPage5Model(SROIRequestedGeneratorAPIRequestView.SROIPage5.Description);

            var fileName = await _analyticsReportService.GenerateSROIPDF(SROIAnalysedDataGeneratorAPIRequestView, projectId);


            var SROI = new AnalyticsReport()
            {
                Name = request.General.ReportName,
                Id = request.Id,
                ParentId = projectId,
                Type = AnalyticsReportTypeEnum.SROI.ToString().ToUpper(),
                ReportConfig = request
            };

            await _analyticsReportService.DeleteById(projectId, oldReportId, AnalyticsReportTypeEnum.SROI);
            var savedSROIMeta = await _analyticsReportService.SaveAnalyticsReport(SROI);
            var SROIDownloadURL = _analyticsReportService.GenerateAnalyticsReportDownloadURL(request.Id, AnalyticsReportTypeEnum.SROI);

            savedSROIMeta.DownloadURL = SROIDownloadURL;

            return Ok(savedSROIMeta);
        }
    }
}