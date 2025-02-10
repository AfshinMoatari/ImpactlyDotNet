using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using API.Handlers;
using API.Models.Admin;
using API.Models.Analytics;
using API.Models.Auth;
using API.Models.Strategy;
using API.Repositories;
using Impactly.Test.IntegrationTests.Client;
using Impactly.Test.IntegrationTests.TestData;
using Impactly.Test.Utils;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Office.Interop.Excel;
using Nest;
using Xunit;
using Xunit.Abstractions;
using SurveyField = API.Models.Strategy.SurveyField;

namespace Impactly.Test.IntegrationTests.Controllers;

[Collection("Integration Test")]
public class AdminController
{
    private readonly ITestOutputHelper _output;
    private readonly TestFixture _fixture;
    private const string BaseUrl = "/api/web/v1/admins";
    private readonly IUserContext _userContext;
    private readonly IAuthHandler _authHandler;
    private readonly IAdminContext _adminContext;

    public AdminController(ITestOutputHelper output, TestFixture fixture)
    {
        _output = output;
        _fixture = fixture;
        _userContext = _fixture.Server.Services.GetRequiredService<IUserContext>();
        _authHandler = _fixture.Server.Services.GetRequiredService<IAuthHandler>(); 
        _adminContext = _fixture.Server.Services.GetRequiredService<IAdminContext>();
    }

    [Fact]
    public async Task TestCreate()
    {
        
        await _fixture.Client.SignInAsAdmin(_fixture.InitAdmin.LoginAdmin).ConfigureAwait(false);
        var userId = Guid.NewGuid().ToString();
        var newUser = new AuthUser()
        {
            FirstName = "One",
            LastName = "Test",
            Email = "han+10@impactly.dk",
            Id = userId,
            PasswordHashB64 = _authHandler.HashUserPassword(userId, "1234"),
        };
        var theUser = await _userContext.Users.ReadOrCreate(newUser).ConfigureAwait(false);
        Assert.NotNull(theUser);
        Assert.Equal(newUser.Id, theUser.Id);
        var response = await _fixture.Client.Post<OverviewUser>(BaseUrl + "/" + newUser.Id, null).ConfigureAwait(false);
        Assert.NotNull(response);
        Assert.Equal(theUser.Id, response.Id);
        var existingAdmin = await _adminContext.Admins.Read(theUser.Id).ConfigureAwait(false);
        Assert.NotNull(existingAdmin);
        
        //clean up:
        await _userContext.Users.Delete(theUser.Id).ConfigureAwait(false);
        await _adminContext.Admins.Delete(existingAdmin.Id).ConfigureAwait(false);
        _fixture.Client.SignOut();
    }


    [Fact]
    public async Task TestGetUsers()
    {
        await _fixture.Client.SignInAsAdmin(_fixture.InitAdmin.LoginAdmin).ConfigureAwait(false);

        var userId = Guid.NewGuid().ToString();
        var newUser = new AuthUser()
        {
            FirstName = "Two",
            LastName = "Test",
            Email = "han+11@impactly.dk",
            Id = userId,
            PasswordHashB64 = _authHandler.HashUserPassword(userId, "1234")
        };
        var theUser = await _userContext.Users.ReadOrCreate(newUser).ConfigureAwait(false);
        var theAdmin = await _adminContext.Admins.Create(AdminUser.FromAuthUser(theUser)).ConfigureAwait(false);
        Assert.NotNull(theUser);
        Assert.Equal(newUser.Id, theUser.Id);
        var users = await _fixture.Client.Fetch<List<OverviewUser>>(HttpMethod.Get, BaseUrl + "/users").ConfigureAwait(false);
        Assert.NotNull(users);
        Assert.Equal(HttpStatusCode.OK, users.StatusCode);
        var expectedUser = users.Value.Where(f => f.Id == newUser.Id).ToList();
        Assert.NotNull(expectedUser);
        Assert.Equal(1, expectedUser.Count);
        Assert.NotNull(expectedUser[0]);
        Assert.Equal(newUser.Email, expectedUser[0].Email);
        Assert.Equal(newUser.LastName, expectedUser[0].LastName);
        
        //clean up:
        await _userContext.Users.Delete(theUser.Id).ConfigureAwait(false);
        await _adminContext.Admins.Delete(theAdmin.Id).ConfigureAwait(false);
        _fixture.Client.SignOut();
    }

    [Fact]
    public async Task TestGetSurveys()
    {
        await _fixture.Client.SignInAsAdmin(_fixture.InitAdmin.LoginAdmin).ConfigureAwait(false);
        var response = await _fixture.Client.Fetch<List<Survey>>(HttpMethod.Get, BaseUrl + "/projects/" + _fixture.InitProject.TestProject.Id + "/surveys").ConfigureAwait(false);
        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var surveys = response.Value;
        Assert.NotNull(surveys);
        Assert.Equal(1, surveys.Count);
        Assert.Equal(_fixture.InitProject.TestCustomSurvey.Name, surveys[0].Name);
        Assert.Equal(_fixture.InitProject.TestCustomSurvey.Fields.Count(), surveys[0].Fields.Count());
        _fixture.Client.SignOut();
    }

