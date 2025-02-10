using API.Models.AnalyticsReport.SROI;
using System;
using System.Collections.Generic;
using System.Linq;

namespace API.Models.Views.AnalyticsReport.SROI
{
    public class SROIGeneratorAPIRequestViewModel
    {
        public string Id { get; set; }
        public string Language { get; set; }
        public string Currency { get; set; }

        public SROIPage1Model SROIPage1 { get; set; }
        public SROIPage2Model SROIPage2 { get; set; }
        public SROIPage3Model SROIPage3 { get; set; }
        public SROIPage4Model SROIPage4 { get; set; }
        public SROIPage5Model SROIPage5 { get; set; }
        public SROIPage6Model SROIPage6 { get; set; }
        public SROIPage7Model SROIPage7 { get; set; }
        public SROIPage8Model SROIPage8 { get; set; }
        public SROIPage9Model SROIPage9 { get; set; }
        public SROIPage10Model SROIPage10 { get; set; }
        public SROIPage11Model SROIPage11 { get; set; }
    }

    public class SROIPage1Model
    {
        public SROIPage1Model() { }
        public SROIPage1Model(
            string reportName,
            string executiveSummary,
            string logo,
            ReturnSummaryModel reportSummary)
        {
            ReportName = reportName;
            ExecutiveSummary = executiveSummary;
            Logo = logo;
            ReportSummary = reportSummary;
        }

        public string ReportName { get; set; }
        public string ExecutiveSummary { get; set; }
        public string Logo { get; set; }
        public ReturnSummaryModel ReportSummary { get; set; }
    }

    public class SROIPage2Model
    {
        public SROIPage2Model() { }
        public SROIPage2Model(string interventionDescription, string interventionVision, List<string> interventionActivities, decimal inteventionCosts, int interventionParticipants, string targetGroupCategory, int targetGroupAgeMax, int targetGroupAgeMin, string targetGroupDescription, string targetGroupRiskFactors, List<StakeholdersSummaryModel> stakeholders)
        {
            InterventionDescription = interventionDescription;
            InterventionVision = interventionVision;
            InterventionActivities = interventionActivities;
            InterventionCosts = inteventionCosts;
            InterventionParticipants = interventionParticipants;
            TargetGroupCategory = targetGroupCategory;
            TargetGroupAgeMax = targetGroupAgeMax;
            TargetGroupAgeMin = targetGroupAgeMin;
            TargetGroupDescription = targetGroupDescription;
            TargetGroupRiskFactors = targetGroupRiskFactors;
            Stakeholders = stakeholders;
        }

        public string InterventionDescription { get; set; }
        public string InterventionVision { get; set; }
        public List<string> InterventionActivities { get; set; }
        public decimal InterventionCosts { get; set; }
        public int InterventionParticipants { get; set; }
        public string TargetGroupCategory { get; set; }
        public int TargetGroupAgeMax { get; set; }
        public int TargetGroupAgeMin { get; set; }
        public string TargetGroupDescription { get; set; }
        public string TargetGroupRiskFactors { get; set; }
        public List<StakeholdersSummaryModel> Stakeholders { get; set; }
    }

    public class SROIPage3Model
    {
        public SROIPage3Model() { }

        public SROIPage3Model(List<OutcomeSummaryModel> outcomeSummaries)
        {
            OutcomeSummaries = outcomeSummaries;
        }

        public List<OutcomeSummaryModel> OutcomeSummaries { get; set; }
    }
    public class OutcomeSummaryModel
    {
        public OutcomeSummaryModel(string valueType, string outcomeName, string outcomeDescription, string beneficiaryName, int population, decimal effectSize, decimal benchmark, decimal effectNumber, string effectType)
        {
            ValueType = valueType;
            OutcomeName = outcomeName;
            OutcomeDescription = outcomeDescription;
            BeneficiaryName = beneficiaryName;
            Population = population;
            EffectSize = effectSize;
            Benchmark = benchmark;
            EffectNumber = effectNumber;
            EffectType = effectType;
        }

        public string ValueType { get; set; }
        public string OutcomeName { get; set; }
        public string OutcomeDescription { get; set; }
        public string BeneficiaryName { get; set; }
        public int Population { get; set; }
        public decimal EffectSize { get; set; }
        public decimal Benchmark { get; set; }
        public decimal EffectNumber { get; set; }
        public string EffectType { get; set; }
    }

    public class SROIPage4Model
    {
        public SROIPage4Model() { }
        public SROIPage4Model(
            InputSummaryModel inputSummary,
            WELLBYSummaryModel wELLBYSummaryModel,
            SubjectiveWellbeingValuationSummaryModel subjectiveWellbeingValuationSummary,
            FinancialBudgetarySavingsSummaryModel financialBudgetarySavingsSummary,
            ReturnSummaryModel reportSummary)
        {
            InputSummary = inputSummary;
            WELLBYSummaryModel = wELLBYSummaryModel;
            SubjectiveWellbeingValuationSummary = subjectiveWellbeingValuationSummary;
            FinancialBudgetarySavingsSummary = financialBudgetarySavingsSummary;
            ReportSummary = reportSummary;
        }

