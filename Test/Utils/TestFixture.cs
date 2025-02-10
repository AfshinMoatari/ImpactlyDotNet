using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Amazon;
using Amazon.DynamoDBv2;
using Amazon.S3.Util;
using API;
using API.Constants;
using API.Dynamo;
using API.Dynamo.Seeds;
using API.Handlers;
using API.Models.Admin;
using API.Models.Auth;
using API.Models.Projects;
using API.Models.Strategy;
using API.Repositories;
using Impactly.Test.IntegrationTests.Client;
using Impactly.Test.IntegrationTests.Models;
using Impactly.Test.IntegrationTests.TestData;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Impactly.Test.Utils
{
    public class TestFixture : IAsyncLifetime
    {
        private readonly IAmazonDynamoDB _dynamo;
        public readonly TestServer Server;
        public readonly HttpClient Client;
        public readonly IAuthHandler AuthHandler;
        public readonly IUserContext UserContext;
        public readonly IAdminContext AdminContext;
        public readonly IProjectContext ProjectContext;
        public readonly IRoleContext RoleContext;
        public readonly IStrategyContext StrategyContext;
        public readonly IPatientContext PatientContext;
        public readonly IAnalyticsContext AnalyticsContext;
        public readonly INotificationContext NotificationContext;
        public readonly ICodeContext CodeContext;
        public AdminUserData InitAdmin;
        public ProjectData InitProject;

        public TestFixture()
        {
            Server = CreateServer();
            Client = Server.CreateClient();
            _dynamo = Server.Services.GetRequiredService<IAmazonDynamoDB>();
            AuthHandler = Server.Services.GetRequiredService<IAuthHandler>();
            UserContext = Server.Services.GetRequiredService<IUserContext>();
            AdminContext = Server.Services.GetRequiredService<IAdminContext>();
            ProjectContext = Server.Services.GetRequiredService<IProjectContext>();
            RoleContext = Server.Services.GetRequiredService<IRoleContext>();
            StrategyContext = Server.Services.GetRequiredService<IStrategyContext>();
            PatientContext = Server.Services.GetRequiredService<IPatientContext>();
            AnalyticsContext = Server.Services.GetRequiredService<IAnalyticsContext>();
            NotificationContext = Server.Services.GetRequiredService<INotificationContext>();
            CodeContext = Server.Services.GetRequiredService<ICodeContext>();
            InitAdmin = new AdminUserData();
            InitProject = new ProjectData("admin", InitAdmin.UserAdmin);
        }

        public async Task InitializeAsync()
        {
            await DynamoTableHelper.CreateTables(Server.Services).ConfigureAwait(false);
            //await DynamoSeedHelper.SeedTables(Server.Services);
            await InitializeTestData().ConfigureAwait(false);
        }

        public async Task InitializeTestData()
        {
            await RoleContext.Roles.Create(InitAdmin.RoleAdministrator).ConfigureAwait(false);
            await RoleContext.Roles.Create(InitAdmin.RoleSuper).ConfigureAwait(false);
            await RoleContext.Roles.Create(InitAdmin.RoleStandard).ConfigureAwait(false);

            await CreateAdminData(InitAdmin).ConfigureAwait(false);
            await CreateProjectData(InitProject).ConfigureAwait(false);
        }

        public async Task DisposeAsync()
        {
            var tableNames = (await _dynamo.ListTablesAsync().ConfigureAwait(false)).TableNames.FindAll(name => name.Contains("test"));
            foreach (var table in tableNames)
            {
                await _dynamo.DeleteTableAsync(table).ConfigureAwait(false);
            }
        }

        public async Task CreateAdminData(AdminUserData adminData)
        {
            adminData.UserAdmin.PasswordHashB64 =
                AuthHandler.HashUserPassword(adminData.UserAdmin.Id, adminData.UserAdminPassword);
            await UserContext.Users.ReadOrCreate(adminData.UserAdmin).ConfigureAwait(false);
            await AdminContext.Admins.Create(AdminUser.FromAuthUser(adminData.UserAdmin)).ConfigureAwait(false);
        }

        private async Task CreateProjectUser(SignInWithEmailRequest login, string roleId, string projectId)
        {
            var userId = Guid.NewGuid().ToString();
            var user = new AuthUser()
            {
                Id = userId,
                Email = login.Email.ToLower(),
                FirstName = "First",
                LastName = "Last",
            };
            await UserContext.Users.ReadOrCreate(user).ConfigureAwait(false);
            await UserContext.Users.UpdateValue(userId, u =>
            {
                u.PrivacyPolicy = true;
                u.PasswordHashB64 = AuthHandler.HashUserPassword(userId, login.Password);
            }).ConfigureAwait(false);
            await ProjectContext.CreateProjectUser(projectId, new ProjectUser()
            {
                Id = user.Id,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                PhoneNumber = user.PhoneNumber,
                RoleId = roleId,
            }).ConfigureAwait(false);
        }
        public async Task CreateProjectData(ProjectData initData)
        {
            await ProjectContext.Projects.Create(initData.TestProject).ConfigureAwait(false);
            await ProjectContext.CreateProjectUser(initData.TestProject.Id, new ProjectUser
            {
                Id = initData.ProjectAdmin.Id,
                Email = initData.ProjectAdmin.Email,
                FirstName = initData.ProjectAdmin.FirstName,
                LastName = initData.ProjectAdmin.LastName,
                PhoneNumber = initData.ProjectAdmin.PhoneNumber,
                RoleId = "administrator",
            }).ConfigureAwait(false);


            await StrategyContext.CreateProjectSurvey(initData.TestProject.Id, initData.TestCustomSurvey)
                .ConfigureAwait(false);
            await StrategyContext.CreateProjectSurvey("TEMPLATE", initData.TestValidatedSurvey).ConfigureAwait(false);
            await StrategyContext.CreateStrategy(initData.TestProject.Id, initData.TestStrategy).ConfigureAwait(false);
            await PatientContext.ProjectPatients.Create(initData.TestProject.Id, initData.TestProjectPatient)
                .ConfigureAwait(false);
            await StrategyContext.StrategyPatients.Create(initData.TestStrategy.Id, new StrategyPatient()
            {
                Anonymity = initData.TestProjectPatient.Anonymity,
                Name = initData.TestProjectPatient.Name,
                Id = initData.TestProjectPatient.Id,
            }).ConfigureAwait(false);
            await AnalyticsContext.CreateEntries(initData.TestCustomSurveyEntryBatch).ConfigureAwait(false);

            await ProjectContext.Reports.Create(initData.TestProject.Id, initData.TestReport).ConfigureAwait(false);
            await CodeContext.SurveyCodes.Create(initData.TestSurveyCode).ConfigureAwait(false);
            await NotificationContext.Notifications.Create(initData.TestNotification).ConfigureAwait(false);

            await ProjectContext.Communicaitons.Create(initData.TestProject.Id, initData.TestWelcomeCommunication)
                .ConfigureAwait(false);

            await ProjectContext.Communicaitons.Create(initData.TestProject.Id, initData.TestSurveyCommunication)
                .ConfigureAwait(false);


            await CreateProjectUser(initData.ProjectStandardUser, InitAdmin.RoleStandard.Id, initData.TestProject.Id)
                .ConfigureAwait(false);
            await CreateProjectUser(initData.ProjectSuperUser, InitAdmin.RoleSuper.Id, initData.TestProject.Id)
                .ConfigureAwait(false);
        }

        public async Task<PatientTag> AddTagToProjectData(string tag, ProjectPatient patient)
        {
            var projectId = patient.ParentId;
            var projectTag = new ProjectTag()
            {
                Id = Guid.NewGuid().ToString(),
                Name = tag,
                Color = "Red",
                ParentId = projectId,
            };
            await ProjectContext.Tags.Create(projectId, projectTag).ConfigureAwait(false);
            var patientTag = new PatientTag()
            {
                Id = Guid.NewGuid().ToString(),
                ParentId = patient.Id,
                Color = projectTag.Color,
                Name = tag,
                ProjectTagId = projectTag.Id,
            };
            await PatientContext.ProjectPatients.UpdateValue(projectId, patient.Id, u => u.Tags = new List<PatientTag>()
                {
                    patientTag,
                })
                .ConfigureAwait(false);
            await AnalyticsContext.AddTags(projectId, patient.Id, new[] { tag }).ConfigureAwait(false);
            return await PatientContext.Tags.Create(patient.Id, patientTag).ConfigureAwait(false);
            
        }

        public async Task<ProjectPatient> AddPatientToProject(ProjectPatient patient, string projectId)
        {
            return await PatientContext.ProjectPatients.Create(projectId, patient).ConfigureAwait(false);
        }

        public async Task<Strategy> AddStrategyToProject(Strategy strategy, string projectId)
        {
            return await StrategyContext.CreateStrategy(projectId, strategy).ConfigureAwait(false);
        }

        public async Task<Project> SetLanguageToProject(string language, string projectId)
        {
            return await ProjectContext.Projects.UpdateValue(projectId, p => p.TextLanguage = language).ConfigureAwait(false);
        }

        
        
        private TestServer CreateServer()
        {
            Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "Test");
            AWSConfigsDynamoDB.Context.TableNamePrefix = EnvironmentMode.TablePrefix;
            //AWSConfigsDynamoDB.Context.TableNamePrefix = "impactly-development-";
            var projectDir = Directory.GetCurrentDirectory().Split("impactly_api").FirstOrDefault();
            projectDir = Path.Combine(projectDir, Path.Combine("impactly_api","API"));

            var configuration = new ConfigurationBuilder()
                .SetBasePath(projectDir)
                .AddJsonFile(Path.Combine(projectDir, "appsettings.json"))
                .AddJsonFile(Path.Combine(projectDir, "appsettings.Development.json"))
                .AddEnvironmentVariables()
                .Build();

            var server = new TestServer(new WebHostBuilder()
                .UseContentRoot(projectDir)
                .UseEnvironment("Development")
                .UseConfiguration(configuration)
                .UseStartup<Startup>());

            return server;
        }
        
        [CollectionDefinition("Integration Test")]
        public class IntegrationTest : ICollectionFixture<TestFixture>
        {
            // This class has no code, and is never created. Its purpose is simply
            // to be the place to apply [CollectionDefinition] and all the
            // ICollectionFixture<> interfaces.
        }
    }
}