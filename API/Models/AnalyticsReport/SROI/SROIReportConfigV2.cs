using System.Collections.Generic;
using System.ComponentModel;
using API.Models.Views.Report;
using static API.Extensions.LanguageAttributeExtension;

namespace API.Models.AnalyticsReport.SROI
{
    public class SROIReportConfigV2
    {
        public string Id { get; set; }
        public string ParentId { get; set; }
        public General General { get; set; }
        public Interventionv2 Intervention { get; set; }
        public TargetGroupv2 TargetGroup { get; set; }
        public List<Stakeholderv2> Stakeholders { get; set; }
        public FundingSourcev2 FundingSource { get; set; }
        public List<Outcomev2> Outcomes { get; set; }
        public Method Method { get; set; }
        public Confirmation Confirmation { get; set; }
    }

    public class General
    {
        public bool IsForcast { get; set; }
        public string ReportName { get; set; }
        public string ExecutiveSummary { get; set; }
        public string ReportLanguage { get; set; } 
        public CurrencyEnum Currency { get; set; }
        public string Logo { get; set; }
    }

    public class Interventionv2
    {
        public string InterventionName { get; set; }
        public string InterventionDescription { get; set; }
        public string Purpose { get; set; }
        public List<string> Activities { get; set; }
        public int? Participants { get; set; }
        public int? BusinessCaseLength { get; set; }
    }


    public class TargetGroupv2
    {
        public CategoryEnum Category { get; set; }
        public string CustomCategory { get; set; }
        public int? AgeGroupMin { get; set; }
        public int? AgeGroupMax { get; set; }
        public string TargetGroupDescription { get; set; }
        public string RiskFactors { get; set; }
    }

    public class Stakeholderv2
    {
        public string StakeholderName { get; set; }
        public int StakeholderAmount { get; set; }
        public List<string> Changes { get; set; }
    }


    public class FundingSourcev2
    {
        public decimal? TotalCosts { get; set; }
        public List<Funding> Fundings { get; set; }
    }

    public class Funding
    {
        public string FundingName { get; set; }
        public decimal Proportion { get; set; }
    }

    public class Outcomev2
    {
        public string OutcomeName { get; set; }
        public string OutcomeDescription { get; set; }
        public MeasurementMethodEnum MeasurementMethod { get; set; }
        public OutcomeStartEnum OutcomeStart { get; set; }
        public int? OutcomeDuration { get; set; } 
        public int? OutcomePopulation { get; set; }
        public EffectTypeEnum EffectType { get; set; }
        public decimal? EffectSize { get; set; }
        public decimal? AnswerRate { get; set; }
        public int? StartYears { get; set; }
        public int? YearsCollected { get; set; }
        public string Significance { get; set; }
        public string Source { get; set; }
        public string Comments { get; set; }
        public bool SkipAlternative { get; set; }
        public Alternative Alternative { get; set; }
        public bool SkipSensitivityAnalysis { get; set; }
        public SensitivityAnalysisv2 SensitivityAnalysis { get; set; }
        public List<Beneficiaryv2> Beneficiaries { get; set; }
    }

    public class Alternative
    {
        public decimal? Amount { get; set; }
        public string Source { get; set; }
        public string Comment { get; set; }
    }

    public class SensitivityAnalysisv2
    {
        public decimal? Deadweight { get; set; }
        public decimal? Displacement { get; set; }
        public decimal? Attribution { get; set; }
        public decimal? Dropoff { get; set; }
        public string Comments { get; set; }
    }

    public class Beneficiaryv2
    {
        public BeneficiaryNameEnum Name { get; set; }
        public BeneficiaryTypeEnum Type { get; set; }
        public string ValueType { get; set; }
        public decimal? Value { get; set; } 
        public string Source { get; set; }
        public string Comments { get; set; }
    }

    public class Method
    {
        public string Description { get; set; } 
    }

    public class Confirmation
    {
        public bool IsSavedTemplate { get; set; }
        public string? TemplateName { get; set; } 
    }

    public enum CurrencyEnum
    {
        [Description("DanishKrone")]
        DKK,

        [Description("Euro")]
        EUR
    }

    public enum EffectTypeEnum
    {
        [Description("Person")]
        Person,

        [Description("Score")]
        Score
    }

    public enum CategoryEnum
    {
        [Description("Vulnerable children and youth")]
        VulnerableChildrenAndYouth,

        [Description("Vulnerable adults")]
        VulnerableAdults,

        [Description("Adults with disabilities")]
        AdultsWithDisabilities,

        [Description("Vulnerable families")]
        VulnerableFamilies,

        [Description("Relatives of vulnerable citizens")]
        RelativesOfVulnerableCitizens,

        [Description("Custom")]
        Custom
    }

    public enum MeasurementMethodEnum
    {
        [Description("Validated questionnaires")]
        ValidatedQuestionnaires,

        [Description("Custom questionnaires")]
        CustomQuestionnaires,

        [Description("Registrations")]
        Registrations,

        [Description("Data extraction")]
        DataExtraction
    }

    public enum OutcomeStartEnum
    {
        [Description("Period after")]
        PeriodAfter,

        [Description("Period of activity")]
        PeriodOfActivity
    }

    public enum BenchmarkMethodEnum
    {
        [Description("Literature Study")]
        LiteratureStudy,

        [Description("Historical Data")]
        HistoricalData,

        [Description("Comparison Group")]
        ComparisonGroup,

        [Description("Assumption")]
        Assumption
    }

    public enum BeneficiaryNameEnum
    {
        [MultiCulturalDescription(new string[] { "State", "" })]
        State,

        [MultiCulturalDescription(new string[] { "Region", "" })]
        Region,

        [MultiCulturalDescription(new string[] { "Municipality", "" })]
        Municipality,

        [MultiCulturalDescription(new string[] { "External organisation", "" })]
        ExternalOrganisation,

        [MultiCulturalDescription(new string[] { "Internal organisation", "" })]
        InternalOrganisation,

        [MultiCulturalDescription(new string[] { "Citizens", "" })]
        Citizens,

        [MultiCulturalDescription(new string[] { "Caretakers next of kin", "" })]
        CaretakersNextOfKin,

        [MultiCulturalDescription(new string[] { "Employees", "" })]
        Employees,

        [MultiCulturalDescription(new string[] { "Volunteers", "" })]
        Volunteers,

        [MultiCulturalDescription(new string[] { "Custom", "" })]
        Custom
    }

    public enum BeneficiaryTypeEnum
    {
        [Description("Budgetary")]
        Budgetary,

        [Description("Social")]
        Social,

        [Description("Non-priced")]
        NonPriced
    }
}