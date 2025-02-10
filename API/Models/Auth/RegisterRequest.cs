using System.ComponentModel.DataAnnotations;

namespace API.Models.Auth
{
    public class RegisterRequest
    {
        [DataType(DataType.Password)] public string Password { get; set; }
        public bool PrivacyPolicy { get; set; }
    }
}