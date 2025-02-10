using API.Constants;
using API.Extensions;
using API.Models.AnalyticsReport.SROI;
using API.Models.Views.AnalyticsReport.SROI;
using Mapster;
using System;
using System.Reflection;

namespace API.Mapping
{
    public class AnalyticReportAPIMappingConfigV2 : IRegister
    {
        private string _language;
        private readonly ILanguageAttributeExtension _languageAttributeExtension;

        public AnalyticReportAPIMappingConfigV2(string language)
        {
            _language = language;
            _languageAttributeExtension = new LanguageAttributeExtension();
        }

        public AnalyticReportAPIMappingConfigV2(){}

        public void Register(TypeAdapterConfig config)
        {
            config.NewConfig<SROIReportConfigV2, SROIAnalyticsAPIViewModel>()
                .Map(dest => dest.Name, src => src.General.ReportName)
                .Map(dest => dest.Cost, src => src.FundingSource.TotalCosts)
                .Map(dest => dest.Unit, src => src.General.Currency.ToString())
                .Map(dest => dest.StartDate, src => DateTime.Now)
                .Map(dest => dest.Duration, src => src.Intervention.BusinessCaseLength)
                .Map(dest => dest.Description, src => src.Intervention.InterventionDescription)
                .Map(dest => dest.Population, src => src.Intervention.Participants)
                .Map(dest => dest.Outcome, src => src.Outcomes);

            config.NewConfig<Outcomev2, Models.Views.AnalyticsReport.SROI.Outcome>()
                .Map(dest => dest.Name, src => src.OutcomeName)
                .Map(dest => dest.Description, src => src.OutcomeDescription)
                //.Map(dest => dest.StartYear, src => src.OutcomeStart == OutcomeStartEnum.PeriodAfter ? DateTime.Now.Year : 0)
                .Map(dest => dest.StartYear, src => src.OutcomeStart == OutcomeStartEnum.PeriodAfter ? DateTime.Now.Year: src.StartYears)
                .Map(dest => dest.DurationYears, src=>src.OutcomeDuration)
                .Map(dest => dest.QuoScenario, src => src.SkipAlternative ? 0 : 1)
                .Map(dest => dest.QuoScenarioSource, src => !src.SkipAlternative ? src.Alternative.Source : "")
                .Map(dest => dest.QuoScenarioComment, src => !src.SkipAlternative ? src.Alternative.Comment : "")
                .Map(dest => dest.AnswerRate, src => !src.SkipAlternative ? src.AnswerRate / 100 : 0)
                .Map(dest => dest.BenchmarkValue, src => !src.SkipAlternative ? src.Alternative.Amount / 100 : 0)
                .Map(dest => dest.Source, src => src.Source)
                .Map(dest => dest.Comments, src => src.Comments)
                .Map(dest => dest.SensitivityComments, src => src.SensitivityAnalysis.Comments != null ? src.SensitivityAnalysis.Comments : "")
                .Map(dest => dest.DeadWeight, src => src.SensitivityAnalysis.Deadweight != null ? src.SensitivityAnalysis.Deadweight / 100 : 0)
                .Map(dest => dest.DisplacementRate, src => src.SensitivityAnalysis.Displacement != null ? src.SensitivityAnalysis.Displacement / 100 : 0)
                .Map(dest => dest.AttributionRate, src => src.SensitivityAnalysis.Attribution != null ? src.SensitivityAnalysis.Attribution / 100 : 0)
                .Map(dest => dest.DropOffRate, src => src.SensitivityAnalysis.Dropoff != null ? src.SensitivityAnalysis.Dropoff / 100 : 0) 
                .Map(dest => dest.Beneficiaries, src => src.Beneficiaries);

            config.NewConfig<Beneficiaryv2, Models.Views.AnalyticsReport.SROI.Beneficiary>()
                .Map(dest => dest.Name, src => src.Name.ToString())
                .Map(dest => dest.BeneficiaryType, src => src.Type.ToString())
                .Map(dest => dest.EffectValue, src => src.Value)
                .Map(dest => dest.Comments, src => src.Comments != null ? src.Comments : "");
        }
    }
}
