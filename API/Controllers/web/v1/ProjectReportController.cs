using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Amazon;
using API.Constants;
using API.Handlers;
using API.Models;
using API.Models.Auth;
using API.Models.Codes;
using API.Models.Projects;
using API.Models.Reports;
using API.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers.web.v1
{
    [Authorize(PolicyNames.ProjectAccess)]
    [ApiController]
    [Route("api/web/v1/projects/{projectId}/reports")]
    public class ProjectReportController : BaseController
    {
        private readonly IProjectContext _projectContext;
        private readonly IImageHandler _imageHandler;
        private readonly ICodeContext _codeContext;

        public ProjectReportController(IProjectContext projectContext, IImageHandler imageHandler, ICodeContext codeContext)
        {
            _projectContext = projectContext;
            _imageHandler = imageHandler;
            _codeContext = codeContext;
        }

        [HttpPut("{reportId}/image")]
        public async Task<ActionResult<AuthUser>> UploadImage(
            [FromRoute] string projectId,
            [FromRoute] string reportId,
            [FromForm] FormFileRequest request)
        {
            var message = GetMessage();
            var report = await _projectContext.Reports.Read(projectId, reportId);
            if (report == null)
            {
                return NotFound(message.ErrorNotFoundReport());
            }
            var s3Key = await _imageHandler.UploadImage(reportId, request);
            report.Images ??= new List<Image>();
            report.Images.Add(new Image
            {
                Key = s3Key,
                UploadedAt = DateTime.Now,
                UploadedBy = CurrentUserId(),
                Title = request.Title,
                Description = request.Description,
                Url = _imageHandler.GetObjectUrl(s3Key)
            });
            // report.ModuleConfigs ??= new List<ReportModuleConfig>();
            // report.ModuleConfigs.Add(CreateReportModuleConfig(report));
            await _projectContext.Reports.Update(projectId, report);
            return Ok(report);
        }


        [HttpDelete("{reportId}/image/{key}")]
        public async Task<ActionResult<Report>> DeleteImage(
            [FromRoute] string projectId, 
            [FromRoute] string reportId,
            [FromRoute] string key)
        {
            var message = GetMessage();
            var report = await _projectContext.Reports.Read(projectId, reportId);
            if (report == null)
            {
                return NotFound(message.ErrorNotFoundReport());
            }
            await _imageHandler.DeleteImage(key);
            if (!report.Images.Any())
            {
                return Ok(report);
            }

            for (var i = report.Images.Count - 1; i >= 0; i--)
            {
                if (report.Images[i].Key == key)
                {
                    report.Images.Remove(report.Images[i]);
                }
            }

            await _projectContext.Reports.Update(projectId, report);
            return Ok(report);
        }
        
        [HttpPost]
        public async Task<ActionResult<Report>> Create([FromRoute] string projectId, [FromBody] Report request)
        {
            request.ModuleConfigs = request.ModuleConfigs.Select(c =>
            {
                c.Id = Guid.NewGuid().ToString();
                return c;
            }).ToList();
            var report = await _projectContext.Reports.Create(projectId, request);
            return Ok(report);
        }

        [HttpGet]
        public async Task<ActionResult<List<Report>>> ReadAll([FromRoute] string projectId)
        {
            var reports = await _projectContext.Reports.ReadAll(projectId);
            return Ok(reports.ToList());
        }

        [HttpGet("{reportId}")]
        public async Task<ActionResult<Report>> Read([FromRoute] string projectId, [FromRoute] string reportId)
        {
            var message = GetMessage();
            // TODO: SHOULD THIS BE FRONT?
            if (reportId == "new" || reportId == "")
                return Ok(new Report()
                {
                    Name = message.Unnamed(),
                    ModuleConfigs = new List<ReportModuleConfig>(),
                });

            var report = await _projectContext.Reports.Read(projectId, reportId);
            if (report == null) return NotFound(message.ErrorNotFoundReport());

            return Ok(report);
        }

        [HttpPut("{reportId}")]
        public async Task<ActionResult<Report>> Update([FromRoute] string projectId, [FromRoute] string reportId,
            [FromBody] Report request)
        {
            request.Id = reportId;
            foreach (var module in request.ModuleConfigs)
            {
                if (module.FreeTextId == null) continue;
                var freeText = request.FreeTexts.Single(f => f.Id == module.FreeTextId);
                module.FreeTextTitle = freeText.Title;
                module.FreeTextContents = freeText.Contents;
                if (module.IsExcludeOnlyOneAnswer)
                {
                    module.Filters.Add(ReportModuleConfig.FilterExcludeOnlyOneAnswer);
                }
            }
            var report = await _projectContext.Reports.Update(projectId, request);
            return Ok(report);
        }

        [HttpDelete("{reportId}")]
        public async Task<ActionResult<Report>> Delete([FromRoute] string projectId, [FromRoute] string reportId)
        {
            await _projectContext.Reports.Delete(projectId, reportId);
            return Ok();
        }

        [HttpPost("{reportId}/share")]
        public async Task<ActionResult<ReportCode>> ShareReport(
            [FromRoute] string projectId,
            [FromRoute] string reportId)
        {
            var report = await _projectContext.Reports.Read(projectId, reportId);
            if (report == null) return NotFound();

            if (report.CodeId != null) return await _codeContext.ReportCodes.Read(report.CodeId);
            
            var code = await _codeContext.ReportCodes.Create(new ReportCode()
            {
                Id = Regex.Replace(Convert.ToBase64String(Guid.NewGuid().ToByteArray()), "[/+=]", ""),
                ProjectId = projectId,
                ReportId = reportId,
            });

            report.CodeId = code.Id;
            await _projectContext.Reports.Update(projectId, report);

            return Ok(code);
        }

        [HttpPost("{reportId}/freetext/")]
        public async Task<ActionResult<Report>> CreateFreeText(
            [FromRoute] string projectId,
            [FromRoute] string reportId,
            [FromBody] FreeText request)
        {
            var message = GetMessage();
            var report = await _projectContext.Reports.Read(projectId, reportId);
            if (report == null)
            {
                return NotFound(message.ErrorNotFoundReport());
            }
            var freeText = new FreeText
            {
                Id = Guid.NewGuid().ToString(),
                Title = request.Title,
                Contents = request.Contents,
                CreatedAt = DateTime.Now,
                CreatedBy = CurrentUserId()
            };
            freeText.UpdatedAt = freeText.CreatedAt;
            report.FreeTexts ??= new List<FreeText>();
            report.FreeTexts.Add(freeText);
            var modules = report.ModuleConfigs.Where(m => m.FreeTextId == freeText.Id);
            foreach (var module in modules)
            {
                module.FreeTextContents = freeText.Contents;
                module.FreeTextTitle = freeText.Title;
            }

            await _projectContext.Reports.Update(projectId, report);
            return Ok(report);
        }

        [HttpPut("{reportId}/freetext/{freeTextId}")]
        public async Task<ActionResult<Report>> UpdateFreeText(
            [FromRoute] string projectId,
            [FromRoute] string reportId,
            [FromRoute] string freeTextId,
            [FromBody] FreeText request)
        {
            var message = GetMessage();
            var report = await _projectContext.Reports.Read(projectId, reportId);
            if (report == null)
            {
                return NotFound(message.ErrorNotFoundReport());
            }

            if (!report.FreeTexts.Any())
            {
                return NotFound(message.ErrorNotFoundFreeText());

            }
            var freeText = report.FreeTexts.FirstOrDefault(f => f.Id == freeTextId);
            if (freeText == null)
            {
                return NotFound(message.ErrorNotFoundFreeText());
            }
            freeText.UpdatedAt = DateTime.Now;
            freeText.Title = request.Title;
            freeText.Contents = request.Contents;
            var modules = report.ModuleConfigs.Where(m => m.FreeTextId == freeText.Id);
            foreach (var module in modules)
            {
                module.FreeTextContents = freeText.Contents;
                module.FreeTextTitle = freeText.Title;
            }
            await _projectContext.Reports.Update(projectId, report);
            return Ok(report);
        }

        [HttpDelete("{reportId}/freetext/{freeTextId}")]
        public async Task<ActionResult<Report>> DeleteFreeText(
            [FromRoute] string projectId,
            [FromRoute] string reportId,
            [FromRoute] string freeTextId)
        {
            var message = GetMessage();
            var report = await _projectContext.Reports.Read(projectId, reportId);
            if (report == null)
            {
                return NotFound(message.ErrorNotFoundReport());
            }
            if (!report.FreeTexts.Any())
            {
                return Ok(report);
            }
            
            for (var i = report.FreeTexts.Count - 1; i >= 0; i--)
            {
                if (report.FreeTexts[i].Id == freeTextId)
                {
                    report.FreeTexts.Remove(report.FreeTexts[i]);
                }
            }
            await _projectContext.Reports.Update(projectId, report);
            return Ok(report);

        }

    }
}