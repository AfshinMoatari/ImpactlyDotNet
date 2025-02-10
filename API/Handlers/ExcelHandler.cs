using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using API.Constants;
using API.Models.Dump;
using API.Helpers;
using FastExcel;
using Microsoft.IdentityModel.Tokens;


namespace API.Handlers
{

    public interface IExcelHandler
    {

        public bool Anonymity { get; set; }
        public ISystemMessage Message { get; set; }
        public Task<string> CreateExcel(ExcelInput excelInput, string fileName);

    }
    

    public class ExportExcelHandler: IExcelHandler
    {
        private readonly IS3Helper _s3Helper;
        public bool Anonymity { get; set; }
        public ISystemMessage Message { get; set; }

        public ExportExcelHandler(IS3Helper s3Helper)
        {
            _s3Helper = s3Helper;
        }

        public async Task<string> CreateExcel(ExcelInput excelInput, string fileName)
        {
            Message ??= new MessageEnglish();
            var folder = Path.Combine(Directory.GetCurrentDirectory(), "Files");
            
            var fileInfo = new FileInfo(Path.Combine(folder,
                fileName));
            IWorkSheetWriter workSheetWriter = null;
            if (excelInput.Type == Message.DumpField(DumpFields.DataEntries))
            {
                workSheetWriter = new DataEntriesWorksheet(_s3Helper);
            }
            else if (excelInput.Type == Message.DumpField(DumpFields.DataStrategies))
            {
                workSheetWriter = new DataStrategiesWorksheet(_s3Helper);
            }
            else if (excelInput.Type == ExcelInput.ExcelTypeImportPatientsLog)
            {
                workSheetWriter = new ImportPatientsLogWorksheet(_s3Helper);
            }
            else if (excelInput.Type == ExcelInput.ExcelTypeDataDumpRaw)
            {
                workSheetWriter = new DataRawWorkSheet(_s3Helper);
            }
            if (workSheetWriter == null) return string.Empty;
            workSheetWriter.Anonymity = Anonymity;
            workSheetWriter.Message = Message;
            using var fastExcel = new FastExcel.FastExcel(await workSheetWriter.GetTemplateFileInfo(folder), fileInfo);
            var worksheet = workSheetWriter.WriteWorksheet(excelInput);
            fastExcel.Write(worksheet,  excelInput.Type);
            fastExcel.Dispose();
            var ur = await _s3Helper.CreateS3File(string.Empty, fileInfo, S3Buckets.DataExport);
            //return FileStorage.DownloadUrl + fileInfo.Name;

            return ur;
        }


        
    }


    
    internal interface IWorkSheetWriter
    {
        bool Anonymity { get; set; }
        ISystemMessage Message { get; set; }
        Task<FileInfo> GetTemplateFileInfo(string folder);
        Worksheet WriteWorksheet(ExcelInput excelInput);
    }
    
 
    
    internal abstract class BaseWorkSheet : IWorkSheetWriter
    {
        public bool Anonymity { get; set; }
        public ISystemMessage Message { get; set; }
        public abstract Task<FileInfo> GetTemplateFileInfo(string folder);
        public abstract Worksheet WriteWorksheet(ExcelInput excelInput);

