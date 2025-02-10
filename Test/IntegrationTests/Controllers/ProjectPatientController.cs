using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Amazon.DynamoDBv2.Model;
using API.Constants;
using API.Models.Analytics;
using API.Models.Codes;
using API.Models.Projects;
using API.Models.Strategy;
using Impactly.Test.IntegrationTests.Client;
using Impactly.Test.IntegrationTests.TestData;
using Impactly.Test.Utils;
using Xunit;
using Xunit.Abstractions;
using Enumerable = System.Linq.Enumerable;

namespace Impactly.Test.IntegrationTests.Controllers;

[Collection("Integration Test")]
public class ProjectPatientController
{
    private readonly ITestOutputHelper _output;
    private readonly TestFixture _fixture;
    private readonly string _path;

    public ProjectPatientController(TestFixture fixture, ITestOutputHelper output)
    {
        _fixture = fixture;
        _output = output;
        _path = "/api/web/v1/projects/" + _fixture.InitProject.TestProject.Id + "/patients";
    }

    [Fact]
    public async Task TestCreate()
    {
        await _fixture.Client.SignInAsProject(_fixture.InitAdmin.LoginAdmin, _fixture.InitProject.TestProject.Id)
            .ConfigureAwait(false);


        var newPatient = new ProjectPatient()
        {
            Id = Guid.NewGuid().ToString(),
            FirstName = "Testfirst",
            LastName = "Testlast",
            Anonymity = false,
            BirthDate = DateTime.Now.AddYears(-20),
            Email = "han+01@impactly.dk",
            Region = "Region1",
            Municipality = "City1"
        };

        var response = await _fixture.Client.Fetch<ProjectPatient>(HttpMethod.Post, _path, newPatient)
            .ConfigureAwait(false);
        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var inserted = await _fixture.PatientContext.ReadPatient(_fixture.InitProject.TestProject.Id, newPatient.Id)
            .ConfigureAwait(false);
        Assert.NotNull(inserted);
        Assert.Equal(newPatient.Name, inserted.Name);

        _fixture.Client.SignOut();
    }


    [Fact]
    public async Task TestReadPage()
    {
        await _fixture.Client
            .SignInAsProject(_fixture.InitProject.ProjectSuperUser, _fixture.InitProject.TestProject.Id)
            .ConfigureAwait(false);
        var response = await _fixture.Client.Fetch<List<ProjectPatient>>(HttpMethod.Get, _path).ConfigureAwait(false);
        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(response.Value);
        Assert.Single(response.Value);
        var patient = response.Value[0];
        Assert.NotNull(patient);
        Assert.Equal(_fixture.InitProject.TestProjectPatient.Id, patient.Id);
        Assert.Equal(AnonymityMessage.HiddingMessage, patient.FirstName);

        _fixture.Client.SignOut();
    }

    [Fact]
    public async Task TestRead()
    {
        await _fixture.Client
            .SignInAsProject(_fixture.InitProject.ProjectSuperUser, _fixture.InitProject.TestProject.Id)
            .ConfigureAwait(false);
        var response = await _fixture.Client
            .Fetch<ProjectPatient>(HttpMethod.Get, _path + "/" + _fixture.InitProject.TestProjectPatient.Id)
            .ConfigureAwait(false);
        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var patient = response.Value;
        Assert.NotNull(patient);
        Assert.Equal(_fixture.InitProject.TestProjectPatient.Id, patient.Id);
        Assert.Equal(AnonymityMessage.HiddingMessage, patient.FirstName);
        _fixture.Client.SignOut();
    }

