using System;
using System.Text.Json.Serialization;
using Nest;

namespace API.Models
{
    public interface ICrudPropModel
    {
        public string ParentId { get; set; }
        public string PK { get; set; }
        public string SK { get; set; }

        public string GSIPK { get; set; }
        public string GSISK { get; set; }
        public string Id { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

    public abstract class CrudModel : CrudPropModel
    {
        [JsonIgnore] public override string ParentId { get; set; } = "META";
    }

    public abstract class CrudPropModel : ICrudPropModel
    {
        // OLTP   
        public const string GlobalSecondaryIndex = "SK-GSISK-index";
        public const string EmailIndex = "Email-index";
        public const string PhoneNumberIndex = "PhoneNumber-index";
        public const string StrategyIdIndex = "StrategyId-index";

        [PropertyName("PK")] 
        [JsonIgnore] 
        public virtual string PK { get; set; }

        [PropertyName("PK")] 
        [JsonIgnore] public virtual string SK { get; set; }

        [JsonIgnore] public virtual string GSIPK { get; set; }
        [JsonIgnore] public virtual string GSISK { get; set; }

        [PropertyName("Id")] public virtual string Id { get; set; }
        [PropertyName("CreatedAt")] public virtual DateTime CreatedAt { get; set; }
        [PropertyName("UpdatedAt")] public virtual DateTime UpdatedAt { get; set; }

        [PropertyName("ParentId")] public virtual string ParentId { get; set; }
    }
}