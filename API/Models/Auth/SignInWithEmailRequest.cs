using System.ComponentModel.DataAnnotations;

namespace API.Models.Auth
{
    public class SignInWithEmailRequest
    {
        [Required, DataType(DataType.EmailAddress)]
        public string Email { get; set; }

        [Required, DataType(DataType.Password)]
        public string Password { get; set; }
        
        public string Language { get; set; }
    }
}