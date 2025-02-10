using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Amazon.Util;
using API.Constants;
using API.Models;
using API.Models.Auth;
using API.Models.Projects;
using API.Models.Reports;
using API.Models.Strategy;
using Nest;

namespace API.Repositories
{
    public interface IProjectContext
    {
        public ICrudRepository<Project> Projects { get; }

        public Task DeleteProject(string projectId);
        public Task DeleteProjectUsers(string projectId);
        public Task DeleteProjectPatients(string projectId);
        public Task<IEnumerable<ProjectPatient>> ReadProjectPatients(string projectId);
        public Task<Project> UpdateProject(Project request);

        // ProjectUser
        public Task<ProjectUser> CreateProjectUser(string projectId, ProjectUser user);

        public Task<ProjectUser> ReadProjectUser(string projectId, string userId);
        public Task<ProjectUser> UpdateProjectUser(string projectId, ProjectUser user);

        public Task DeleteProjectUser(string projectId, string userId);
        public Task<IEnumerable<UserProject>> ReadAllUserProjects(string userId);

        public Task<IEnumerable<ProjectUser>> ReadAllProjectUsers(string projectId);
        public Task<IEnumerable<ProjectPatient>> GetAllProjectPatientsByStrategyId(string strategyId);

        // Reports
        public ICrudPropertyRepository<Report> Reports { get; }

        // Tags
        public ICrudPropertyRepository<ProjectTag> Tags { get; }

        public ICrudPropertyRepository<ProjectCommunication> Communicaitons { get; }

        public ICrudPropertyRepository<ProjectPatient> ProjectPatients { get; }
    }

    public class ProjectContext : IProjectContext
    {
        public ICrudRepository<Project> Projects { get; }
        public ICrudPropertyRepository<ProjectPatient> ProjectPatients { get; }
        private ICrudPropertyRepository<ProjectUser> ProjectUsers { get; }
        private ICrudPropertyRepository<UserProject> UserProjects { get; }

        public ICrudPropertyRepository<Report> Reports { get; }
        public ICrudPropertyRepository<ProjectTag> Tags { get; }

        public ICrudPropertyRepository<ProjectCommunication> Communicaitons { get; }

        public ProjectContext(IAmazonDynamoDB client)
        {
            Projects = new ProjectsRepository(client);
            ProjectPatients = new ProjectPatientRepository(client);
            ProjectUsers = new ProjectUsersRepository(client);
            UserProjects = new UserProjectsRepository(client);
            Reports = new ProjectReportRepository(client);
            Tags = new ProjectTagRepository(client);
            Communicaitons = new ProjectCommunicationRepository(client);
        }

        public async Task DeleteProject(string projectId)
        {

            await Projects.Delete(projectId);

            //// GET everything
            //var project = await Projects.Read(projectId);
            //var projectBatch = Projects.Context.CreateBatchWrite<Project>();
            //projectBatch.AddDeleteItem(project);

            //// Users
            //var projectUsers = await ReadAllProjectUsers(projectId);
            //var projectUsersBatch = Projects.Context.CreateBatchWrite<ProjectUser>();
            //projectUsersBatch.AddDeleteItems(projectUsers);

            //var userProject = await ReadUserProjects(userId, projectId);
            //var usersProjectBatch = Projects.Context.CreateBatchWrite<UserProject>();
            //usersProjectBatch.AddDeleteItem(userProject);

            //// Patients
            //// var projectPatients = await ReadAllProjectUsers(projectId);
            //// var projectPatientsBatch = Projects.Context.CreateBatchWrite<ProjectUser>();
            //// projectUsersBatch.AddDeleteItems(projectUsers);

            //// Strategies

            //// surveys

            //// Tags

            //// Jobs


            //await Projects.Context.ExecuteBatchWriteAsync(
            //    new BatchWrite[]
            //    {
            //        projectBatch,
            //        projectUsersBatch,
            //        usersProjectBatch
            //    }
            //);
        }

        public async Task DeleteProjectUsers(string projectId)
        {
            var projectUsers = await ReadAllProjectUsers(projectId);
            var projectUsersBatch = Projects.Context.CreateBatchWrite<ProjectUser>();
            projectUsersBatch.AddDeleteItems(projectUsers);
        }

        public async Task DeleteProjectPatients(string projectId)
        {
            var projectPatients = await ReadProjectPatients(projectId);
            var projectPatientsBatch = Projects.Context.CreateBatchWrite<ProjectPatient>();
            projectPatientsBatch.AddDeleteItems(projectPatients);
        }

        public async Task<IEnumerable<ProjectPatient>> ReadProjectPatients(string projectId)
        {
            return await ProjectPatients.ReadAll(projectId);
        }

