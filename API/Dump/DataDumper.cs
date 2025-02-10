using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Transactions;
using API.Constants;
using API.Models.Analytics;
using API.Models.Dump;
using API.Models.Projects;
using API.Models.Views.AnalyticsReport.SROI;
using API.Repositories;
using Mapster;
using Microsoft.AspNetCore.Mvc.ModelBinding.Binders;
using Microsoft.IdentityModel.Tokens;
using Nest;

namespace API.Dump
{
    public interface IDataDumper
    {
        public Task<List<DumpData>> ReadDumpData(string projectId, DateTime start, DateTime end, string filter);
        public string CreateFileName(string projectName, string sortedBy);
    }

    public class DataDumpQuery
    {
        protected readonly IPatientContext _patientContext;
        protected readonly IAnalyticsContext _analyticsContext;
        protected readonly IStrategyContext _strategyContext;
        protected readonly ISystemMessage _message;
        public DataDumpQuery(IPatientContext patientContext, IAnalyticsContext analyticsContext,
            IStrategyContext strategyContext, ISystemMessage message)
        {
            _patientContext = patientContext;
            _analyticsContext = analyticsContext;
            _strategyContext = strategyContext;
            _message = message;
        }

        public async Task<List<ProjectPatient>> GetAllPatients(string projectId)
        {
            return (await _patientContext.ProjectPatients.ReadAll(projectId)).ToList();
        }
        public DumpData SetDumpDataBatch(ProjectPatient patient, EntryBatch entry)
        {
            var dumpData = new DumpData()
            {
                AnsweredAt = entry.AnsweredAt,
                Tags = entry.Tags,
                SurveyId = entry.SurveyId,
                ProjectId = entry.ProjectId,
                SurveyScore = entry.Score.ToString(CultureInfo.InvariantCulture),
                SurveyOrRegistration = _message.DumpField(DumpFields.Survey),
            };
            if (patient == null)
            {
                dumpData.PatientFirstName = string.Empty;
                dumpData.PatientLastName = string.Empty;
                dumpData.StrategyName = string.Empty;
                dumpData.Anonymity = false;
            }
            else
            {
                dumpData.PatientFirstName = patient.FirstName;
                dumpData.PatientLastName = patient.LastName;
                dumpData.StrategyName = patient.StrategyName;
                dumpData.Anonymity = patient.Anonymity;
            }

            return dumpData;
        }

        public DumpData SetDumpDataEntry(ProjectPatient patient, FieldEntry entry)
        {
            var dumpData = new DumpData()
            {
                AnsweredAt = entry.AnsweredAt,
                Tags = entry.Tags,
                Answer = entry.Text,
                FieldIndex = entry.FieldIndex,
                Question = entry.FieldText,
                SurveyId = entry.SurveyId,
                FieldId = entry.FieldId,
                ProjectId = entry.ProjectId,
                Index = entry.Index.ToString(),
                Value = entry.Value.ToString(CultureInfo.CurrentCulture),
                SurveyOrRegistration = _message.DumpField(DumpFields.Survey)
            };
            if (patient == null)
            {
                dumpData.PatientFirstName = string.Empty;
                dumpData.PatientLastName = string.Empty;
                dumpData.StrategyName = string.Empty;
                dumpData.Anonymity = false;
            }
            else
            {
                dumpData.PatientFirstName = patient.FirstName;
                dumpData.PatientLastName = patient.LastName;
                dumpData.StrategyName = patient.StrategyName;
                dumpData.Anonymity = patient.Anonymity;
            }


            return dumpData;
        }

        public DumpData SetDumpDataRegistration(ProjectPatient patient, Registration entry)
        {
            var dumpData = new DumpData()
            {
                AnsweredAt = entry.CreatedAt,
                Tags = entry.Tags,
                EffectName = entry.EffectName,
                EffectId = entry.EffectId,
                FieldId = entry.PatientId,
                ProjectId = entry.ProjectId,
                RegistrationType = entry.Type,
                RegistrationCategory = entry.Category,
                Value = entry.Value.ToString(CultureInfo.CurrentCulture),
                Note = entry.Note,
                SurveyOrRegistration = _message.DumpField(DumpFields.Registration)
            };
            if (dumpData.RegistrationType == _message.DumpField(DumpFields.RegistrationTypeStatus))
            {
                dumpData.Answer = dumpData.EffectName;
            }
            else if (dumpData.RegistrationType == _message.DumpField(DumpFields.RegistrationTypeNumeric))
            {
                dumpData.Answer = dumpData.Value.ToString(CultureInfo.InvariantCulture);
            }
            
            dumpData.Question = _message.GetRegistrationTypeName(dumpData.RegistrationType);
            if (patient == null)
            {
                dumpData.PatientFirstName = string.Empty;
                dumpData.PatientLastName = string.Empty;
                dumpData.StrategyName = string.Empty;
                dumpData.Anonymity = false;
            }
            else
            {
                dumpData.PatientFirstName = patient.FirstName;
                dumpData.PatientLastName = patient.LastName;
                dumpData.StrategyName = patient.StrategyName;
                dumpData.Anonymity = patient.Anonymity;
            }

            return dumpData;
        }


