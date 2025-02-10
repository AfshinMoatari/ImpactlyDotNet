using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using API.Constants;
using API.Models.Auth;
using API.Models.Dump;
using API.Models.Projects;
using API.Views;
using Impactly.Test.IntegrationTests.Client;
using Impactly.Test.Utils;
using Xunit;
using Xunit.Abstractions;

namespace Impactly.Test.IntegrationTests.Controllers;

[Collection("Integration Test")]
public class MeController
{
    private readonly ITestOutputHelper _output;
    private readonly TestFixture _fixture;
    private const string BaseUrl = "/api/web/v1/me";

    public MeController(ITestOutputHelper output, TestFixture fixture)
    {
        _fixture = fixture;
        _output = output;
    }

    [Fact]
    public async Task TestReadAllProjects()
    {
        const string path = BaseUrl + "/projects";
        await _fixture.Client.SignInAsProject(_fixture.InitAdmin.LoginAdmin, _fixture.InitProject.TestProject.Id)
            .ConfigureAwait(false);
        var response = await _fixture.Client.Fetch<List<ProjectUser>>(HttpMethod.Get, path).ConfigureAwait(false);
        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var projects = response.Value;
        Assert.NotNull(projects);
        Assert.NotEmpty(projects);
        _fixture.Client.SignOut();
    }

    [Fact]
    public async Task TestGetDumpOptions()
    {
        await _fixture.SetLanguageToProject(
                Languages.English, _fixture.InitProject.TestProject.Id)
            .ConfigureAwait(false);
        await _fixture.Client.SignInAsProject(_fixture.InitAdmin.LoginAdmin, _fixture.InitProject.TestProject.Id).ConfigureAwait(false);
        var path = BaseUrl + "/projects/" + _fixture.InitProject.TestProject.Id  + "/dump/options";
        var response = await _fixture.Client.Fetch<DumpRequestOptions>(HttpMethod.Get, path).ConfigureAwait(false);
        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var options = response.Value;
        Assert.NotNull(options);
        Assert.NotEmpty(options.Fields);
        Assert.NotEmpty(options.Filter);
        Assert.NotEmpty(options.OrderBy);
        Assert.NotEmpty(options.SortedBy);
        var sortedBys = options.SortedBy.Order().ToList();
        Assert.Equal(2, sortedBys.Count);
        Assert.Equal("Data Entries",  sortedBys[0]);
        _fixture.Client.SignOut();
        
        await _fixture.SetLanguageToProject(
                Languages.Danish, _fixture.InitProject.TestProject.Id)
            .ConfigureAwait(false);
        await _fixture.Client.SignInAsProject(_fixture.InitAdmin.LoginAdmin, _fixture.InitProject.TestProject.Id).ConfigureAwait(false);
        response = await _fixture.Client.Fetch<DumpRequestOptions>(HttpMethod.Get, path).ConfigureAwait(false);
        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        options = response.Value;
        Assert.NotNull(options);
        Assert.NotEmpty(options.Fields);
        Assert.NotEmpty(options.Filter);
        Assert.NotEmpty(options.OrderBy);
        Assert.NotEmpty(options.SortedBy);
        sortedBys = options.SortedBy.Order().ToList();
        Assert.Equal(2, sortedBys.Count);
        Assert.Equal("Datapunkter",  sortedBys[0]);
        _fixture.Client.SignOut();
    }


    public async Task TestDumpData()
    {
        var path = BaseUrl + "/projects/" + _fixture.InitProject.TestProject.Id + "/dump";
        await _fixture.Client.SignInAsProject(_fixture.InitAdmin.LoginAdmin, _fixture.InitProject.TestProject.Id)
            .ConfigureAwait(false);
        var request = new DumpRequest()
        {
            StartDate = DateTime.Now.AddDays(-1),
            EndDate = DateTime.Now.AddDays(1),
            Fields = new List<string>()
            {
                DumpFields.PatientFirstName,
                DumpFields.PatientLastName,
                DumpFields.QuestionNumber,
                DumpFields.Questions,
                DumpFields.Index,
                DumpFields.Answers,
                DumpFields.SurveyScore,
                DumpFields.Tags,
                DumpFields.Strategy,
                DumpFields.AnsweredAt,

            },
            SortedBy = DumpFields.DataEntries,
            Filter = DumpFields.FilterAll
        };
        var response = await _fixture.Client.Fetch<DataDumpEmail>(HttpMethod.Post, path, request).ConfigureAwait(false);
        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        
        _fixture.Client.SignOut();
    }


}