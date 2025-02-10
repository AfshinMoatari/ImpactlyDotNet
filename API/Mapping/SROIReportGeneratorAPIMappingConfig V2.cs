using API.Models.AnalyticsReport.SROI;
using API.Models.Views.AnalyticsReport.SROI;
using Mapster;
using Microsoft.IdentityModel.Tokens;
using System.Collections.Generic;
using System.Linq;

namespace API.Mapping
{
    public class SROIReportGeneratorAPIMappingConfigV2 : IRegister
    {
        public SROIReportGeneratorAPIMappingConfigV2()
        {
        }

        public void Register(TypeAdapterConfig config)
        {
            config.NewConfig<SROIReportConfigV2, SROIGeneratorAPIRequestViewModel>()
                .Map(dest => dest.Id, src => src.Id)
                .Map(dest => dest.SROIPage1.ReportName, src => src.General.ReportName)
                .Map(dest => dest.Currency, src => src.General.Currency.ToString())
                .Map(dest => dest.SROIPage1.ExecutiveSummary, src => src.General.ExecutiveSummary)
                .Map(dest => dest.SROIPage1.Logo, src => src.General.Logo)
                .Map(dest => dest.SROIPage2.InterventionDescription, src => src.Intervention.InterventionName)
                .Map(dest => dest.SROIPage2.InterventionVision, src => src.Intervention.Purpose)
                .Map(dest => dest.SROIPage2.InterventionActivities, src => src.Intervention.Activities)
                .Map(dest => dest.SROIPage2.InterventionCosts, src => src.FundingSource.TotalCosts)
                .Map(dest => dest.SROIPage2.InterventionParticipants, src => src.Intervention.Participants)
                .Map(dest => dest.SROIPage2.TargetGroupCategory, src => src.TargetGroup.Category.ToString())
                .Map(dest => dest.SROIPage2.TargetGroupAgeMin, src => src.TargetGroup.AgeGroupMin)
                .Map(dest => dest.SROIPage2.TargetGroupAgeMax, src => src.TargetGroup.AgeGroupMax)
                .Map(dest => dest.SROIPage2.TargetGroupDescription, src => src.TargetGroup.TargetGroupDescription)
                .Map(dest => dest.SROIPage2.TargetGroupRiskFactors, src => src.TargetGroup.RiskFactors)
                .Map(dest => dest.SROIPage2.Stakeholders, src => src.Stakeholders.Select(stakeholder => new StakeholdersSummaryModel
                {
                    Name = stakeholder.StakeholderName,
                    Changes = stakeholder.Changes,
                    Amount = stakeholder.StakeholderAmount
                }).ToList())
                .Map(dest => dest.SROIPage4.InputSummary.InvestmentAmount, src => src.FundingSource.TotalCosts)
                .Map(dest => dest.SROIPage4.InputSummary.TotalCost, src => src.FundingSource.TotalCosts)
                .Map(dest => dest.SROIPage5.Description, src => src.Method.Description)
                .AfterMapping((src, dest) =>
                {
                    dest.SROIPage4.InputSummary.FundingSources = MapFundingSources(src.FundingSource.Fundings);
                });

            config.NewConfig<SROIAnalyticsResponseViewModel, SROIGeneratorAPIRequestViewModel>()
                .Map(dest => dest.SROIPage1.ReportSummary.Social, src => src.SocialCostBenefitRatio)
                .Map(dest => dest.SROIPage1.ReportSummary.Budgetary, src => src.BudgetCostBenefitRatio)
                .Map(dest => dest.SROIPage1.ReportSummary.SocioEconomic, src => src.TotalCostBenefitRatio)
                .Map(dest => dest.SROIPage3.OutcomeSummaries, src => src.OutcomeBeneficiaryList)
                .Map(dest => dest.SROIPage4.InputSummary.Length, src => src.Duration)
                .Map(dest => dest.SROIPage4.WELLBYSummaryModel.PointTotal, src => src.TotalWellbyPoint)
                .Map(dest => dest.SROIPage4.WELLBYSummaryModel.PointPerParticipant, src => src.TotalWellbyPerPerson)
                .Map(dest => dest.SROIPage4.WELLBYSummaryModel.CostPerPoint, src => src.CostPerWellbyPoint)
                .Map(dest => dest.SROIPage4.SubjectiveWellbeingValuationSummary.TotalSocialValue, src => src.TotalSocialValue)
                .Map(dest => dest.SROIPage4.FinancialBudgetarySavingsSummary.TotalBudgetaryValue, src => src.TotalBudgetaryValue)
                .Map(dest => dest.SROIPage4.ReportSummary.Social, src => src.SocialCostBenefitRatio)
                .Map(dest => dest.SROIPage4.ReportSummary.Budgetary, src => src.BudgetCostBenefitRatio)
                .Map(dest => dest.SROIPage4.ReportSummary.SocioEconomic, src => src.TotalCostBenefitRatio)
                .Map(dest => dest.SROIPage6.OutcomeTableRecords, src => src.OutcomeBeneficiaryList.Select(outcome => new OutcomeTableRecord
                {
                    ValueType = outcome.ValueType,
                    OutcomeName = outcome.OutcomeName,
                    n = outcome.Population,
                    Results = outcome.EffectSize,
                    Source = outcome.Source,
                    AnswerRate = outcome.AnswerRate,
                    Duration = outcome.Duration,
                    Significance = outcome.Significance,
                }).ToList())
                .Map(dest => dest.SROIPage7.BasicAlternativesTableRecords, src => src.OutcomeBeneficiaryList.Select(outcome => new BasicAlternativesTableRecord
                {
                    ValueType = outcome.ValueType,
                    OutcomeName = outcome.OutcomeName,
                    PCTAmount = outcome.BenchmarkPct,
                    Amount = outcome.Benchmark,
                    Source = outcome.Source,
                    Comments = outcome.OutcomeComments,
                }).ToList())
                .Map(dest => dest.SROIPage8.PricesTableRecords, src => src.OutcomeBeneficiaryList.Select(outcome => new PricesTableRecord
                {
                    ValueType = outcome.ValueType,
                    OutcomeName = outcome.OutcomeName,
                    Beneficiary = outcome.BeneficiaryName,
                    GrossValue = outcome.GrossUnitValue,
                    Source = outcome.Source,
                    Comments = outcome.BeneficiaryComments
                }).ToList())
                .Map(dest => dest.SROIPage9.WELLBYTableRecords, src => src.OutcomeBeneficiaryList.Select(outcome => new WELLBYTableRecord
                {
                    OutcomeName = outcome.OutcomeName,
                    Unit = outcome.EffectType,
                    EffectSize = outcome.EffectNumber,
                    GrossValue = outcome.GrossUnitValue,
                    ValuePrOutcome = outcome.WellbyPerPerson,
                    TotalWellby = outcome.WellbyPoints,
                    TotalSocial = outcome.SocialValueTotal,
                    Source = outcome.Source
                }).ToList())
                .Map(dest => dest.SROIPage10.SensitivityAnalysisRecords, src => src.OutcomeBeneficiaryList.Select(outcome => new SensitivityAnalysisRecord
                {
                    ValueType = outcome.ValueType,
                    OutcomeName = outcome.OutcomeName,
                    Beneficiary = outcome.BeneficiaryName,
                    Gross = outcome.GrossUnitValue,
                    EffectSize = outcome.EffectSize,
                    Total = outcome.TotalGrossValue,
                    DeadWeight = outcome.DeadweightRate,
                    Displacement = outcome.DisplacementRate,
                    Attribution = outcome.AttributionRate,
                    DropOff = outcome.DropoffRate,
                    Net = outcome.TotalNetValue
                }).ToList())
                .Map(dest => dest.SROIPage11.AdditionalCommentsRecords, src => src.OutcomeBeneficiaryList.Select(outcome => new AdditionalCommentsRecord
                {
                    ValueType = outcome.ValueType,
                    OutcomeName = outcome.OutcomeName,
                    Beneficiary = outcome.BeneficiaryName,
                    OutcomeComments = outcome.OutcomeComments,
                    SensitivityComments = outcome.SensitivityComments
                }).ToList())

                .AfterMapping((src, dest) =>
                {
                    dest.SROIPage4.SubjectiveWellbeingValuationSummary.ValueCategories = MapSubjectiveWellbeingValuationSummaryOutcomeDistribution(src.OutcomeDistribution);
                    dest.SROIPage4.SubjectiveWellbeingValuationSummary.Beneficiaries = MapSubjectiveWellbeingValuationSummaryBeneficiaryDistribution(src.BeneficiaryDistribution);
                    dest.SROIPage4.FinancialBudgetarySavingsSummary.ValueCategories = MapFinancialBudgetarySavingsSummaryOutcomeDistribution(src.OutcomeDistribution);
                    dest.SROIPage4.FinancialBudgetarySavingsSummary.Beneficiaries = MapFinancialBudgetarySavingsSummaryBeneficiaryDistribution(src.BeneficiaryDistribution);
                });
        }

