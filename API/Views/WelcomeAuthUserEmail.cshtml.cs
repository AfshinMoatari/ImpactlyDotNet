using API.Constants;

namespace API.Views
{
    public class WelcomeAuthUserEmail : BaseEmail
    {
        
        public string UserName { get; set; }
        
        public string InviterName { get; set; }
        public string ProjectName { get; set; }

        public override string GetThisClassName()
        {
            return GetType().Name;
        }

        public WelcomeAuthUserEmail(ISystemMessage message) : base(message)
        {
            Lines = message.WelcomeAuthUserEmailLines();
        }
        
    }
}