using System;
using System.Collections.Generic;
using System.Linq;
using API.Constants;
using API.Models.Projects;

namespace API.Models.Dump;

public class DumpData
{
    public string ProjectName { get; set; }

    public string PatientLastName { get; set; }

    public string PatientFirstName { get; set; }

    public string PatientId { get; set; }

    public string PatientIsActive { get; set; }
    
    
    public string PatientPostcode { get; set; }
    
    public string PatientEmail { get; set; }
    
    public string PatientPhone { get; set; }
    
    public string PatientGender { get; set; }
    
    public string PatientRegion { get; set; }
    
    public string PatientCity { get; set; }
    
    public bool Anonymity { get; set; }
    
    public string Question { get; set; }
    public string SurveyName { get; set; }
    public string Answer { get; set; }
    public string StrategyName { get; set; }
    
    public string FieldId { get; set; }
    
    public int FieldIndex { get; set; }
    public string FieldText { get; set; }
    
    public string FieldType { get; set; }

    public string ChoiceId { get; set; }
    public string ChoiceText { get; set; }
    public int ChoiceIndex { get; set; }
    
    public DateTime AnsweredAt { get; set; }
    
    public string FreeText { get; set; }
    
    public List<string> Tags { get; set; }
    
    public string SurveyId { get; set; }
    
    public string EffectId { get; set; }
    
    public string EffectName { get; set; }
    
    public string Value { get; set; }
    
    public string SurveyScore { get; set; }
        
    public string ProjectId { get; set; }

    public string PatientAge { get; set; }
    
    public string RegistrationCategory { get; set; }
    
    public string TagsToString()
    {
        return Tags == null ? "" : Tags.Aggregate("", (current, tag) => current + (tag + ", "));
    }
    
    public string Index { get; set; }
    
    public string SurveyOrRegistration { get; set; }
    public string SurveyType { get; set; }
    public string RegistrationType { get; set; }

    
    
    public string Note { get; set; }

    /*
    public static string ToRegistrationTypeName(string registrationType)
    {
        return registrationType switch
        {
            DumpFields.RegistrationTypeStatus => DumpFields.FilterStatusRegistration,
            DumpFields.RegistrationTypeIncident => DumpFields.FilterIncidentRegistration,
            DumpFields.RegistrationTypeNumeric => DumpFields.FilterNumericRegistration,
            _ => string.Empty
        };
    }
    public string GetRegistrationTypeName()
    {
        if (RegistrationType == null) return null;
        return RegistrationType switch
        {
            DumpFields.RegistrationTypeStatus => DumpFields.FilterStatusRegistration,
            DumpFields.RegistrationTypeIncident => DumpFields.FilterIncidentRegistration,
            DumpFields.RegistrationTypeNumeric => DumpFields.FilterNumericRegistration,
            _ => null
        };
    }
    */
}

