using System;
using System.Collections.Generic;
using Amazon.DynamoDBv2.DataModel;
using API.Constants;

namespace API.Models.Projects;

[DynamoDBTable(TableNames.Projects)]
public class ProjectCommunication: CrudModel
{
    public const string CommunicationTypeWelcome = "Welcome";
    public const string CommunicationTypeSurvey = "Survey";
    

    public const string Prefix = "COMMUNICATION";
    public string MessageType { get; set; }
    public string MessageContent { get; set; }
    

}