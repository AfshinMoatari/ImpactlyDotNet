namespace API.Models.Auth
{
    public class SignInResponse
    {
        public Authorization Authorization { get; set; }
        public AuthUser User { get; set; }
    }
}