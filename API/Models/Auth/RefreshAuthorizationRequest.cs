using System.ComponentModel.DataAnnotations;

namespace API.Models.Auth
{
    public class RefreshAuthorizationRequest
    {
        [DataType(DataType.Text)] public string RefreshToken { get; set; }
        
        [DataType(DataType.Text)]  public string Language { get; set; }
    }
}