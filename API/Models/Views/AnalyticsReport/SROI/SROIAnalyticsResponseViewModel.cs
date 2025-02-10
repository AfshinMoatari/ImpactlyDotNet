using Newtonsoft.Json;
using System.Collections.Generic;

public class RootObject
{
    [JsonProperty("code")]
    public int Code { get; set; }

    [JsonProperty("msg")]
    public string Msg { get; set; }

    [JsonProperty("results")]
    public SROIAnalyticsResponseViewModel Results { get; set; }

    [JsonProperty("count")]
    public int Count { get; set; }
}
public class SROIAnalyticsResponseViewModel
{
    [JsonProperty("id")]
    public int Id { get; set; }

    [JsonProperty("beneficiary_distribution")]
    public List<BeneficiaryDistribution> BeneficiaryDistribution { get; set; }

    [JsonProperty("outcome_distribution")]
    public List<OutcomeDistribution> OutcomeDistribution { get; set; }

    [JsonProperty("outcome_beneficiary_list")]
    public List<OutcomeBeneficiaryList> OutcomeBeneficiaryList { get; set; }

    [JsonProperty("start_year")]
    public int StartYear { get; set; }

    [JsonProperty("duration")]
    public int Duration { get; set; }

    [JsonProperty("total_namely_value")]
    public decimal TotalNamelyValue { get; set; }

    [JsonProperty("total_present_value")]
    public decimal TotalPresentValue { get; set; }

    [JsonProperty("net_present_value")]
    public decimal NetPresentValue { get; set; }

    [JsonProperty("social_return")]
    public decimal SocialReturn { get; set; }

    [JsonProperty("total_social_value")]
    public decimal TotalSocialValue { get; set; }

    [JsonProperty("total_budgetary_value")]
    public decimal TotalBudgetaryValue { get; set; }

    [JsonProperty("total_social_value_namely")]
    public decimal TotalSocialValueNamely { get; set; }

    [JsonProperty("total_budgetary_value_namely")]
    public decimal TotalBudgetaryValueNamely { get; set; }

    [JsonProperty("social_cost_benefit_ratio")]
    public decimal SocialCostBenefitRatio { get; set; }

    [JsonProperty("budget_cost_benefit_ratio")]
    public decimal BudgetCostBenefitRatio { get; set; }

    [JsonProperty("total_cost_benefit_ratio")]
    public decimal TotalCostBenefitRatio { get; set; }

    [JsonProperty("total_wellby_point")]
    public decimal TotalWellbyPoint { get; set; }

    [JsonProperty("total_wellby_per_person")]
    public decimal TotalWellbyPerPerson { get; set; }

    [JsonProperty("cost_per_wellby_point")]
    public decimal CostPerWellbyPoint { get; set; }

    [JsonProperty("social_investment")]
    public int SocialInvestment { get; set; }
}

public class BeneficiaryDistribution
{
    [JsonProperty("id")]
    public int Id { get; set; }

    [JsonProperty("beneficiary_name")]
    public string BeneficiaryName { get; set; }

    [JsonProperty("total_value")]
    public decimal TotalValue { get; set; }

    [JsonProperty("social_value")]
    public decimal SocialValue { get; set; }

    [JsonProperty("budge_value")]
    public decimal BudgeValue { get; set; }

    [JsonProperty("value_rate")]
    public decimal ValueRate { get; set; }

    [JsonProperty("wellby_point")]
    public decimal WellbyPoint { get; set; }

    [JsonProperty("wellby_per_person")]
    public decimal WellbyPerPerson { get; set; }

    [JsonProperty("total_value_proportion")]
    public decimal TotalValueProportion { get; set; }

    [JsonProperty("social_value_proportion")]
    public decimal SocialValueProportion { get; set; }

    [JsonProperty("budget_value_proportion")]
    public decimal BudgetValueProportion { get; set; }

    [JsonProperty("social_value_total")]
    public int SocialValueTotal { get; set; }
}

