using System;
using System.Threading.Tasks;
using OpenAI.GPT3;
using OpenAI.GPT3.Managers;
using OpenAI.GPT3.ObjectModels.RequestModels;
using OpenAI.GPT3.ObjectModels;

namespace API.Helpers;

public static class Gtp3Helper
{
    private const string Key = "";

    public const string ProviderName = "OpenAI-ChatGTP";
    
    public static OpenAIService Gtp3Service { set; get; }

    public static OpenAIService GetGtp3Service()
    {
        return Gtp3Service ??= new OpenAIService(new OpenAiOptions()
        {
            ApiKey = Key
        });
    }
    
    public static async Task<string> ReadCompletion(string question)
    {
        var gpt3 = GetGtp3Service();
        var answers = string.Empty;
        var completionResult = await gpt3.Completions.CreateCompletion(new CompletionCreateRequest()
        {
            Prompt = question,
            Model = OpenAI.GPT3.ObjectModels.Models.TextDavinciV2,
            Temperature = 0.5F,
            MaxTokens = 100
        });
        if (completionResult.Successful)
        {
            foreach (var choice in completionResult.Choices)
            {
                answers += choice.Text + Environment.NewLine;
            }
        }
        else
        {
            if (completionResult.Error == null)
            {
                throw new Exception("Unknown Error");
            }
            Console.WriteLine($"{completionResult.Error.Code}: {completionResult.Error.Message}");
        }
        return answers;
    }
}