    [Fact]
    public async Task TestUpdate()
    {
        var newProject = new ProjectData("Patient_TestUpdate", _fixture.InitAdmin.UserAdmin);
        await _fixture.CreateProjectData(newProject).ConfigureAwait(false);

        await _fixture.Client.SignInAsProject(newProject.ProjectSuperUser, newProject.TestProject.Id)
            .ConfigureAwait(false);
        var path = "/api/web/v1/projects/" + newProject.TestProject.Id + "/patients/" +
                   newProject.TestProjectPatient.Id;

        var response = await _fixture.Client.Fetch<ProjectPatient>(HttpMethod.Put, path, new ProjectPatient() { })
            .ConfigureAwait(false);
        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        _fixture.Client.SignOut();

        await _fixture.Client.SignInAsProject(_fixture.InitAdmin.LoginAdmin, newProject.TestProject.Id)
            .ConfigureAwait(false);

        newProject.TestProjectPatient.FirstName = "New";
        newProject.TestProjectPatient.Anonymity = false;

        response = await _fixture.Client.Fetch<ProjectPatient>(HttpMethod.Put, path, newProject.TestProjectPatient)
            .ConfigureAwait(false);
        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var patient = response.Value;
        Assert.NotNull(patient);
        Assert.Equal("New", patient.FirstName);
        Assert.Equal(false, patient.Anonymity);
        patient = await _fixture.PatientContext.ReadPatient(newProject.TestProject.Id, newProject.TestProjectPatient.Id)
            .ConfigureAwait(false);
        Assert.NotNull(patient);
        Assert.Equal("New", patient.FirstName);
        Assert.Equal(false, patient.Anonymity);
        var strategyPatients = await _fixture.StrategyContext.StrategyPatients.ReadAll(newProject.TestStrategy.Id)
            .ConfigureAwait(false);
        Assert.NotNull(strategyPatients);
        var enumerable = strategyPatients.ToList();
        Assert.Single(enumerable);
        var strategyPatient = enumerable[0];
        Assert.NotNull(strategyPatient);
        Assert.Contains("New", strategyPatient.Name);
        Assert.Equal(false, strategyPatient.Anonymity);

        _fixture.Client.SignOut();
    }

    [Fact]
    public async Task TestDelete()
    {
        var newProject = new ProjectData("Patient_TestDelete", _fixture.InitAdmin.UserAdmin);
        await _fixture.CreateProjectData(newProject).ConfigureAwait(false);
        var patientTag = await _fixture.AddTagToProjectData("testTag", newProject.TestProjectPatient)
            .ConfigureAwait(false);
        Assert.NotNull(patientTag);
        await _fixture.Client.SignInAsProject(newProject.ProjectSuperUser, newProject.TestProject.Id)
            .ConfigureAwait(false);
        var path = "/api/web/v1/projects/" + newProject.TestProject.Id + "/patients/" +
                   newProject.TestProjectPatient.Id;

        Assert.NotNull(await _fixture.PatientContext.ProjectPatients
            .Read(newProject.TestProject.Id, newProject.TestProjectPatient.Id).ConfigureAwait(false));
        Assert.NotNull(await _fixture.StrategyContext.StrategyPatients
            .Read(newProject.TestStrategy.Id, newProject.TestProjectPatient.Id).ConfigureAwait(false));
        Assert.NotNull(await _fixture.PatientContext.Tags.Read(newProject.TestProjectPatient.Id, patientTag.Id)
            .ConfigureAwait(false));

        var response = await _fixture.Client.Fetch<ProjectPatient>(HttpMethod.Delete, path).ConfigureAwait(false);
        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Null(await _fixture.PatientContext.ProjectPatients
            .Read(newProject.TestProject.Id, newProject.TestProjectPatient.Id).ConfigureAwait(false));
        Assert.Null(await _fixture.StrategyContext.StrategyPatients
            .Read(newProject.TestStrategy.Id, newProject.TestProjectPatient.Id).ConfigureAwait(false));
        Assert.Null(await _fixture.ProjectContext.Tags.Read(newProject.TestProjectPatient.Id, patientTag.Id)
            .ConfigureAwait(false));
        Assert.NotNull(await _fixture.ProjectContext.Tags.Read(newProject.TestProject.Id, patientTag.ProjectTagId)
            .ConfigureAwait(false));
        Assert.Null(await _fixture.AnalyticsContext.EntryBatches.Read(newProject.TestCustomSurvey.Id)
            .ConfigureAwait(false));

        _fixture.Client.SignOut();
    }