public class OutcomeDistribution
{
    [JsonProperty("id")]
    public int Id { get; set; }

    [JsonProperty("outcome_name")]
    public string OutcomeName { get; set; }

    [JsonProperty("total_value")]
    public decimal TotalValue { get; set; }

    [JsonProperty("value_rate")]
    public decimal ValueRate { get; set; }

    [JsonProperty("wellby_point")]
    public decimal WellbyPoint { get; set; }

    [JsonProperty("wellby_per_person")]
    public decimal WellbyPerPerson { get; set; }

    [JsonProperty("social_value")]
    public decimal SocialValue { get; set; }

    [JsonProperty("budget_value")]
    public decimal BudgetValue { get; set; }

    [JsonProperty("social_value_rate")]
    public decimal SocialValueRate { get; set; }

    [JsonProperty("budget_value_rate")]
    public decimal BudgetValueRate { get; set; }

    [JsonProperty("total_value_proportion")]
    public decimal TotalValueProportion { get; set; }

    [JsonProperty("social_value_proportion")]
    public decimal SocialValueProportion { get; set; }

    [JsonProperty("budget_value_proportion")]
    public decimal BudgetValueProportion { get; set; }

    [JsonProperty("social_value_total")]
    public int SocialValueTotal { get; set; }
}

public class OutcomeBeneficiaryList
{
    [JsonProperty("id")]
    public int Id { get; set; }

    [JsonProperty("outcome_name")]
    public string OutcomeName { get; set; }

    [JsonProperty("effect_type")]
    public string EffectType { get; set; }

    [JsonProperty("value_type")]
    public string ValueType { get; set; }

    [JsonProperty("beneficiary_name")]
    public string BeneficiaryName { get; set; }

    [JsonProperty("population")]
    public int Population { get; set; }

    [JsonProperty("duration")]
    public int Duration { get; set; }

    [JsonProperty("effect_size")]
    public decimal EffectSize { get; set; }

    [JsonProperty("effect_number")]
    public decimal EffectNumber { get; set; }

    [JsonProperty("benchmark")]
    public decimal Benchmark { get; set; }

    [JsonProperty("benchmark_pct")]
    public decimal BenchmarkPct { get; set; }

    [JsonProperty("gross_unit_value")]
    public decimal GrossUnitValue { get; set; }

    [JsonProperty("total_gross_value")]
    public decimal TotalGrossValue { get; set; }

    [JsonProperty("deadweight_rate")]
    public decimal DeadweightRate { get; set; }

    [JsonProperty("displacement_rate")]
    public decimal DisplacementRate { get; set; }

    [JsonProperty("attribution_rate")]
    public decimal AttributionRate { get; set; }

    [JsonProperty("dropoff_rate")]
    public decimal DropoffRate { get; set; }

    [JsonProperty("total_net_value")]
    public decimal TotalNetValue { get; set; }

    [JsonProperty("answer_rate")]
    public decimal AnswerRate { get; set; }

    [JsonProperty("wellby_per_person")]
    public decimal WellbyPerPerson { get; set; }

    [JsonProperty("wellby_points")]
    public decimal WellbyPoints { get; set; }

    [JsonProperty("total_social_value")]
    public decimal TotalSocialValue { get; set; }

    [JsonProperty("total_budget_value")]
    public decimal TotalBudgetValue { get; set; }

    [JsonProperty("significance")]
    public string Significance { get; set; }

    [JsonProperty("outcome_description")]
    public string OutcomeDescription { get; set; }

    [JsonProperty("source")]
    public string Source { get; set; }

    [JsonProperty("outcome_comments")]
    public string OutcomeComments { get; set; }

    [JsonProperty("beneficiary_comments")]
    public string BeneficiaryComments { get; set; }

    [JsonProperty("sensitivity_comments")]
    public string SensitivityComments { get; set; }

    [JsonProperty("social_value_total")]
    public int SocialValueTotal { get; set; }
}
