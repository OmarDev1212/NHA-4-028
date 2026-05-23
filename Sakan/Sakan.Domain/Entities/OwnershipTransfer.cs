using System;
using System.Collections.Generic;

namespace Sakan.Domain.Entities;

public  class OwnershipTransfer:BaseEntity<Guid>
{


    public string FromOwnerId { get; set; }

    public string ToOwnerId { get; set; }

    public string? AgentId { get; set; }

    public decimal AgreedPrice { get; set; }

    public string Status { get; set; }

    public DateTimeOffset InitiatedAt { get; set; }

    public DateTimeOffset? CompletedAt { get; set; }

    public Guid? EscrowPaymentId { get; set; }

    public string NotaryNotes { get; set; }

    public string CancellationReason { get; set; }

    public virtual ApplicationUser Agent { get; set; }

    public virtual Payment EscrowPayment { get; set; }

    public virtual ApplicationUser FromOwner { get; set; }

    public virtual Property Property { get; set; }
    public Guid PropertyId { get; set; }

    public virtual ApplicationUser ToOwner { get; set; }
}