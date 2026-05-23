using System;
using System.Collections.Generic;

namespace Sakan.Domain.Entities;

public partial class PropertyPhoto:BaseEntity<Guid>
{

    public Guid PropertyId { get; set; }

    public string PhotoUrl { get; set; }

    public int SortOrder { get; set; }

    public  Property Property { get; set; }
}