        public async Task<List<EntryBatch>> GetBatches(string projectId, List<string> patientIds, DateTime start, DateTime end)
        {
            var results = new List<EntryBatch>();
            foreach (var patientId in patientIds)
            {
                results.AddRange(await  _analyticsContext.EntryBatches.ReadBetween(new SurveyAccess()
                {
                    PatientId = patientId,
                    SearchStart = start,
                    SearchEnd = end,
                    ProjectId = projectId,
                }));
            }
            return results;
        }

        public async Task<List<FieldEntry>> GetEntriesIncludeBatch(string projectId, List<string> patientIds,
            DateTime start, DateTime end)
        {
            var batches = await GetBatches(projectId, patientIds, start, end);
            var fields = new List<FieldEntry>();
            foreach (var batch in batches)
            {
                var batchAsField = new FieldEntry()
                {
                    AnsweredAt = batch.AnsweredAt,
                    CreatedAt = batch.CreatedAt,
                    ParentId = batch.ParentId,
                    PatientId = batch.PatientId,
                    ProjectId = batch.ProjectId,
                    SurveyId = batch.SurveyId,
                    Tags = batch.Tags,
                    Index = -2,
                    FieldIndex = -2,
                    Value = batch.Score,
                };
                fields.Add(batchAsField);
                var items =  await _analyticsContext.FieldEntries.ReadAll(batch.Id);
                fields.AddRange(items.ToList());
            }

            return fields;
        }
        
        public async Task<List<FieldEntry>> GetEntries(string projectId, List<string> patientIds, DateTime start, DateTime end)
        {
            var batches = await GetBatches(projectId, patientIds, start, end);
            var fields = new List<FieldEntry>();
            foreach (var batch in batches)
            {
               var items =  await _analyticsContext.FieldEntries.ReadAll(batch.Id);
               fields.AddRange(items.ToList());
            }
            return fields;
        }

        public async Task<List<Registration>> GetRegistrations(string projectId, List<string> patientIds, DateTime start, DateTime end)
        {
            var results = new List<Registration>();
            foreach (var patientId in patientIds)
            {
                results.AddRange(await  _analyticsContext.Registrations.ReadBetween(new RegistrationAccess()
                {
                    ProjectId = projectId,
                    SearchStart = start,
                    SearchEnd = end,
                    PatientId = patientId,
                }));
            }

            return results;
        }


        public async Task<List<DumpData>> GetSurveyNames(List<DumpData> dumps)
        {
            foreach (var row in dumps)
            {
                var survey = await _strategyContext.Surveys.Read(row.ProjectId, row.SurveyId);
                if (survey == null)
                {
                    survey = await _strategyContext.Surveys.Read("TEMPLATE", row.SurveyId);
                    row.SurveyType = _message.DumpField(DumpFields.SurveyTypeValidated);
                }
                else
                {
                    row.SurveyType = _message.DumpField(DumpFields.SurveyTypeCustom);
                }

                row.SurveyName = survey?.Name;
            }

            return dumps;
        }

        public async Task<List<DumpData>> GetFieldType(List<DumpData> dumps)
        {
            foreach (var row in dumps)
            {
                var fields = await _strategyContext.SurveyFields.Read(row.SurveyId, row.FieldId);
                row.FieldType = fields?.Type;
            }

            return dumps;
        }
        
    }
    public class DumpDataEntries : IDataDumper
    {
        private readonly DataDumpQuery _query;
        private readonly ISystemMessage _message;

        public DumpDataEntries(IPatientContext patientContext, IAnalyticsContext analyticsContext, IStrategyContext strategyContext, ISystemMessage message)
        {
            _query = new DataDumpQuery(patientContext, analyticsContext, strategyContext, message);
            _message = message;
        }
        public string CreateFileName(string projectName, string sortedBy)
        {
            return "export_" + sortedBy.Replace(" ", "_").ToLower() + "-" + projectName.Replace(" ", "_").ToLower()
                   + "-" + DateTime.Now.Year + DateTime.Now.Month + DateTime.Now.Day + 
                   + DateTime.Now.Hour + DateTime.Now.Minute + DateTime.Now.Second + ".xlsx";
        }

