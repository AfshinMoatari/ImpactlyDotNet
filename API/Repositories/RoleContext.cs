using Amazon.DynamoDBv2;
using API.Models.Auth;

namespace API.Repositories
{
    public interface IRoleContext
    {
        public ICrudRepository<Role> Roles { get; }
    }
    
    public class RoleContext : IRoleContext
    {
        public ICrudRepository<Role> Roles { get; }

        public RoleContext(IAmazonDynamoDB client)
        {
            Roles = new RoleRepository(client);
        }
    }
    
    public class RoleRepository : CrudRepository<Role>
    {
        public override string SortKeyValue(Role model) => model.Id;
        public override string ModelPrefix => Role.Prefix;

        public RoleRepository(IAmazonDynamoDB client) : base(client)
        {
        }
    }
}