        protected IEnumerable<DumpData> OrderDumpData(DumpRequest request, IEnumerable<DumpData> data)
        {

            var ordered = data.OrderBy(d => d.AnsweredAt).ThenBy(d=>d.SurveyId).ThenBy(d=>d.PatientFirstName).ThenBy(d => d.FieldIndex);

            return ordered;
            //return request.OrderBy switch
            //{
            //    DumpFields.PatientLastName => data.OrderBy(o => o.PatientLastName),
            //     DumpFields.AnsweredAt => data.OrderBy(o => o.AnsweredAt),
            //    DumpFields.Strategy => data.OrderBy(o => o.StrategyName),
            //    DumpFields.SurveyAndRegistration => data.OrderBy(o => o.SurveyName),
            //    DumpFields.Questions => data.OrderBy(o => o.Question),
            //    DumpFields.Answers => data.OrderBy(o => o.Answer),
            //    DumpFields.Tags => data.OrderBy(o => o.Tags),
            //    DumpFields.Index => data.OrderBy(o => o.Index),
            //    _ => data
            //};
        }
        
        
        protected List<Row> CreateRowsAndAddRequestInfo(DumpRequest request)
        {
            var rows = new List<Row>();
            var cells = new List<Cell> { new(1, Message.DumpField(DumpFields.ProjectName) + ": "), new(2, request.ProjectName) };
            rows.Add(new Row(1, cells));
            cells = new List<Cell> { new(1, Message.DumpField(DumpFields.ExportedBy) + ": "), new(2, request.UserName)};
            rows.Add(new Row(2, cells));
            cells = new List<Cell> { new(1, Message.DumpField(DumpFields.SortedBy) + ": "), new(2, request.SortedBy)};
            rows.Add(new Row(3, cells));
            cells = new List<Cell> { new(1, Message.DumpField(DumpFields.Filter) + ": "), new(2, $"{request.Filter}, {request.StartDate:dd/MM/yyyy} - {request.EndDate:dd/MM/yyyy}") };
            rows.Add(new Row(4, cells));
            cells = new List<Cell> { new(1, Message.DumpField(DumpFields.Data) + ": "), new(2, request.AllFieldsToString()) };
            rows.Add(new Row(5, cells));
            return rows;
        }
        
        protected List<Cell> AddHeaders(List<string> data)
        {
            var cells = new List<Cell>();
            var i = 1;
            foreach (var field in data)
            {
                cells.Add(new Cell(i, field));
                i++;
            }
            return cells;
        }
        
        protected List<Cell> AddContents(List<string> requestFields, DumpData line)
        {
            var cells = new List<Cell>();
            var i = 1;
            foreach (var field in requestFields)
            {
                cells.Add(new Cell(i, MapDumpData(line, field)));
                i++;
            }

            return cells;
        }

        private string MapDumpData(DumpData line, string fieldName)
        {
            if (fieldName == Message.DumpField(DumpFields.Answers))
                return line.Answer;
            if (fieldName == Message.DumpField(DumpFields.QuestionNumber))
                return string.IsNullOrEmpty(line.Index) ? string.Empty: (line.FieldIndex + 1).ToString();
            if (fieldName ==  Message.DumpField(DumpFields.Questions))
                return line.Question ?? line.SurveyName;
            if (fieldName == Message.DumpField(DumpFields.Strategy))
                return line.StrategyName;
            if (fieldName ==  Message.DumpField(DumpFields.Tags))
                return line.TagsToString();
            if (fieldName == Message.DumpField(DumpFields.AnsweredAt))
                return line.AnsweredAt.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture);
            if (fieldName == Message.DumpField(DumpFields.PatientFirstName))
                return IfAnonymity(line) ? AnonymityMessage.HiddingMessage: line.PatientFirstName;
            if (fieldName == Message.DumpField(DumpFields.PatientLastName))
                return IfAnonymity(line) ? AnonymityMessage.HiddingMessage: line.PatientLastName;
            if (fieldName == Message.DumpField(DumpFields.SurveyAndRegistration))
                return line.SurveyName ?? Message.GetRegistrationTypeName(line.RegistrationType);
            if (fieldName == Message.DumpField(DumpFields.Index))
                return line.Index ?? string.Empty;
            if (fieldName == Message.DumpField(DumpFields.SurveyScore))
                return line.SurveyScore ?? string.Empty;
            return string.Empty;
        }

        public int AddSurveys(List<string> surveys, List<Row> rows, int rowNumber, DumpRequest request, List<DumpData> data)
        {
            foreach (var survey in surveys)
            {
                var cells = new List<Cell> { new(1, Message.DumpField(DumpFields.Survey) + ": " + survey) };
                rows.Add(new Row(rowNumber, cells));
                rowNumber++;
                cells = AddHeaders(request.Fields);
                rows.Add(new Row(rowNumber, cells));
                var contents = data.Where(s => s.SurveyName == survey);
                contents = OrderDumpData(request, contents);
                foreach (var line in contents)
                {
                    rowNumber++;
                    cells = AddContents(request.Fields, line);
                    rows.Add(new Row(rowNumber, cells));
                }

                rowNumber += 3;
            }

            return rowNumber;
        }
        
