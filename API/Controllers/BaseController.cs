using API.Constants;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Net.Http.Headers;

namespace API.Controllers
{
    public class BaseController : ControllerBase
    {
        protected string CurrentUserId() => HttpContext.User.FindFirst(JwtClaimNames.UserId)?.Value;
        protected string CurrentUserName() => HttpContext.User.FindFirst(JwtClaimNames.UserDisplayName)?.Value;
        protected string CurrentUserEmail() => HttpContext.User.FindFirst(JwtClaimNames.UserEmail)?.Value;
        protected string CurrentProjectId() => HttpContext.User.FindFirst(JwtClaimNames.ProjectId)?.Value;
        protected string CurrentProjectName() => HttpContext.User.FindFirst(JwtClaimNames.ProjectName)?.Value;

        protected string CurrentLanguage() => HttpContext.User.FindFirst(JwtClaimNames.TextLanguage)?.Value;
        protected string CurrentRoleId() => HttpContext.User.FindFirst(JwtClaimNames.RoleId)?.Value;
        
        protected string CurrentAccessToken()
        {
            var header = Request.Headers[HeaderNames.Authorization].ToString();
            return string.IsNullOrEmpty(header) ? null : header.Replace("Bearer ", "");
        }

        protected ISystemMessage GetMessage()
        {
            var language = CurrentLanguage()??Languages.Default;
            return language switch
            {
                Languages.English => new MessageEnglish(),
                Languages.Danish => new MessageDanish(),
                _ => new MessageDanish()
            };
        }
        
        protected bool CheckLanguage(string language)
        {
            if (language.IsNullOrEmpty())
            {
                language = Languages.Default;
            }

            if (CurrentLanguage().IsNullOrEmpty())
            {
                return true;
            }

            return language == CurrentLanguage();
        }
    }
}