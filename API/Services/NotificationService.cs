using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using API.Models.Notifications;
using API.Repositories;

namespace API.Services
{
    /// <summary>
    /// Service class for managing notifications.
    /// </summary>
    public class NotificationService
    {
        private readonly INotificationContext _notification;

        /// <summary>
        /// Initializes a new instance of the <see cref="NotificationService"/> class.
        /// </summary>
        /// <param name="notification">The notification context.</param>
        public NotificationService(INotificationContext notification)
        {
            _notification = notification;
        }

        /// <summary>
        /// Retrieves all notifications.
        /// </summary>
        /// <returns>A list of all notifications.</returns>
        public async Task<IEnumerable<Notification>> GetNotifications()
        {
            return (await _notification.Notifications.ReadAll()).ToList();
        }


        public async Task<IEnumerable<Notification>> GetNotifications(string projectId)
        {
            var notifications = (await _notification.Notifications.ReadAll()).ToList();
            var filteredNotifications = notifications.Where((x) => x.ProjectId == projectId && x.AnsweredAt == null);
            return filteredNotifications;
        }

        /// <summary>
        /// Retrieves notifications of a specific type.
        /// </summary>
        /// <param name="notificationType">The type of notifications to retrieve.</param>
        /// <returns>A list of notifications of the specified type.</returns>
        public async Task<List<Notification>> GetNotifications(NotificationType notificationType)
        {
            try
            {
                var notifications = await _notification.Notifications.ReadAll();
                var filteredNotifications = notifications
                    .Where(n => n.NotificationType == notificationType)
                    .ToList();
                return filteredNotifications;
            }
            catch (ArgumentException e)
            {
                throw new ArgumentException("The argument is not of type NotificationType.", e);
            }
        }

        /// <summary>
        /// Saves a new notification.
        /// </summary>
        /// <param name="notification">The notification to save.</param>
        /// <returns>True if the notification was successfully saved; otherwise, false.</returns>
        public async Task<bool> SaveNotification(Notification notification)
        {
            try
            {
                await _notification.Notifications.Create(notification);
                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        /// <summary>
        /// Updates an existing notification.
        /// </summary>
        /// <param name="notification">The notification to update.</param>
        /// <returns>True if the notification was successfully updated; otherwise, false.</returns>
        public async Task<bool> UpdateNotification(Notification notification)
        {
            try
            {
                await _notification.Notifications.Update(notification);
                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        public async Task<bool> DeleteNotification(string projectId, string notificationId)
        {
            try
            {
                await _notification.Notifications.Delete(notificationId);
            }
            catch (Exception e)
            {
                throw;
            }
            return true;
        }

        public async Task<bool> DeleteNotifications(List<Notification> notifications)
        {
            try
            {
                await _notification.Notifications.DeleteBatch(notifications);
            }
            catch (Exception e)
            {
                throw;
            }
            return true;
        }
    }
}