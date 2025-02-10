using System.Threading.Tasks;
using API.Models.Projects;
using API.Repositories;
using Impactly.Test.IntegrationTests.Client;
using Impactly.Test.Utils;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Impactly.Test.IntegrationTests.Repositories
{
    [Collection("Integration Test")]
    public class UsersContextTest
    {
        private readonly IUserContext _userContext;
        private readonly TestFixture _fixture;

        public UsersContextTest(TestFixture fixture)
        {
            _fixture = fixture;
            _userContext = _fixture.Server.Services.GetRequiredService<IUserContext>();
        }
        

        public async Task ReadByPhoneNumber()
        {
            await _fixture.Client.SignInProject();
            
            var phoneNumber = "5305000454";
            var user = new ProjectUser()
            {
                Email = "test2+user@innosocial.dk",
                FirstName = "Test",
                LastName = "Patient",
                PhoneNumber = phoneNumber,
            };
            
            await _fixture.Client.Post("/api/web/v1/projects/" + AuthExtension.AdminProjectId + "/users", user);

            var authUser = await _userContext.Users.ReadByPhoneNumber(phoneNumber);
            Assert.NotEmpty(authUser.Id);
            Assert.Equal(user.Email, authUser.Email);
            Assert.Equal(user.FirstName, authUser.FirstName);
            Assert.Equal(user.LastName, authUser.LastName);
            
            _fixture.Client.SignOut();
        }
    }
}