        public async Task<List<DumpData>> ReadDumpData(string projectId, DateTime start, DateTime end, string filter)
        {
            var patients = await _query.GetAllPatients(projectId);
            var patientIds = patients.Select(p=>p.Id).ToList();
            var batches = await _query.GetBatches(projectId, patientIds, start, end);
            var dumpList = (from entry in batches
                let patient = patients.FirstOrDefault(s => s.Id == entry.PatientId)
                select _query.SetDumpDataBatch(patient, entry)).ToList();

            var searchResult = await _query.GetEntries(projectId, patientIds, start, end);

            var dumpEntries = (from entry in searchResult
                let patient = patients.FirstOrDefault(s => s.Id == entry.PatientId)
                select _query.SetDumpDataEntry(patient, entry)).ToList();

            dumpList.AddRange(dumpEntries);
            dumpList = await _query.GetSurveyNames(dumpList);
            dumpList = await _query.GetFieldType(dumpList);

            var searchRegistrations = await _query.GetRegistrations(projectId, patientIds, start, end);
            var registrationList = (from registration in searchRegistrations
                let patient =
                    patients.FirstOrDefault(s => s.Id == registration.PatientId)
                select _query.SetDumpDataRegistration(
                    patient, registration)).ToList();
            dumpList.AddRange(registrationList);
            return dumpList;
        }

    }

    public class DumpDataRaw : IDataDumper
    {
        private readonly DataDumpQuery _query;
        private readonly ISystemMessage _message;

        public DumpDataRaw(IPatientContext patientContext, IAnalyticsContext analyticsContext, IStrategyContext strategyContext, ISystemMessage message)
        {
            _query = new DataDumpQuery(patientContext, analyticsContext, strategyContext, message);
            _message = message;
        }
        
        public string CreateFileName(string projectName, string sortedBy)
         {
             return "export_raw-"  + projectName.Replace(" ", "_").ToLower()
                    + "-" + DateTime.Now.Year + DateTime.Now.Month + DateTime.Now.Day + 
                    DateTime.Now.Hour + DateTime.Now.Minute + DateTime.Now.Second + ".xlsx";
         }

