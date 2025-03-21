﻿using System;
using System.Collections.Generic;
using Amazon.DynamoDBv2.DataModel;
using API.Constants;

namespace API.Models.Strategy
{
    [DynamoDBTable(TableNames.Strategy)]
    public class BatchSendoutFrequency : CrudPropModel
    {
        public const string Prefix = "FREQUENCY";
        public string Name { get; set; }
        public End End { get; set; }
        public string CronExpression { get; set; }

        public List<string> PatientsId { get; set; }
        public List<SurveyProperty> Surveys { get; set; }
    }

    public enum EndType
    {
        Never,
        Occur,
        IMMEDIATE
    }

    public class End
    {
        public EndType Type { get; set; }
        public int Occurrences { get; set; }
        public DateTime EndDate { get; set; }
    }
}