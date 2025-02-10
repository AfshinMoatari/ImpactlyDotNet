using System;
using System.Threading.Tasks;
using API.Handlers;
using API.Models.Admin;
using API.Models.Auth;
using API.Models.Projects;
using API.Repositories;
using Impactly.Test.IntegrationTests.Models;
using Impactly.Test.Utils;
using Microsoft.Extensions.DependencyInjection;
using Xunit.Abstractions;

namespace Impactly.Test.IntegrationTests.ServiceHelper;

public class UserHelper
{
    private readonly TestFixture _fixture;
    private readonly ITestOutputHelper _output;

    public UserHelper(TestFixture testFixture, ITestOutputHelper testOutput)
    {
        _fixture = testFixture;
        _output = testOutput;
    }

    public async Task<AdminUser> AddAdminUser(SignInWithEmailRequest loginRequest)
    {
        var userContext = _fixture.Server.Services.GetRequiredService<IUserContext>();
        var authHandler = _fixture.Server.Services.GetRequiredService<IAuthHandler>();
        var adminContext = _fixture.Server.Services.GetRequiredService<IAdminContext>();
        var id = Guid.NewGuid().ToString();
        var adminUser = new AuthUser()
        {
            Active = true,
            Email = loginRequest.Email,
            Id = id,
            PasswordHashB64 = authHandler.HashUserPassword(id, loginRequest.Password),
        };
        await userContext.Users.ReadOrCreate(adminUser).ConfigureAwait(false);
        return await adminContext.Admins.Create(AdminUser.FromAuthUser(adminUser)).ConfigureAwait(false);
    }

    public async Task<ProjectUser> AddUserToProject(string projectId, AuthUser authUser, string roleId)
    {
        var projectContext = _fixture.Server.Services.GetRequiredService<IProjectContext>();
        return await projectContext.CreateProjectUser(projectId, new ProjectUser
        {
            Id = authUser.Id,
            Email = authUser.Email,
            FirstName = authUser.FirstName,
            LastName = authUser.LastName,
            PhoneNumber = authUser.PhoneNumber,
            RoleId = roleId,
        }).ConfigureAwait(false);
    }
}