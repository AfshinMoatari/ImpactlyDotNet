using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Threading.Tasks;
using Amazon.DynamoDBv2.Model;
using Amazon.SimpleEmailV2.Model;
using API.Constants;
using API.Dump;
using API.Handlers;
using API.Helpers;
using API.Models;
using API.Models.Cron;
using API.Models.Dump;
using API.Models.Logs;
using API.Models.Projects;
using API.Models.Strategy;
using API.Repositories;
using API.Views;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Nest;


namespace API.Controllers.web.v1
{
    [Authorize]
    [ApiController]
    [Route("api/web/v1/me")]
    public class MeController : BaseController
    {
        private readonly IProjectContext _projectContext;
        private readonly IExcelHandler _excelHandler;
        private readonly IPatientContext _patientContext;
        private readonly IStrategyContext _strategyContext;
        private readonly IAnalyticsContext _analyticsContext;
        private readonly IEmailHandler _emailHandler;

        
        public MeController(IProjectContext projectContext, IExcelHandler excelHandler, IAnalyticsContext analyticsContext,
            IPatientContext patientContext, IStrategyContext strategyContext, IEmailHandler emailHandler)
        {
            _projectContext = projectContext;
            _excelHandler = excelHandler;
            _patientContext = patientContext;
            _strategyContext = strategyContext;
            _emailHandler = emailHandler;
            _analyticsContext = analyticsContext;

        }

        [HttpGet("projects")]
        public async Task<ActionResult<IEnumerable<UserProject>>> ReadAllProjects()
        {
            var message = GetMessage();
            var currentUserId = CurrentUserId();
            if (currentUserId == null)
                return ErrorResponse.Forbidden(message.ErrorNotLogin());

            var projects = await _projectContext.ReadAllUserProjects(currentUserId);

            if (projects == null)
                return ErrorResponse.NotFound(message.ErrorUserNotConnectedToAny());

            return Ok(projects);
        }

        [HttpPost("projects/{projectId}/dump")]
        public async Task<ActionResult<SendEmailResponse>> DumpData(
            [FromRoute] string projectId,
            [FromBody] DumpRequest request)
        {
            request.UserName = CurrentUserName();
            var message = GetMessage();
            var user = await _projectContext.ReadProjectUser(projectId, CurrentUserId());
            var project = await _projectContext.Projects.Read(projectId);
            request.ProjectName = project.Name;
            IDataDumper dumper = new DumpDataEntries(_patientContext, _analyticsContext, _strategyContext, message);
            var startDate = request.StartDate;
            var endDate = request.EndDate;
            var fileName = dumper.CreateFileName(request.ProjectName, request.SortedBy);
            var dumpData = await dumper.ReadDumpData(projectId, startDate, endDate, request.Filter);
            var excelInput = new ExcelInput()
            {
                Type = request.SortedBy,
                DumpRequest = request,
                DumpDataList = dumpData
            };
            _excelHandler.Anonymity = (CurrentRoleId() is not RoleSeed.AdministratorRoleId);
            _excelHandler.Message = message;
            var url = await _excelHandler.CreateExcel(excelInput, fileName);
            if (url.IsNullOrEmpty())
            {
                return BadRequest(message.ErrorEmailFailed());
            }
            //const string contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
            var dumpEmail = new DataDumpEmail(message)
            {
                Title = message.OkExportReady(),
                Email = user.Email,
                ProjectName = project.Name,
                FileName = fileName,
                UserName = user.Name,
                ExportType = request.SortedBy,
                CreatedAt = TimeZoneInfo.ConvertTimeFromUtc(DateTime.Now.ToUniversalTime(), TimeZoneInfo.FindSystemTimeZoneById("Central European Standard Time")),
                StartDate = request.StartDate,
                EndDate = request.EndDate,
                DownloadUrl = url,
            };
           var emailRes = await _emailHandler.SendEmail(project.Name, user.Email, message.OkExportReady(), dumpEmail, projectId
               );
            
           return Ok(emailRes);
        }
        

