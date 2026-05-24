using Sakan.Domain.Entities;

namespace Sakan.Application.DTO;

public class OwnershipTransferDto
{
    public Guid Id { get; set; }
    public Guid PropertyId { get; set; }
    public string PropertyTitle { get; set; } = default!;
    public string FromOwnerId { get; set; } = default!;
    public string ToOwnerId { get; set; } = default!;
    public string? AgentId { get; set; }
    public decimal AgreedPrice { get; set; }
    public string Status { get; set; } = default!;
    public DateTimeOffset InitiatedAt { get; set; }
    public DateTimeOffset? CompletedAt { get; set; }
    public Guid? EscrowPaymentId { get; set; }
    public string? NotaryNotes { get; set; }

    public static OwnershipTransferDto FromEntity(OwnershipTransfer transfer, string propertyTitle)
    {
        return new OwnershipTransferDto
        {
            Id = transfer.Id,
            PropertyId = transfer.PropertyId,
            PropertyTitle = propertyTitle,
            FromOwnerId = transfer.FromOwnerId,
            ToOwnerId = transfer.ToOwnerId,
            AgentId = transfer.AgentId,
            AgreedPrice = transfer.AgreedPrice,
            Status = transfer.Status,
            InitiatedAt = transfer.InitiatedAt,
            CompletedAt = transfer.CompletedAt,
            EscrowPaymentId = transfer.EscrowPaymentId,
            NotaryNotes = transfer.NotaryNotes
        };
    }
}

public class CreateOwnershipTransferDto
{
    public Guid PropertyId { get; set; }
    public string ToOwnerId { get; set; } = default!;
    public decimal AgreedPrice { get; set; }
    public string? AgentId { get; set; }
}

public class LegalReviewDto
{
    public string NotaryNotes { get; set; } = default!;
    public bool Approved { get; set; }
}
