using API.Models.Projects;
using API.Models.Strategy;
using Nest;
using System;
using System.Collections.Generic;
using System.Xml.Linq;

namespace API.Models.Analytics
{
    public class RegistrationStatus
    {
        public string Id { get; set; } = string.Empty;
        public string ProjectId { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public bool IsRegistered { get; set; } = false;
        public string Status { get; set; } = string.Empty;
        public DateTime LatestRegisteredDate { get; set; } = DateTime.Now;
        public string Note { get; set; } = string.Empty;

        public string EffectId { get; set; } = string.Empty;
        public double Value { get; set; }
        public string Category { get; set; } = string.Empty;
        public List<PatientTag> Tags { get; set; } = new List<PatientTag>();
        public StrategyEffect LatestEffect { get; set; }

        public RegistrationStatus(string id, string projectId, string name, List<PatientTag> tags, string category)
        { Id = id; ProjectId = projectId; Name = name; Tags = tags;  Category = category; }

        public RegistrationStatus(string id, string projectId, string name, string type, bool isRegistered, string status, DateTime latestRegisteredDate, string note, string effectId, string category, List<PatientTag> tags, StrategyEffect latestEffect)
        {
            Id = id;
            ProjectId = projectId;
            Name = name;
            Type = type;
            IsRegistered = isRegistered;
            Status = status;
            LatestRegisteredDate = latestRegisteredDate;
            Note = note;
            EffectId = effectId;
            Category = category;
            Tags = tags;
            LatestEffect = latestEffect;
        }
    }
}