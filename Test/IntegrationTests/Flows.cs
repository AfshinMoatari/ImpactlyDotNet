using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using API.Models.Projects;
using Impactly.Test.IntegrationTests.Client;
using Impactly.Test.Utils;
using Xunit;
using Xunit.Abstractions;

namespace Impactly.Test.IntegrationTests
{
    [Collection("Integration Test")]
    public class Flows
    {
        private readonly ITestOutputHelper _output;
        private readonly TestFixture _fixture;

        private const string PatientPath = "/api/web/v1/projects/" + AuthExtension.AdminProjectId + "/patients";
        private const string TagPath = "/api/web/v1/projects/" + AuthExtension.AdminProjectId + "/tags";


        public Flows(TestFixture fixture, ITestOutputHelper output)
        {
            _fixture = fixture;
            _output = output;
        }


        public async Task CrudTags()
        {
            await _fixture.Client.SignInProject();

            var patient = await _fixture.Client.Post(PatientPath,
                new ProjectPatient
                {
                    Email = "test+tag@innosocial.dk",
                    FirstName = "Test",
                    LastName = "Tag",
                });
            
            var noTags = await _fixture.Client.ReadAll<ProjectTag>(TagPath);

            var createdProjectTag = await _fixture.Client.Post(TagPath, new ProjectTag() { Name = "Test", Color =  "#344563" });
            
            _output.PrintJson(createdProjectTag);
            _output.WriteLine($"{PatientPath}/{patient.Id}/tags");
            
            var patientWithTags = await _fixture.Client.Fetch<ProjectPatient>(HttpMethod.Post, $"{PatientPath}/{patient.Id}/tags", new List<ProjectTag> {createdProjectTag});
            
            _output.PrintJson(patientWithTags.Value);
            
            Assert.NotEmpty(patientWithTags.Value.Tags);
            Assert.Equal(patientWithTags.Value.Tags[0].ProjectTagId, createdProjectTag.Id);


            var patientWithDeletedTags = await _fixture.Client.Update<ProjectPatient>($"{PatientPath}/{patient.Id}/tags/{patientWithTags.Value.Tags[0].Id}/archive");
            Assert.Empty(patientWithDeletedTags.Tags);
                
            var readProjectTag = await _fixture.Client.ReadAll<ProjectTag>(TagPath);
            Assert.Equal(createdProjectTag.Id, readProjectTag[0].Id);
            Assert.Equal(createdProjectTag.Name, readProjectTag[0].Name);
            Assert.Equal(createdProjectTag.Color, readProjectTag[0].Color);

            var archivedProjectTag = await _fixture.Client.Update<ProjectTag>($"{TagPath}/{createdProjectTag.Id}/archive");
            Assert.Equal(createdProjectTag.Id, archivedProjectTag.Id);
            Assert.NotNull(archivedProjectTag.DeletedAt);

            _fixture.Client.SignOut();
        }
    }
}