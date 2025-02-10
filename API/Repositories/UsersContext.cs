using System.Linq;
using System.Threading.Tasks;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using API.Models;
using API.Models.Auth;

namespace API.Repositories
{
    public interface IUserContext
    {
        public IUsersRepository Users { get; }
    }

    public class UserContext : IUserContext
    {
        public IUsersRepository Users { get; }

        public UserContext(IAmazonDynamoDB client)
        {
            Users = new UsersRepository(client);
        }
    }

    public interface IUsersRepository : ICrudRepository<AuthUser>
    {
        public Task<AuthUser> Upsert(AuthUser user);
        public Task<AuthUser> ReadOrCreate(AuthUser user);
        public Task<AuthUser> ReadByEmail(string email);
        public Task<AuthUser> ReadByPhoneNumber(string phoneNumber);
    }

    public class UsersRepository : CrudRepository<AuthUser>, IUsersRepository
    {
        public override string Prefix => AuthUser.Prefix;
        public override string SortKeyValue(AuthUser model) => model.FirstName + model.LastName;
        public override bool Descending => false;

        public UsersRepository(IAmazonDynamoDB client) : base(client)
        {
        }

        public async Task<AuthUser> Upsert(AuthUser user)
        {
            var createdUser = await ReadOrCreate(user);
            // upsert cannot override following fields
            user.Id = createdUser.Id;
            user.Email = createdUser.Email;
            user.PhoneNumber = createdUser.PhoneNumber;
            user.PasswordHashB64 = createdUser.PasswordHashB64;
            return await Update(user);
        }

        public async Task<AuthUser> ReadOrCreate(AuthUser user)
        {
            var userById = !string.IsNullOrEmpty(user.Id) ? await Read(user.Id) : null;
            if (userById != null)
            {
                return userById;
            }

            var userByEmail = !string.IsNullOrEmpty(user.Email) ? await ReadByEmail(user.Email) : null;
            if (userByEmail != null)
            {
                return userByEmail;
            }

            var userByPhoneNumber = !string.IsNullOrEmpty(user.PhoneNumber)
                ? await ReadByPhoneNumber(user.PhoneNumber)
                : null;

            if (userByPhoneNumber != null)
            {
                return userByPhoneNumber;
            }

            return await Create(user);
        }

        public async Task<AuthUser> ReadByEmail(string email)
        {
            if (string.IsNullOrEmpty(email)) return null;
            var emailLower = email.ToLower().Trim();
            var users = await Context.QueryAsync<AuthUser>(emailLower, new DynamoDBOperationConfig
            {
                IndexName = CrudPropModel.EmailIndex,
            }).GetRemainingAsync();
            return users.FirstOrDefault();
        }

        public async Task<AuthUser> ReadByPhoneNumber(string phoneNumber)
        {
            if (string.IsNullOrEmpty(phoneNumber)) return null;
            var phoneNumberLower = phoneNumber.ToLower().Trim();
            var users = await Context.QueryAsync<AuthUser>(phoneNumberLower, new DynamoDBOperationConfig
            {
                IndexName = CrudPropModel.PhoneNumberIndex,
            }).GetRemainingAsync();
            return users.FirstOrDefault();
        }
    }
}