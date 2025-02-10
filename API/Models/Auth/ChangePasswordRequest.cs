using System.ComponentModel.DataAnnotations;

namespace API.Models.Auth
{
    public class ChangePasswordRequest
    {
        public string OldPassword { get; set; }
        
        [RegularExpression(@"^(?=.*[a-zæøå])(?=.*[A-ZÆØÅ])(?=.*\d).{6,64}$",
            ErrorMessage =
                "Adgangskoden skal være minimum 6 tegn lang, og den skal indeholde minimum ét stort og ét småt bogstav, samt minimum ét tal.")]
        public string NewPassword { get; set; }
    }
}