        public InputSummaryModel InputSummary { get; set; }
        public WELLBYSummaryModel WELLBYSummaryModel { get; set; }
        public SubjectiveWellbeingValuationSummaryModel SubjectiveWellbeingValuationSummary { get; set; }
        public FinancialBudgetarySavingsSummaryModel FinancialBudgetarySavingsSummary { get; set; }
        public ReturnSummaryModel ReportSummary { get; set; }
    }
    public class StakeholdersSummaryModel
    {
        public StakeholdersSummaryModel() { }
        public StakeholdersSummaryModel(string name, int amount, List<string> changes)
        {
            Name = name;
            Amount = amount;
            Changes = changes;
        }

        public string Name { get; set; }
        public int Amount { get; set; }
        public List<string> Changes { get; set; }
    }
    public class FinancialBudgetarySavingsSummaryModel
    {
        public FinancialBudgetarySavingsSummaryModel(
            decimal totalBudgetaryValue,
            Dictionary<string, decimal> valueCategories,
            Dictionary<string, decimal> beneficiaries)
        {
            TotalBudgetaryValue = totalBudgetaryValue;
            ValueCategories = valueCategories;
            Beneficiaries = beneficiaries;
        }

        public decimal TotalBudgetaryValue { get; set; }
        public Dictionary<string, decimal> ValueCategories { get; set; }
        public Dictionary<string, decimal> Beneficiaries { get; set; }
    }
    public class InputSummaryModel
    {
        public InputSummaryModel(decimal investmentAmount, int length, decimal totalCost, List<FundingSourceModel> fundingSources)
        {
            InvestmentAmount = investmentAmount;
            Length = length;
            TotalCost = totalCost;
            FundingSources = fundingSources;
        }

        public decimal InvestmentAmount { get; set; }
        public int Length { get; set; }
        public decimal TotalCost { get; set; }
        public List<FundingSourceModel> FundingSources { get; set; }

        public string GetFundingSourceString()
        {
            return string.Join(" ", FundingSources.Select(fs => $"{fs.Name} ({fs.Value}%)"));
        }
    }
    public class FundingSourceModel
    {
        public FundingSourceModel(string name, decimal value)
        {
            Name = name;
            Value = value;
        }

        public string Name { get; set; }
        public decimal Value { get; set; }
    }
    public class ReturnSummaryModel
    {
        public ReturnSummaryModel(decimal social, decimal budgetary, decimal socioEconomic)
        {
            Social = social;
            Budgetary = budgetary;
            SocioEconomic = socioEconomic;
        }

        public decimal Social { get; set; }
        public decimal Budgetary { get; set; }
        public decimal SocioEconomic { get; set; }
    }
    public class SubjectiveWellbeingValuationSummaryModel
    {
        public SubjectiveWellbeingValuationSummaryModel(
            decimal totalSocialValue,
            Dictionary<string, decimal> valueCategories,
            Dictionary<string, decimal> beneficiaries)
        {
            TotalSocialValue = totalSocialValue;
            ValueCategories = valueCategories;
            Beneficiaries = beneficiaries;
        }

        public decimal TotalSocialValue { get; set; }
        public Dictionary<string, decimal> ValueCategories { get; set; }
        public Dictionary<string, decimal> Beneficiaries { get; set; }
    }
    public class WELLBYSummaryModel
    {
        public WELLBYSummaryModel(decimal pointTotal, decimal pointPerParticipant, decimal costPerPoint)
        {
            PointTotal = pointTotal;
            PointPerParticipant = pointPerParticipant;
            CostPerPoint = costPerPoint;
        }

        public decimal PointTotal { get; set; }
        public decimal PointPerParticipant { get; set; }
        public decimal CostPerPoint { get; set; }
    }

    public class SROIPage5Model
    {
        public SROIPage5Model() { }
        public SROIPage5Model(string description) {
            Description = description;
        }

        public string Description { get; set; }
    }

    public class SROIPage6Model
    {
        public SROIPage6Model() { }

        public List<OutcomeTableRecord> OutcomeTableRecords { get; set; }
    }
    public class OutcomeTableRecord
    {
        public OutcomeTableRecord()
        {
        }

        public OutcomeTableRecord(string valueType, string outcomeName, int n, decimal results, string source, decimal answerRate, int duration, string significance)
        {
            ValueType = valueType;
            OutcomeName = outcomeName;
            this.n = n;
            Results = results;
            Source = source;
            AnswerRate = answerRate;
            Duration = duration;
            Significance = significance;
        }

        public string ValueType { get; set; }
        public string OutcomeName { get; set; }
        public int n { get; set; }
        public decimal Results { get; set; }
        public string Source { get; set; }
        public decimal AnswerRate { get; set; }
        public int Duration { get; set; }
        public string Significance { get; set; }
    }

