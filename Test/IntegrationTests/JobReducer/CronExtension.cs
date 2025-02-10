using System;
using System.Threading.Tasks;
using API.Handlers;
using API.Jobs;
using API.Lib;
using API.Repositories;
using Impactly.Test.Utils;
using Mapster;
using Microsoft.Extensions.DependencyInjection;
using Nest;
using TimeZoneConverter;
using Xunit;
using Xunit.Abstractions;

namespace Impactly.Test.IntegrationTests.JobReducer;

[Collection("Integration Test")]
public class CronExtension
{
    private readonly ITestOutputHelper _output;
    private readonly TestFixture _fixture;
    private readonly ICronContext _cronContext;
    private readonly ILogHandler _logHandler;
    private readonly IProjectContext _projectContext;
    private readonly IStrategyContext _strategy;
    private readonly ISurveyHandler _surveyHandler;
    private readonly IPatientContext _patientContext;
    
    public CronExtension(ITestOutputHelper output, TestFixture fixture)
    {
        _output = output;
        _fixture = fixture;
        _cronContext = _fixture.Server.Services.GetRequiredService<ICronContext>();
        _logHandler = _fixture.Server.Services.GetRequiredService<ILogHandler>();
        _projectContext = _fixture.Server.Services.GetRequiredService<IProjectContext>();
        _strategy = _fixture.Server.Services.GetRequiredService<IStrategyContext>();
        _surveyHandler = _fixture.Server.Services.GetRequiredService<ISurveyHandler>();
        _patientContext = _fixture.Server.Services.GetRequiredService<IPatientContext>();

    }

    [Fact]
    public async Task TestGetNextGeneration()
    {
        var cronExp = "0 12 1 */6 *";
        var surveyJob = new API.Models.Cron.SurveyJob
        {
            Type = "SurveyJob",
            CronExpression = "0 12 1 */6 *"
        };
        var next1 =
            CronExpressionTimes.Parse(cronExp)
                .GetNextOccurrenceISO8601DateFormat(DateTime.UtcNow);
        var nextExecutionUtc = DateTime.Parse(next1);
        var next = API.Extensions.JobReducer.GetNextExecutionTime(surveyJob, nextExecutionUtc);
        Assert.NotEmpty(next1);
        Assert.NotEmpty(next);
        
    }

}