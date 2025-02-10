using System;
using System.Collections.Generic;

namespace API.Models.Views.Strategy
{
    public class RegistrationViewModel
    {
        public string Id { get; set; }
        public string ProjectId { get; set; }
        public string PatientId { get; set; }
        public string EffectId { get; set; }
        public string EffectName { get; set; }
        public string Note { get; set; }
        public DateTime Date { get; set; }
        public string Type { get; set; }
        public double Value { get; set; }
        public string Category { get; set; }
        public StrategyEffectViewModel Before { get; set; }
        public StrategyEffectViewModel Now { get; set; }
        public List<string> Tags { get; set; }
    }
}
