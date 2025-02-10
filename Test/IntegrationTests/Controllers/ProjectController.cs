using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using API.Constants;
using API.Models.Projects;
using Impactly.Test.IntegrationTests.Client;
using Impactly.Test.IntegrationTests.TestData;
using Impactly.Test.Utils;
using Xunit;
using Xunit.Abstractions;

namespace Impactly.Test.IntegrationTests.Controllers;

[Collection("Integration Test")]
public class ProjectController
{
    private readonly ITestOutputHelper _output;
    private readonly TestFixture _fixture;
    private readonly string _baseUrl = "/api/web/v1/projects";

    public ProjectController(TestFixture testFixture, ITestOutputHelper output)
    {
        _fixture = testFixture;
        _output = output;
    }

    [Fact]
    public async Task TestCreate()
    {
        await _fixture.Client.SignInAsAdmin(_fixture.InitAdmin.LoginAdmin).ConfigureAwait(false);
        
        var toBeCreated = new Project()
        {
            Id = Guid.NewGuid().ToString(),
            Name = "Test New Create",
        };
        var response = await _fixture.Client.Fetch<Project>(HttpMethod.Post, _baseUrl, toBeCreated)
            .ConfigureAwait(false);
        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(response.Value);
        Assert.Equal(toBeCreated.Id, response.Value.Id);
        Assert.Equal(toBeCreated.Name, response.Value.Name);
        var projectUser = await _fixture.ProjectContext.ReadProjectUser(toBeCreated.Id, _fixture.InitAdmin.UserAdmin.Id)
            .ConfigureAwait(false);
        Assert.NotNull(projectUser);
        Assert.Equal(projectUser.Email, _fixture.InitAdmin.UserAdmin.Email);
        
        _fixture.Client.SignOut();
    }
    
    [Fact]
    public async Task TestUpdate()
    {
        await _fixture.Client.SignInAsAdmin(_fixture.InitAdmin.LoginAdmin).ConfigureAwait(false);
        
        var toBeUpdated = new Project()
        {
            Id = Guid.NewGuid().ToString(),
            Name = "Test New Create 2",
        };
        var response = await _fixture.Client.Fetch<Project>(HttpMethod.Post, _baseUrl, toBeUpdated)
            .ConfigureAwait(false);
        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        toBeUpdated.Name = "Test Changed";
        response = await _fixture.Client.Fetch<Project>(HttpMethod.Put, _baseUrl + "/" + toBeUpdated.Id, toBeUpdated)
            .ConfigureAwait(false);
        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(response.Value);
        Assert.Equal(toBeUpdated.Id, response.Value.Id);
        Assert.Equal("Test Changed", response.Value.Name);
        var projectUser = await _fixture.ProjectContext.ReadProjectUser(toBeUpdated.Id, _fixture.InitAdmin.UserAdmin.Id)
            .ConfigureAwait(false);
        Assert.NotNull(projectUser);
        Assert.Equal(projectUser.Email, _fixture.InitAdmin.UserAdmin.Email);
        
        _fixture.Client.SignOut();
    }

    [Fact]
    public async Task TestDelete()
    {
        await _fixture.Client.SignInAsAdmin(_fixture.InitAdmin.LoginAdmin).ConfigureAwait(false);
        
        var toBeDeleted = new Project()
        {
            Id = Guid.NewGuid().ToString(),
            Name = "Test New Create 3",
        };
        var response = await _fixture.Client.Fetch<Project>(HttpMethod.Post, _baseUrl, toBeDeleted)
            .ConfigureAwait(false);
        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        response = await _fixture.Client.Fetch<Project>(HttpMethod.Delete, _baseUrl + "/" + toBeDeleted.Id)
            .ConfigureAwait(false);
        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(response.Value);
        Assert.Equal(toBeDeleted.Id, response.Value.Id);

        var projectUser = await _fixture.ProjectContext.ReadProjectUser(toBeDeleted.Id, _fixture.InitAdmin.UserAdmin.Id)
            .ConfigureAwait(false);
        Assert.Null(projectUser);
        
        _fixture.Client.SignOut();
    }

