using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Amazon.Util;
using API.Constants;
using API.Handlers;
using API.Jobs;
using API.Lib;
using API.Models.Analytics;
using API.Models.Cron;
using API.Models.Logs;
using API.Repositories;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using static System.Globalization.DateTimeStyles;
using SurveyJob = API.Models.Cron.SurveyJob;

namespace API.Extensions
{
    public class JobRunner : IHostedService, IDisposable
    {
        private readonly IServiceScope _scope;

        private readonly IJobReducer _jobReducer;



        private Timer _timer;

        public JobRunner(IServiceScopeFactory scopeFactory)
        {
            _scope = scopeFactory.CreateScope();
            _jobReducer = _scope.ServiceProvider.GetService<IJobReducer>();
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _timer = new Timer(async (state) =>
                await RunJobAsync(state), null, TimeSpan.FromMinutes(0), TimeSpan.FromMinutes(1));

            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _timer?.Change(Timeout.Infinite, 0);
            return Task.CompletedTask;
        }

        public void Dispose()
        {
            _timer?.Dispose();
            _scope?.Dispose();
        }

        private async Task RunJobAsync(object state)
        {
            await _jobReducer.ExecuteAsync();
        }
    }

    public interface IJobReducer
    {
        Task ExecuteAsync();
    }

    public class JobReducer : IJobReducer
    {
        private readonly ICronContext _cronContext;
        private readonly ISurveyJob _surveyJob;
        private readonly ILogHandler _logHandler;

        private static readonly SemaphoreLocker Locker = new SemaphoreLocker();
        private const int SearchStartOffsetHours = -24;
        private const int SearchEndOffsetHours = 1;

        public JobReducer(ICronContext cronContext, ISurveyJob surveyJob, ILogHandler logHandler)
        {
            _cronContext = cronContext;
            _surveyJob = surveyJob;
            _logHandler = logHandler;
        }

        public async Task ExecuteAsync()
        {

            var now = DateTime.UtcNow;
            try
            {
                await Locker.LockAsync(async () =>
                {
                    var jobs =
                        await _cronContext.SurveyJobs.ReadBetween(
                            new RangeFilter
                            {
                                SearchStart = now.AddHours(SearchStartOffsetHours),
                                SearchEnd = now.AddHours(SearchEndOffsetHours)
                            }
                        );
                    if (jobs.Count == 0) return;

                    foreach (var job in jobs)
                    {
                        try
                        {
                            switch (job.Status)
                            {
                                case JobStatus.Error:
                                case JobStatus.InProgress:
                                case JobStatus.Completed:
                                    continue;
                            }

                            //var nextExecutionUtc = DateTime.Parse(job.NextExecution).ToUniversalTime();
                            DateTime nextExc;
                            string[] formats = { "yyyy-MM-ddTHH:mm:ss.fffZ", "yyyy-MM-ddTHH.mm.ss.fffZ" };
                            if (job.NextExecution == null || !DateTime.TryParseExact(job.NextExecution, formats,
                                    CultureInfo.InvariantCulture, None, out nextExc))
                            {
                                throw new Exception("Datetime parsing error: " + job.NextExecution);
                            }

                            var nextExecutionUtc = nextExc.ToUniversalTime();
                            job.NextExecution = GetNextExecutionTime(job, nextExecutionUtc);

                            if (now.ToUniversalTime() < nextExecutionUtc) continue;

                            await SetAsInProgress(job);
                            await _surveyJob.ExecuteAsync(job);
                            await SetJobDone(job);
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine(e);

                            await SetJobError(job);
                            await SetExceptionLogs(job, e);
                            //throw;
                        }
                    }

                });
            }
            catch (Exception e)
            {
                // TODO
                Console.WriteLine(e);
                throw;
            }
        }

        public static string GetNextExecutionTime(SurveyJob job, DateTime nextExecutionUtc)
        {
            if (job.Type is JobType.Immediate)
            {
                return nextExecutionUtc.AddYears(70).ToString(Languages.ISO8601DateFormat);
            }

            nextExecutionUtc = TimeZoneInfo.ConvertTimeToUtc(nextExecutionUtc);
            return CronExpressionTimes
                .Parse(job.CronExpression)
                .GetNextOccurrence(nextExecutionUtc.AddMinutes(1))?.DateTime
                .ToString(Languages.ISO8601DateFormat);
        }

        private async Task SetAsInProgress(SurveyJob job)
        {
            job.Status = JobStatus.InProgress;
            await _cronContext.SurveyJobs.UpdateValue(job.Id, j => j.Status = job.Status);
        }

        private async Task SetJobDone(SurveyJob job)
        {
            if (job.Type is JobType.Immediate)
            {
                job.Status = JobStatus.Completed;
                job.ExecutionCount = 0;
            }
            else if (job.Status is not JobStatus.Completed)
            {
                job.Status = JobStatus.Queued;
            }
            await _cronContext.SurveyJobs.UpdateValue(job.Id, j =>
            {
                j.NextExecution = job.NextExecution;
                j.ExecutionCount = job.ExecutionCount;
                j.Status = job.Status;
            });
        }

        private async Task SetJobError(SurveyJob job)
        {
            job.Status = JobStatus.Error;
            await _cronContext.SurveyJobs.UpdateValue(job.Id, j =>
            {
                j.NextExecution = job.NextExecution;
                j.Status = job.Status;
            });
        }

        private async Task SetExceptionLogs(SurveyJob job, Exception exception)
        {
            var body = $"<b>TimeStamp</b>: {DateTime.Now}<br/>" +
                                   $"<b>Environment</b>: {EnvironmentMode.Environment}<br/>" +
                                   $"<b>Version</b>: {EnvironmentMode.Version}<br/>" +
                                   $"<b>ExceptionMessage</b>: {exception.Message}<br/>" +
                                   $"<b>StackTrace</b>:<br/>{exception.StackTrace}<br/>".Replace("\n", "<br/>");
            var log = new Log()
            {
                Type = Log.LogtypeException,
                ParentId = Log.LogtypeException,
                Body = body,
                Subject = exception.Message,
                Status = HttpStatusCode.InternalServerError.ToString(),
                ProjectId = job.ProjectId,
            };
            await _logHandler.AddLog(log);
        }
    }

    public static class CronExtension
    {
        public static IServiceCollection AddRecurring(this IServiceCollection services)
        {
            // TODO HOW TO TEST
            if (EnvironmentMode.IsTest) return services;

            services.AddScoped<ISurveyJob, Jobs.SurveyJob>();
            services.AddScoped<IJobReducer, JobReducer>();
            services.AddHostedService<JobRunner>();

            return services;
        }
    }
}