using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using API.Models.Analytics;
using API.Models.Auth;
using API.Models.Codes;
using API.Models.Cron;
using API.Models.Notifications;
using API.Models.Projects;
using API.Models.Reports;
using API.Models.Strategy;

namespace Impactly.Test.IntegrationTests.TestData;

public class ProjectData
{
    public Survey TestCustomSurvey { get; set; }
    public Survey TestValidatedSurvey { get; set; }
    public Strategy TestStrategy { get; set; }
    
    public ProjectPatient TestProjectPatient { get; set; }
    public Project TestProject { get; set; }
    
    public Report TestReport { get; set; }

    public EntryBatch TestCustomSurveyEntryBatch { get; set; }
    
    public EntryBatch TestValidatedSurveyEntryBatch { get; set; }
    
    public Notification TestNotification { get; set; }

    private readonly string prefixName;

    public AuthUser ProjectAdmin { get; set; }
    
    public SignInWithEmailRequest ProjectSuperUser { get; set; }
    
    public SignInWithEmailRequest ProjectStandardUser { get; set; }
    
    
    public ProjectCommunication TestWelcomeCommunication { get; set; }
    
    public ProjectCommunication TestSurveyCommunication { get; set; }
    

    public SurveyCode TestSurveyCode { get; set; }
    

    public ProjectData(string projectName, AuthUser projectAdmin)
    {
        prefixName = projectName + "_";
        ProjectAdmin = projectAdmin;
        TestProject = new Project()
        {
            Id = Guid.NewGuid().ToString(),
            Name = projectName,
        };
        
         TestCustomSurvey = new Survey()
        {
            Id = Guid.NewGuid().ToString(),
            Name = prefixName + "Test Customer Survey",
            LongName = prefixName +"Test Survey Long Name",
            Fields = new List<SurveyField>()
            {
                new()
                {
                    Id = Guid.NewGuid().ToString(),
                    Text = prefixName +"test survey field 1",
                    Index = 1,
                    Choices = new List<FieldChoice>()
                    {
                        new()
                        {
                            Id = Guid.NewGuid().ToString(),
                            Index = 1,
                            Text = prefixName +"choice 1",
                        },
                        new()
                        {
                            Id = Guid.NewGuid().ToString(),
                            Index = 2,
                            Text = prefixName +"choice 2",
                        }
                    }
                }
            }
        };

        TestValidatedSurvey = new Survey()
        {
            Id = Guid.NewGuid().ToString(),
            Name = prefixName +"Test Validated Survey",
            Fields = new List<SurveyField>()
            {
                new()
                {
                    Id = Guid.NewGuid().ToString(),
                    Text = prefixName +"test survey field 1",
                    Index = 1,
                    Choices = new List<FieldChoice>()
                    {
                        new()
                        {
                            Id = Guid.NewGuid().ToString(),
                            Index = 1,
                            Text = prefixName +"choice 1"
                        },
                        new()
                        {
                            Id = Guid.NewGuid().ToString(),
                            Index = 2,
                            Text = prefixName +"choice 2",
                        }
                    }
                }
            }
        };

        
        
        TestStrategy = new Strategy()
        {
            Id = Guid.NewGuid().ToString(),
            Name = prefixName +"Test Strategy",
            ParentId = TestProject.Id,
            Surveys = new List<SurveyProperty>()
            {
                new ()
                {
                    Id = TestCustomSurvey.Id,
                    Name = TestCustomSurvey.Name,
                }
            }
        };
        
        TestProjectPatient = new ProjectPatient()
        {
            FirstName = prefixName +"Patient",
            LastName = prefixName +"Lastname",
            Anonymity = true,
            IsActive = true,
            Id = Guid.NewGuid().ToString(),
            ParentId = TestProject.Id,
            Email = prefixName + "patient@email.com",
            PhoneNumber = "12342234",
            Region = "Capital",
            StrategyId = TestStrategy.Id,
        };
        var id = Guid.NewGuid().ToString();
        TestCustomSurveyEntryBatch = new EntryBatch()
        {
            Id = id,
            AnsweredAt = DateTime.Now,
            StrategyId = TestStrategy.Id,
            PatientId = TestProjectPatient.Id,
            ProjectId = TestProject.Id,
            SurveyId = TestCustomSurvey.Id,
            Entries = new List<FieldEntry>()
            {
                new()
                {
                    Id = Guid.NewGuid().ToString(),
                    FieldId = TestCustomSurvey.Fields.ToList()[0].Id,
                    FieldIndex = TestCustomSurvey.Fields.ToList()[0].Index,
                    FieldText = TestCustomSurvey.Fields.ToList()[0].Text,
                    ChoiceId = TestCustomSurvey.Fields.ToList()[0].Choices.ToList()[0].Id,
                    ChoiceText = TestCustomSurvey.Fields.ToList()[0].Choices.ToList()[0].Text,
                    PatientId = TestProjectPatient.Id,
                    StrategyId = TestStrategy.Id,
                    SurveyId = TestCustomSurvey.Id,
                    ProjectId = TestProject.Id,
                    ParentId = id,
                }
            }
        };

        
        TestReport = new Report()
        {
            Name = prefixName +"Test Report",
            Id = Guid.NewGuid().ToString(),
            ModuleConfigs = new List<ReportModuleConfig>()
            {
                new()
                {
                    Id = Guid.NewGuid().ToString(),
                    FieldId = TestCustomSurvey.Fields.ToList()[0].Id,
                    ProjectId = TestProject.Id,
                    SurveyId = TestCustomSurvey.Id,
                    StrategyId = TestStrategy.Id,
                    Name = TestCustomSurvey.Fields.ToList()[0].Text,
                }
            }
        };

        TestSurveyCode = new SurveyCode()
        {
            ProjectId = TestProject.Id,
            Id = Regex.Replace(Convert.ToBase64String(Guid.NewGuid().ToByteArray()), "[/+=]", ""),
            PatientId = TestProjectPatient.Id,
            StrategyId = TestStrategy.Id,
            Properties = new List<SurveyProperty>()
            {
                new SurveyProperty()
                {
                    ParentId = TestProject.Id,
                    Id = TestCustomSurvey.Id,
                }
            }

        };

        TestNotification = new Notification()
        {
            ProjectId = TestProject.Id,
            Message = "project " + TestProject.Id,
            NotificationType = NotificationType.Survey,
            PatientId = TestProjectPatient.Id,
            StrategyId = TestStrategy.Id,
            SendOutDate = DateTime.Now,
            SurveyCode = TestSurveyCode.Id.ToLower(),
        };

        TestWelcomeCommunication = new ProjectCommunication()
        {
            MessageContent = "Test Welcome Communication",
            MessageType = ProjectCommunication.CommunicationTypeWelcome,
        };

        TestSurveyCommunication = new ProjectCommunication()
        {
            MessageContent = "Test Survey Communication",
            MessageType = ProjectCommunication.CommunicationTypeSurvey,
        };

        ProjectSuperUser = new SignInWithEmailRequest()
        {
            Email = prefixName  + "2@impactly.dk",
            Password = "abcd",
        };
        
        ProjectStandardUser = new SignInWithEmailRequest()
        {
            Email = prefixName  + "3@impactly.dk",
            Password = "abcd",
        };


  
    }
}