         public async Task<List<DumpData>> ReadDumpData(string projectId, DateTime start, DateTime end, string filter)
         {
             var dumpList = new List<DumpData>();
             var dumpSurveys = new List<DumpData>();
             var dumpRegistrations = new List<DumpData>();
             var patients = await _query.GetAllPatients(projectId);
             var patientIds = patients.Select(p=>p.Id).ToList();
             List<FieldEntry> searchSurveys;
             List<DumpData> dumpEntries;
             List<Registration> searchRegistrations;
             var filterName = _message.DumpFieldMapToFieldName(filter);
             switch (filterName)
             {
                 case DumpFields.FilterOnlyPatients:
                     dumpList.AddRange(patients.Select(SetDumpDataPatientOnly));
                     break;
                 case DumpFields.FilterCustom:
                     searchSurveys = await _query.GetEntriesIncludeBatch(projectId, patientIds, start, end);
                     var customs = searchSurveys.Where(e =>
                         ValidatedSurveys.ifValidatedSurvey(_message.GetLanguageName(), e.SurveyId)).ToList();
                     dumpEntries = (from entry in customs
                         let patient = patients.FirstOrDefault(s => s.Id == entry.PatientId)
                         select SetDumpDataEntry(patient, entry)).ToList();
                     dumpSurveys.AddRange(dumpEntries);
                     break;
                 case DumpFields.FilterValidated:
                     searchSurveys = await _query.GetEntriesIncludeBatch(projectId, patientIds, start, end);
                     var validates = searchSurveys.Where(e =>
                         !ValidatedSurveys.ifValidatedSurvey(_message.GetLanguageName(), e.SurveyId)).ToList();
                     dumpEntries = (from entry in validates
                         let patient = patients.FirstOrDefault(s => s.Id == entry.PatientId)
                         select SetDumpDataEntry(patient, entry)).ToList();
                     dumpSurveys.AddRange(dumpEntries);
                     break;
                 case DumpFields.FilterAllSurvey:
                     searchSurveys = await _query.GetEntriesIncludeBatch(projectId, patientIds, start, end);
                     dumpEntries = (from entry in searchSurveys
                         let patient = patients.FirstOrDefault(s => s.Id == entry.PatientId)
                         select SetDumpDataEntry(patient, entry)).ToList();
                     dumpSurveys.AddRange(dumpEntries);
                     break;
                 case DumpFields.FilterIncidentRegistration:
                     searchRegistrations = await _query.GetRegistrations(projectId, patientIds, start, end);
                     var incidents = searchRegistrations.Where(r => r.Type == DumpFields.RegistrationTypeIncident);
                     dumpEntries = (from registration in incidents
                         let patient =
                             patients.FirstOrDefault(s => s.Id == registration.PatientId)
                         select SetDumpDataRegistration(
                             patient, registration)).ToList();
                     dumpRegistrations.AddRange(dumpEntries);
                     break;
                 case DumpFields.FilterNumericRegistration:
                     searchRegistrations = await _query.GetRegistrations(projectId, patientIds, start, end);
                     var numerics = searchRegistrations.Where(r => r.Type == DumpFields.FilterNumericRegistration);
                     dumpEntries = (from registration in numerics
                         let patient =
                             patients.FirstOrDefault(s => s.Id == registration.PatientId)
                         select SetDumpDataRegistration(
                             patient, registration)).ToList();
                     dumpRegistrations.AddRange(dumpEntries);
                     break;
                 case DumpFields.FilterStatusRegistration:
                     searchRegistrations = await _query.GetRegistrations(projectId, patientIds, start, end);
                     var statuses = searchRegistrations.Where(r => r.Type == DumpFields.FilterStatusRegistration);
                     dumpEntries = (from registration in statuses
                         let patient =
                             patients.FirstOrDefault(s => s.Id == registration.PatientId)
                         select SetDumpDataRegistration(
                             patient, registration)).ToList();
                     dumpRegistrations.AddRange(dumpEntries);
                     break;
                 case DumpFields.FilterAllRegistration:
                     searchRegistrations = await _query.GetRegistrations(projectId, patientIds, start, end);
                     dumpEntries = (from registration in searchRegistrations
                         let patient =
                             patients.FirstOrDefault(s => s.Id == registration.PatientId)
                         select SetDumpDataRegistration(
                             patient, registration)).ToList();
                     dumpRegistrations.AddRange(dumpEntries);
                     break;
                 default:
                     searchSurveys = await _query.GetEntriesIncludeBatch(projectId, patientIds, start, end);
                     dumpEntries = (from entry in searchSurveys
                         let patient = patients.FirstOrDefault(s => s.Id == entry.PatientId)
                         select SetDumpDataEntry(patient, entry)).ToList();
                     dumpSurveys.AddRange(dumpEntries);
                     searchRegistrations = await _query.GetRegistrations(projectId, patientIds, start, end);
                     dumpEntries = (from registration in searchRegistrations
                         let patient =
                             patients.FirstOrDefault(s => s.Id == registration.PatientId)
                         select SetDumpDataRegistration(
                             patient, registration)).ToList();
                     dumpRegistrations.AddRange(dumpEntries);
                     break;
             }
             
             dumpSurveys = await _query.GetSurveyNames(dumpSurveys);
             dumpSurveys = await _query.GetFieldType(dumpSurveys);
             dumpSurveys = OrderDumpData(dumpSurveys);
             dumpList.AddRange(dumpSurveys);
             dumpList.AddRange(dumpRegistrations);
             return dumpList;
         }
         
