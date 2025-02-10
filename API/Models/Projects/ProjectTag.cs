using System;
using Amazon.DynamoDBv2.DataModel;
using API.Constants;

namespace API.Models.Projects
{
    [DynamoDBTable(TableNames.Projects)]
    public class ProjectTag : CrudPropModel
    {
        public const string Prefix = "TAG";
        public string Name { get; set; }
        public string Color { get; set; }
        public DateTime? DeletedAt { get; set; } = null;
    }

    [DynamoDBTable(TableNames.Patients)]
    public class PatientTag : ProjectTag
    {
        public string ProjectTagId { get; set; }
    }
}