using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.DocumentModel;
using Amazon.DynamoDBv2.Model;
using API.Constants;
using API.Models;
using API.Models.Projects;
using API.Models.Strategy;
using Nest;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OpenAI.GPT3.ObjectModels.ResponseModels;
using static API.Constants.Permissions;
using static Nest.JoinField;
using Project = API.Models.Projects.Project;
using Strategy = API.Models.Strategy.Strategy;

namespace API.Repositories
{
    public interface IStrategyContext
    {
        // Surveys
        public ICrudPropertyRepository<Survey> Surveys { get; }
        public ICrudPropertyRepository<SurveyField> SurveyFields { get; }
        public ICrudPropertyRepository<FieldChoice> FieldChoices { get; }

        // Strategy
        public ICrudPropertyRepository<Strategy> Strategies { get; }
        public ICrudPropertyRepository<StrategyEffect> Effects { get; }
        public ICrudPropertyRepository<StrategyPatient> StrategyPatients { get; }
        //public ICrudPropertyRepository<StrategyFrequency> Frequencies { get; }
        public ICrudPropertyRepository<BatchSendoutFrequency> BatchFrequencies { get; }
        
        public ICrudPropertyRepository<FieldTemplate> FieldTemplates { get; }

        //  Methods

        public Task<Survey> ReadSurvey(string projectId, string surveyId);
        public Task<IEnumerable<Survey>> ReadTemplateSurveys();

        public Task<IEnumerable<Survey>> ReadSurveysFromProperty(List<SurveyProperty> surveyProperties);

        public Task<List<Strategy>> ReadStrategies(string projectId);
        public Task<Strategy> ReadStrategy(string projectId, string strategyId);

        public Task<Strategy> CreateStrategy(string projectId, Strategy strategy);
        public Task<IEnumerable<Survey>> ReadProjectSurveys(string projectId);
        public Task<Survey> CreateProjectSurvey(string projectId, Survey survey);
        public Task<Survey> UpdateProjectSurvey(string projectId, Survey survey);
        public Task<Survey> DeleteProjectSurvey(string projectId, string surveyId);
        public Task<List<Survey>> GetAllStrategySurveysByStrategyId(string projectId, string strategyId);
        public Task<BatchSendoutFrequency> CreateBatchSendout(BatchSendoutFrequency batchSendoutFrequency);

        public Task<FieldTemplate> CreateFieldTemplate(string projectId, FieldTemplate fieldTemplate);

        public Task<IEnumerable<FieldTemplate>> ReadFieldTemplates(string projectId);

        public Task<Survey> AddTemplateFields(string projectId, Survey survey);
    }

    public class StrategyContext : IStrategyContext
    {
        public DynamoDBContext DynamoDbContext { get; }

        public ICrudPropertyRepository<Survey> Surveys { get; }
        public ICrudPropertyRepository<SurveyField> SurveyFields { get; }
        public ICrudPropertyRepository<FieldChoice> FieldChoices { get; }

        public ICrudPropertyRepository<Strategy> Strategies { get; }
        public ICrudPropertyRepository<StrategyEffect> Effects { get; }
        public ICrudPropertyRepository<StrategyPatient> StrategyPatients { get; }
        //public ICrudPropertyRepository<StrategyFrequency> Frequencies { get; }
        public ICrudPropertyRepository<BatchSendoutFrequency> BatchFrequencies { get; }
        
        public ICrudPropertyRepository<FieldTemplate> FieldTemplates { get; set; }

        public StrategyContext(IAmazonDynamoDB client)
        {
            DynamoDbContext = new DynamoDBContext(client);
            Surveys = new SurveyRepository(client);
            SurveyFields = new SurveyQuestionRepository(client);
            FieldChoices = new SurveyOptionRepository(client);

            Strategies = new DataStrategyRepository(client);
            StrategyPatients = new StrategyPatients(client);
            Effects = new StrategyEffectRepository(client);
            //Frequencies = new FrequencyRepository(client);
            BatchFrequencies = new BatchFrequencyRepository(client);

            FieldTemplates = new FieldTemplateRepository(client);
        }

        public async Task<BatchSendoutFrequency> CreateBatchSendout(BatchSendoutFrequency batchSendoutFrequency)
        {
            return await BatchFrequencies.Create(batchSendoutFrequency.ParentId, batchSendoutFrequency);
        }

        public async Task<FieldTemplate> CreateFieldTemplate(string projectId, FieldTemplate fieldTemplate)
        {
            return await FieldTemplates.Create(projectId, fieldTemplate);
        }

        public async Task<IEnumerable<FieldTemplate>>ReadFieldTemplates(string projectId)
        {
            return await FieldTemplates.ReadAll(projectId);
        }

