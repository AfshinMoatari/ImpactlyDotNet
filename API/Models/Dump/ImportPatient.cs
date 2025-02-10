using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using API.Models.Projects;

namespace API.Models.Dump;

public class ImportPatient
{
    public const string ImportStatusPreparing = "preparing";
    public const string ImportStatusInserted = Logs.Log.BulkImportStatusInserted;
    public const string ImportStatusSkipped = Logs.Log.BulkImportStatusSkipped;
    public const string ImportStatusUpdated = Logs.Log.BulkImportStatusUpdated;
    public const string ImportStatusToBeUpdated = "to be updated";
    public const string ImportStatusToBeInserted = "to be inserted";
    public const string ImportStatusInsertFailed = "insert failed";
    public const string ImportStatusUpdateFailed = "update failed";
    
    public const string ColumnNameBirthDate = "Birth Date";
    public const string ColumnNameEmail = "Email";
    public const string ColumnNameIsActive = "Is Active";
    public const string ColumnNameLastName = "Last Name";
    public const string ColumnNameMunicipality = "Municipality";
    public const string ColumnNameFirstName = "First Name";
    public const string ColumnNamePhoneNumber = "Phone Number";
    public const string ColumnNamePostalNumber = "Postal Number";
    public const string ColumnNameSex = "Sex";
    public const string ColumnNameStrategy = "Strategy";
    public const string ColumnNameTags = "Tags";
    public const string ColumnNameStatus = "Import Status";
    public const string ColumnNameSkipReason = "Skipped Reason";
    public const string ColumnNamePatientId = "Impactly Citizen Id";
    public const string ColumnNameUpdateMessage = "Update Detail";
    public const string ColumnNameRegion = "Region";


    public static List<string> GetColumnNamesPatients()
    {
        var list = new List<string>
        {
            ColumnNameFirstName,
            ColumnNameLastName,
            ColumnNameBirthDate,
            ColumnNameSex,
            ColumnNameIsActive,
            ColumnNameMunicipality,
            ColumnNameRegion,
            ColumnNameEmail,
            ColumnNamePhoneNumber,
            ColumnNamePostalNumber,
            ColumnNameStrategy,
            ColumnNameTags,
            ColumnNameStatus,
        };
        return list;
    }
    public static List<string> GetColumnNamesForSkipped()
    {
        var list = GetColumnNamesPatients();
        list.Add(ColumnNameSkipReason);
        return list;
    }
    
    public static List<string> GetColumnNamesForInserted()
    {
        var list = GetColumnNamesPatients();
        list.Add(ColumnNamePatientId);
        return list;
    }
    
    public static List<string> GetColumnNamesForUpdated()
    {
        var list = GetColumnNamesPatients();
        list.Add(ColumnNameUpdateMessage);
        return list;
    }

    
    
    public ProjectPatient ProjectPatient { get; set; }
    
    public ImportPatientRequest ImportPatientRequest { get; set; }
    
    public string Status { get; set; }
    
    public string Message { get; set; }
    
    public string ProjectId { get; set; }
    
    public List<ProjectTag> ProjectTags { get; set; }
    
    public List<string> UpdateFields { get; set; }

    public string UpdateFieldsToString()
    {
        return UpdateFields.Aggregate("", (current, field) => current + (field + ", "));
    }

    public List<string> GetPatientsBasic()
    {
        return new List<string>
        {
            ImportPatientRequest.Name,
            ImportPatientRequest.LastName,
            ImportPatientRequest.BirthDate.ToString(),
            ImportPatientRequest.Sex,
            ImportPatientRequest.IsActive.ToString(),
            ImportPatientRequest.Municipality,
            ImportPatientRequest.Region,
            ImportPatientRequest.Email,
            ImportPatientRequest.PhoneNumber,
            ImportPatientRequest.PostalNumber,
            ImportPatientRequest.Strategy,
            ImportPatientRequest.Tags,
            Status
        };
    }

    public List<string> GetImportSkipped()
    {
        var list = GetPatientsBasic();
        list.Add(Message);
        return list;
    }

    public List<string> GetImportInserted()
    {
        var list = GetPatientsBasic();
        list.Add(ProjectPatient.Id);
        return list;
    }

    public List<string> GetImportUpdated()
    {
        var list = GetPatientsBasic();
        list.Add(UpdateFieldsToString());
        return list;
    }
}