    [Fact]
    public async Task TestReadSurveyAnswers()
    {
        await _fixture.Client.SignInAsProject(_fixture.InitProject.ProjectSuperUser,
            _fixture.InitProject.TestProject.Id).ConfigureAwait(false);
        var path = "/api/web/v1/projects/" + _fixture.InitProject.TestProject.Id +
                   "/patients/" + _fixture.InitProject.TestProjectPatient.Id + "/surveys";

        var response = await _fixture.Client.Fetch<List<EntryBatch>>(HttpMethod.Get, path).ConfigureAwait(false);
        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var batches = response.Value;
        Assert.NotNull(batches);
        Assert.Equal(1, batches.Count);

        response = await _fixture.Client.Fetch<List<EntryBatch>>(HttpMethod.Get, path + "?type=validated")
            .ConfigureAwait(false);
        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        batches = response.Value;
        Assert.Empty(batches);

        response = await _fixture.Client.Fetch<List<EntryBatch>>(HttpMethod.Get, path + "?type=custom")
            .ConfigureAwait(false);
        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        batches = response.Value;
        Assert.NotEmpty(batches);
        Assert.Single(batches);


        _fixture.Client.SignOut();

    }

    [Fact]
    public async Task TestReadyCustomSurveyAnswers()
    {
        await _fixture.Client.SignInAsProject(_fixture.InitProject.ProjectSuperUser,
            _fixture.InitProject.TestProject.Id).ConfigureAwait(false);
        var path = "/api/web/v1/projects/" + _fixture.InitProject.TestProject.Id +
                   "/patients/" + _fixture.InitProject.TestProjectPatient.Id + "/custom-surveys/";
        //no surveyId and no fieldId
        var response = await _fixture.Client.Fetch<List<FieldEntry>>(HttpMethod.Get, path).ConfigureAwait(false);
        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var batches = response.Value;
        Assert.NotNull(batches);
        Assert.Equal(1, batches.Count);
        //only surveyId
        response = await _fixture.Client
            .Fetch<List<FieldEntry>>(HttpMethod.Get, path + "?surveyId=" + _fixture.InitProject.TestCustomSurvey.Id)
            .ConfigureAwait(false);
        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        batches = response.Value;
        Assert.NotNull(batches);
        Assert.Equal(1, batches.Count);
        //only fieldId
        response = await _fixture.Client.Fetch<List<FieldEntry>>(HttpMethod.Get,
            path + "?fieldId=" + _fixture.InitProject.TestCustomSurvey.Fields.ToList()[0].Id).ConfigureAwait(false);
        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        batches = response.Value;
        Assert.NotNull(batches);
        Assert.Equal(1, batches.Count);
        //both surveyId and fieldId
        response = await _fixture.Client.Fetch<List<FieldEntry>>(HttpMethod.Get, path +
            "?fieldId=" + _fixture.InitProject.TestCustomSurvey.Fields.ToList()[0].Id +
            "&&surveyId=" + _fixture.InitProject.TestCustomSurvey.Id).ConfigureAwait(false);
        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        batches = response.Value;
        Assert.NotNull(batches);
        Assert.Equal(1, batches.Count);

        _fixture.Client.SignOut();

    }