    [Fact]
    public async Task TestCreateSurvey()
    {
        await _fixture.Client.SignInAsAdmin(_fixture.InitAdmin.LoginAdmin).ConfigureAwait(false);
        var surveyId = Guid.NewGuid().ToString();
        var field1Id = Guid.NewGuid().ToString();
        var choice1Id = Guid.NewGuid().ToString();
        var choice2Id = Guid.NewGuid().ToString();
        var newSurvey = new Survey()
        {
            Id = surveyId,
            Name =  "Test Survey",
            Fields = new List<SurveyField>()
            {
                new ()
                {
                    Id = field1Id,
                    Index = 1,
                    Text = "Field 1",
                    Choices = new List<FieldChoice>()
                    {
                        new ()
                        {
                            Id = choice1Id,
                            Index = 1,
                            Text = "Choice 1",
                        },
                        new ()
                        {
                            Id = choice2Id,
                            Index = 1,
                            Text = "Choice 2",
                        }
                        
                    }
                }
            }
        };
        var response = await _fixture.Client.Post<Survey>(BaseUrl + "/projects/" + _fixture.InitProject.TestProject.Id + "/surveys", newSurvey).ConfigureAwait(false);
        Assert.NotNull(response);
        Assert.Equal(newSurvey.Name, response.Name);
        Assert.Equal(newSurvey.Id,response.Id);
        Assert.Equal(_fixture.InitProject.TestProject.Id,response.ParentId);
        var inserted = await _fixture.StrategyContext.ReadSurvey(_fixture.InitProject.TestProject.Id, newSurvey.Id)
            .ConfigureAwait(false);
        Assert.NotNull(inserted);
        Assert.Equal(newSurvey.Name, inserted.Name);
        Assert.NotNull(inserted.Fields);
        Assert.Equal(1, inserted.Fields.Count()); 
        
        //clean up:
        await _fixture.StrategyContext.Surveys.Delete(_fixture.InitProject.TestProject.Id, newSurvey.Id)
            .ConfigureAwait(false);
        _fixture.Client.SignOut();
    }

    [Fact]
    public async Task TestUpdateSurvey()
    {
        await _fixture.Client.SignInAsAdmin(_fixture.InitAdmin.LoginAdmin).ConfigureAwait(false);
        var newProject = new ProjectData("TestUpdateSurvey", _fixture.InitAdmin.UserAdmin);
        await _fixture.CreateProjectData(newProject).ConfigureAwait(false);
        var newSurvey = newProject.TestCustomSurvey;
        newSurvey.LongName = "changed long name";
        newSurvey.Name = "changed name";
        newSurvey.Fields.ToList()[0].Text = "changed question name";
        newSurvey.Fields.ToList()[0].Choices.ToList()[0].Text = "changed choice name";
        newSurvey.Fields.ToList().Add(
            new SurveyField
            {
                Id  = Guid.NewGuid().ToString(),
                Text = "new add field",
                Index = 2,
                Choices = new List<FieldChoice>()
                {
                    new ()
                    {
                        Id = Guid.NewGuid().ToString(),
                        Text = "new add choice",
                        Index = 1,
                    }
                }
            }
            );
        var response = await _fixture.Client.Fetch<Survey>(HttpMethod.Put,
            BaseUrl + "/projects/" + newProject.TestProject.Id + "/surveys", newSurvey).ConfigureAwait(false);
        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var readSurvey = await _fixture.StrategyContext.ReadSurvey(newProject.TestProject.Id, newSurvey.Id)
            .ConfigureAwait(false);
        Assert.NotNull(readSurvey);
        Assert.Equal(newSurvey.Name, readSurvey.Name);
        Assert.Equal(newSurvey.LongName, readSurvey.LongName);
        var fieldEntries = await _fixture.AnalyticsContext.FieldEntries
            .ReadByPatient(newProject.TestProject.Id, newProject.TestProjectPatient.Id)
            .ConfigureAwait(false);
        Assert.NotNull(fieldEntries);
        Assert.Equal(2, fieldEntries.Count);
        var fieldEntry =  fieldEntries.FirstOrDefault(f=>f.FieldId == readSurvey.Fields.ToList()[0].Id);
        Assert.NotNull(fieldEntry);
        Assert.Equal(fieldEntry.FieldId, newSurvey.Fields.ToList()[0].Id);
        Assert.Equal(newSurvey.Fields.ToList()[0].Choices.ToList()[0].Text, fieldEntry.ChoiceText);
        Assert.Equal(newSurvey.Fields.ToList()[0].Text, fieldEntry.FieldText);
        var readStrategy = await _fixture.StrategyContext
            .ReadStrategy(newProject.TestProject.Id, newProject.TestStrategy.Id).ConfigureAwait(false);
        Assert.NotNull(readStrategy);
        var readStrategySurvey = readStrategy.Surveys[0];
        Assert.NotNull(readStrategySurvey);
        Assert.Equal(newSurvey.Name, readStrategySurvey.Name);
        var readReport = await _fixture.ProjectContext.Reports
            .Read(newProject.TestProject.Id, newProject.TestReport.Id).ConfigureAwait(false);
        Assert.NotNull(readReport);
        var readModuleConfig =
            readReport.ModuleConfigs.FirstOrDefault(f =>
                f.Id == newProject.TestReport.ModuleConfigs.ToList()[0].Id);
        Assert.NotNull(readModuleConfig);
        Assert.Equal(newSurvey.Fields.ToList()[0].Text, readModuleConfig.Name);
        
        _fixture.Client.SignOut();
    }
}