    [Fact]
    public async Task TestRead()
    {
        await _fixture.Client.SignInAsProject(_fixture.InitAdmin.LoginAdmin, _fixture.InitProject
            .TestProject
            .Id).ConfigureAwait(false);

        var response = await _fixture.Client.Fetch<Project>(HttpMethod.Get, _baseUrl + "/" + _fixture.InitProject.TestProject.Id)
            .ConfigureAwait(false);
        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(response.Value);
        Assert.Equal(_fixture.InitProject.TestProject.Id, response.Value.Id);

        var expected = await _fixture.ProjectContext.Projects.Read(_fixture.InitProject.TestProject.Id).ConfigureAwait(false);
        Assert.NotNull(expected);
        Assert.Equal(expected.Id, response.Value.Id);
        Assert.Equal(expected.Name, response.Value.Name);
        
        _fixture.Client.SignOut();
    }


    [Fact]
    public async Task TestReadAll()
    {
        await _fixture.Client.SignInAsAdmin(_fixture.InitAdmin.LoginAdmin).ConfigureAwait(false);
        
        var response = await _fixture.Client.Fetch<List<Project>>(HttpMethod.Get, _baseUrl)
            .ConfigureAwait(false);
        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var projects = response.Value;
        Assert.NotNull(projects);
        Assert.True(projects.Count > 0);
        var expected = await _fixture.ProjectContext.Projects.ReadAll().ConfigureAwait(false);
        Assert.NotNull(expected);
        Assert.Equal(expected.Count(), projects.Count);
        
        _fixture.Client.SignOut();
        
    }

    [Fact]
    public async Task TestGetCommunication()
    {
        await _fixture.Client.SignInAsProject(_fixture.InitAdmin.LoginAdmin, _fixture.InitProject.TestProject.Id)
            .ConfigureAwait(false);
        var response = await _fixture.Client.Fetch<List<ProjectCommunication>>(HttpMethod.Get, _baseUrl + "/" + _fixture.InitProject.TestProject.Id + "/communication").ConfigureAwait(false);
        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var communications = response.Value;
        Assert.NotNull(communications);
        Assert.Equal(2, communications.Count);
        var expected = await _fixture.ProjectContext.Communicaitons.ReadAll(_fixture.InitProject.TestProject.Id)
            .ConfigureAwait(false);
        Assert.NotNull(expected);
        Assert.Equal(2, expected.Count());
        foreach (var comm in communications)
        {
            switch (comm.MessageType)
            {
                case ProjectCommunication.CommunicationTypeWelcome:
                    Assert.Equal("Test Welcome Communication", comm.MessageContent);
                    break;
                case ProjectCommunication.CommunicationTypeSurvey:
                    Assert.Equal("Test Survey Communication", comm.MessageContent);
                    break;
            }
        }
        _fixture.Client.SignOut();
    }
    
    [Fact]
    public async Task TestUpdateCommunication()
    {
        var newProject = new ProjectData("project_UpdateComm_", _fixture.InitAdmin.UserAdmin);
        await _fixture.CreateProjectData(newProject).ConfigureAwait(false);
        await _fixture.Client.SignInAsProject(_fixture.InitAdmin.LoginAdmin, newProject.TestProject.Id)
            .ConfigureAwait(false);
        var update = new ProjectCommunicationRequest()
        {
            SurveyMessage = "New Survey Message",
            WelcomeMessage = "New Welcome Message"
        };
        var response = await _fixture.Client.Fetch<Project>(HttpMethod.Post, _baseUrl + "/" + newProject.TestProject.Id + "/communication", update).ConfigureAwait(false);
        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var project = response.Value;
        Assert.NotNull(project);
        var expected = await _fixture.ProjectContext.Communicaitons.ReadAll(newProject.TestProject.Id)
            .ConfigureAwait(false);
        Assert.NotNull(expected);
        var projectCommunications = expected.ToList();
        Assert.Equal(2, projectCommunications.Count());
        foreach (var comm in projectCommunications)
        {
            switch (comm.MessageType)
            {
                case ProjectCommunication.CommunicationTypeWelcome:
                    Assert.Equal(update.WelcomeMessage, comm.MessageContent);
                    break;
                case ProjectCommunication.CommunicationTypeSurvey:
                    Assert.Equal(update.SurveyMessage, comm.MessageContent);
                    break;
            }
        }
        _fixture.Client.SignOut();
    }
}