    [Fact]
    public async Task TestCreateRegistration()
    {
        var newProject = new ProjectData("Patient_CreateReg", _fixture.InitAdmin.UserAdmin);
        await _fixture.CreateProjectData(newProject).ConfigureAwait(false);
        await _fixture.AddTagToProjectData("TestCreateRegistration", newProject.TestProjectPatient)
            .ConfigureAwait(false);
        var request = new Registration()
        {
            Id = Guid.NewGuid().ToString(),
            Category = "status",
            EffectName = "EffectName",
            Tags = new List<string>()
            {
                "TestCreateRegistration",
            },
        };
        await _fixture.Client.SignInAsProject(_fixture.InitAdmin.LoginAdmin, newProject.TestProject.Id)
            .ConfigureAwait(false);
        var path = "/api/web/v1/projects/" + newProject.TestProject.Id +
                   "/patients/" + newProject.TestProjectPatient.Id + "/registrations";
        var response = await _fixture.Client.Fetch<Registration>(HttpMethod.Post, path, request)
            .ConfigureAwait(false);
        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var registration = await _fixture.AnalyticsContext.Registrations.Read(request.Id).ConfigureAwait(false);
        Assert.NotNull(registration);
        Assert.NotEmpty(registration.Tags);
        Assert.Equal("TestCreateRegistration", registration.Tags[0]);

        _fixture.Client.SignOut();
    }

    [Fact]
    public async Task TestGetRegistration()
    {
        await _fixture.Client.SignInAsProject(_fixture.InitProject.ProjectSuperUser,
            _fixture.InitProject.TestProject.Id).ConfigureAwait(false);
        var registration = new Registration()
        {
            Id = Guid.NewGuid().ToString(),
            PatientId = _fixture.InitProject.TestProjectPatient.Id,
            ProjectId = _fixture.InitProject.TestProject.Id,
        };
        await _fixture.AnalyticsContext.Registrations.Create(registration).ConfigureAwait(false);

        var path = "/api/web/v1/projects/" + _fixture.InitProject.TestProject.Id +
                   "/patients/" + _fixture.InitProject.TestProjectPatient.Id + "/registrations";

        var response = await _fixture.Client.Fetch<List<Registration>>(HttpMethod.Get, path).ConfigureAwait(false);

        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(response.Value);
        Assert.Single(response.Value);
        Assert.Equal(registration.Id, response.Value[0].Id);

        await _fixture.AnalyticsContext.Registrations.Delete(registration.Id).ConfigureAwait(false);
        _fixture.Client.SignOut();
    }

    [Fact]
    public async Task TestEditRegistration()
    {
        await _fixture.Client.SignInAsProject(_fixture.InitProject.ProjectSuperUser,
            _fixture.InitProject.TestProject.Id).ConfigureAwait(false);
        var registration = new Registration()
        {
            Id = Guid.NewGuid().ToString(),
            PatientId = _fixture.InitProject.TestProjectPatient.Id,
            ProjectId = _fixture.InitProject.TestProject.Id,
            Type = "OldType",
        };
        await _fixture.AnalyticsContext.Registrations.Create(registration).ConfigureAwait(false);

        var path = "/api/web/v1/projects/" + _fixture.InitProject.TestProject.Id +
                   "/patients/" + _fixture.InitProject.TestProjectPatient.Id + "/registrations/" + registration.Id;

        registration.Type = "NewType";
        var response = await _fixture.Client.Fetch<Registration>(HttpMethod.Put, path, registration)
            .ConfigureAwait(false);

        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(response.Value);
        Assert.Equal(registration.Id, response.Value.Id);
        Assert.Equal("NewType", response.Value.Type);

        var expected = await _fixture.AnalyticsContext.Registrations.Read(registration.Id).ConfigureAwait(false);
        Assert.NotNull(expected);
        Assert.Equal("NewType", expected.Type);
        await _fixture.AnalyticsContext.Registrations.Delete(registration.Id).ConfigureAwait(false);
        _fixture.Client.SignOut();
    }


