using System;
using System.Collections.Generic;
using API.Models.Notifications;
using API.Models.Projects;

namespace API.Dynamo.Seeds
{
    public class NotificationSeed
    {
        public static readonly List<Notification> All = new List<Notification>
        {
            new Notification()
            {
                Active = true,
                CreatedAt = DateTime.Now,
                DeliveryType = "",
                NotificationType = NotificationType.Survey,
            },
        };
    }
}
