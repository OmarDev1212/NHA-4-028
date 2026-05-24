using Sakan.Application.DTO;
using Sakan.Domain.Contracts;
using Sakan.Domain.Entities;

namespace Sakan.Application.Services.Notifications;

public class NotificationService(IUnitOfWork unitOfWork) : INotificationService
{
    public async Task NotifyAsync(
        string recipientId,
        string title,
        string body,
        Guid? relatedEntityId = null,
        string? relatedEntityType = null)
    {
        var repo = unitOfWork.GetRepository<Notification, Guid>();
        await repo.AddAsync(new Notification
        {
            Id = Guid.NewGuid(),
            RecipientId = recipientId,
            Title = title,
            Body = body,
            Channel = "InApp",
            IsRead = false,
            CreatedAt = DateTimeOffset.UtcNow,
            RelatedEntityId = relatedEntityId,
            RelatedEntityType = relatedEntityType,
            IsDeleted = false
        });
        await unitOfWork.SaveChangesAsync();
    }

    public async Task<IEnumerable<NotificationDto>> GetInboxAsync(string userId, bool unreadOnly = false)
    {
        var repo = unitOfWork.GetRepository<Notification, Guid>();
        var notifications = (await repo.GetAllAsync(n =>
            !n.IsDeleted && n.RecipientId == userId && (!unreadOnly || !n.IsRead)))
            .OrderByDescending(n => n.CreatedAt);
        return notifications.Select(NotificationDto.FromEntity);
    }

    public async Task<bool> MarkAsReadAsync(Guid notificationId, string userId)
    {
        var repo = unitOfWork.GetRepository<Notification, Guid>();
        var notification = await repo.GetByIdAsync(notificationId);
        if (notification is null || notification.IsDeleted || notification.RecipientId != userId)
            return false;

        notification.IsRead = true;
        repo.Update(notification);
        return await unitOfWork.SaveChangesAsync() > 0;
    }

    public async Task<int> GetUnreadCountAsync(string userId)
    {
        var repo = unitOfWork.GetRepository<Notification, Guid>();
        var notifications = await repo.GetAllAsync(n =>
            !n.IsDeleted && n.RecipientId == userId && !n.IsRead);
        return notifications.Count();
    }
}
