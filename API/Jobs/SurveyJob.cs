using System;
using System.Threading.Tasks;
using Amazon.Util;
using API.Constants;
using API.Handlers;
using API.Lib;
using API.Models.Cron;
using API.Models.Strategy;
using API.Repositories;

namespace API.Jobs
{
    public interface ISurveyJob
    {
        Task<JobResult> ExecuteAsync(Models.Cron.SurveyJob job);
    }

    public class SurveyJob : ISurveyJob
    {
        private readonly IProjectContext _projectContext;
        private readonly IStrategyContext _strategy;
        private readonly ISurveyHandler _surveyHandler;
        private readonly IPatientContext _patientContext;
        private readonly ICronContext _cronContext;

        public SurveyJob(IStrategyContext strategy, ISurveyHandler surveyHandler, IProjectContext projectContext,
            IPatientContext patientContext, ICronContext cronContext)
        {
            _strategy = strategy;
            _surveyHandler = surveyHandler;
            _projectContext = projectContext;
            _patientContext = patientContext;
            _cronContext = cronContext;
        }

        public async Task<JobResult> ExecuteAsync(Models.Cron.SurveyJob surveyJob)
        {
            try
            {
                var patient = await _patientContext.ReadPatient(surveyJob.ProjectId, surveyJob.PatientId);
                if (patient is not { IsActive: true })
                {
                    return new JobResult { EmailSent = false };
                }

                var frequency = await _strategy.BatchFrequencies.Read(surveyJob.StrategyId, surveyJob.FrequencyId);

                var exCount = surveyJob.ExecutionCount + 1;
                if (frequency.End.Type == EndType.Occur &&
                    exCount >= frequency.End.Occurrences)
                {
                    surveyJob.Status = JobStatus.Completed;
                    return new JobResult { EmailSent = false };
                }

                var project = await _projectContext.Projects.Read(surveyJob.ProjectId);

                var surveyResult = await _surveyHandler.SendSurvey(
                    project.Id,
                    project.Name,
                    patient,
                    surveyJob.StrategyId,
                    frequency.Surveys,
                    frequency.Id
                );

                surveyJob.ExecutionCount += 1;

                return new JobResult
                {
                    EmailSent = surveyResult.success,
                    EmailId = surveyResult.code?.Id
                };
            }
            catch (Exception ex)
            {
                // You might want to log the exception here
                return new JobResult
                {
                    EmailSent = false,
                    EmailId = null
                };
            }
        }
    }
}