    [Fact]
    public async Task TestDeleteRegistration()
    {
        await _fixture.Client.SignInAsProject(_fixture.InitProject.ProjectSuperUser,
            _fixture.InitProject.TestProject.Id).ConfigureAwait(false);
        var registration = new Registration()
        {
            Id = Guid.NewGuid().ToString(),
            PatientId = _fixture.InitProject.TestProjectPatient.Id,
            ProjectId = _fixture.InitProject.TestProject.Id,
            Type = "OldType",
        };
        await _fixture.AnalyticsContext.Registrations.Create(registration).ConfigureAwait(false);

        var path = "/api/web/v1/projects/" + _fixture.InitProject.TestProject.Id +
                   "/patients/" + _fixture.InitProject.TestProjectPatient.Id + "/registrations/" + registration.Id;

        var response = await _fixture.Client.Fetch<Registration>(HttpMethod.Delete, path).ConfigureAwait(false);

        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(response.Value);

        var expected = await _fixture.AnalyticsContext.Registrations.Read(registration.Id).ConfigureAwait(false);
        Assert.Null(expected);
        _fixture.Client.SignOut();
    }

    [Fact]
    public async Task TestAddTags()
    {
        var newProject = new ProjectData("Patient_AddTags", _fixture.InitAdmin.UserAdmin);
        await _fixture.CreateProjectData(newProject).ConfigureAwait(false);

        var path = "/api/web/v1/projects/" + newProject.TestProject.Id +
                   "/patients/" + newProject.TestProjectPatient.Id + "/tags";
        var newProjectTag = new ProjectTag()
        {
            Id = Guid.NewGuid().ToString(),
            Color = "Green",
            Name = "NewProjectTag1"
        };
        await _fixture.ProjectContext.Tags.Create(newProject.TestProject.Id, newProjectTag).ConfigureAwait(false);

        await _fixture.Client.SignInAsProject(newProject.ProjectSuperUser, newProject.TestProject.Id)
            .ConfigureAwait(false);
        var response = await _fixture.Client
            .Fetch<ProjectPatient>(HttpMethod.Post, path, new List<ProjectTag> { newProjectTag }).ConfigureAwait(false);
        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var patientTags = await _fixture.PatientContext.Tags.ReadAll(newProject.TestProjectPatient.Id)
            .ConfigureAwait(false);
        Assert.NotNull(patientTags);
        var tags = patientTags.ToList();
        Assert.Single(tags);
        Assert.NotNull(tags[0]);
        Assert.Equal("NewProjectTag1", tags[0].Name);
        Assert.Equal(newProjectTag.Id, tags[0].ProjectTagId);

        var access = new SurveyAccess()
        {
            PatientId = newProject.TestProjectPatient.Id,
            ProjectId = newProject.TestProject.Id,
            SearchStart = DateTime.Now.AddDays(-1),
            SearchEnd = DateTime.Now.AddDays(1),
            Tags = new[] { "NewProjectTag1" },
        };
        var answers = await _fixture.AnalyticsContext.EntryBatches.ReadBetween(access).ConfigureAwait(false);

        Assert.NotNull(answers);
        Assert.Single(answers);

        _fixture.Client.SignOut();
    }

    [Fact]
    public async Task TestDeleteTag()
    {
        var newProject = new ProjectData("Patient_DeleteTage", _fixture.InitAdmin.UserAdmin);
        await _fixture.CreateProjectData(newProject).ConfigureAwait(false);
        var patientTag = await _fixture.AddTagToProjectData("NewTag", newProject.TestProjectPatient)
            .ConfigureAwait(false);
        var path = "/api/web/v1/projects/" + newProject.TestProject.Id +
                   "/patients/" + newProject.TestProjectPatient.Id + "/tags/" + patientTag.Id;

        await _fixture.Client.SignInAsProject(newProject.ProjectSuperUser, newProject.TestProject.Id)
            .ConfigureAwait(false);
        var response = await _fixture.Client
            .Fetch<ProjectPatient>(HttpMethod.Delete, path).ConfigureAwait(false);
        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var patientTags = await _fixture.PatientContext.Tags.Read(newProject.TestProjectPatient.Id, patientTag.Id)
            .ConfigureAwait(false);
        Assert.Null(patientTags);

        var patient = await _fixture.PatientContext
            .ReadPatient(newProject.TestProject.Id, newProject.TestProjectPatient.Id).ConfigureAwait(false);
        Assert.NotNull(patient);
        Assert.Empty(patient.Tags);

        var answers = await _fixture.AnalyticsContext.EntryBatches
            .ReadByTag(newProject.TestProject.Id, newProject.TestProjectPatient.Id, "NewTag").ConfigureAwait(false);
        Assert.Empty(answers);
        var fields = await _fixture.AnalyticsContext.FieldEntries
            .ReadByTag(newProject.TestProject.Id, newProject.TestProjectPatient.Id, "NewTag").ConfigureAwait(false);
        Assert.Empty(fields);
        _fixture.Client.SignOut();
    }

