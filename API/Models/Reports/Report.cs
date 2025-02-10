using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Amazon.DynamoDBv2.DataModel;
using API.Constants;

namespace API.Models.Reports
{
    [DynamoDBTable(TableNames.Projects)]
    public class Report : CrudPropModel
    {
        public const string Prefix = "REPORT";
        public string Name { get; set; }
        public List<ReportModuleConfig> ModuleConfigs { get; set; } = new List<ReportModuleConfig>();
        public string CodeId { get; set; }
        public Dictionary<string, List<Layout>> Layouts { get; set; }

        public List<FreeText> FreeTexts { get; set; }

        public List<Image> Images { get; set; }
    }

    public class Layout
    {
        public string I { get; set; }
        public int X { get; set; }
        public int? Y { get; set; }
        public int W { get; set; }
        public int H { get; set; }
        public int MinW { get; set; }
        public int MinH { get; set; }
    }



    public class FreeText
    {
        public string Id { get; set; }
        public string Title { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public string Contents { get; set; }
    }

    public class Image
    {
        public string Title { get; set; }
        public string Key { get; set; }
        public string Url { get; set; }
        public string Description { get; set; }
        public string UploadedBy { get; set; }
        public DateTime UploadedAt { get; set; }
        
    }
}