        private Dictionary<string, decimal> MapSubjectiveWellbeingValuationSummaryOutcomeDistribution(List<OutcomeDistribution> outcomeDistribution)
        {
            if (outcomeDistribution == null)
                return null;

            var valueCategories = new Dictionary<string, decimal>();
            foreach (var item in outcomeDistribution)
            {
                string key = item.OutcomeName;
                valueCategories[key] = item.SocialValueProportion;
            }
            return valueCategories;
        }

        private Dictionary<string, decimal> MapSubjectiveWellbeingValuationSummaryBeneficiaryDistribution(List<BeneficiaryDistribution> beneficiaryDistribution)
        {
            if (beneficiaryDistribution == null)
                return null;

            var valueCategories = new Dictionary<string, decimal>();
            foreach (var item in beneficiaryDistribution)
            {
                string key = item.BeneficiaryName;
                valueCategories[key] = item.TotalValueProportion;
            }
            return valueCategories;
        }

        private Dictionary<string, decimal> MapFinancialBudgetarySavingsSummaryOutcomeDistribution(List<OutcomeDistribution> outcomeDistribution)
        {
            if (outcomeDistribution == null)
                return null;

            var valueCategories = new Dictionary<string, decimal>();
            foreach (var item in outcomeDistribution)
            {
                string key = item.OutcomeName;
                valueCategories[key] = item.SocialValueProportion;
            }
            return valueCategories;
        }

        private Dictionary<string, decimal> MapFinancialBudgetarySavingsSummaryBeneficiaryDistribution(List<BeneficiaryDistribution> beneficiaryDistribution)
        {
            if (beneficiaryDistribution == null)
                return null;

            var valueCategories = new Dictionary<string, decimal>();
            foreach (var item in beneficiaryDistribution)
            {
                string key = item.BeneficiaryName;
                valueCategories[key] = item.BudgetValueProportion;
            }
            return valueCategories;
        }

        private List<FundingSourceModel> MapFundingSources(List<Funding> fundingSources)
        {
            if (fundingSources == null)
                return null;

            return fundingSources.Select(fs => new FundingSourceModel(fs.FundingName, fs.Proportion)).ToList();
        }
    }
}