    [Fact]
    public async Task TestAssign()
    {
        var newProject = new ProjectData("Patient_Assign", _fixture.InitAdmin.UserAdmin);
        await _fixture.CreateProjectData(newProject).ConfigureAwait(false);

        var newStrategy = await _fixture.AddStrategyToProject(new Strategy()
        {
            Id = Guid.NewGuid().ToString(),
            Name = "New Strategy",
        }, newProject.TestProject.Id).ConfigureAwait(false);

        var newPatient = await _fixture.AddPatientToProject(new ProjectPatient()
        {
            Id = Guid.NewGuid().ToString(),
            FirstName = "First",
            LastName = "Last",
            Anonymity = false,
            IsActive = true,
            Email = "han@impactly.dk",
        }, newProject.TestProject.Id).ConfigureAwait(false);
        var path = "/api/web/v1/projects/" + newProject.TestProject.Id +
                   "/patients/" + newPatient.Id + "/assign/" + newStrategy.Id;

        await _fixture.Client.SignInAsProject(newProject.ProjectSuperUser, newProject.TestProject.Id)
            .ConfigureAwait(false);
        var response = await _fixture.Client.Fetch<ProjectPatient>(HttpMethod.Put, path).ConfigureAwait(false);

        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var strategyPatients =
            await _fixture.StrategyContext.StrategyPatients.ReadAll(newStrategy.Id).ConfigureAwait(false);
        Assert.NotNull(strategyPatients);
        Assert.Single(strategyPatients);
        Assert.NotNull(strategyPatients.ToList()[0]);
        Assert.Equal("First Last", strategyPatients.ToList()[0].Name);

        var patient = await _fixture.PatientContext.ReadPatient(newProject.TestProject.Id, newPatient.Id)
            .ConfigureAwait(false);
        Assert.NotNull(patient);
        Assert.Equal(newStrategy.Id, patient.StrategyId);
        Assert.Equal("New Strategy", patient.StrategyName);

        _fixture.Client.SignOut();
    }

    [Fact]
    public async Task TestGetSurveyCode()
    {
        var path = "/api/web/v1/projects/" + _fixture.InitProject.TestProject.Id +
                   "/patients/" + _fixture.InitProject.TestProjectPatient.Id + "/code/" + _fixture.InitProject.TestStrategy.Id;

        var surveyProperties = new List<SurveyProperty>()
        {
            new()
            {
                Id = Guid.NewGuid().ToString(),
                Name = "Survey Property 1",
            }
        };
        
        await _fixture.Client
            .SignInAsProject(_fixture.InitProject.ProjectSuperUser, _fixture.InitProject.TestProject.Id)
            .ConfigureAwait(false);

        var response = await _fixture.Client.Fetch<SurveyCode>(HttpMethod.Post, path, surveyProperties)
            .ConfigureAwait(false);
        
        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var code = response.Value;
        Assert.NotNull(code);
        Assert.Equal(_fixture.InitProject.TestProjectPatient.Id, code.PatientId);
        Assert.Single(code.Properties);
        _fixture.Client.SignOut();
    }

