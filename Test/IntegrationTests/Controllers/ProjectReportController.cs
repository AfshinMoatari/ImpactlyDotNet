using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using API.Models.Reports;
using Impactly.Test.IntegrationTests.Client;
using Impactly.Test.Utils;
using Xunit;
using Xunit.Abstractions;

namespace Impactly.Test.IntegrationTests.Controllers;

[Collection("Integration Test")]
public class ProjectReportController
{
    private readonly ITestOutputHelper _output;
    private readonly TestFixture _fixture;
    private readonly string _path;

    public ProjectReportController(TestFixture testFixture, ITestOutputHelper testOutputHelper)
    {
        _fixture = testFixture;
        _output = testOutputHelper;
        _path = "/api/web/v1/projects/" + _fixture.InitProject.TestProject.Id + "/reports";
    }

    [Fact]
    public async Task TestCreate()
    {
        var request = new Report()
        {
            Id = Guid.NewGuid().ToString(),
            Name = "New Report",
            ModuleConfigs = new List<ReportModuleConfig>()
            {
                new()
                {
                    Name = "New Config",
                }
            },
        };
        await _fixture.Client
            .SignInAsProject(_fixture.InitProject.ProjectSuperUser, _fixture.InitProject.TestProject.Id)
            .ConfigureAwait(false);
        var response = await _fixture.Client.Fetch<Report>(HttpMethod.Post, _path, request).ConfigureAwait(false);
        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        _fixture.Client.SignOut();
    }

    [Fact]
    public async Task TestReadAll()
    {
        await _fixture.Client
            .SignInAsProject(_fixture.InitProject.ProjectStandardUser, _fixture.InitProject.TestProject.Id)
            .ConfigureAwait(false);
        var response = await _fixture.Client.Fetch<List<Report>>(HttpMethod.Get, _path).ConfigureAwait(false);
        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var reports = response.Value;
        Assert.NotNull(reports);
        Assert.Single(reports);
        _fixture.Client.SignOut();
    }

    [Fact]
    public async Task TestRead()
    {
        var path = _path + "/" + _fixture.InitProject.TestReport.Id;
        await _fixture.Client
            .SignInAsProject(_fixture.InitProject.ProjectStandardUser, _fixture.InitProject.TestProject.Id)
            .ConfigureAwait(false);
        var response = await _fixture.Client.Fetch<Report>(HttpMethod.Get, path).ConfigureAwait(false);
        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(response.Value);
        Assert.Equal(_fixture.InitProject.TestReport.Name, response.Value.Name);
        _fixture.Client.SignOut();
    }

    [Fact]
    public async Task TestUpdate()
    {
        var report = new Report()
        {
            Id = Guid.NewGuid().ToString(),
            Name = "Old Name",
        };
        report.FreeTexts ??= new List<FreeText>();
        report.FreeTexts.Add(new FreeText()
        {
            Id = Guid.NewGuid().ToString(),
            Title = "NewFreeText1",
            Contents = "New Free Text Content",
        });
        report.ModuleConfigs ??= new List<ReportModuleConfig>();
        report.ModuleConfigs.Add(new ReportModuleConfig()
        {
            Id = Guid.NewGuid().ToString(),
            FreeTextId = report.FreeTexts[0].Id,
            FreeTextTitle = report.FreeTexts[0].Title,
            FreeTextContents = report.FreeTexts[0].Contents,
        });
        await _fixture.ProjectContext.Reports.Create(_fixture.InitProject.TestProject.Id, report).ConfigureAwait(false);
        report.Name = "New Name";
        report.FreeTexts[0].Title = "New Title";
        await _fixture.Client
            .SignInAsProject(_fixture.InitProject.ProjectSuperUser, _fixture.InitProject.TestProject.Id)
            .ConfigureAwait(false);
        var path = _path + "/" + report.Id;
        var response = await _fixture.Client.Fetch<Report>(HttpMethod.Put, path, report).ConfigureAwait(false);
        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var reportInDb = await _fixture.ProjectContext.Reports
            .Read(_fixture.InitProject.TestProject.Id, report.Id).ConfigureAwait(false);
        Assert.NotNull(reportInDb);
        Assert.Equal("New Name", reportInDb.Name);
        Assert.Single(reportInDb.FreeTexts);
        Assert.Equal("New Title", reportInDb.FreeTexts[0].Title);
        Assert.NotNull(reportInDb.ModuleConfigs);
        Assert.Single(reportInDb.ModuleConfigs);

        await _fixture.ProjectContext.Reports.Delete(_fixture.InitProject.TestProject.Id, report.Id).ConfigureAwait(false);

        _fixture.Client.SignOut();
    }
}