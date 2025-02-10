using System;
using System.Threading.Tasks;
using API.Lib;
using API.Models.Strategy;
using API.Repositories;

namespace API.Services
{
    public interface ISendoutService
    {
        /// <summary>
        /// Create a strategy sendout records.
        /// </summary>
        /// <returns></returns>
        public Task<BatchSendoutFrequency> CreateSendouts(string projectId, string jobType, BatchSendoutFrequency batchSendoutFrequency);
        /// <summary>
        /// Create a strategy sendout records.
        /// </summary>
        /// <returns>ANewly created strategy sendout.</returns>
        public Task<BatchSendoutFrequency> CreateStrategyFrequency(BatchSendoutFrequency batchSendoutFrequency, string jobType);
    }

    /// <summary>
    /// Service class for managing sendouts.
    /// </summary>
    public class SendoutService : ISendoutService
    {
        private readonly IJobsService _jobsService;
        private readonly IStrategyContext _strategyContext;

        /// <summary>
        /// Initializes a new instance of the <see cref="SendoutService"/> class.
        /// </summary>
        /// <param name="jobsService">The _jobsService.</param>
        /// <param name="strategyContext">The _strategyContext.</param>
        public SendoutService(
            IJobsService jobsService, 
            IStrategyContext strategyContext)
        {
            _jobsService = jobsService;
            _strategyContext = strategyContext;
        }

        public async Task<BatchSendoutFrequency> CreateSendouts(string projectId, string jobType, BatchSendoutFrequency batchSendoutFrequency)
        {
            var batchStrategyFreq = await CreateStrategyFrequency(batchSendoutFrequency, jobType);
            await _jobsService.CreateJobs(batchStrategyFreq, projectId, jobType);
            return batchSendoutFrequency;
        }

        public async Task<BatchSendoutFrequency> CreateStrategyFrequency(BatchSendoutFrequency batchSendoutFrequency, string jobType)
        {
            try
            {
                if (!jobType.Equals("Immediate"))
                {
                    try
                    {
                        CronExpressionTimes.Parse(batchSendoutFrequency.CronExpression);
                    }
                    catch (Exception e)
                    {
                        throw new ArgumentException($"Could not parse the CronExpression: {batchSendoutFrequency.CronExpression}", e);
                    }
                }

                return await _strategyContext.CreateBatchSendout(batchSendoutFrequency);
            }
            catch (ArgumentException e)
            {
                throw new ArgumentException($"Could not create a strategy sendout record for sendout: {batchSendoutFrequency.Id}", e);
            }
        }
    }
}