        [HttpGet("projects/{projectId}/dump/options")]
        public async Task<DumpRequestOptions> GetDumpOptions()
        {
            var message = GetMessage();
            var options = new DumpRequestOptions
            {
                SortedBy = new List<string>()
                {
                    message.DumpField(DumpFields.DataEntries),
                    message.DumpField(DumpFields.DataStrategies),
                },
                Filter = new List<string>()
                {
                    message.DumpField(DumpFields.FilterAllSurvey),
                    message.DumpField(DumpFields.FilterValidated),
                    message.DumpField(DumpFields.FilterCustom),
                    message.DumpField(DumpFields.FilterAllRegistration),
                    message.DumpField(DumpFields.FilterIncidentRegistration),
                    message.DumpField(DumpFields.FilterStatusRegistration),
                    message.DumpField(DumpFields.FilterNumericRegistration),
                    message.DumpField(DumpFields.FilterAll),
                },
                Fields = new List<string>()
                {
                    message.DumpField(DumpFields.PatientFirstName),
                    message.DumpField(DumpFields.PatientLastName),
                    message.DumpField(DumpFields.QuestionNumber),
                    message.DumpField(DumpFields.Questions),
                    message.DumpField(DumpFields.Index),
                    message.DumpField(DumpFields.Answers),
                    message.DumpField(DumpFields.SurveyScore),
                    message.DumpField(DumpFields.Tags),
                    message.DumpField(DumpFields.Strategy),
                    message.DumpField(DumpFields.AnsweredAt),
                    
                }
            };
            options.OrderBy = options.Fields;
            return options;
        }


        [HttpPost("projects/{projectId}/dump/raw")]
        public async Task<ActionResult<SendEmailResponse>> DumpRawData(
            [FromRoute] string projectId,
            [FromBody] DumpRequest request)
        {
            request.UserName = CurrentUserName();
            var message = GetMessage();
            var user = await _projectContext.ReadProjectUser(projectId, CurrentUserId());
            var project = await _projectContext.Projects.Read(projectId);
            request.ProjectName = project.Name;
            IDataDumper dumper = new DumpDataRaw(_patientContext, _analyticsContext, _strategyContext, message);
            var startDate = request.StartDate;
            var endDate = request.EndDate;
            var fileName = dumper.CreateFileName(request.ProjectName, request.SortedBy);
            var dumpData = await dumper.ReadDumpData(projectId, startDate, endDate, request.Filter);
            var excelInput = new ExcelInput()
            {
                Type = ExcelInput.ExcelTypeDataDumpRaw,
                DumpRequest = request,
                DumpDataList = dumpData,
                ProjectName = CurrentProjectName(),
            };
            _excelHandler.Anonymity = (CurrentRoleId() is not RoleSeed.AdministratorRoleId);
            _excelHandler.Message = message;
            var url = await _excelHandler.CreateExcel(excelInput, fileName);
            if (url.IsNullOrEmpty())
            {
                return BadRequest(message.ErrorEmailFailed());
            }
            //const string contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
            var dumpEmail = new DataDumpEmail(message)
            {
                Title = message.OkExportReady(),
                Email = user.Email,
                ProjectName = project.Name,
                FileName = fileName,
                UserName = user.Name,
                ExportType = request.SortedBy,
                CreatedAt = TimeZoneInfo.ConvertTimeFromUtc(DateTime.Now.ToUniversalTime(), TimeZoneInfo.FindSystemTimeZoneById("Central European Standard Time")),
                StartDate = request.StartDate,
                EndDate = request.EndDate,
                DownloadUrl = url,
            };
           var emailRes = await _emailHandler.SendEmail(project.Name, user.Email, message.OkExportReady(), dumpEmail, projectId
               );
            
           return Ok(emailRes);
        }

