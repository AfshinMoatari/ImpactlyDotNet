using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Amazon.DynamoDBv2.DataModel;
using API.Constants;

namespace API.Models.Strategy;

[DynamoDBTable(TableNames.Strategy)]
public class FieldTemplate: CrudPropModel
{
    public const string Prefix = "FIELDTEMPLATE";
    public string Name { get; set; }
    public string TextLanguage { get; set; }
    public List<FieldChoice> FieldChoices { get; set; }
}