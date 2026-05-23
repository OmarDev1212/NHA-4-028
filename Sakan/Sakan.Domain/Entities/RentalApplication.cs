using Sakan.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;

namespace Sakan.Domain.Entities
{
    public class RentalApplication:BaseEntity<Guid>
    {
        public DateOnly RequestedFrom { get; set; }
        public DateOnly RequestedTo { get; set; }
        public decimal MonthlyBudget { get; set; }
        public string? Message { get; set; }
        public ApplicationStatus Status { get; set; } = ApplicationStatus.Submitted;
        public DateTimeOffset? ReviewedAt { get; set; }
        public Guid? ContractDocumentId { get; set; }

        // Navigation
        public PropertyListing Listing { get; set; } = null!;
        public Guid ListingId { get; set; }
        public ApplicationUser Applicant { get; set; } = null!;
        public string ApplicantId { get; set; }
        public Document? ContractDocument { get; set; }
        public ICollection<Payment> Payments { get; set; } = new List<Payment>();
    }
}
