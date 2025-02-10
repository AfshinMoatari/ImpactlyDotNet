using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using API.Models.Auth;
using Impactly.Test.IntegrationTests.Client;
using Impactly.Test.Utils;
using Nest;
using Xunit;
using Xunit.Abstractions;
using Authorization = API.Models.Auth.Authorization;

namespace Impactly.Test.IntegrationTests.Controllers;

[Collection("Integration Test")]
public class AuthController
{

    private readonly ITestOutputHelper _output;
    private readonly TestFixture _fixture;
    private const string BaseUrl = "/api/web/v1/auth";

    public AuthController(ITestOutputHelper output, TestFixture testFixture)
    {
        _fixture = testFixture;
        _output = output;
    }

    [Fact]
    public async Task TestSignInWithToken()
    {
        await _fixture.Client.SignInAsAdmin(_fixture.InitAdmin.LoginAdmin).ConfigureAwait(false);
        var response = await _fixture.Client.Fetch<SignInResponse>(HttpMethod.Get, BaseUrl).ConfigureAwait(false);
        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var user = response.Value.User;
        var auth = response.Value.Authorization;
        Assert.NotNull(user);
        Assert.NotNull(auth);
        Assert.Equal(_fixture.InitAdmin.UserAdmin.Id, user.Id);
        Assert.NotEmpty(auth.AccessToken);
        _fixture.Client.SignOut();
    }

    [Fact]
    public async Task TestSignInWithEmail()
    {
        var request = new SignInWithEmailRequest()
        {
            Email = _fixture.InitProject.ProjectAdmin.Email,
            Password = _fixture.InitAdmin.UserAdminPassword,
        };
        var response = await _fixture.Client.Fetch<SignInResponse>(HttpMethod.Post, BaseUrl, request).ConfigureAwait(false);
        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var user = response.Value.User;
        var auth = response.Value.Authorization;
        Assert.NotNull(user);
        Assert.NotNull(auth);
        Assert.Equal(_fixture.InitAdmin.UserAdmin.Id, user.Id);
        Assert.NotEmpty(auth.AccessToken);
        _fixture.Client.SignOut();
        
    }

    [Fact]
    public async Task TestSignInWithProject()
    {
        await _fixture.Client.SignInAsAdmin(_fixture.InitAdmin.LoginAdmin).ConfigureAwait(false);
        var path = BaseUrl + "/projects/" + _fixture.InitProject.TestProject.Id;
        var response = await _fixture.Client.Post<ProjectAuthResponse>(path, null).ConfigureAwait(false);
        Assert.NotNull(response);
        var project = response.Project;
        var auth = response.Authorization;
        Assert.NotNull(project);
        Assert.Equal(_fixture.InitProject.TestProject.Id, project.Id);
        Assert.NotNull(auth);
        Assert.NotEmpty(auth.AccessToken);
        _fixture.Client.SignOut();
    }

    public async Task TestRefreshAuthorization()
    {
        var resp = await _fixture.Client.Fetch<SignInResponse>(HttpMethod.Post, BaseUrl, _fixture.InitAdmin.LoginAdmin).ConfigureAwait(false);
        Assert.NotNull(resp);
        Assert.Equal(HttpStatusCode.OK, resp.StatusCode);
        var auth = resp.Value.Authorization;
        const string path = BaseUrl + "/refresh";
        var expiredAt = auth.ExpiresAt;
        Assert.NotEmpty(auth.RefreshToken);
        _output.WriteLine("refresh token: " + auth.RefreshToken);
        var response = await _fixture.Client.Fetch<Authorization>(HttpMethod.Post, path,
            new RefreshAuthorizationRequest()
            {
                RefreshToken = auth.RefreshToken,
            }).ConfigureAwait(false);
        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.OK,response.StatusCode);
        Assert.NotNull(response.Value);
        Assert.True(response.Value.ExpiresAt.CompareTo(expiredAt) > 0);
        Assert.NotEmpty(response.Value.AccessToken);
        _fixture.Client.SignOut();
    }
    
    
}