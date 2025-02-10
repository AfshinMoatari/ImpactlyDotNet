using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Amazon.DynamoDBv2.DataModel;
using API.Constants;
using API.Extensions;
using API.Handlers;
using API.Models;
using API.Models.Analytics;
using API.Models.Codes;
using API.Models.Cron;
using API.Models.Projects;
using API.Models.Strategy;
using API.Repositories;
using API.Views;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers.web.v1
{
    [Authorize(PolicyNames.ProjectAccess)]
    [ApiController]
    [Route("api/web/v1/projects/{projectId}/patients")]
    public class ProjectPatientController : BaseController
    {
        private readonly IPatientContext _patientContext;
        private readonly IStrategyContext _strategyContext;
        private readonly IProjectContext _projectContext;
        private readonly ICronContext _cronContext;
        private readonly IEmailHandler _emailHandler;
        private readonly IAnalyticsContext _analytics;
        private readonly ISMSHandler _smsHandler;
        private readonly Regex _regex = new Regex(@"\+45");
        private readonly IAnonymityHandler _anonymityHandler;
        private readonly ISurveyHandler _surveyHandler;
        
        public ProjectPatientController(IPatientContext patientContext, IStrategyContext strategyContext,
            IProjectContext projectContext, ICronContext cronContext, ISurveyHandler surveyHandler,
            IEmailHandler emailHandler, ISMSHandler smsHandler, IAnalyticsContext analytics, IAnonymityHandler anonymityHandler)
        {
            _patientContext = patientContext;
            _strategyContext = strategyContext;
            _projectContext = projectContext;
            _cronContext = cronContext;
            _emailHandler = emailHandler;
            _smsHandler = smsHandler;
            _analytics = analytics;
            _anonymityHandler = anonymityHandler;
            _surveyHandler = surveyHandler;
        }

        [HttpPost, Authorize(Permissions.Users.Write)]
        public async Task<ActionResult<ProjectPatient>> Create([FromRoute] string projectId,
            [FromBody] ProjectPatient request)
        {
            var message = GetMessage();
            var existingProject = await _projectContext.Projects.Read(projectId);
            if (existingProject == null) return ErrorResponse.NotFound(message.ErrorNotFoundProject());

            var comm = await _projectContext.Communicaitons.ReadAll(projectId);
            var welcomeComm = comm
                .FirstOrDefault(i => i.MessageType == ProjectCommunication.CommunicationTypeWelcome);
            var welcomeText = welcomeComm == null
                ? message.DefaultWelcomeMessage().Replace("@Model.ProjectName", existingProject.Name)
                : welcomeComm.MessageContent;

            var name = request.Anonymity ? AnonymityMessage.HiddenPatientName : request.Name;
            if (!string.IsNullOrEmpty(request.PhoneNumber))
            {
                var match = _regex.Match(request.PhoneNumber);
                if (!match.Success)
                    request.PhoneNumber = "+45" + request.PhoneNumber;

                var body =
                    message.ShortMessage(name, welcomeText,  existingProject.Name);
                await _smsHandler.SendSMS(existingProject.Name, request.PhoneNumber, body, existingProject.Id);
            }

            if (!string.IsNullOrEmpty(request.Email))
            {
                if (string.IsNullOrEmpty(request.PhoneNumber))
                    await _emailHandler.SendEmail(existingProject.Name,
                        request.Email, message.WelcomeTo(existingProject.Name),
                        new WelcomePatientEmail(message)
                        {
                            Title = message.WelcomeTo(existingProject.Name),
                            ProjectName = existingProject.Name,
                            UserName = name,
                            Message = welcomeText
                        }, existingProject.Id);
            }

            var response = await _patientContext.ProjectPatients.Create(projectId, request);
            return Ok(response);
        }

        [HttpGet, Authorize(Permissions.Users.Read)]
        public async Task<ActionResult<List<ProjectPatient>>> ReadPage([FromRoute] string projectId)
        {
            
            var response = await _patientContext.ProjectPatients.ReadAll(projectId);
            if (response == null) return NotFound();
            var roleId = CurrentRoleId();
            if (roleId == RoleSeed.AdministratorRoleId) return Ok(response);
            return Ok(_anonymityHandler.HidePatients(response));
        }

        [HttpGet("{patientId}"), Authorize(Permissions.Users.Read)]
        public async Task<ActionResult<ProjectPatient>> Read([FromRoute] string projectId, [FromRoute] string patientId)
        {
            var response = await _patientContext.ReadPatient(projectId, patientId);
            if (response == null) return NotFound();
            var roleId = CurrentRoleId();
            if (roleId == RoleSeed.AdministratorRoleId) return Ok(response);
            //HideMessages(response);
            return Ok(_anonymityHandler.HidePatient(response));
        }

        [HttpPut("{patientId}"), Authorize(Permissions.Users.Write)]
        public async Task<ActionResult<ProjectPatient>> Update([FromRoute] string projectId,
            [FromRoute] string patientId,
            [FromBody] ProjectPatient projectPatient)
        {
            var message = GetMessage();
            var roleId = CurrentRoleId();
            projectPatient.Id = patientId;
            var patient = await _patientContext.ProjectPatients.Read(projectId, patientId);
            if (patient == null)
            {
                return NotFound();
            }
            if (roleId != RoleSeed.AdministratorRoleId && patient.Anonymity)
            {
                return BadRequest(message.ErrorUserNotAdmin());
            }
            var result = await _patientContext.ProjectPatients.Update(projectId, projectPatient);
            var strategies = await _strategyContext.Strategies.ReadAll(projectId);

            var jobsBatch = _strategyContext.Strategies.Context.CreateBatchWrite<SurveyJob>();
            foreach (var strategy in strategies)
            {
                var strategyPatient = await _strategyContext.StrategyPatients.Read(strategy.Id, patientId);
                if (strategyPatient == null) continue;
                strategyPatient.Name = projectPatient.Name;
                strategyPatient.Anonymity = projectPatient.Anonymity;
                await _strategyContext.StrategyPatients.Update(strategy.Id, strategyPatient);

                //update outdated existing jobs
                //if the old patient is active, skip the following.
                //if the new update is inactive, skip the following.
                if (patient.IsActive || !result.IsActive) continue;
                var jobs = await _cronContext.SurveyJobs.ReadAll(
                    new SurveyJobAccess
                    {
                        StrategyId = strategy.Id,
                        PatientId = patientId
                    });
                foreach (var job in jobs.Where(x => x.Type is JobType.Frequent && x.Status is not JobStatus.Completed))
                {
                    // var updatedJob = SurveyJob.CreateSurveyJobWithType(job.CronExpression, projectId, job.StrategyId, job.FrequencyId, job.PatientId, "FREQUENT", "Queued");
                    await _cronContext.SurveyJobs.UpdateValue(job.Id, j =>
                    {
                        j.NextExecution = JobReducer.GetNextExecutionTime(j, DateTime.Now);
                        j.Status = "Queued";
                    });
                    //jobsBatch.AddPutItem(_cronContext.SurveyJobs.ToDynamoItem(updatedJob));
                }
            }

            await _strategyContext.Strategies.Context.ExecuteBatchWriteAsync(new BatchWrite[]
            {
                jobsBatch
            });

            return result;
        }

        [HttpDelete("{patientId}"), Authorize(Permissions.Users.Write)]
        public async Task<ActionResult<ProjectPatient>> Delete([FromRoute] string projectId,
            [FromRoute] string patientId)
        {
            var message = GetMessage();
            var existingProjectPatient = await _patientContext.ProjectPatients.Read(projectId, patientId);
            if (existingProjectPatient == null)
                return ErrorResponse.NotFound(message.ErrorNotFoundProject());

            var projectPatientBatch =
                _patientContext.ProjectPatients.Context.CreateBatchWrite<ProjectPatient>();
            projectPatientBatch.AddDeleteItem(existingProjectPatient);

            var batchWrite = new List<BatchWrite>
            {
                projectPatientBatch
            };

            var strategyId = existingProjectPatient.StrategyId;
            if (strategyId != null)
            {
                var strategyPatient =
                    await _strategyContext.StrategyPatients.Read(strategyId, existingProjectPatient.Id);
                var strategyPatientBatch =
                    _strategyContext.StrategyPatients.Context.CreateBatchWrite<StrategyPatient>();
                strategyPatientBatch.AddDeleteItem(strategyPatient);
                batchWrite.Add(strategyPatientBatch);
            }

            // registrations
            var registrationsBatch = _analytics.Registrations.Context.CreateBatchWrite<Registration>();
            var registrations =
                await _analytics.Registrations.ReadBetween(
                    new RegistrationAccess
                    {
                        ProjectId = projectId,
                        PatientId = patientId,
                        SearchStart = DateTime.MinValue,
                        SearchEnd = DateTime.MaxValue
                    });
            registrationsBatch.AddDeleteItems(registrations);

            // survey entries
            var entryBatchesBatch = _analytics.EntryBatches.Context.CreateBatchWrite<EntryBatch>();
            var fieldEntryBatch = _analytics.EntryBatches.Context.CreateBatchWrite<FieldEntry>();
            var entryBatches =
                await _analytics.EntryBatches.ReadBetween(
                    new SurveyAccess
                    {
                        ProjectId = projectId,
                        PatientId = existingProjectPatient.Id,
                        SearchStart = DateTime.MinValue,
                        SearchEnd = DateTime.MaxValue
                    }
                );
            foreach (var entryBatch in entryBatches)
            {
                entryBatchesBatch.AddDeleteItem(entryBatch);
                var fieldEntries = await _analytics.FieldEntries.ReadAll(entryBatch.Id);
                fieldEntryBatch.AddDeleteItems(fieldEntries);
            }

            // tags
            var tagBatch = _patientContext.Tags.Context.CreateBatchWrite<PatientTag>();
            var patientTags = await _patientContext.Tags.ReadAll(existingProjectPatient.Id);
            tagBatch.AddDeleteItems(patientTags);

            // jobs
            var jobBatch = _cronContext.SurveyJobs.Context.CreateBatchWrite<SurveyJob>();
            var jobs = await _cronContext.SurveyJobs.ReadAll(
                new SurveyJobAccess
                {
                    StrategyId = strategyId,
                });
            var patientjobs = jobs.Where(x => x.PatientId == existingProjectPatient.Id).ToList();
            jobBatch.AddDeleteItems(patientjobs);

            //frequency 
            var strategyFrequencies = await _strategyContext.BatchFrequencies.ReadAll(strategyId);
            var batchUpdateFrequencies = new List<BatchSendoutFrequency>();
            var batchDeleteFrequencies = new List<BatchSendoutFrequency>();

            foreach (var strategyFrequency in strategyFrequencies)
            {
                if (strategyFrequency.PatientsId != null)
                {
                    if (strategyFrequency.PatientsId.Count.Equals(0))
                    {
                        batchDeleteFrequencies.Add(strategyFrequency);
                    }

                    if (strategyFrequency.PatientsId.Count > 0 && strategyFrequency.PatientsId.Contains(patientId))
                    {
                        strategyFrequency.PatientsId.Remove(patientId);
                        if (strategyFrequency.PatientsId.Count.Equals(0))
                        {
                            batchDeleteFrequencies.Add(strategyFrequency);
                        }
                        else
                        {
                            batchUpdateFrequencies.Add(strategyFrequency);
                        }
                    }
                }
                else
                {
                    batchDeleteFrequencies.Add(strategyFrequency);
                }
            }

            // write changes
            batchWrite.Add(registrationsBatch);
            batchWrite.Add(entryBatchesBatch);
            batchWrite.Add(fieldEntryBatch);
            batchWrite.Add(tagBatch);
            batchWrite.Add(jobBatch);
            await _analytics.Registrations.Context.ExecuteBatchWriteAsync(batchWrite.ToArray());
            await _strategyContext.BatchFrequencies.UpdateBatch(batchUpdateFrequencies);
            await _strategyContext.BatchFrequencies.DeleteBatch(batchDeleteFrequencies);
            return Ok(existingProjectPatient);
        }

        [HttpGet("{patientId}/surveys"), Authorize(Permissions.Users.Read)]
        public async Task<ActionResult<List<EntryBatch>>> ReadSurveyAnswers(
            [FromRoute] string projectId,
            [FromRoute] string patientId,
            [FromQuery] string type,
            [FromQuery] DateTime? start,
            [FromQuery] DateTime? end
        )
        {
            var customSurveys = await _strategyContext.ReadProjectSurveys(projectId);
            var customSurveysIds = customSurveys.Select(e => e.Id).ToList();
            var response = await _analytics.EntryBatches.ReadBetween(
                new SurveyAccess
                {
                    ProjectId = projectId,
                    PatientId = patientId,
                    SearchStart = start??DateTime.Now.AddYears(-3),
                    SearchEnd = end??DateTime.Now.AddYears(1),
                }
            );
            response = type switch
            {
                "validated" => response.FindAll(e => !customSurveysIds.Contains(e.SurveyId)),
                "custom" => response.FindAll(e =>  customSurveysIds.Contains(e.SurveyId)),
                _ => response
            };
            return Ok(response);
        }

        [HttpGet("{patientId}/custom-surveys/")]
        public async Task<ActionResult<List<FieldEntry>>> ReadyCustomSurveyAnswers(
            [FromRoute] string projectId,
            [FromRoute] string patientId,
            [FromQuery] string surveyId,
            [FromQuery] string fieldId,
            [FromQuery] DateTime? start,
            [FromQuery] DateTime? end)
        {
            start = start?.AddDays(1) ?? DateTime.MinValue;
            end = end?.AddDays(1) ?? DateTime.MaxValue;
            
            if (string.IsNullOrEmpty(fieldId) && string.IsNullOrEmpty(surveyId))
            {
                var surveys = await _strategyContext.Surveys.ReadAll(projectId);
                var surveysIds = surveys.Select(f => f.Id);
                var fieldEntries = new List<FieldEntry>();
                foreach (var id in surveysIds)
                {
                    fieldEntries.AddRange(await GetEntryFieldsBySurveyId(projectId, patientId, id, start, end));
                }

                return Ok(fieldEntries);
            }

            if (string.IsNullOrEmpty(fieldId))
            {
                var fieldEntries = await GetEntryFieldsBySurveyId(projectId, patientId, surveyId, start, end);
                return Ok(fieldEntries);
            }

            if (string.IsNullOrEmpty(surveyId))
            {
                var fieldEntries = await GetEntryFieldsByFieldId(projectId, patientId, fieldId, start, end);
                return Ok(fieldEntries);
            }
            else
            {
                var fieldEntries =
                    await GetEntryFieldsByFieldsIdAndSurveyId(projectId, patientId, surveyId, fieldId, start, end);
                return Ok(fieldEntries);
            }
        }


        [HttpPost("{patientId}/registrations")]
        public async Task<ActionResult<Registration>> CreateRegistration([FromRoute] string projectId,
            [FromRoute] string patientId,
            [FromBody] Registration request)
        {
            var message = GetMessage();
            request.ProjectId = projectId;
            request.PatientId = patientId;
            if (request.Type != "status") request.Category = null;
            var patient = await _patientContext.ReadPatient(projectId, patientId);
            if (patient == null) return ErrorResponse.NotFound(message.ErrorNotFoundCitizen());
            request.Tags = patient.Tags.Select(t => t.Name).ToList();
            var response = await _analytics.Registrations.CreateRegisteration(request, patient.StrategyId);

            return Ok(response);
        }

        [HttpGet("{patientId}/registrations")]
        public async Task<ActionResult<List<Registration>>> ReadRegistrations(
            [FromRoute] string projectId,
            [FromRoute] string patientId,
            [FromQuery] DateTime start,
            [FromQuery] DateTime end
        )
        {
            var registrations = await _analytics.Registrations.ReadBetween(
                new RegistrationAccess
                {
                    ProjectId = projectId,
                    PatientId = patientId,
                    SearchStart = DateTime.MinValue,
                    SearchEnd = DateTime.MaxValue
                }
            );
            
            return Ok(registrations);
        }

        [HttpPut("{patientId}/registrations/{registrationId}"), Authorize(Permissions.Users.Write)]
        public async Task<ActionResult<Registration>> EditRegistration(
            [FromRoute] string projectId,
            [FromRoute] string patientId,
            [FromRoute] string registrationId,
            [FromBody] Registration request)
        {
            request.Id = registrationId;
            request.ProjectId = projectId;
            request.PatientId = patientId;
            var registration = await _analytics.Registrations.Read(registrationId);
            request.CreatedAt = registration.CreatedAt;
            request.GSISK2 = registration.GSISK2;
            request.GSIPK2 = registration.GSIPK2;
            request.GSISK3 = registration.GSISK3;
            request.GSIPK3 = registration.GSIPK3;
            var result = await _analytics.Registrations.Update(request);
            return Ok(result);
        }


        [HttpDelete("{patientId}/registrations/{registrationId}"), Authorize(Permissions.Users.Write)]
        public async Task<ActionResult<Registration>> DeleteRegistration(
            [FromRoute] string projectId,
            [FromRoute] string registrationId)
        {
            var message = GetMessage();
            var existingRegistration = await _analytics.Registrations.Read(registrationId);
            if (existingRegistration == null)
                return ErrorResponse.NotFound(message.ErrorNotFoundRegistration());
            await _analytics.Registrations.Delete(registrationId);
            return Ok(existingRegistration);
        }


        [HttpPost("{patientId}/tags")]
        public async Task<ActionResult<ProjectPatient>> AddTags(
            [FromRoute] string projectId,
            [FromRoute] string patientId,
            [FromBody] List<ProjectTag> projectTags)
        {
            // TODO THIS IS A TRANSACTION BETWEEN OBJ 
            // SO USE THE EQ API
            var patientTagBatch = _patientContext.DynamoDbContext.CreateBatchWrite<PatientTag>(
                new DynamoDBOperationConfig { SkipVersionCheck = true, IgnoreNullValues = true });

            var patientTags = projectTags.Select(pt =>
                _patientContext.Tags.ToDynamoItem(patientId,
                    new PatientTag
                    {
                        Name = pt.Name,
                        Color = pt.Color,
                        ProjectTagId = pt.Id,
                    }
                )).ToList();
            patientTagBatch.AddPutItems(patientTags);

            await _patientContext.DynamoDbContext
                .ExecuteBatchWriteAsync(new BatchWrite[]
                    { patientTagBatch });

            await _patientContext.ProjectPatients.Update(
                projectId,
                patientId,
                patient => patient.Tags.AddRange(patientTags)
            );
            var tagNames = patientTags.Select(p => p.Name).Distinct();
            var patient = await _patientContext.ReadPatient(projectId, patientId);
            await _analytics.AddTags(projectId, patientId, tagNames);
            return Ok(patient);
        }

        [HttpDelete("{patientId}/tags/{tagId}")]
        public async Task<ActionResult<ProjectPatient>> DeleteTag(
            [FromRoute] string projectId,
            [FromRoute] string patientId,
            [FromRoute] string tagId)
        {
            var shallowPatient = await _patientContext.ProjectPatients.Read(projectId, patientId);
            shallowPatient.Tags = shallowPatient.Tags.Where(tag => tag.Id != tagId).ToList();
            await _patientContext.ProjectPatients.Update(projectId, shallowPatient);

            var tag = await _patientContext.Tags.Read(patientId, tagId);
            await _patientContext.Tags.Delete(patientId, tag.Id);
            await _analytics.DeleteTag(projectId, patientId, tag.Name);
            var deepPatient = await _patientContext.ReadPatient(projectId, patientId);
            return Ok(deepPatient);
        }

        [HttpPut("{patientId}/assign/{strategyId}")]
        public async Task<ActionResult<ProjectPatient>> Assign(
            [FromRoute] string projectId, [FromRoute] string strategyId, [FromRoute] string patientId)
        {
            var message = GetMessage();
            var existingStrategy = await _strategyContext.Strategies.Read(projectId, strategyId);
            if (existingStrategy == null)
                return ErrorResponse.NotFound(message.ErrorNotFoundStrategy());

            var existingPatient = await _patientContext.ProjectPatients.Read(projectId, patientId);
            if (existingPatient == null) return ErrorResponse.NotFound(message.ErrorNotFoundCitizen());

            var existingStrategyPatient =
                await _strategyContext.StrategyPatients.Read(existingStrategy.Id, existingPatient.Id);
            if (existingStrategyPatient != null) return ErrorResponse.BadRequest(message.ErrorPatientHasStrategy());

            existingStrategyPatient =
                await _strategyContext.StrategyPatients.Read(existingPatient.StrategyId, existingPatient.Id);

            if (existingStrategyPatient != null)
            {
                await _strategyContext.StrategyPatients.Delete(existingPatient.StrategyId, existingPatient.Id);
            }
            
            await _strategyContext.StrategyPatients.Create(existingStrategy.Id, new StrategyPatient
            {
                Id = existingPatient.Id,
                Name = existingPatient.Name
            });

            // Disabled since we are not creating jobs on 
            // CREATE JOB FOR ASSIGNED
            //var existingFrequencies = await _strategyContext.Frequencies.ReadAll(existingStrategy.Id);
            //foreach (var existingStrategyFrequency in existingFrequencies)
            //{
            //    var job = SurveyJob.CreateSurveyJob(existingStrategyFrequency.CronExpression, projectId, strategyId,
            //        existingStrategyFrequency.Id, patientId);
            //    await _cronContext.SurveyJobs.Create(job);
            //}

            return await _patientContext.ProjectPatients.UpdateValue(projectId, patientId, e =>
            {
                e.StrategyName = existingStrategy.Name;
                e.StrategyId = existingStrategy.Id;
            });
        }

        [HttpPost("{patientId}/code/{strategyId}")]
        public async Task<ActionResult<SurveyCode>> GetSurveyCode
        (
            [FromRoute] string projectId,
            [FromRoute] string patientId,
            [FromRoute] string strategyId,
            [FromBody] List<SurveyProperty> surveyProperties
        )
        {
            var message = GetMessage();
            
            var patient = await _patientContext.ReadPatient(projectId, patientId);
            if (patient == null) return ErrorResponse.NotFound(message.ErrorNotFoundCitizen());

            //TODO: this hsould be part of the refactoring sendout so that we can track the sendout happens through patients page with frequency id rather than passing null param here
            var code = await _surveyHandler.CreateSurveyCode(patient, strategyId, surveyProperties, null);
            if (code == null)
            {
                return ErrorResponse.BadRequest(message.ErrorCodeCreateFailed());
            }

            return Ok(code);
        }

        [HttpPost("{patientId}/code/{strategyId}/send")]
        public async Task<ActionResult<SurveyCode>> SendSurvey
        (
            [FromRoute] string projectId,
            [FromRoute] string patientId,
            [FromRoute] string strategyId,
            [FromBody] List<SurveyProperty> surveyProperties
        )
        {
            var message = GetMessage();
            _surveyHandler.SetMessage(message);
            var project = await _projectContext.Projects.Read(projectId);
            if (project == null) return ErrorResponse.NotFound(message.ErrorNotFoundProject());

            var patient = await _patientContext.ReadPatient(projectId, patientId);
            if (patient == null) return ErrorResponse.NotFound(message.ErrorNotFoundCitizen());

            //TODO: this hsould be part of the refactoring sendout so that we can track the sendout happens through patients page with frequency id rather than passing null param here
            var surveyResult = await _surveyHandler.SendSurvey(projectId, project.Name, patient, strategyId, surveyProperties, null);
            if (!surveyResult.success || surveyResult.code == null)
            {
                return ErrorResponse.BadRequest(message.ErrorCodeCreateFailed());
            }

            return Ok(surveyResult.code);
        }

        [HttpPost("{patientId}/code/{strategyId}/send/{surveyCodeId}/{notificationId}")]
        public async Task<ActionResult<SurveyCode>> SendSurvey
        (
            [FromRoute] string projectId,
            [FromRoute] string patientId,
            [FromRoute] string strategyId,
            [FromRoute] string surveyCodeId,
            [FromRoute] string notificationId)
        {
            var message = GetMessage();
            _surveyHandler.SetMessage(message);
            var project = await _projectContext.Projects.Read(projectId);
            if (project == null) return ErrorResponse.NotFound(message.ErrorNotFoundProject());

            var patient = await _patientContext.ReadPatient(projectId, patientId);
            if (patient == null) return ErrorResponse.NotFound(message.ErrorNotFoundCitizen());
            var surveyResult = await _surveyHandler.SendSurvey(projectId, project.Name, patient, strategyId, surveyCodeId, notificationId);
            if (!surveyResult.success || surveyResult.code == null)
            {
                return ErrorResponse.BadRequest(message.ErrorCodeCreateFailed());
            }

            return Ok(surveyResult.code);
        }

        [HttpPut("anonymity"), Authorize(Permissions.Users.Write)]
        public async Task<ActionResult<List<ProjectPatient>>> UpdateAnonymity([FromRoute] string projectId,
            [FromBody] List<PatientAnonymityRequest> requests)
        {
            var message = GetMessage();
            var updates = new List<ProjectPatient>();
            var project = await _projectContext.Projects.Read(projectId);
            if (project == null)
            {
                return ErrorResponse.NotFound(message.ErrorNotFoundProject());
            }
            foreach (var request in requests)
            {
                var patientId = request.PatientId;
                var patient = await _patientContext.ReadPatient(projectId, patientId);
                if (patient == null)
                {
                    continue;
                }
                var anonymity = request.Anonymity;
                var update = await _patientContext.ProjectPatients.Update(projectId, patientId,
                    p => p.Anonymity = anonymity);
                if (patient.StrategyId != null)
                {
                   await _strategyContext.StrategyPatients.Update(patient.StrategyId, patientId, sp=>sp.Anonymity=anonymity);
                }
                updates.Add(update);
            }
            
            return Ok(updates);
        }

        [HttpPut("activation"), Authorize(Permissions.Users.Write)]
        public async Task<ActionResult<List<ProjectPatient>>> UpdateActivation([FromRoute] string projectId,
    [FromBody] List<PatientActivationRequest> requests)
        {
            var message = GetMessage();
            var updates = new List<ProjectPatient>();
            var project = await _projectContext.Projects.Read(projectId);
            if (project == null)
            {
                return ErrorResponse.NotFound(message.ErrorNotFoundProject());
            }
            foreach (var request in requests)
            {
                var patientId = request.PatientId;
                var patient = await _patientContext.ReadPatient(projectId, patientId);
                if (patient == null)
                {
                    continue;
                }
                var activation = request.IsActive;
                var update = await _patientContext.ProjectPatients.Update(projectId, patientId,
                    p => p.IsActive = activation);

                updates.Add(update);
            }

            return Ok(updates);
        }

        private async Task<List<FieldEntry>> GetEntryFieldsAll(string projectId, string patientId, DateTime? start,
            DateTime? end)
        {
            var results = new List<FieldEntry>();
            var batches = await _analytics.EntryBatches.ReadBetween(new SurveyAccess()
            {
                PatientId = patientId,
                ProjectId = projectId,
                SearchStart = start ?? DateTime.Now.AddYears(-3),
                SearchEnd = end ?? DateTime.Now.AddYears(1),
            });
            foreach (var batch in batches)
            {
                results.AddRange(await _analytics.FieldEntries.ReadAll(batch.Id));
            }
            return results;
        }

        private async Task<List<FieldEntry>> GetEntryFieldsBySurveyId(string projectId, string patientId,
            string surveyId, DateTime? start, DateTime? end)
        {
            var results = new List<FieldEntry>();
            var batches = await _analytics.EntryBatches.ReadBetween(new SurveyAccess()
            {
                ProjectId = projectId,
                SurveyId = surveyId,
                SearchStart = start ?? DateTime.Now.AddYears(-3),
                SearchEnd = end ?? DateTime.Now.AddYears(1),
            });
            foreach (var batch in batches.Where(b=>b.PatientId == patientId))
            {
                results.AddRange(await _analytics.FieldEntries.ReadAll(batch.Id));
            }
            return results;
        }

        private async Task<List<FieldEntry>> GetEntryFieldsByFieldId(string projectId, string patientId, string fieldId,
            DateTime? start, DateTime? end)
        {
            var fields = await GetEntryFieldsAll(projectId, patientId, start, end);
            return fields.Where(f => f.FieldId == fieldId).ToList();
        }

        private async Task<List<FieldEntry>> GetEntryFieldsByFieldsIdAndSurveyId(string projectId, string patientId,
            string surveyId, string fieldId, DateTime? start, DateTime? end)
        {
            var fields = await GetEntryFieldsBySurveyId(projectId, patientId, surveyId, start, end);
            return fields.Where(f => f.FieldId == fieldId).ToList();

        }

        private async Task<List<BarChartItem>> GetBarChartItem(IReadOnlyCollection<FieldEntry> fieldEntries,
            string fieldId)
        {
            var choices = await _strategyContext.FieldChoices.ReadAll(fieldId);
            return choices.Select(c =>
                new BarChartItem
                {
                    Id = c.Id,
                    Text = c.Text,
                    Value = fieldEntries.Count(f => f.Text == c.Text),
                    FieldEntries = fieldEntries.Where(f => f.Text == c.Text).ToList()
                }).ToList();
        }

        private async Task<List<BarChartItem>> GetBarChartItem(IReadOnlyCollection<FieldEntry> fieldEntries,
            IEnumerable<SurveyField> fields)
        {
            var bars = new List<BarChartItem>();
            foreach (var field in fields)
            {
                bars.AddRange(await GetBarChartItem(fieldEntries, field.Id));
            }

            return bars.ToList();
        }
    }
}