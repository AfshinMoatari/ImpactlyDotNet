using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using API.Constants;
using API.Helpers;
using API.Models.AnalyticsReport;
using API.Models.AnalyticsReport.SROI;
using API.Models.Reports;
using API.Models.Views.AnalyticsReport.SROI;
using API.Models.Views.Report;
using API.Repositories;
using API.Services.External;
using Nest;

namespace API.Services
{
    public interface IAnalyticsReportService
    {
        /// <summary>
        /// Get all the SROI reports for a projectId
        /// </summary>
        /// <returns>List of SROI reports.</returns>
        public Task<List<AnalyticsReport>> GetAll(string projectId);
        /// <summary>
        /// Get the SROI report from their Id
        /// </summary>
        /// <returns>SROI report object.</returns>
        public Task<AnalyticsReport> GetById(string analyticsReportId, string projectId);
        /// <summary>
        /// Call external analytics service to analyse the requested data
        /// </summary>
        /// <returns>An object with related key value paires conating figures for generator.</returns>
        public Task<SROIAnalyticsResponseViewModel> GenerateSROIData(SROIAnalyticsAPIViewModel requestDate);
        /// <summary>
        /// Call external generator service to generate a PDF report and store it on S3
        /// </summary>
        /// <returns>Return the PDF file name.</returns>
        public Task<string> GenerateSROIPDF(SROIGeneratorAPIRequestViewModel requestDate, string projectId);
        /// <summary>
        /// Save and SROI object in the analytics table.
        /// </summary>
        /// <returns>SROI report.</returns>
        public Task<AnalyticsReport> SaveAnalyticsReport(AnalyticsReport analyticsReport);
        /// <summary>
        /// Update and SROI object in the analytics table.
        /// </summary>
        /// <returns>SROI report.</returns>
        public Task<AnalyticsReport> UpdateAnalyticsReport(string reportId, SROIReportConfigV2 request);
        /// <summary>
        /// Generate SROI Download URL.
        /// </summary>
        /// <returns>SROI Download URL.</returns>
        public string GenerateAnalyticsReportDownloadURL(string reportId, AnalyticsReportTypeEnum type);
        /// <summary>
        /// Find & delete the SROI by it's Id.
        /// </summary>
        /// <returns>True if the SROI is deleted.</returns>
        public Task DeleteById(string projectId, string reportId, AnalyticsReportTypeEnum type);
    }

    /// <summary>
    /// Service class for managing SROI reports.
    /// </summary>
    public class AnalyticsReportService : IAnalyticsReportService
    {
        private readonly IAnalyticsAPIService _analyticsAPIService;
        private readonly IGeneratorAPIService _generatorAPIService;
        private readonly IAnalyticsReportContext _analyticsReportContext;
        private readonly IProjectService _projectService;
        private readonly IS3Helper _s3helper;

        /// <summary>
        /// Initializes a new instance of the <see cref="AnalyticsAPIService"/> class.
        /// </summary>
        /// <param name="analyticsAPIService">The _analyticsAPIService.</param>
        /// <param name="generatorAPIService">The _generatorAPIService.</param>
        /// <param name="analyticsContext">The _analyticsService.</param>
        public AnalyticsReportService( IAnalyticsAPIService analyticsAPIService, IGeneratorAPIService generatorAPIService, IAnalyticsReportContext analyticsReportContext, IProjectService projectService, IS3Helper s3Helper)
        {
            _analyticsAPIService = analyticsAPIService;
            _generatorAPIService = generatorAPIService;
            _analyticsReportContext = analyticsReportContext;
            _projectService = projectService;
            _s3helper = s3Helper;
        }

        public async Task<List<AnalyticsReport>> GetAll(string projectId)
        {
            try
            {
                return await _analyticsReportContext.ReadAllByProjectId(projectId);
            }
            catch (ArgumentException e)
            {
                throw new ArgumentException($"Something went wrong with fetching the SROIs for projectId: {projectId}", e);
            }
        }

        public async Task<AnalyticsReport> GetById(string analyticsReportId, string projectId)
        {
            try
            {
                return await _analyticsReportContext.ReadByPKProjectIdAndAnalyticsReportId(projectId, analyticsReportId);
            }
            catch (ArgumentException e)
            {
                throw new ArgumentException($"Something went wrong with fetching the analyticsReport by id: {analyticsReportId}", e);
            }
        }

        public async Task<SROIAnalyticsResponseViewModel> GenerateSROIData(SROIAnalyticsAPIViewModel requestDate)
        {
            try
            {
                return await _analyticsAPIService.GenerateSROIData(requestDate);
            }
            catch (ArgumentException e)
            {
                throw new ArgumentException($"Something went wrong with the getting the data from external analytics API service for the following analyticsReport object {requestDate}", e);
            }
        }

        public async Task<string> GenerateSROIPDF(SROIGeneratorAPIRequestViewModel requestDate, string projectId)
        {
            try
            {
                var env = (EnvironmentMode.IsStaging || EnvironmentMode.IsDevelopment) ? "staging" : "production";
                var lang = await _projectService.GetProjectLanguageById(projectId);

                requestDate.Language = lang;

                var res = await _generatorAPIService.GenerateSROIPDF(requestDate, env);

                return res;
            }
            catch (ArgumentException e)
            {
                throw new ArgumentException($"Something went wrong with generating the pdf with external generator API service for the following object {requestDate}", e);
            }
        }

        public async Task<AnalyticsReport> SaveAnalyticsReport(AnalyticsReport analyticsReport)
        {
            try
            {
                return await _analyticsReportContext.WriteAnalyticsReport(analyticsReport);
            }
            catch (ArgumentException e)
            {
                throw new ArgumentException($"Something went wrong with saving the AnalyticsReport data: {analyticsReport}", e);
            }
        }

        public async Task<AnalyticsReport> UpdateAnalyticsReport(string reportId, SROIReportConfigV2 request)
        {
            try
            {
                var sroiReport = await GetById(reportId, request.ParentId);
                sroiReport.ReportConfig = request;
                sroiReport.Name = request.General.ReportName;
                return await _analyticsReportContext.UpdateAnalyticsReport(sroiReport);
            }
            catch (ArgumentException e)
            {
                throw new ArgumentException($"Something went wrong with updating the AnalyticsReport data: {request}", e);
            }
        }

        public async Task DeleteById(string projectId, string reportId, AnalyticsReportTypeEnum type)
        {
            try
            {
                await _analyticsReportContext.DeleteAnalyticsReport(projectId, reportId);
            }
            catch (ArgumentException e)
            {
                throw new ArgumentException($"Something went wrong with deleting the analyticsReport record for analyticsReportId: {reportId}", e);
            }
            try
            {
                var fileName = $"#{type}#{reportId}";
                await _s3helper.DeleteS3File(fileName, null, S3Buckets.SROI, FileFormat.PDF);
            }
            catch (ArgumentException e)
            {
                throw new ArgumentException($"Something went wrong with deleting the AnalyticsReport file on S3 with analyticsReportId of: {reportId} and type of: {type}", e);
            }
        }

        public string GenerateAnalyticsReportDownloadURL(string reportId, AnalyticsReportTypeEnum type)
        {
            var fileName = $"#{type}#{reportId}";
            return _s3helper.GetFilePreSignedUrl(fileName, S3Buckets.SROI, FileFormat.PDF);
        }
    }
}