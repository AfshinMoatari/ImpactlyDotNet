using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using API.Models;
using API.Models.Projects;

namespace API.Repositories
{
    public interface IPatientContext
    {
        public DynamoDBContext DynamoDbContext { get; }

        public ICrudPropertyRepository<ProjectPatient> ProjectPatients { get; }

        public ICrudPropertyRepository<PatientTag> Tags { get; set; }

        public Task<ProjectPatient> ReadPatient(string projectId, string patientId);
        public Task<IEnumerable<ProjectPatient>> GetPatientsByStrategyId(string strategyId);
    }


    public class PatientContext : IPatientContext
    {
        public DynamoDBContext DynamoDbContext { get; }
        public ICrudPropertyRepository<ProjectPatient> ProjectPatients { get; }
        public ICrudPropertyRepository<PatientTag> Tags { get; set; }

        public PatientContext(IAmazonDynamoDB client)
        {
            DynamoDbContext = new DynamoDBContext(client);
            ProjectPatients = new ProjectPatientRepository(client);
            Tags = new TagRepository(client);
        }

        public async Task<ProjectPatient> ReadPatient(string projectId, string patientId)
        {
            var patient = await ProjectPatients.Read(projectId, patientId);
            if (patient == null) return null;

            var tags = await Tags.ReadAll(patientId);
            // TODO
            patient.Tags = tags.Where(tag => tag.DeletedAt == null).ToList();

            return patient;
        }

        public async Task<IEnumerable<ProjectPatient>> GetPatientsByStrategyId(string strategyId)
        {
            var patients = await ProjectPatients.Context.QueryAsync<ProjectPatient>(strategyId, new DynamoDBOperationConfig
            {
                IndexName = CrudPropModel.StrategyIdIndex,
            }).GetRemainingAsync();

            // Sort patients by Name (case-insensitive) before returning
            return patients.OrderBy(p => p.Name, StringComparer.OrdinalIgnoreCase);
        }

    }

    public class ProjectPatientRepository : CrudPropertyRepository<ProjectPatient>
    {
        public override string ParentPrefix => Project.Prefix;
        public override string ModelPrefix => ProjectPatient.Prefix;
        public override string SortKeyValue(ProjectPatient model) => model.Name;

        public ProjectPatientRepository(IAmazonDynamoDB client) : base(client)
        {
        }
    }

    public class TagRepository : CrudPropertyRepository<PatientTag>
    {
        public override string ParentPrefix => ProjectPatient.Prefix;
        public override string ModelPrefix => ProjectTag.Prefix;

        public override string SortKeyValue(PatientTag model) => model.Name;

        public TagRepository(IAmazonDynamoDB client) : base(client)
        {
        }
    }
}