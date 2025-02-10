using API.Constants;

namespace API.Views
{
    public class WelcomePatientEmail : BaseEmail
    {
        public string UserName { get; set; }
        public string ProjectName { get; set; }
        
        public override string GetThisClassName()
        {
            return GetType().Name;
        }

        public WelcomePatientEmail(ISystemMessage message) : base(message)
        {
            Lines = message.WelcomePatientEmailLines();
        }
    }
}