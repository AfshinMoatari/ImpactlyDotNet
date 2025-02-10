using System;
using System.Collections.Generic;
using API.Constants;
using API.Models.Auth;
using Impactly.Test.IntegrationTests.Models;

namespace Impactly.Test.IntegrationTests.TestData;

public class AdminUserData
{
    public AuthUser UserAdmin { get; set; }
    public SignInWithEmailRequest LoginAdmin { get; set; }
    public Role RoleAdministrator { get; set; }
    public Role RoleSuper { get; set; }
    public Role RoleStandard { get; set; }
    public readonly string UserAdminPassword = "1234";

    public AdminUserData()
    {
        UserAdmin = new AuthUser()
        {
            Id = Guid.NewGuid().ToString(),
            Email = "han@impactly.dk",
            FirstName = "Firstname",
            LastName = "Lastname",
        };

        LoginAdmin = new SignInWithEmailRequest()
        {
            Email = "han@impactly.dk",
            Password = UserAdminPassword,
        };
        
        RoleAdministrator = new Role()
        {
            Id = "administrator",
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

        RoleStandard = new Role()
        {
            Id = "standard",
            Permissions = new List<string>()
            {
                Permissions.Users.Read,
            }
        };

        RoleSuper = new Role()
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
    }
    
}