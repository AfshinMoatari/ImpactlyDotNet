using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace API.Models.Views.AnalyticsReport.SROI
{
    public class SROIAnalyticsAPIViewModel
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("cost")]
        public decimal Cost { get; set; }

        [JsonProperty("unit")]
        public string Unit { get; set; }

        [JsonProperty("start_date")]
        public DateTime StartDate { get; set; }

        [JsonProperty("duration")]
        public int Duration { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonProperty("population")]
        public int Population { get; set; }

        [JsonProperty("outcome")]
        public List<Outcome> Outcome { get; set; }
    }

    public class Outcome
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonProperty("start_year")]
        public int StartYear { get; set; }

        [JsonProperty("duration_years")]
        public int DurationYears { get; set; }

        [JsonProperty("effect_size")]
        public decimal EffectSize { get; set; }

        [JsonProperty("effect_type")]
        public string EffectType { get; set; }

        [JsonProperty("quo_scenario")]
        public decimal QuoScenario { get; set; }

        [JsonProperty("quo_scenario_source")]
        public string QuoScenarioSource { get; set; }

        [JsonProperty("quo_scenario_comment")]
        public string QuoScenarioComment { get; set; }

        [JsonProperty("answer_rate")]
        public decimal AnswerRate { get; set; }

        [JsonProperty("benchmark_value_pct")]
        public decimal BenchmarkValue { get; set; }

        [JsonProperty("source1")]
        public string Source { get; set; }

        [JsonProperty("comments")]
        public string Comments { get; set; }

        [JsonProperty("sensitivity_comments")]
        public string SensitivityComments { get; set; }

        [JsonProperty("dead_weight")]
        public decimal DeadWeight { get; set; }

        [JsonProperty("displacement_rate")]
        public decimal DisplacementRate { get; set; }

        [JsonProperty("attribution_rate")]
        public decimal AttributionRate { get; set; }

        [JsonProperty("drop_off_rate")]
        public decimal DropOffRate { get; set; }

        [JsonProperty("beneficiary")]
        public List<Beneficiary> Beneficiaries { get; set; }
    }

    public class Beneficiary
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("beneficiary_type")]
        public string BeneficiaryType { get; set; }

        [JsonProperty("effect_value")]
        public decimal EffectValue { get; set; }

        [JsonProperty("comments")]
        public string Comments { get; set; }
    }
}