        public async Task<IEnumerable<Survey>> ReadTemplateSurveys()
        {
            var surveys = await Surveys.ReadAll("TEMPLATE");
            return await Task.WhenAll(surveys.Select(async s => await DecorateSurvey(s)));
        }
        
        

        public async Task<Survey> ReadSurvey(string parentId, string surveyId)
        {
            var survey = await Surveys.Read(parentId, surveyId);
            return await DecorateSurvey(survey);
        }

        public async Task<IEnumerable<Survey>> ReadSurveysFromProperty(List<SurveyProperty> surveyProperties)
        {
            var surveyBatch = DynamoDbContext.CreateBatchGet<Survey>(
                new DynamoDBOperationConfig { SkipVersionCheck = true, IgnoreNullValues = true });

            foreach (var surveyProperty in surveyProperties)
            {
                surveyBatch.AddKey(
                    Surveys.PartitionKeyByParentId(surveyProperty.ParentId),
                    Surveys.SortKeyById(surveyProperty.Id)
                );
            }

            await surveyBatch.ExecuteAsync();
            return await Task.WhenAll(surveyBatch.Results.Select(async s => await DecorateSurvey(s)));
        }

        public async Task<Strategy> CreateStrategy(string projectId, Strategy strategy)
        {
            var newStrategy = Strategies.ToDynamoItem(projectId, strategy);

            var strategyBatch = DynamoDbContext.CreateBatchWrite<Strategy>(
                new DynamoDBOperationConfig {SkipVersionCheck = true, IgnoreNullValues = true});
            strategyBatch.AddPutItem(newStrategy);

            var patientBatch = DynamoDbContext.CreateBatchWrite<StrategyPatient>(
              new DynamoDBOperationConfig { SkipVersionCheck = true, IgnoreNullValues = true });
            patientBatch.AddPutItems(strategy.Patients.Select(p => StrategyPatients.ToDynamoItem(newStrategy.Id, p)));

            var frequencyBatch = DynamoDbContext.CreateBatchWrite<BatchSendoutFrequency>(
                new DynamoDBOperationConfig {SkipVersionCheck = true, IgnoreNullValues = true});
            frequencyBatch.AddPutItems(strategy.Frequencies.Select(f => BatchFrequencies.ToDynamoItem(newStrategy.Id, f)));

            var effectBatch = DynamoDbContext.CreateBatchWrite<StrategyEffect>(
                new DynamoDBOperationConfig {SkipVersionCheck = true, IgnoreNullValues = true});
            effectBatch.AddPutItems(strategy.Effects.Select(e => Effects.ToDynamoItem(newStrategy.Id, e)));

            await DynamoDbContext.ExecuteBatchWriteAsync(
                new BatchWrite[]
                {
                    strategyBatch,
                    patientBatch,
                    frequencyBatch,
                    effectBatch
                }
            );

            return await ReadStrategy(projectId, newStrategy.Id);
        }

        public async Task<List<Strategy>> ReadStrategies(string projectId)
        {
            var strategies = await Strategies.ReadAll(projectId);
            if (strategies == null) return new List<Strategy>();
            strategies =
                await Task.WhenAll(strategies.Select(async strategy => await DecorateStrategy(strategy)));

            
            return strategies.ToList();
        }

        public async Task<Strategy> ReadStrategy(string projectId, string strategyId)
        {
            var strategy = await Strategies.Read(projectId, strategyId);
            if (strategy == null) return null;
            return await DecorateStrategy(strategy);
        }
        
        public async Task<IEnumerable<Survey>> ReadProjectSurveys(string projectId)
        {
            var surveys = await Surveys.ReadAll(projectId);
            return await Task.WhenAll(surveys.Select(async s => await DecorateSurvey(s)));
        }

        public async Task<Survey> CreateProjectSurvey(string projectId, Survey survey)
        {

            var createdSurvey = await Surveys.Create(projectId, survey);
            
            await Task.WhenAll(survey.Fields.Select(async field =>
                {
                    field.Language = survey.TextLanguage ?? Languages.Default;
                    var createdField = await SurveyFields.Create(createdSurvey.Id, field);
                    await Task.WhenAll(field.Choices.Select(async choice =>
                    {
                        choice.TextLanguage = survey.TextLanguage ?? Languages.Default;
                        await FieldChoices.Create(createdField.Id, choice);
                    }));
                })
            );

            return await ReadSurvey(projectId, createdSurvey.Id);
        }
        
