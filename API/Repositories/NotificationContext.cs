using Amazon.DynamoDBv2;
using API.Models.Notifications;

namespace API.Repositories;

public interface INotificationContext
{
    public ICrudRepository<Notification> Notifications { get; }
}

public class NotificationContext : INotificationContext
{
    public ICrudRepository<Notification> Notifications { get; }

    public NotificationContext(IAmazonDynamoDB client)
    {
        Notifications = new NotificationRepository(client);
    }
}

public class NotificationRepository : CrudRepository<Notification>
{
    public override string SortKeyValue(Notification model) => model.Id;
    public override string ModelPrefix => Notification.Prefix;
    public override string ParentPrefix => "NOTIFICATION";

    public NotificationRepository(IAmazonDynamoDB client) : base(client)
    {
    }
}