namespace Sakan.Domain.Entities;

public class Notification : BaseEntity<Guid>
{
    public string Channel { get; set; } = "InApp";
    public string Title { get; set; } = null!;
    public string Body { get; set; } = null!;
    public bool IsRead { get; set; }
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public Guid? RelatedEntityId { get; set; }
    public string? RelatedEntityType { get; set; }
    public string RecipientId { get; set; } = null!;
    public ApplicationUser Recipient { get; set; } = null!;
}
