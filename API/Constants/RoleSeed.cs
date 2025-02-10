using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;

namespace API.Constants
{
    public class RoleSeed
    {
        // TODO for permission based auth
        public string Id;
        public List<Claim> Claims;

        public const string StandardRoleId = "std";
        public const string SuperRoleId = "super";
        public const string AdministratorRoleId = "administrator";

        public static readonly RoleSeed Standard = new RoleSeed
        {
            Id = StandardRoleId,
            Claims = new List<Claim>
            {
                // new Claim(JwtClaimNames.Permission, Inquiries.Read),
                // new Claim(JwtClaimNames.Permission, Inquiries.Write),
                // new Claim(JwtClaimNames.Permission, Surveys.Read),
                // new Claim(JwtClaimNames.Permission, Surveys.Write),
                // new Claim(JwtClaimNames.Permission, Journals.Read),
                // new Claim(JwtClaimNames.Permission, Journals.Write),
                new Claim(JwtClaimNames.Permission, Permissions.Users.Read),
                // new Claim(JwtClaimNames.Permission, Youth.Read),
                // new Claim(JwtClaimNames.Permission, Youth.Write),
            },
        };

        public static readonly RoleSeed Super = new RoleSeed
        {
            Id = SuperRoleId,
            Claims = Standard.Claims.Concat(new List<Claim>
            {
                new Claim(JwtClaimNames.Permission, Permissions.Users.Write),
                new Claim(JwtClaimNames.Permission, Permissions.Project.Write)
            }).ToList(),
        };
        
        public static readonly RoleSeed Adiministrator = new RoleSeed
        {
            Id = AdministratorRoleId,
            Claims = Standard.Claims.Concat(new List<Claim>
            {
                new Claim(JwtClaimNames.Permission, Permissions.Users.Read),
            }).ToList(),
        };

        public static readonly List<RoleSeed> All = new List<RoleSeed> {Standard, Super, Adiministrator};
    }
}