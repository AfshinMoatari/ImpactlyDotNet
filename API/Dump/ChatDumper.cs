using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using API.Models.Analytics;
using API.Models.Dump;
using API.Repositories;
using Nest;

namespace API.Dump;

public interface IChatDumper
{
    public Task<List<ChatData>> CreateChatMessages(string projectId, string id);
}

public class SurveyChatDumper : IChatDumper
{
    private readonly IAnalyticsContext _analyticsContext;
    private readonly IStrategyContext _strategyContext;
    
    public SurveyChatDumper(IAnalyticsContext analyticsContext, IStrategyContext strategyContext)
    {
        _analyticsContext = _analyticsContext;
        _strategyContext = strategyContext;
    }

    public async Task<List<ChatData>> CreateChatMessages(string projectId, string surveyId)
    {
        var entries = await GetEntryBatches(projectId, surveyId);
        var questions = new List<Question>();
        var messages = new List<ChatData>();
        foreach (var entry in entries)
        {
            var survey = await _strategyContext.Surveys.Read(projectId, entry.SurveyId) ??
                         await _strategyContext.Surveys.Read("TEMPLATE", entry.SurveyId);
            var name = survey?.Name;
            questions.Add(new Question()
            {
                Name = name,
                Score = entry.Score
            });
        }
        var names = questions.Select(s => s.Name).Distinct().ToList();
        foreach (var name in names)
        {
            var q1 = "What is " + name + "?";
            var q2 = "What does the score " + questions.FirstOrDefault(s => s.Name == name)!.Score + " mean for " +
                     name + "?";
            messages.Add(new ChatData()
            {
                Question = q1,
            });
            messages.Add(new ChatData()
            {
                Question = q2,
            });
        }

        return messages;
    }
    private async Task<List<EntryBatch>> GetEntryBatches(
        string projectId,
        string surveyId
    )
    {
        var start = DateTime.MinValue;
        var end = DateTime.MaxValue;
        var results = await _analyticsContext.EntryBatches.ReadBetween(new SurveyAccess()
        {
            ProjectId = projectId,
            SearchStart = start,
            SearchEnd = end,
        });
        
        return results.Where(r=>r.SurveyId == surveyId).ToList();
    }
    internal class Question
    {
        public string Name { get; set; }
        public double Score { get; set; }
    }
   
}

