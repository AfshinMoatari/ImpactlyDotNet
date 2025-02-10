using API.Constants;
using API.Extensions;
using API.Models.AnalyticsReport.SROI;
using API.Models.Views.AnalyticsReport.SROI;
using Mapster;
using System;
using System.Reflection;
using Nest;

namespace API.Mapping
{
    public class AnalyticReportAPIMappingConfig : IRegister
    {
        private string _language;
        private readonly ILanguageAttributeExtension _languageAttributeExtension;

        public AnalyticReportAPIMappingConfig(string language)
        {
            _language = language;
            _languageAttributeExtension = new LanguageAttributeExtension();
        }

        public AnalyticReportAPIMappingConfig() { }

        public void Register(TypeAdapterConfig config)
        {
            //config.NewConfig<SROIReportConfig, SROIAnalyticsAPIViewModel>()
            //    .Map(dest => dest.Name, src => src.ReportName)
            //    .Map(dest => dest.Cost, src => src.Intervention.Cost)
            //    .Map(dest => dest.Unit, src => src.Currency.ToString())
            //    .Map(dest => dest.StartDate, src => DateTime.Now)
            //    .Map(dest => dest.Duration, src => src.Intervention.BusinessCaseLength)
            //    .Map(dest => dest.Description, src => src.Intervention.Description)
            //    .Map(dest => dest.Population, src => src.Intervention.Participants)
            //    .Map(dest => dest.Outcome, src => src.Outcomes);

            //config.NewConfig<Models.AnalyticsReport.SROI.Outcome, Models.Views.AnalyticsReport.SROI.Outcome>()
            //    .Map(dest => dest.Name, src => src.OutcomeName)
            //    .Map(dest => dest.Description, src => src.Description)
            //    .Map(dest => dest.StartYear,
            //        src => src.OutcomeStart == OutcomeStartEnum.PeriodAfter ? DateTime.Now.Year : 0)
            //    .Map(dest => dest.DurationYears, src => src.OutcomeDuration)
            //    .Map(dest => dest.QuoScenario, src => src.Benchmark != null && src.Benchmark.Amount.HasValue ? 1 : 0)
            //    .Map(dest => dest.QuoScenarioSource, src => src.Benchmark != null ? src.Benchmark.Source : "")
            //    .Map(dest => dest.QuoScenarioComment, src => src.Benchmark != null ? src.Benchmark.Comments : "")
            //    .Map(dest => dest.AnswerRate, src => src.Benchmark != null ? src.AnswerRate / 100 : 0)
            //    .Map(dest => dest.BenchmarkValue, src => src.Benchmark != null ? src.Benchmark.BenchmarkValue / 100 : 0)
            //    .Map(dest => dest.Source, src => src.Benchmark != null ? src.Source : "")
            //    .Map(dest => dest.Comments, src => src.Benchmark != null ? src.Comments : "")
            //    .Map(dest => dest.SensitivityComments,
            //        src => src.SensitivityAnalysis.Comments != null ? src.SensitivityAnalysis.Comments : "")
            //    .Map(dest => dest.DeadWeight,
            //        src => src.SensitivityAnalysis.Deadweight != null ? src.SensitivityAnalysis.Deadweight / 100 : 0)
            //    .Map(dest => dest.DisplacementRate,
            //        src => src.SensitivityAnalysis.Displacement != null
            //            ? src.SensitivityAnalysis.Displacement / 100
            //            : 0)
            //    .Map(dest => dest.AttributionRate,
            //        src => src.SensitivityAnalysis.Attribution != null ? src.SensitivityAnalysis.Attribution / 100 : 0)
            //    .Map(dest => dest.DropOffRate,
            //        src => src.SensitivityAnalysis.DropOff != null ? src.SensitivityAnalysis.DropOff / 100 : 0)
            //    .Map(dest => dest.Beneficiaries, src => src.Beneficiaries);

            //config.NewConfig<Models.AnalyticsReport.SROI.Beneficiary, Models.Views.AnalyticsReport.SROI.Beneficiary>()
            //    .Map(dest => dest.Name, src =>  src.Name.ToString())
            //    .Map(dest => dest.BeneficiaryType, src => src.Type.ToString())
            //    .Map(dest => dest.EffectValue, src => src.ValueUnit)
            //    .Map(dest => dest.Comments, src => src.Comments != null ? src.Comments : "");
        }
    }
}
