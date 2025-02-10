using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using API.Models.Notifications;
using Impactly.Test.IntegrationTests.Client;
using Impactly.Test.IntegrationTests.TestData;
using Impactly.Test.Utils;
using Microsoft.VisualBasic.CompilerServices;
using Xunit;
using Xunit.Abstractions;

namespace Impactly.Test.IntegrationTests.Controllers;

[Collection("Integration Test")]
public class NotificationController
{
    private readonly ITestOutputHelper _output;
    private readonly TestFixture _fixture;

    public NotificationController(ITestOutputHelper output, TestFixture testFixture)
    {
        _fixture = testFixture;
        _output = output;
    }

    [Fact]
    public async Task TestGetNotifications()
    {
        var path = "/api/web/v1/projects/" + _fixture.InitProject.TestProject.Id + "/notifications";
        await _fixture.Client.SignInAsProject(_fixture.InitAdmin.LoginAdmin, _fixture.InitProject.TestProject.Id)
            .ConfigureAwait(false);
        var response = await _fixture.Client.Fetch<List<Notification>>(HttpMethod.Get, path).ConfigureAwait(false);
        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var notifications = response.Value;
        Assert.NotEmpty(notifications);
        Assert.Equal("project " + _fixture.InitProject.TestProject.Id, notifications[0].Message);
        Assert.Equal(_fixture.InitProject.TestStrategy.Id, notifications[0].StrategyId);
        _fixture.Client.SignOut();
    }

    [Fact]
    public async Task TestSaveNotification()
    {
        await _fixture.Client.SignInAsAdmin(_fixture.InitAdmin.LoginAdmin).ConfigureAwait(false);
        var newProject = new TestData.ProjectData("notification", _fixture.InitAdmin.UserAdmin);
        await _fixture.CreateProjectData(newProject).ConfigureAwait(false);
        await _fixture.Client.SignInAsProject(_fixture.InitAdmin.LoginAdmin, newProject.TestProject.Id).ConfigureAwait(false);
        var path =  "/api/web/v1/projects/" + newProject.TestProject.Id + "/notifications/notification";
        var newNotification = new Notification()
        {
            Id = Guid.NewGuid().ToString(),
            NotificationType = NotificationType.Survey,
            ProjectId = newProject.TestProject.Id,
            Message = "project " + newProject.TestProject.Id,
            SurveyCode = newProject.TestSurveyCode.Id,
        };
        var response = await _fixture.Client.Fetch<bool>(HttpMethod.Post, path, newNotification).ConfigureAwait(false);
        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(response.Value);
        Assert.True(response.Value);
        var savedNotification = await _fixture.NotificationContext.Notifications.Read(newNotification.Id).ConfigureAwait(false);
        Assert.NotNull(savedNotification);
        Assert.Equal(newProject.TestSurveyCode.Id,  savedNotification.SurveyCode);
        _fixture.Client.SignOut();
    }

    [Fact]
    public async Task TestDeleteNotification()
    {
        await _fixture.Client.SignInAsAdmin(_fixture.InitAdmin.LoginAdmin).ConfigureAwait(false);
        var newProject = new TestData.ProjectData("notification2", _fixture.InitAdmin.UserAdmin);
        await _fixture.CreateProjectData(newProject).ConfigureAwait(false);
        await _fixture.Client.SignInAsProject(_fixture.InitAdmin.LoginAdmin, newProject.TestProject.Id).ConfigureAwait(false);
        var path =  "/api/web/v1/projects/" + newProject.TestProject.Id + "/notifications/" + newProject.TestNotification.Id;
        
        var response = await _fixture.Client.Fetch<bool>(HttpMethod.Delete, path).ConfigureAwait(false);
        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(response.Value);
        Assert.True(response.Value);
        var deleted = await _fixture.NotificationContext.Notifications.Read(newProject.TestNotification.Id).ConfigureAwait(false);
        Assert.Null(deleted);
        _fixture.Client.SignOut();
    }
}