        [HttpPost("projects/{projectId}/upload/citizens")]
        public async Task<ActionResult> UploadCitizens(
            [FromRoute] string projectId,
            [FromBody] List<ImportPatientRequest> requests)
        {
            var message = GetMessage();
            var existingProject = await _projectContext.Projects.Read(projectId);
            if (existingProject == null) 
                return ErrorResponse.NotFound(message.ErrorNotFoundProject());
            var existingPatients = await _patientContext.ProjectPatients.ReadAll(projectId);
            var importPatients = new List<ImportPatient>();
            foreach (var request in requests)
            {
                var importPatient = await CreateImportPatient(request, projectId);
                importPatients.Add(importPatient);
            }

            foreach (var importPatient in importPatients)
            {
                if (importPatient.Status != ImportPatient.ImportStatusPreparing)
                {
                    continue;
                }
                
                var existing = existingPatients.FirstOrDefault(p => string.Equals(p.LastName, importPatient.ImportPatientRequest.LastName, StringComparison.CurrentCultureIgnoreCase) &&
                                                                    string.Equals(p.FirstName, importPatient.ImportPatientRequest.Name, StringComparison.CurrentCultureIgnoreCase));
                if (existing != null)
                {
                    importPatient.ProjectPatient.Id = existing.Id;
                    importPatient.Status = ImportPatient.ImportStatusToBeUpdated;
                }
                else
                {
                    importPatient.Status = ImportPatient.ImportStatusToBeInserted;
                }

            }

            importPatients = await UpdateImportPatient(importPatients);
            
            
            var user = await _projectContext.ReadProjectUser(projectId, CurrentUserId());
            var excelInput = new ExcelInput()
            {
                ImportPatients = importPatients,
                UserName = CurrentUserName(),
                ProjectName = CurrentProjectName(),
                Type = ExcelInput.ExcelTypeImportPatientsLog
            };
            var filename = "import_citizens_" + excelInput.ProjectName.Replace(" ", "_").ToLower() 
                                              + "_" + DateTime.Now.Year + DateTime.Now.Month + DateTime.Now.Day
                                              + DateTime.Now.Hour + DateTime.Now.Minute + ".xlsx";
            
            var url = await _excelHandler.CreateExcel(excelInput, filename);

            var dumpEmail = new BulkUploadEmail(message)
            {
                Title = message.OkLogEmailReady(),
                Email = user.Email,
                FileName = filename,
                DownloadUrl = url,
                CreatedAt = TimeZoneInfo.ConvertTimeFromUtc(DateTime.Now.ToUniversalTime(), TimeZoneInfo.FindSystemTimeZoneById("Central European Standard Time")),
            };
            var emailRes = await _emailHandler.SendEmail(excelInput.ProjectName, user.Email,
                message.OkLogEmailReady(), dumpEmail, projectId
            );

            return Ok(emailRes.HttpStatusCode);
            
        }

        [HttpPost("post_exception"), Authorize(Permissions.Admin.All)]
        public async Task<List<string>> CreateExceptions(
            [FromBody] CustomerExceptionRequest request)
        {
            switch (request.ExceptionName)
            {
                case "NullReferenceException":
                    throw new NullReferenceException("New NullReferenceException: " + request.ExceptionMessage);
                case "FormatException":
                    throw new FormatException("new Format Exception: " + request.ExceptionMessage);
                case "SystemException":
                    throw new SystemException("New System Exception: " + request.ExceptionMessage);
                default:
                {
                    var exception = new Exception(request.ExceptionMessage);
                    throw exception;
                }
            }
        } 
        
        
        [HttpGet("projects/downloads/{fileName}"), Authorize(Permissions.Admin.All)]
        public async Task<IActionResult> Download(
            string fileName)
        {
            var path = Path.Combine(FileStorage.FileStorageDir, fileName);
            if (System.IO.File.Exists(path))
            {
                // Get all bytes of the file and return the file with the specified file contents 
                var b = await System.IO.File.ReadAllBytesAsync(path);
                return File(b, "application/octet-stream");
            }
            else
            {
                // return error if file not found
                return StatusCode(StatusCodes.Status500InternalServerError);
            }

        }
        protected async Task<List<ImportPatient>> UpdateImportPatient(List<ImportPatient> importPatients)
        {
            var message = GetMessage();
            foreach (var importPatient in importPatients)
            {
                if (importPatient.Status == ImportPatient.ImportStatusToBeInserted)
                {
                    var created = await 
                        _patientContext.ProjectPatients.Create(importPatient.ProjectId, importPatient.ProjectPatient);
                    if (importPatient.ProjectPatient.StrategyId != null)
                    {
                        var strategyId = importPatient.ProjectPatient.StrategyId;
                        var patientId = created.Id;
                        var existingStrategyPatient =
                            await _strategyContext.StrategyPatients.Read(strategyId, patientId);
                        if (existingStrategyPatient == null)
                        {
                            await _strategyContext.StrategyPatients.Create(strategyId, new StrategyPatient
                            {
                                Id = patientId,
                                Name = created.Name
                            });
                        }

                        //// CREATE JOB FOR ASSIGNED
                        //// TEMPORARY DISABLED SINCE WE NOW HAVE BATCH SENDOUT

                        //var existingFrequencies = await _strategyContext.Frequencies.ReadAll(strategyId);
                        //foreach (var existingStrategyFrequency in existingFrequencies)
                        //{
                        //    var job = SurveyJob.CreateSurveyJob(existingStrategyFrequency.CronExpression, importPatient.ProjectId, strategyId,
                        //        existingStrategyFrequency.Id, patientId);
                        //    await _cronContext.SurveyJobs.Create(job);
                        //}

                        await _patientContext.ProjectPatients.UpdateValue(importPatient.ProjectId, patientId, e =>
                        {
                            e.StrategyName = importPatient.ProjectPatient.StrategyName;
                            e.StrategyId = importPatient.ProjectPatient.StrategyId;
                        });
                    }

                    foreach (var projectTag in importPatient.ProjectTags)
                    {
                        var patientTag = await _patientContext.Tags.Create(created.Id, new PatientTag()
                        {
                            Name = projectTag.Name,
                            Color = projectTag.Color,
                            ProjectTagId = projectTag.Id,
                        });
                        var projectPatient = await _patientContext.ProjectPatients.Update(
                            importPatient.ProjectId,
                            created.Id,
                            patient => patient.Tags.Add(patientTag)
                        );

                    }

                    var checking = await _patientContext.ProjectPatients.Read(importPatient.ProjectId, created.Id);
                    if (checking == null)
                    {
                        importPatient.Status = ImportPatient.ImportStatusInsertFailed;
                    }

                    importPatient.Status = ImportPatient.ImportStatusInserted;
                    importPatient.ProjectPatient = checking;
                }

                if (importPatient.Status == ImportPatient.ImportStatusToBeUpdated)
                {
                    //to update tags, also need to update analytics
                    var existing = await 
                        _patientContext.ProjectPatients.Read(importPatient.ProjectId, importPatient.ProjectPatient.Id);
                    var toUpdate = await DoUpdates(importPatient, existing);
                    if (toUpdate == null)
                    {
                        importPatient.Status = ImportPatient.ImportStatusSkipped;
                        importPatient.Message =
                            message.InfoImportCitizens();
                    }
                    else
                    {
                        importPatient.Status = ImportPatient.ImportStatusUpdated;
                        importPatient.ProjectPatient = toUpdate;
                    }
                }
            }

            return importPatients;
        }