        public async Task<Survey> UpdateProjectSurvey(string projectId, Survey survey)
        {
            var oldSurvey = new Survey();
            
            oldSurvey = await ReadSurvey(projectId, survey.Id);
            var updatedSurvey = await Surveys.Update(projectId, survey);

            // I presume new fields come with updated indices from client

            // Delete old field not part of new survey
            var fieldsToBeDeleted =
                oldSurvey.Fields.Where(oldField => survey.Fields.All(newField => newField.Id != oldField.Id));
            await Task.WhenAll(fieldsToBeDeleted.Select(async field => await DeleteField(survey.Id, field)));

            //Refetch the oldSurvey in case fields got removed
            oldSurvey = await ReadSurvey(projectId, survey.Id);

            //Delete old choices that are not part of new survey
            var choicesToBeDeleted = new List<FieldChoice>();
            foreach (var oldField in oldSurvey.Fields)
            {
                foreach (var newField in survey.Fields)
                {
                    if (newField.Id == oldField.Id)
                    {

                        var deletedChoices = oldField.Choices.Where(item1 => !newField.Choices.Any(item2 => item2.Id == item1.Id));
                        foreach (var deletedChoice in deletedChoices)
                        {
                            await DeleteFieldChoice(deletedChoice);
                        }
                    }
                }
            }
           
            // Update old field with new field properties
            await Task.WhenAll(survey.Fields.Select(async field =>
            {
                if (String.IsNullOrEmpty(field.Id)) return;
                var updatedField = await SurveyFields.Update(survey.Id, field);
                await Task.WhenAll(field.Choices.Select(async choice =>
                {
                    await FieldChoices.Delete(field.Id, choice.Id);
                }));
                await Task.WhenAll(field.Choices.Select(async choice =>
                {
                    if (choice.Id.ToLower().Contains("new"))
                    {
                        choice.Id = Guid.NewGuid().ToString();
                    }
                    await FieldChoices.Create(field.Id, choice);
                }));
            }));

            // Add new field not part of old survey
            //var fieldsToBeAdded = 
            //    survey.Fields.Where(newField => oldSurvey.Fields.All(oldField => oldField.Id != newField.Id));
            //await Task.WhenAll(fieldsToBeAdded.Select(async field => await CreateField(survey.Id, field)));
            
            return await ReadSurvey(projectId, updatedSurvey.Id);
        }

        //private async Task CreateField(string surveyId, SurveyField field)
        //{
        //    var createdField = await SurveyFields.Create(surveyId, field);
        //    await Task.WhenAll(field.Choices.Select(async choice =>
        //    {
        //        await FieldChoices.Create(createdField.Id, choice);
        //    }));
        //}

        public async Task<Survey> DeleteProjectSurvey(string projectId, string surveyId)
        {
            var survey = await Surveys.Read(projectId, surveyId);
            if (survey == null) return null;
            
            // Check if used by any strategy
            var strategies = await Strategies.ReadAll(projectId);
            var used = strategies.FirstOrDefault(strategy => strategy.Surveys.FirstOrDefault(s => s.Id == surveyId) != null);
            if (used != null) return null;
            
            // Delete if not used
            await Surveys.Delete(projectId, surveyId);
            var fields = (await SurveyFields.ReadAll(surveyId)).ToList();
            await Task.WhenAll(fields.Select(async f => await DeleteField(surveyId, f)));

            return survey;
        }

        private async Task DeleteField(string surveyId, SurveyField field)
        {
            var choices = await FieldChoices.ReadAll(field.Id);
            await Task.WhenAll(choices.Select(async c => await FieldChoices.Delete(field.Id, c.Id)));
            await SurveyFields.Delete(surveyId, field.Id);
        }

        private async Task DeleteFieldChoice(FieldChoice fieldChoice)
        {
            var choice = await FieldChoices.Read(fieldChoice.ParentId, fieldChoice.Id);
            await FieldChoices.Delete(fieldChoice.ParentId, choice.Id);
        }

        private async Task<Strategy> DecorateStrategy(Strategy strategy)
        {
            var strategyFrequencies = await BatchFrequencies.ReadAll(strategy.Id);
            var strategyEffects = await Effects.ReadAll(strategy.Id);
            strategy.Patients = await StrategyPatients.ReadAll(strategy.Id);
            strategy.Effects = strategyEffects.ToList();
            strategy.Frequencies = strategyFrequencies.ToList();
            for (int i = 0; i < strategy.Surveys.Count; i++)
            {
                if (strategy.Surveys[i] == null) continue;
                var validated = strategy.Surveys[i].validated;
                strategy.Surveys[i] = (SurveyProperty)await DecorateSurvey((Survey)strategy.Surveys[i]);
                strategy.Surveys[i].validated = validated;
            }
            strategy.Surveys = strategy.Surveys.OrderBy(s => s.Index).ToList();
            return strategy;
        }

        private async Task<Survey> DecorateSurvey(Survey survey)
        {
            var fields = await SurveyFields.ReadAll(survey.Id);

                survey.Fields = await Task.WhenAll(fields.Select(async f =>
                {
                    f.Choices = await FieldChoices.ReadAll(f.Id);
                    var fieldChoices = f.Choices as FieldChoice[] ?? f.Choices.ToArray();
                    f.Choices = fieldChoices.OrderBy(x => x.Index);
                    return f;
                }));

            return survey;
        }

