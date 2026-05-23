using System;
using System.Collections.Generic;

namespace Sakan.Domain.Entities;

public class PropertyFeature : BaseEntity<Guid>
{
    public string Feature { get; set; }
    public Guid PropertyId { get; set; }

    public Property Property { get; set; }
}