using Sakan.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sakan.Domain.Entities
{
    public class MaintanceRequest : BaseEntity<Guid>
    {
        public MaintenanceCategory Category { get; set; }
        public string Title { get; set; } = null!;
        public string Description { get; set; } = null!;
        public Priority Priority { get; set; }
        public RequestStatus Status { get; set; } = RequestStatus.Open;
        public decimal? EstimatedCostUSD { get; set; }
        public decimal? ActualCostUSD { get; set; }
        public DateTimeOffset RequestedAt { get; set; } = DateTimeOffset.UtcNow;
        public DateTimeOffset? ResolvedAt { get; set; }


        // Navigation
        public Property Property { get; set; } = null!;
        public Guid PropertyId { get; set; }


        public ApplicationUser RequestedBy { get; set; } = null!;
        public string RequestedById { get; set; }
        public ApplicationUser? AssignedTo
        {
            get; set;
        }
        public string? AssignedToId { get; set; }
    }
}
