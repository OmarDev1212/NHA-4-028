using Sakan.Application.DTO;

namespace Sakan.Application.Services.Notifications;

public interface INotificationService
{
    Task NotifyAsync(string recipientId, string title, string body, Guid? relatedEntityId = null, string? relatedEntityType = null);
    Task<IEnumerable<NotificationDto>> GetInboxAsync(string userId, bool unreadOnly = false);
    Task<bool> MarkAsReadAsync(Guid notificationId, string userId);
    Task<int> GetUnreadCountAsync(string userId);
}
