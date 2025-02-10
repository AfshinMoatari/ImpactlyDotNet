using API.Constants;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace API.Views
{
    public class BaseEmail : PageModel
    {
        protected readonly ISystemMessage SystemMessage;

        public BaseEmail(ISystemMessage message)
        {
            this.SystemMessage = message;
        }
        public string DownloadUrl { get; set; }
        public string Title { get; set; }
        public string[] Lines { get; set; }
        public string Message { get; set; }

        public virtual string GetThisClassName()
        {
            return GetType().Name;
        }
        
        public string GetLine(int index)
        {
            return Lines[index];
        }
    }
}