        public async Task<Project> UpdateProject(Project request)
        {
            var updatedProject = await Projects.Update(request);
            if (updatedProject == null) throw new KeyNotFoundException();

            var projectUsers = await ProjectUsers.ReadAll(updatedProject.Id);
            foreach (var projectUser in projectUsers)
            {
                await UserProjects.UpdateValue(
                    projectUser.Id,
                    updatedProject.Id,
                    userProject => { userProject.Name = updatedProject.Name; }
                    );
            }

            return updatedProject;
        }

        public async Task<ProjectUser> CreateProjectUser(string projectId, ProjectUser user)
        {
            var project = await Projects.Read(projectId);
            if (project == null || string.IsNullOrEmpty(project.Id)) return null;

            await ProjectUsers.Create(projectId, user);
            await UserProjects.Create(user.Id, new UserProject
            {
                Id = project.Id,
                Name = project.Name,
            });

            return user;
        }

        public async Task<ProjectUser> UpdateProjectUser(string projectId, ProjectUser user)
        {
            var project = await Projects.Read(projectId);
            if (project == null || string.IsNullOrEmpty(project.Id)) return null;

            var projectUser = await ProjectUsers.Read(projectId, user.Id);
            if (projectUser == null) return null;

            await ProjectUsers.Update(projectId, user);

            return user;
        }

        public async Task<ProjectUser> ReadProjectUser(string projectId, string userId)
        {
            return await ProjectUsers.Read(projectId, userId);
        }

        public async Task DeleteProjectUser(string projectId, string userId)
        {
            await ProjectUsers.Delete(projectId, userId);
            await UserProjects.Delete(userId, projectId);
        }

        public Task<IEnumerable<UserProject>> ReadAllUserProjects(string userId)
        {
            return UserProjects.ReadAll(userId);
        }

        public Task<UserProject> ReadUserProjects(string userId, string projectId)
        {
            return UserProjects.Read(userId, projectId);
        }

        public Task<IEnumerable<ProjectUser>> ReadAllProjectUsers(string projectId)
        {
            return ProjectUsers.ReadAll(projectId);
        }


        public async Task<IEnumerable<ProjectPatient>> GetAllProjectPatientsByStrategyId(string strategyId)
        {

            var patients = await ProjectPatients.Context.QueryAsync<ProjectPatient>(strategyId, new DynamoDBOperationConfig
            {
                IndexName = CrudPropModel.StrategyIdIndex,
            }).GetRemainingAsync();
            return patients;
        }

        public class ProjectsRepository : CrudRepository<Project>
        {
            public override string SortKeyValue(Project model) => model.Name;
            public override bool Descending => false;

            public ProjectsRepository(IAmazonDynamoDB client) : base(client)
            {
            }
        }

        public class ProjectUsersRepository : CrudPropertyRepository<ProjectUser>
        {
            public override string ParentPrefix => Project.Prefix;
            public override string ModelPrefix => AuthUser.Prefix;
            public override string SortKeyValue(ProjectUser model) => model.Name;

            public ProjectUsersRepository(IAmazonDynamoDB client) : base(client)
            {
            }
        }

        public class UserProjectsRepository : CrudPropertyRepository<UserProject>
        {
            public override string ParentPrefix => AuthUser.Prefix;
            public override string ModelPrefix => Project.Prefix;
            public override string SortKeyValue(UserProject model) => model.Name;
            public override bool Descending => false;

            public UserProjectsRepository(IAmazonDynamoDB client) : base(client)
            {
            }
        }

        public class ProjectReportRepository : CrudPropertyRepository<Report>
        {
            public override string ParentPrefix => Project.Prefix;
            public override string ModelPrefix => Report.Prefix;

            public override string SortKeyValue(Report model) => model.UpdatedAt
                .ToUniversalTime().ToString(Languages.ISO8601DateFormat);

            public ProjectReportRepository(IAmazonDynamoDB client) : base(client)
            {
            }
        }

        public class ProjectTagRepository : CrudPropertyRepository<ProjectTag>
        {
            public override string ParentPrefix => Project.Prefix;
            public override string ModelPrefix => ProjectTag.Prefix;

            public ProjectTagRepository(IAmazonDynamoDB client) : base(client)
            {
            }

        }

        public class ProjectCommunicationRepository : CrudPropertyRepository<ProjectCommunication>
        {
            public override string ParentPrefix => Project.Prefix;
            public override string ModelPrefix => ProjectCommunication.Prefix;

            public ProjectCommunicationRepository(IAmazonDynamoDB client) : base(client)
            {

            }
        }

        public class ProjectPatientRepository : CrudPropertyRepository<ProjectPatient>
        {
            public override bool Descending => false;
            public override string ParentPrefix => Project.Prefix;
            public override string ModelPrefix => ProjectPatient.Prefix;

            public ProjectPatientRepository(IAmazonDynamoDB client) : base(client)
            {
            }
        }

    }
}