using System.Collections.Generic;
using API.Constants;
using API.Models.Auth;

namespace API.Dynamo.Seeds
{
    public static class RoleSeeds
    {
        public static readonly Role StandardRole = new Role()
        {
            Id = "standard",
            Permissions = new List<string>()
            {
                Permissions.Users.Read,
            }
        };
        
        public static readonly Role SuperRole = new Role()
        {
            Id = "super",
            Permissions = new List<string>()
            {
                Permissions.Strategy.Read,
                Permissions.Strategy.Write,
                Permissions.Users.Read,
                Permissions.Users.Write,
                Permissions.Project.Read,
                Permissions.Project.Write,
            }
        };

        public static List<Role> Roles = new List<Role>() {StandardRole, SuperRole};
    }
}