        protected async Task<ProjectPatient> DoUpdates(ImportPatient importPatient, ProjectPatient existing)
        {
            var message = GetMessage();
            importPatient.UpdateFields = new List<string>();
            var isActiveChanged = existing.IsActive != importPatient.ImportPatientRequest.IsActive;
            var isEmailChanged = !CheckStringEquals(existing.Email, importPatient.ImportPatientRequest.Email);
            var isPhoneChanged = !CheckStringEquals(existing.PhoneNumber, importPatient.ImportPatientRequest.PhoneNumber);
            var isMunicipalityChanged  = !CheckStringEquals(existing.Municipality, importPatient.ImportPatientRequest.Municipality);
            var isSexChanged = !CheckStringEquals(existing.Sex, importPatient.ImportPatientRequest.Sex);
            var isPostalNumberChanged =
                !CheckStringEquals(existing.PostalNumber, importPatient.ImportPatientRequest.PostalNumber.ToString());
            ProjectPatient updating = null;
            if (isActiveChanged)
            {
                importPatient.UpdateFields.Add(message.BulkUploadUpdateStatus("IsActive", existing.IsActive.ToString(), importPatient.ImportPatientRequest.IsActive.ToString()));
                updating = await _patientContext.ProjectPatients.Update(importPatient.ProjectId, existing.Id, e =>
                {
                    e.IsActive = importPatient.ImportPatientRequest.IsActive;
                });
            }

            if (isEmailChanged)
            {
                importPatient.UpdateFields.Add(message.BulkUploadUpdateStatus("Email", existing.Email, importPatient.ImportPatientRequest.Email));
                updating = await _patientContext.ProjectPatients.Update(importPatient.ProjectId, existing.Id, e =>
                {
                    e.Email = importPatient.ImportPatientRequest.Email;
                });
            }

            if (isPhoneChanged)
            {
                importPatient.UpdateFields.Add(message.BulkUploadUpdateStatus("PhoneNumber", existing.PhoneNumber, importPatient.ImportPatientRequest.PhoneNumber));
                updating = await _patientContext.ProjectPatients.Update(importPatient.ProjectId, existing.Id, e =>
                {
                    e.PhoneNumber = importPatient.ImportPatientRequest.PhoneNumber.ToString();
                });
            }

            if (isMunicipalityChanged)
            {
                importPatient.UpdateFields.Add(message.BulkUploadUpdateStatus("Municipality", existing.Municipality, importPatient.ImportPatientRequest.Municipality));
                updating = await _patientContext.ProjectPatients.Update(importPatient.ProjectId, existing.Id, e =>
                {
                    e.Municipality = importPatient.ImportPatientRequest.Municipality;
                });

            }
            if (isSexChanged)
            {
                importPatient.UpdateFields.Add(message.BulkUploadUpdateStatus("Sex",  existing.Sex, importPatient.ImportPatientRequest.Sex));
                updating = await _patientContext.ProjectPatients.Update(importPatient.ProjectId, existing.Id, e =>
                {
                    e.Sex = importPatient.ImportPatientRequest.Sex;
                });
            }

            if (isPostalNumberChanged)
            {
                importPatient.UpdateFields.Add(message.BulkUploadUpdateStatus("PostNumber", existing.PostalNumber,   importPatient.ImportPatientRequest.PostalNumber));
                updating = await _patientContext.ProjectPatients.Update(importPatient.ProjectId, existing.Id, e =>
                {
                    e.PostalNumber = importPatient.ImportPatientRequest.PostalNumber.ToString();
                });
            }
            return updating;
        }
        
