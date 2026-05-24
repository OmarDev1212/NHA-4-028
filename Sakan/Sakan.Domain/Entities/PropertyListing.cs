using Sakan.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sakan.Domain.Entities
{
    public class PropertyListing:BaseEntity<Guid>
    {
        public ListingType ListingType { get; set; }
        public decimal Price { get; set; }
        public string Currency { get; set; } = default!;    
        public DateOnly AvailableFrom { get; set; }
        public DateOnly? AvailableTo { get; set; }
        public ListingStatus Status { get; set; } = ListingStatus.Draft;
        public bool IsFeatured { get; set; }
        public int ViewCount { get; set; }
        public DateTimeOffset? PublishedAt { get; set; }
        public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;


        public Property  Property { get; set; } 
        public Guid PropertyId { get; set; }
        public ApplicationUser ListedBy { get; set; }
        public string ListedById { get; set; }
    }
}
