using Amazon.DynamoDBv2;
using API.Models.Admin;

namespace API.Repositories
{
    public interface IAdminContext
    {
        public ICrudRepository<AdminUser> Admins { get; }
    }

    public class AdminContext : IAdminContext
    {
        public ICrudRepository<AdminUser> Admins { get; }

        public AdminContext(IAmazonDynamoDB client)
        {
            Admins = new AdminRepository(client);
        }
    }

    public class AdminRepository : CrudRepository<AdminUser>
    {
        public override string SortKeyValue(AdminUser model) => model.Id;
        public override string ModelPrefix => AdminUser.Prefix;
        public override string ParentPrefix => "ADMIN";

        public AdminRepository(IAmazonDynamoDB client) : base(client)
        {
        }
    }
}