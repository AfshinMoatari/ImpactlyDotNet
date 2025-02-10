using System.Runtime.CompilerServices;
using API.Constants;

namespace API.Views
{
    public class ForgotPasswordEmail : BaseEmail
    {
        public string UserName { get; set; }
        public override string GetThisClassName()
        {
            return GetType().Name;
        }

        public ForgotPasswordEmail(ISystemMessage message) : base(message)
        {
            Lines = message.ForgetPasswordEmailLines();
        }
        
    }
}