        public async Task<List<Survey>> GetAllStrategySurveysByStrategyId(string projectId, string strategyId)
        {
            var strategy = await Strategies.Read(projectId, strategyId);
            if (strategy?.Surveys == null) return null;

            var surveys = new List<Survey>();
            foreach (var s in strategy.Surveys)
            {
                if (s == null) continue;
                var survey = (Survey)s;
                var surveyDescription = "";
                if (!s.ParentId.Equals("TEMPLATE"))
                {
                    var projectSurvey = await Strategies.Context.LoadAsync<Survey>(Surveys.PartitionKeyByParentId(projectId), Surveys.SortKeyById(s.Id));
                    surveyDescription = projectSurvey.Description;
                }
                survey.Description = surveyDescription;
                surveys.Add(survey);
            }

            return surveys;
        }

        public async Task<Survey> AddTemplateFields(string projectId, Survey survey)
        {
            foreach (var surveyField in survey.Fields)
            {
                if (surveyField.FieldTemplateId == null) continue;
                var fieldTemplate = await FieldTemplates.Read(projectId, surveyField.FieldTemplateId);
                surveyField.Choices = fieldTemplate.FieldChoices;
            }

            return survey;
        }
    }


    public class SurveyRepository : CrudPropertyRepository<Survey>
    {
        public override string ParentPrefix => Project.Prefix;
        public override string ModelPrefix => Survey.Prefix;
        public override string SortKeyValue(Survey model) => model.Name.ToString();
        
        public SurveyRepository(IAmazonDynamoDB client) : base(client)
        {
        }
    }

    public class SurveyQuestionRepository : CrudPropertyRepository<SurveyField>
    {
        public override string ParentPrefix => Survey.Prefix;
        public override string ModelPrefix => SurveyField.Prefix;
        public override string SortKeyValue(SurveyField model) => model.Index.ToString();

        public override bool Descending => false;

        public SurveyQuestionRepository(IAmazonDynamoDB client) : base(client)
        {
        }
    }

    public class SurveyOptionRepository : CrudPropertyRepository<FieldChoice>
    {
        public override string ParentPrefix => SurveyField.Prefix;
        public override string ModelPrefix => FieldChoice.Prefix;
        public override string SortKeyValue(FieldChoice model) => model.Index.ToString();

        public override bool Descending => false;

        public SurveyOptionRepository(IAmazonDynamoDB client) : base(client)
        {
        }
    }

    public class DataStrategyRepository : CrudPropertyRepository<Strategy>
    {
        public override string ParentPrefix => Project.Prefix;
        public override string ModelPrefix => Strategy.Prefix;
        public override string SortKeyValue(Strategy model) => model.Name;
        
        public override bool Descending => false;
        
        public DataStrategyRepository(IAmazonDynamoDB client) : base(client)
        {
        }
    }

    //public class FrequencyRepository : CrudPropertyRepository<StrategyFrequency>
    //{
    //    public override string ParentPrefix => Strategy.Prefix;
    //    public override string ModelPrefix => StrategyFrequency.Prefix;
        
    //    public FrequencyRepository(IAmazonDynamoDB client) : base(client)
    //    {
    //    }
    //}

    public class BatchFrequencyRepository : CrudPropertyRepository<BatchSendoutFrequency>
    {
        public override string ParentPrefix => Strategy.Prefix;
        public override string ModelPrefix => BatchSendoutFrequency.Prefix;

        public BatchFrequencyRepository(IAmazonDynamoDB client) : base(client)
        {
        }
    }

    public class StrategyPatients : CrudPropertyRepository<StrategyPatient>
    {
        public override string ParentPrefix => Strategy.Prefix;
        public override string ModelPrefix => ProjectPatient.Prefix;
        public override string SortKeyValue(StrategyPatient model) => model.Name;

        public override bool Descending => false;
        
        public StrategyPatients(IAmazonDynamoDB client) : base(client)
        {
        }
    }

    public class StrategyEffectRepository : CrudPropertyRepository<StrategyEffect>
    {
        public override string ParentPrefix => Strategy.Prefix;
        public override string ModelPrefix => StrategyEffect.Prefix;

        public StrategyEffectRepository(IAmazonDynamoDB client) : base(client)
        {
        }
    }

    public class FieldTemplateRepository : CrudPropertyRepository<FieldTemplate>
    {
        public FieldTemplateRepository(IAmazonDynamoDB client) : base(client)
        {
        }

        public override string ParentPrefix => Project.Prefix;
        public override string ModelPrefix => FieldTemplate.Prefix;
    }
}