    public class SROIPage7Model
    {
        public SROIPage7Model() { }

        public List<BasicAlternativesTableRecord> BasicAlternativesTableRecords { get; set; }
    }
    public class BasicAlternativesTableRecord
    {
        public BasicAlternativesTableRecord()
        {
        }

        public BasicAlternativesTableRecord(string valueType, string outcomeName, decimal pCTAmount, decimal amount, string source, string comments)
        {
            ValueType = valueType;
            OutcomeName = outcomeName;
            PCTAmount = pCTAmount;
            Amount = amount;
            Source = source;
            Comments = comments;
        }

        public string ValueType { get; set; }
        public string OutcomeName { get; set; }
        public decimal PCTAmount { get; set; }
        public decimal Amount { get; set; }
        public string Source { get; set; }
        public string Comments { get; set; }
    }

    public class SROIPage8Model
    {
        public SROIPage8Model()
        {
        }

        public List<PricesTableRecord> PricesTableRecords { get; set; } 
    }
    public class PricesTableRecord
    {
        public PricesTableRecord()
        {
        }

        public PricesTableRecord(string valueType, string outcomeName, string beneficiary, decimal grossValue, string source, string comments)
        {
            ValueType = valueType;
            OutcomeName = outcomeName;
            Beneficiary = beneficiary;
            GrossValue = grossValue;
            Source = source;
            Comments = comments;
        }

        public string ValueType { get; set; }
        public string OutcomeName { get; set; }
        public string Beneficiary { get; set; }
        public decimal GrossValue { get; set; }
        public string Source { get; set;}
        public string Comments { get; set; }
    }

    public class SROIPage9Model
    {
        public SROIPage9Model()
        {
        }

        public List<WELLBYTableRecord> WELLBYTableRecords { get; set; }
    }
    public class WELLBYTableRecord
    {
        public WELLBYTableRecord()
        {
        }

        public WELLBYTableRecord(string outcomeName, string unit, decimal effectSize, decimal grossValue, decimal valuePrOutcome, decimal totalWellby, decimal totalSocial, string source)
        {
            OutcomeName = outcomeName;
            Unit = unit;
            EffectSize = effectSize;
            GrossValue = grossValue;
            ValuePrOutcome = valuePrOutcome;
            TotalWellby = totalWellby;
            TotalSocial = totalSocial;
            Source = source;
        }

        public string OutcomeName { get; set; }
        public string Unit { get; set; }
        public decimal EffectSize { get; set; }
        public decimal GrossValue { get; set; }
        public decimal ValuePrOutcome { get; set; }
        public decimal TotalWellby { get; set; }
        public decimal TotalSocial { get; set; }
        public string Source { get; set; }
    }

    public class SROIPage10Model
    {
        public SROIPage10Model()
        {
        }

        public List<SensitivityAnalysisRecord> SensitivityAnalysisRecords { get; set; }
    }
    public class SensitivityAnalysisRecord
    {
        public SensitivityAnalysisRecord()
        {
        }

        public SensitivityAnalysisRecord(string valueType, string outcomeName, string beneficiary, decimal gross, decimal effectSize, decimal total, decimal deadWeight, decimal displacement, decimal attribution, decimal dropOff, decimal net)
        {
            ValueType = valueType;
            OutcomeName = outcomeName;
            Beneficiary = beneficiary;
            Gross = gross;
            EffectSize = effectSize;
            Total = total;
            DeadWeight = deadWeight;
            Displacement = displacement;
            Attribution = attribution;
            DropOff = dropOff;
            Net = net;
        }

        public string ValueType { get; set; }
        public string OutcomeName { get; set; }
        public string Beneficiary { get; set; }
        public decimal Gross  { get; set; }
        public decimal EffectSize { get; set; }
        public decimal Total { get; set; }
        public decimal DeadWeight { get; set; }
        public decimal Displacement { get; set; }
        public decimal Attribution { get; set; }
        public decimal DropOff { get; set; }
        public decimal Net { get; set; }
    }

    public class SROIPage11Model 
    {
        public SROIPage11Model() { }

        public List<AdditionalCommentsRecord> AdditionalCommentsRecords { get; set; }
    }
    public class AdditionalCommentsRecord
    {
        public AdditionalCommentsRecord()
        {
        }

        public AdditionalCommentsRecord(string valueType, string outcomeName, string beneficiary, string outcomeComments, string sensitivityComments)
        {
            ValueType = valueType;
            OutcomeName = outcomeName;
            Beneficiary = beneficiary;
            OutcomeComments = outcomeComments;
            SensitivityComments = sensitivityComments;
        }

        public string ValueType { get; set; }
        public string OutcomeName { get; set; }
        public string Beneficiary { get; set; }
        public string OutcomeComments { get; set; }
        public string SensitivityComments { get; set; }
    }

}