        private new DumpData SetDumpDataEntry(ProjectPatient patient, FieldEntry entry)
        {
            patient ??= new ProjectPatient();
            var dumpData = new DumpData()
            {
                AnsweredAt = entry.AnsweredAt,
                PatientId  = patient.Id,
                Tags = entry.Tags,
                Answer = entry.Text,
                SurveyId = entry.SurveyId,
                FieldId = entry.FieldId,
                FieldText = entry.FieldText,
                FieldIndex = entry.FieldIndex,
                ProjectId = entry.ProjectId,
                Index = entry.Index.ToString(),
                SurveyScore = entry.FieldIndex == -2 ? entry.Value.ToString(CultureInfo.CurrentCulture):string.Empty,
                SurveyOrRegistration = _message.DumpField(DumpFields.Survey),
                ChoiceId = entry.ChoiceId,
                ChoiceText = entry.ChoiceText,
                ChoiceIndex = entry.Index,
                Value = entry.Value.ToString(CultureInfo.CurrentCulture),
                FreeText = entry.ChoiceId.IsNullOrEmpty()? entry.Text: string.Empty,
            };
            dumpData.PatientFirstName = patient.FirstName;
            dumpData.PatientLastName = patient.LastName;
            dumpData.StrategyName = patient.StrategyName;
            dumpData.Anonymity = patient.Anonymity;
            dumpData.PatientEmail = patient.Email;
            dumpData.PatientGender = patient.Sex;
            dumpData.PatientPhone = patient.PhoneNumber;
            dumpData.PatientEmail = patient.Email;
            dumpData.PatientIsActive = patient.IsActive.ToString();
            dumpData.PatientPostcode = patient.PostalNumber;
            dumpData.PatientRegion = patient.Region;
            dumpData.PatientCity = patient.Municipality;
            if (patient.BirthDate != null)
                dumpData.PatientAge = (DateTime.Now.Year - patient.BirthDate.Value.Year).ToString();

            return dumpData;
        }

        
        private new DumpData SetDumpDataRegistration(ProjectPatient patient, Registration entry)
        {
            patient ??= new ProjectPatient();
            var dumpData = new DumpData()
            {
                AnsweredAt = entry.CreatedAt,
                Tags = entry.Tags,
                EffectName = entry.EffectName,
                EffectId = entry.EffectId,
                FieldId = entry.PatientId,
                ProjectId = entry.ProjectId,
                RegistrationType = entry.Type,
                RegistrationCategory = entry.Category,
                Value = entry.Value.ToString(CultureInfo.InvariantCulture),
                Note = entry.Note,
                SurveyOrRegistration = _message.DumpField(DumpFields.Registration),
                PatientId  = patient.Id,
                SurveyType = string.Empty,
                FieldIndex = -2,
                ChoiceIndex = -1,
            };
            if (dumpData.RegistrationType == _message.DumpField(DumpFields.RegistrationTypeStatus))
            {
                dumpData.Answer = dumpData.EffectName;
            }
            else if (dumpData.RegistrationType == _message.DumpField(DumpFields.RegistrationTypeNumeric))
            {
                dumpData.Answer = dumpData.Value.ToString(CultureInfo.InvariantCulture);
            }

            dumpData.PatientFirstName = patient.FirstName;
            dumpData.PatientLastName = patient.LastName;
            dumpData.StrategyName = patient.StrategyName;
            dumpData.Anonymity = patient.Anonymity;
            dumpData.PatientEmail = patient.Email;
            dumpData.PatientGender = patient.Sex;
            dumpData.PatientPhone = patient.PhoneNumber;
            dumpData.PatientEmail = patient.Email;
            dumpData.PatientIsActive = patient.IsActive.ToString();
            dumpData.PatientPostcode = patient.PostalNumber;
            dumpData.PatientRegion = patient.Region;
            dumpData.PatientCity = patient.Municipality;
            if (patient.BirthDate != null)
                dumpData.PatientAge = (DateTime.Now.Year - patient.BirthDate.Value.Year).ToString();

            return dumpData;
        }

        private DumpData SetDumpDataPatientOnly(ProjectPatient patient)
        {
            var dumpData = new DumpData()
            {
                AnsweredAt = patient.CreatedAt,
                PatientId  = patient.Id,
                Tags = new List<string>(),
                Answer = string.Empty,
                SurveyId = string.Empty,
                FieldId = string.Empty,
                FieldText = string.Empty,
                FieldIndex = -2,
                ProjectId = patient.ParentId,
                Index = string.Empty,
                Value = string.Empty,
                SurveyOrRegistration = string.Empty,
                ChoiceId = string.Empty,
                ChoiceText = string.Empty,
                ChoiceIndex = -2,
                FreeText = string.Empty,
            };
            dumpData.PatientFirstName = patient.FirstName;
            dumpData.PatientLastName = patient.LastName;
            dumpData.StrategyName = patient.StrategyName;
            dumpData.Anonymity = patient.Anonymity;
            dumpData.PatientEmail = patient.Email;
            dumpData.PatientGender = patient.Sex;
            dumpData.PatientPhone = patient.PhoneNumber;
            dumpData.PatientEmail = patient.Email;
            dumpData.PatientIsActive = patient.IsActive.ToString();
            dumpData.PatientPostcode = patient.PostalNumber;
            dumpData.PatientRegion = patient.Region;
            dumpData.PatientCity = patient.Municipality;
            return dumpData;
        }

        private static List<DumpData> OrderDumpData(IEnumerable<DumpData> dumpDataList)
        {

            return dumpDataList.OrderBy(x => x.PatientLastName)
                .ThenBy(x => x.SurveyName)
                .ThenBy(x => x.FieldIndex)
                .GroupBy(x=>x.AnsweredAt)
                .SelectMany(g=>g)
                .ToList();
        }
    }
}