        public int AddRegistration(string registrationType, List<Row> rows, int rowNumber, DumpRequest request, List<DumpData> data)
        {
            if (data == null)
            {
                return rowNumber;
            }
            var contents = data.Where(s => s.RegistrationType == registrationType);
            var dumpDatas = contents.ToList();
            if (!dumpDatas.Any())
                return rowNumber;
            var cells = new List<Cell> { new(1, Message.DumpField(DumpFields.Registration) + ": " + Message.GetRegistrationTypeName(registrationType)) };
            rows.Add(new Row(rowNumber, cells));
            rowNumber++;
            cells = AddHeaders(request.Fields);
            rows.Add(new Row(rowNumber, cells));
            
            contents = OrderDumpData(request, dumpDatas);
            foreach (var line in contents)
            {
                rowNumber++;
                cells = AddContents(request.Fields, line);
                rows.Add(new Row(rowNumber, cells));
            }

            rowNumber += 3;
            return rowNumber;
        }

        protected bool IfAnonymity(DumpData data)
        {
            return Anonymity && data.Anonymity;
        }
        
    }

    internal class DataRawWorkSheet : IWorkSheetWriter
    {
        public bool Anonymity { get; set; }
        public ISystemMessage Message { get; set; }
        private readonly IS3Helper _s3Helper;

        public DataRawWorkSheet()
        { }

        public DataRawWorkSheet(IS3Helper s3Helper)
        {
            _s3Helper = s3Helper;
        }

        public async Task<FileInfo> GetTemplateFileInfo(string folder)
        {
            var templateFile = Message.GetLanguageName() == Languages.Danish
                ? "rawdatatemplate.xlsx"
                : "rawdatatemplateen.xlsx";
            var path = Path.Combine(folder, templateFile);
            if (!File.Exists(path))
            {
                await _s3Helper.ReadS3AndSave(templateFile, folder, S3Buckets.DataExport);
            }
            return new FileInfo(path);

        }

        public Worksheet WriteWorksheet(ExcelInput excelInput)
        {
            var worksheet = new Worksheet();
            var rows = new List<Row> { AddHeaders() };
            var i = 1;
            foreach (var item in excelInput.DumpDataList)
            {
                i++;
                var registrationValue = item.RegistrationType switch
                {
                    "status" => item.EffectName,
                    "count" => "1",
                    _ => item.Value
                };
                var cells = new List<Cell>()
                {
                    new(1, item.PatientId),
                    new(2, excelInput.ProjectName),
                    new(3, item.PatientPostcode),
                    new(4, item.PatientFirstName),
                    new(5, item.PatientLastName),
                    new(6, item.PatientIsActive),
                    new(7, item.StrategyName),
                    new(8, item.TagsToString()),
                    new(9, item.PatientEmail),
                    new(10, item.PatientPhone),
                    new(11, item.PatientCity),
                    new(12, item.PatientRegion),
                    new(13, item.PatientGender),
                    new(14, item.PatientAge),
                    new(15, item.AnsweredAt.ToString(CultureInfo.CurrentCulture)),
                    new(16, ""),
                    new(17, item.SurveyName),
                    new(18, item.SurveyId),
                    new(19, item.SurveyId.IsNullOrEmpty()?string.Empty: item.SurveyType),
                    new(20, item.FieldText),
                    new(21, item.FieldIndex >= 0  ? item.FieldIndex.ToString(): string.Empty),
                    new(22, item.FieldType),   
                    new(23, item.ChoiceText),
                    new(24, item.Value),
                    new(25, item.FreeText),
                    new(26, item.SurveyScore),
                    new(27, item.RegistrationType),
                    new(28, item.RegistrationType=="status"?item.RegistrationCategory: item.EffectName),
                    new(29, registrationValue),
                };
                rows.Add(new Row(i, cells));
            }

            worksheet.Rows = rows;
            return worksheet;
        }

        private Row AddHeaders()
        {
            var cells = new List<Cell>()
            {
                new(1, "PatientId"),
                new(2, "Project"),
                new(3, "Postal code"),
                new(4, "First name"),
                new(5, "Last name"),
                new(6, "Active/inactive"),
                new(7, "Strategy"),
                new(8, "Tags"),
                new(9, "Email"),
                new(10, "Telephone"),
                new(11, "City"),
                new(12, "Region"),
                new(13, "Gender"),
                new(14, "Age"),
                new(15, "Date"),
                new(16, "Touch point"),
                new(17, "Questionnaire"),
                new(18, "Questionnaire Id"),
                new(19, "Questionnaire type"),
                new(20, "Question"),
                new(21, "Question index"),
                new(22, "Question type"),
                new(23, "Choice text"),
                new(24, "Choice value"),
                new(25, "Free text"),
                new(26, "Score"),
                new(27, "Registration type"),
                new(28, "Registration name"),
                new(29, "Registration input"),

            };
            return new Row(1, cells);
        }
    }

    internal class ImportPatientsLogWorksheet : IWorkSheetWriter
    {
        public bool Anonymity { get; set; }
        public ISystemMessage Message { get; set; }
        private readonly IS3Helper _s3Helper;

        public ImportPatientsLogWorksheet(IS3Helper s3Helper)
        {
            _s3Helper = s3Helper;
        }
        public ImportPatientsLogWorksheet() {}

        public async Task<FileInfo> GetTemplateFileInfo(string folder)
        {
            var templateFile = Message.GetLanguageName() == Languages.Danish
                ? "logtemplate1.xlsx"
                : "logtemplate1en.xlsx";
            var path = Path.Combine(folder, templateFile);
            if (!File.Exists(path))
            {
                await _s3Helper.ReadS3AndSave(templateFile, folder, S3Buckets.DataExport);
            }
            return new FileInfo(path);
            
        }

        public Worksheet WriteWorksheet(ExcelInput excelInput)
        {
            var importPatients = excelInput.ImportPatients;
            var worksheet = new Worksheet();

            var rows = new List<Row>();
            var cells = new List<Cell> { new(1, Message.DumpField(DumpFields.ProjectName) + ": "), new(2, excelInput.ProjectName) };
            rows.Add(new Row(1, cells));
            cells = new List<Cell> { new(1, Message.DumpField(DumpFields.ExportedBy) + ": "), new(2, excelInput.UserName)};
            rows.Add(new Row(2, cells));
            cells = new List<Cell> { new(1,  Message.DumpField(DumpFields.ImportTime) + ": "), new(2, DateTime.Now.ToString(CultureInfo.InvariantCulture))};
            rows.Add(new Row(3, cells));

            var rowNumber = 7;
            cells = new List<Cell> { new(1, Message.DumpField(DumpFields.SkippedOrFailed)) };
            rows.Add(new Row(rowNumber, cells));
            var skippedColumns = ImportPatient.GetColumnNamesForSkipped();
            rowNumber++;
            cells = AddCells(skippedColumns);
            rows.Add(new Row(rowNumber, cells));
            var skips = importPatients.Where(p => p.Status is ImportPatient.ImportStatusSkipped or ImportPatient.ImportStatusInsertFailed);
            foreach (var item in skips)
            {
                rowNumber++;
                cells = AddCells(item.GetImportSkipped());
                rows.Add(new Row(rowNumber, cells));
            }

            rowNumber += 3;
            cells = new List<Cell> { new(1, Message.DumpField(DumpFields.Inserted)) };
            rows.Add(new Row(rowNumber, cells));
            var insertedColumns = ImportPatient.GetColumnNamesForInserted();
            rowNumber++;
            cells = AddCells(insertedColumns);
            rows.Add(new Row(rowNumber, cells));
            var inserted = importPatients.Where(p => p.Status == ImportPatient.ImportStatusInserted);
            foreach (var item in inserted)
            {
                rowNumber++;
                cells = AddCells(item.GetImportInserted());
                rows.Add(new Row(rowNumber, cells));
            }
            
            rowNumber += 3;
            cells = new List<Cell> { new(1, Message.DumpField(DumpFields.Updated)) };
            rows.Add(new Row(rowNumber, cells));
            var updatedColumns = ImportPatient.GetColumnNamesForUpdated();
            rowNumber++;
            cells = AddCells(updatedColumns);
            rows.Add(new Row(rowNumber, cells));
            var updated = importPatients.Where(p => p.Status == ImportPatient.ImportStatusUpdated);
            foreach (var item in updated)
            {
                rowNumber++;
                cells = AddCells(item.GetImportUpdated());
                rows.Add(new Row(rowNumber, cells));
            }

            worksheet.Rows = rows;
            
            return worksheet;
        }
        
        protected List<Cell> AddCells(List<string> data)
        {
            var cells = new List<Cell>();
            var i = 1;
            foreach (var field in data)
            {
                cells.Add(new Cell(i, field));
                i++;
            }
            return cells;
        } 
    }
     
    internal class DataEntriesWorksheet : BaseWorkSheet
    {
        private readonly IS3Helper _s3Helper;

        public DataEntriesWorksheet(IS3Helper s3Helper)
        {
            _s3Helper = s3Helper;
        }
        public DataEntriesWorksheet(){}

        public override async Task<FileInfo> GetTemplateFileInfo(string folder)
        {
            var templateFile = this.Message.GetLanguageName() == Languages.Danish
                ? "template1.xlsx"
                : "template1en.xlsx";
            var path = Path.Combine(folder, templateFile);
            if (!File.Exists(path))
            {
                await _s3Helper.ReadS3AndSave(templateFile, folder, S3Buckets.DataExport);
            }
            return new FileInfo(path);
        }
        public override Worksheet WriteWorksheet(ExcelInput excelInput)
        {            
            
            var request = excelInput.DumpRequest;
            var data = excelInput.DumpDataList;
            var worksheet = new Worksheet();
            
            var rows = CreateRowsAndAddRequestInfo(request);
            
            var rowNumber = rows.Count + 3;

            if (request.Filter == Message.DumpField(DumpFields.FilterAllSurvey))
            {
                var surveysAll = data.Where(d => d.SurveyOrRegistration == Message.DumpField(DumpFields.Survey)).Select(s => s.SurveyName).Distinct().ToList();
                AddSurveys(surveysAll, rows, rowNumber, request, data);
            }
            else if (request.Filter == Message.DumpField(DumpFields.FilterValidated))
            {
                var surveysValidated = data.Where(d => d.SurveyOrRegistration == Message.DumpField(DumpFields.Survey) && d.SurveyType == Message.DumpField(DumpFields.SurveyTypeValidated)).Select(s => s.SurveyName).Distinct().ToList();
                AddSurveys(surveysValidated, rows, rowNumber, request, data);
            }
            else if (request.Filter == Message.DumpField(DumpFields.FilterCustom))
            {
                var surveysCustom = data.Where(d => d.SurveyOrRegistration == Message.DumpField(DumpFields.Survey) && d.SurveyType == Message.DumpField(DumpFields.SurveyTypeCustom)).Select(s => s.SurveyName).Distinct().ToList();
                AddSurveys(surveysCustom, rows, rowNumber, request, data);
            }
            else if (request.Filter == Message.DumpField(DumpFields.FilterIncidentRegistration))
            {
                AddRegistration(Message.DumpField(DumpFields.RegistrationTypeIncident), rows, rowNumber, request, data);
            }
            else if (request.Filter == Message.DumpField(DumpFields.FilterNumericRegistration))
            {
                AddRegistration(Message.DumpField(DumpFields.RegistrationTypeNumeric), rows, rowNumber, request, data);
            }
            else if (request.Filter == Message.DumpField(DumpFields.FilterStatusRegistration))
            {
                AddRegistration(Message.DumpField(DumpFields.RegistrationTypeStatus), rows, rowNumber, request, data);
            }
            else if (request.Filter == Message.DumpField(DumpFields.FilterAllRegistration))
            {
                rowNumber = AddRegistration(Message.DumpField(DumpFields.RegistrationTypeIncident), rows, rowNumber, request, data);
                rowNumber = AddRegistration(Message.DumpField(DumpFields.RegistrationTypeNumeric), rows, rowNumber, request, data);
                AddRegistration(Message.DumpField(DumpFields.RegistrationTypeStatus), rows, rowNumber, request, data);
            }
            else
            {
                var surveys = data.Where(d => d.SurveyOrRegistration == Message.DumpField(DumpFields.Survey)).Select(s => s.SurveyName).Distinct().ToList();
                rowNumber = AddSurveys(surveys, rows, rowNumber, request, data);
                rowNumber = AddRegistration(Message.DumpField(DumpFields.RegistrationTypeIncident), rows, rowNumber, request, data);
                rowNumber = AddRegistration(Message.DumpField(DumpFields.RegistrationTypeNumeric), rows, rowNumber, request, data);
                AddRegistration(Message.DumpField(DumpFields.RegistrationTypeStatus), rows, rowNumber, request, data);

            }
            
            
            worksheet.Rows = rows;

            return worksheet;

        }

    }

    internal class DataStrategiesWorksheet : BaseWorkSheet
    {
        private readonly IS3Helper _s3Helper;

        public DataStrategiesWorksheet(IS3Helper s3Helper)
        {
            _s3Helper = s3Helper;
        }
        public DataStrategiesWorksheet() { }

        public override async Task<FileInfo> GetTemplateFileInfo(string folder)
        {
            var templateFile = this.Message.GetLanguageName() == Languages.Danish
                ? "template2.xlsx"
                : "template2en.xlsx";
            var path = Path.Combine(folder, templateFile);
            if (!File.Exists(path))
            {
                await _s3Helper.ReadS3AndSave(templateFile, folder, S3Buckets.DataExport);
            }
            return new FileInfo(path);
        }
        public override Worksheet WriteWorksheet(ExcelInput excelInput)
        {

            var request = excelInput.DumpRequest;
            var data = excelInput.DumpDataList;
            
            var worksheet = new Worksheet();
            
            var rows = CreateRowsAndAddRequestInfo(request);
            var rowNumber = rows.Count + 3;
            var strategies = data.Select(s => s.StrategyName).Distinct().ToList();
            foreach (var strategy in strategies)
            {
                var cells = new List<Cell> { new(1, DumpFields.Strategy + ": " + strategy) };
                rows.Add(new Row(rowNumber, cells));
                rowNumber++;
                cells = AddHeaders(request.Fields);
                rows.Add(new Row(rowNumber, cells));
                var contents = data.Where(s => s.StrategyName == strategy);
                contents = request.Filter switch
                {
                    DumpFields.FilterAllSurvey => contents.Where(s => s.SurveyOrRegistration == DumpFields.Survey),
                    DumpFields.FilterValidated => contents.Where(s =>
                        s.SurveyOrRegistration == DumpFields.Questions && s.SurveyType == DumpFields.SurveyTypeValidated),
                    DumpFields.FilterCustom => contents.Where(s =>
                        s.SurveyOrRegistration == DumpFields.Survey && s.SurveyType == DumpFields.SurveyTypeCustom),
                    DumpFields.FilterIncidentRegistration => contents.Where(s =>
                        s.SurveyOrRegistration == DumpFields.Registration &&
                        s.RegistrationType == DumpFields.RegistrationTypeIncident),
                    DumpFields.FilterNumericRegistration => contents.Where(s =>
                        s.SurveyOrRegistration == DumpFields.Registration &&
                        s.RegistrationType == DumpFields.RegistrationTypeNumeric),
                    DumpFields.FilterStatusRegistration => contents.Where(s =>
                        s.SurveyOrRegistration == DumpFields.Registration &&
                        s.RegistrationType == DumpFields.RegistrationTypeStatus),
                    DumpFields.FilterAllRegistration => contents.Where(s => s.SurveyOrRegistration == DumpFields.Registration),
                    _ => contents
                };

                contents = OrderDumpData(request, contents);
                foreach (var line in contents)
                {
                    rowNumber++;
                    cells = AddContents(request.Fields, line);
                    rows.Add(new Row(rowNumber, cells));
                }

                rowNumber+=3;

            }
            
            worksheet.Rows = rows;

            return worksheet;
        }

        
    }

}