    [Fact]
    public async Task TestSendSurvey()
    {
        var path = "/api/web/v1/projects/" + _fixture.InitProject.TestProject.Id +
                   "/patients/" + _fixture.InitProject.TestProjectPatient.Id + "/code/" + _fixture.InitProject.TestStrategy.Id  + "/send";
        var surveyProperties = new List<SurveyProperty>()
        {
            new()
            {
                Id = Guid.NewGuid().ToString(),
                Name = "Survey Property 1",
            }
        };
        
        await _fixture.Client
            .SignInAsProject(_fixture.InitProject.ProjectSuperUser, _fixture.InitProject.TestProject.Id)
            .ConfigureAwait(false);

        var response = await _fixture.Client.Fetch<SurveyCode>(HttpMethod.Post, path, surveyProperties)
            .ConfigureAwait(false);
        
        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var code = response.Value;
        Assert.NotNull(code);
        Assert.Equal(_fixture.InitProject.TestProjectPatient.Id, code.PatientId);
        Assert.Single(code.Properties);

        path += "/" + _fixture.InitProject.TestSurveyCode.Id +  "/" + _fixture.InitProject.TestNotification.Id;
        response = await _fixture.Client.Fetch<SurveyCode>(HttpMethod.Post, path, surveyProperties)
            .ConfigureAwait(false);
        
        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        code = response.Value;
        Assert.NotNull(code);
        Assert.Equal(_fixture.InitProject.TestProjectPatient.Id, code.PatientId);
        Assert.Single(code.Properties);
        
        _fixture.Client.SignOut();
        
    }

    [Fact]
    public async Task TestUpdateAnonmity()
    {
        var path = "/api/web/v1/projects/" + _fixture.InitProject.TestProject.Id +
                   "/patients/anonymity";
        var request = new List<PatientAnonymityRequest>()
        {
            new()
            {
                PatientId = _fixture.InitProject.TestProjectPatient.Id,
                Anonymity = false,
            },
        };
        await _fixture.Client.SignInAsProject(_fixture.InitAdmin.LoginAdmin, _fixture.InitProject.TestProject.Id).ConfigureAwait(false);
        var response = await _fixture.Client.Fetch<List<ProjectPatient>>(HttpMethod.Put, path, request)
            .ConfigureAwait(false);
        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(response.Value);
        Assert.Single(response.Value);
        Assert.False(response.Value.ToList()[0].Anonymity);
        var patient = await _fixture.PatientContext
            .ReadPatient(_fixture.InitProject.TestProject.Id, _fixture.InitProject.TestProjectPatient.Id)
            .ConfigureAwait(false);
        Assert.NotNull(patient);
        Assert.False(patient.Anonymity);
        var strategyPatient = await _fixture.StrategyContext.StrategyPatients
            .Read(_fixture.InitProject.TestStrategy.Id, _fixture.InitProject.TestProjectPatient.Id)
            .ConfigureAwait(false);
        Assert.NotNull(strategyPatient);
        Assert.False(strategyPatient.Anonymity);
        request = new List<PatientAnonymityRequest>()
        {
            new()
            {
                PatientId = _fixture.InitProject.TestProjectPatient.Id,
                Anonymity = true,
            },
        };
        response = await _fixture.Client.Fetch<List<ProjectPatient>>(HttpMethod.Put, path, request)
            .ConfigureAwait(false);
        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(response.Value);
        Assert.Single(response.Value);
        Assert.True(response.Value.ToList()[0].Anonymity);
        patient = await _fixture.PatientContext
            .ReadPatient(_fixture.InitProject.TestProject.Id, _fixture.InitProject.TestProjectPatient.Id)
            .ConfigureAwait(false);
        Assert.NotNull(patient);
        Assert.True(patient.Anonymity);
        strategyPatient = await _fixture.StrategyContext.StrategyPatients
            .Read(_fixture.InitProject.TestStrategy.Id, _fixture.InitProject.TestProjectPatient.Id)
            .ConfigureAwait(false);
        Assert.NotNull(strategyPatient);
        Assert.True(strategyPatient.Anonymity);
        
        _fixture.Client.SignOut();
    }
}