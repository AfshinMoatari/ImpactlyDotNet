using System.ComponentModel.DataAnnotations;

namespace API.Models.Auth
{
    public class ForgotPasswordRequest
    {
        [Required, DataType(DataType.EmailAddress)]
        public string Email { get; set; }
    }
}