        protected async Task<ImportPatient> CreateImportPatient(ImportPatientRequest request, string projectId)
        {
            var message = GetMessage();
            var importPatient = new ImportPatient
            {
                ProjectPatient = null,
                ImportPatientRequest = request,
                ProjectId = projectId,
                ProjectTags = new List<ProjectTag>()
            };
            var projectPatient = new ProjectPatient
            {
                Email = request.Email,
                Municipality = request.Municipality,
                LastName = request.LastName,
                FirstName = request.Name,
                Region = request.Region,
                BirthDate = request.BirthDate,
                Sex = request.Sex,
                PhoneNumber = request.PhoneNumber.ToString(),
                PostalNumber = request.PostalNumber.ToString(),
                IsActive = request.IsActive
            };

            if (!string.IsNullOrEmpty(request.Strategy))
            {
                var strategies = await _strategyContext.Strategies.ReadAll(projectId);
                var strategy = strategies.FirstOrDefault(s => s.Name.Trim().Equals(request.Strategy.Trim(), StringComparison.CurrentCultureIgnoreCase));
                if (strategy == null)
                {
                    importPatient.Status = ImportPatient.ImportStatusSkipped;
                    importPatient.Message = request.Strategy + " : " + message.ErrorNotFoundStrategy();
                    return importPatient;
                }

                projectPatient.StrategyId = strategy.Id;
                projectPatient.StrategyName = strategy.Name;
                importPatient.ProjectPatient = projectPatient;
                importPatient.Status = ImportPatient.ImportStatusPreparing;
                
            
            }

            if (string.IsNullOrEmpty(request.Tags))
            {
                importPatient.ProjectPatient = projectPatient;
                importPatient.Status = ImportPatient.ImportStatusPreparing;
                return importPatient;
            }
            
            var tags = await _projectContext.Tags.ReadAll(projectId);
            var tagNames = request.TagsToList();
            foreach (var tagName in tagNames)
            {
                var projectTags = tags.ToList();
                var tag = projectTags.FirstOrDefault(t => t.Name.Trim().Equals(tagName.Trim(), StringComparison.CurrentCultureIgnoreCase));
                if (tag == null)
                {
                    importPatient.Status = ImportPatient.ImportStatusSkipped;
                    importPatient.Message = tagName + ": " + message.ErrorNotFoundTag();
                    return importPatient;
                }

                importPatient.ProjectTags.Add(tag);

            }
            importPatient.ProjectPatient = projectPatient;
            importPatient.Status = ImportPatient.ImportStatusPreparing;
            return importPatient;

        }

        protected bool CheckStringEquals(string obj1, string obj2)
        {
            if (obj1.IsNullOrEmpty() && obj2.IsNullOrEmpty())
                return true;
            obj1 = obj1?.Trim();
            obj2 = obj2?.Trim();
            return string.Equals(obj1, obj2);
        }
}
}