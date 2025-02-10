using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Amazon.DynamoDBv2.DataModel;
using API.Models.Cron;
using API.Models.Strategy;
using API.Operators;
using API.Repositories;

namespace API.Services
{
    public interface IJobsService
    {
        /// <summary>
        /// Create jobs per patients
        /// </summary>
        /// <returns>A newly created strategy.</returns>
        public Task CreateJobs(BatchSendoutFrequency sendoutFrequency, string projectId, string JobType);

        /// <summary>
        /// Get all the jobs for a strategyId
        /// </summary>
        /// <returns>A list of survey jobs for the strategy Id.</returns>
        public Task<List<SurveyJob>> ReadAllJobs(string strategyId);

        /// <summary>
        /// Remove survey jobs by frequencyId
        /// </summary>
        /// <returns></returns>
        public Task DeleteJobs(List<SurveyJob> jobs, string frequencyId);
    }

    /// <summary>
    /// Service class for managing jobs.
    /// </summary>
    /// <param name="cronContext">The _cronContext.</param>
    /// <param name="jobsOperatorContext">The _jobsOperatorContext.</param>
    public class JobsService : IJobsService
    {
        private readonly ICronContext _cronContext;
        private readonly IJobsOperatorContext _jobsOperatorContext;
        private readonly ILockContext _lockContext;

        /// <summary>
        /// Initializes a new instance of the <see cref="JobsService"/> class.
        /// </summary>
        /// <param name="cronContext">The _cronContext.</param>
        /// <param name="jobsOperatorContext">The _jobsOperatorContext.</param>
        /// <param name="lockContext">The _lockContext.</param>
        public JobsService(
            ICronContext cronContext,
            IJobsOperatorContext jobsOperatorContext,
            ILockContext lockContext
        )
        {
            _cronContext = cronContext;
            _jobsOperatorContext = jobsOperatorContext;
            _lockContext = lockContext;
        }

        public async Task CreateJobs(BatchSendoutFrequency sendoutFrequency, string projectId, string JobType)
        {
            try
            {
                const int BATCH_SIZE = 50;
                const int MINUTES_BETWEEN_BATCHES = 5;
                
                // Generate a unique instance ID for this batch of jobs
                string currentInstanceId = Guid.NewGuid().ToString();
                
                // Acquire a global lock for job creation to prevent multiple instances from creating the same jobs
                var lockId = $"create-jobs-{sendoutFrequency.Id}";
                if (!await _lockContext.AcquireLock(lockId, currentInstanceId, TimeSpan.FromMinutes(15)))
                {
                    throw new InvalidOperationException($"Another instance is currently creating jobs for sendout: {sendoutFrequency.Id}");
                }

                try
                {
                    var jobsBatch = _cronContext.SurveyJobs.Context.CreateBatchWrite<SurveyJob>();
                    var patients = sendoutFrequency.PatientsId.ToList();
                    bool hasPendingItems = false;
                    
                    for (int i = 0; i < patients.Count; i++)
                    {
                        int batchNumber = i / BATCH_SIZE;
                        DateTime baseExecutionTime = DateTime.UtcNow.AddMinutes(batchNumber * MINUTES_BETWEEN_BATCHES);
                        
                        // Create a unique identifier for this job that includes the batch number
                        string jobInstanceId = $"{sendoutFrequency.Id}-batch-{batchNumber}-{i}";
                        
                        var job = _jobsOperatorContext.CreateSurveyJob(
                            JobType, 
                            projectId, 
                            sendoutFrequency.ParentId, 
                            sendoutFrequency.Id, 
                            patients[i], 
                            "Queued", 
                            sendoutFrequency.CronExpression
                        );
                        
                        // Override the NextExecution time based on batch number
                        job.NextExecution = baseExecutionTime.ToString("yyyy-MM-ddTHH:mm:ss.fffZ");
                        job.InstanceId = jobInstanceId;
                        
                        jobsBatch.AddPutItem(_cronContext.SurveyJobs.ToDynamoItem(job));
                        hasPendingItems = true;

                        // Flush the batch every 25 items to prevent memory issues
                        if (i > 0 && i % 25 == 0)
                        {
                            await _cronContext.SurveyJobs.Context.ExecuteBatchWriteAsync(new BatchWrite[] { jobsBatch });
                            jobsBatch = _cronContext.SurveyJobs.Context.CreateBatchWrite<SurveyJob>();
                            hasPendingItems = false;
                        }
                    }

                    // Write any remaining items
                    if (hasPendingItems)
                    {
                        await _cronContext.SurveyJobs.Context.ExecuteBatchWriteAsync(new BatchWrite[] { jobsBatch });
                    }
                }
                finally
                {
                    // Release the lock with the instance ID
                    await _lockContext.ReleaseLock(lockId, currentInstanceId);
                }
            }
            catch (Exception e)
            {
                throw new ArgumentException($"Could not create jobs for the sendout: {sendoutFrequency.Id}", e);
            }
        }

        public async Task<List<SurveyJob>> ReadAllJobs(string strategyId)
        {
            try
            {
                return await _cronContext.SurveyJobs.ReadAll(
                    new SurveyJobAccess
                    {
                        StrategyId = strategyId
                    });
            }
            catch (Exception e)
            {
                throw new ArgumentException($"Could not get all the jobs for strategyId: {strategyId}", e);
            }
        }

        public async Task DeleteJobs(List<SurveyJob> jobs, string frequencyId)
        {
            try
            {
                var jobsBatch = _cronContext.SurveyJobs.Context.CreateBatchWrite<SurveyJob>();

                foreach (var job in jobs.Where(x => x.FrequencyId == frequencyId))
                {
                    jobsBatch.AddDeleteItem(job);
                }

                await _cronContext.SurveyJobs.Context.ExecuteBatchWriteAsync(new BatchWrite[]
                {
                    jobsBatch
                });
            }
            catch (Exception e)
            {
                throw new ArgumentException($"Could not delete the jobs for the frequencyId : {frequencyId}", e);
            }
        }
    }
}