using System.Collections.Generic;

namespace API.Constants;

public static class DumpFields
{
    
    public const string PatientLastName = "PatientLastName";
    public const string PatientFirstName = "PatientFirstName";
    public const string Tags = "Tags";
    public const string SurveyAndRegistration = "SurveyAndRegistration";
    public const string QuestionNumber = "QuestionNumber";
    public const string Questions = "Questions";
    public const string Answers = "Answers";
    public const string AnsweredAt = "AnsweredAt";
    public const string UpdatedAt = "UpdatedAt";
    public const string RegisteredAt = "RegisteredAt";
    public const string Strategy = "Strategy";
    public const string DataEntries = "DataEntries";
    public const string DataStrategies = "DataStrategies";
    public const string Survey = "Survey";
    public const string Registration = "Registration";
    public const string SurveyScore = "SurveyScore";

    public const string ProjectName = "ProjectName";
    public const string ExportedBy = "ExportedBy";
    public const string SortedBy = "SortedBy";
    public const string Filter = "FilterFilter";
    public const string Data = "Data";
    
    public const string Index = "Index";
    
    public const string FilterAllSurvey = "FilterAllSurvey";
    public const string FilterValidated = "FilterValidated";
    public const string FilterCustom = "FilterCustom";
    public const string FilterAllRegistration = "FilterAllRegistration";
    public const string FilterStatusRegistration = "FilterStatusRegistration";
    public const string FilterNumericRegistration = "FilterNumericRegistration";
    public const string FilterIncidentRegistration = "FilterIncidentRegistration";
    public const string FilterAll = "FilterAll";
    public const string FilterOnlyPatients = "FilterOnlyPatients";

    public const string SurveyTypeValidated = "SurveyTypeValidated";
    public const string SurveyTypeCustom = "SurveyTypeCustom";
    public const string RegistrationTypeStatus = "RegistrationTypeStatus";
    public const string RegistrationTypeNumeric = "RegistrationTypeNumeric";
    public const string RegistrationTypeIncident = "RegistrationTypeIncident";

    public const string ImportTime = "ImportTime";
    public const string SkippedOrFailed = "SkippedOrFailed";
    public const string Inserted = "Inserted";
    public const string Updated = "Updated";

    public static readonly Dictionary<string, string> FieldsInDanish = new Dictionary<string, string>()
    {
        {PatientLastName,"Efternavn"},
        {PatientFirstName,"Fornavn"},
        {Tags,"Tags"},
        {SurveyAndRegistration,"Svar/registreringer"},
        {QuestionNumber,"Spg. Nr."},
        {Questions,"Spørgsmål"},
        {Answers,"Svar eller registering"},
        {AnsweredAt,"Tidspunkt besvaret"},
        {UpdatedAt,"Tidspunkt opdateret"},
        {RegisteredAt,"Tidspunkt registreret"},
        {Strategy,"Strategier"},
        {DataEntries,"Datapunkter"},
        {DataStrategies,"Strategier"},
        {Survey,"Spørgeskema"},
        {Registration,"Registrering"},
        {SurveyScore,"Scoringer"},
        {ProjectName,"Projekt navn"},
        {ExportedBy,"Eksporteret af"},
        {SortedBy,"Sorteret via"},
        {Filter,"Filtre"},
        {Data,"Data"},
        {Index,"Index"},
        {FilterAllSurvey,"Alle Spørgeskemaer"},
        {FilterValidated,"Validerede spørgeskemaer"},
        {FilterCustom,"Egne spørgeskemaer"},
        {FilterAllRegistration,"Alle registreringer"},
        {FilterStatusRegistration,"Status Registreringer"},
        {FilterNumericRegistration,"Numeriske registreringer"},
        {FilterIncidentRegistration,"Hændelses registreringer"},
        {FilterAll,"Alle typer data"},
        {SurveyTypeValidated,"Validerede"},
        {SurveyTypeCustom,"Egne"},
        {RegistrationTypeStatus,"status"},
        {RegistrationTypeNumeric,"numeric"},
        {RegistrationTypeIncident,"count"},
        {ImportTime, "Importtid"},
        {SkippedOrFailed,  "Springet over eller mislykkedes"},
        {Inserted, "Indsat"},
        {Updated, "Opdateret"}
    };


    public static readonly Dictionary<string, string> FieldsInEnglish = new Dictionary<string, string>()
    {
        { PatientLastName, "Last Name" },
        { PatientFirstName, "First Name" },
        { Tags, "Tags" },
        { SurveyAndRegistration, "Survey/Registration" },
        { QuestionNumber, "Question Number" },
        { Questions, "Questions" },
        { Answers, "Answer or Registration" },
        { AnsweredAt, "Answered at" },
        { UpdatedAt, "Updated At" },
        { RegisteredAt, "Registered At" },
        { Strategy, "Strategy" },
        { DataEntries, "Data Entries" },
        { DataStrategies, "Strategies" },
        { Survey, "Survey" },
        { Registration, "Registration" },
        { SurveyScore, "Score" },
        { ProjectName, "Project Name" },
        { ExportedBy, "Exported by" },
        { SortedBy, "Sorted by" },
        { Filter, "Filter" },
        { Data, "Data" },
        { Index, "Index" },
        { FilterAllSurvey, "All Surveys" },
        { FilterValidated, "Validated Surveys" },
        { FilterCustom, "Custom Surveys" },
        { FilterAllRegistration, "All Registrations" },
        { FilterStatusRegistration, "Status Registration" },
        { FilterNumericRegistration, "Numeric Registration" },
        { FilterIncidentRegistration, "Incident Registration" },
        { FilterAll, "All" },
        { SurveyTypeValidated, "Validated" },
        { SurveyTypeCustom, "Custom" },
        { RegistrationTypeStatus, "Status" },
        { RegistrationTypeNumeric, "Numeric" },
        { RegistrationTypeIncident, "Incident" },
        {ImportTime, "Import Time"},
        {SkippedOrFailed, "Skipped or Failed"},
        {Inserted, "Inserted"},
        {Updated, "Updated"},
    };

}