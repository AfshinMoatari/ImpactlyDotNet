using API.Constants;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace API.Views
{
    public class SurveyEmail : BaseEmail
    {
        public string UserName { get; set; }
        public string ProjectName { get; set; }
        
        public override string GetThisClassName()
        {
            return GetType().Name;
        }

        public SurveyEmail(ISystemMessage message) : base(message)
        {
            Lines = message.SurveyEmailLines();
        }
    }
}