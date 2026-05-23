using Sakan.Domain.Entities;
using System;
using System.Collections.Generic;

namespace Sakan.Models;

public  class Notification:BaseEntity<Guid>
{
    public string Channel { get; set; }

    public string Title { get; set; }

    public string Body { get; set; }

    public bool IsRead { get; set; }

    public DateTimeOffset CreatedAt { get; set; }
    public Guid? RelatedEntityId { get; set; }

    public string RelatedEntityType { get; set; }


    public string RecipientId { get; set; }
    public virtual ApplicationUser Recipient { get; set; }
}