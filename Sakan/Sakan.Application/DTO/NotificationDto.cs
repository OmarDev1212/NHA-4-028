using Sakan.Domain.Entities;

namespace Sakan.Application.DTO;

public class NotificationDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = default!;
    public string Body { get; set; } = default!;
    public bool IsRead { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public Guid? RelatedEntityId { get; set; }
    public string? RelatedEntityType { get; set; }

    public static NotificationDto FromEntity(Notification notification) => new()
    {
        Id = notification.Id,
        Title = notification.Title,
        Body = notification.Body,
        IsRead = notification.IsRead,
        CreatedAt = notification.CreatedAt,
        RelatedEntityId = notification.RelatedEntityId,
        RelatedEntityType = notification.RelatedEntityType
    };
}
