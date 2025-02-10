using API.Models.Auth;
using API.Models.Projects;

namespace API.Dynamo.Seeds
{
    public static class DynamoSeedAdmin
    {
        public static readonly Project Project = new Project
        {
            Id = "admin",
            Name = "Admin",
        };

        public static readonly AuthUser AdminUser = new AuthUser
        {
            Id = "admin",
            FirstName = "Admin",
            LastName = "InnoSocial",
            Email = "admin@innosocial.dk",
            PhoneNumber = "31430761",
            Active = true,
        };

        public const string AdminPassword = "Password1";
    }
}