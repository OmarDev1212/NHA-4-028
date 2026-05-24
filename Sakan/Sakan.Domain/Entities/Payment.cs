using Sakan.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sakan.Domain.Entities
{
    public class Payment:BaseEntity<Guid>
    {
        public decimal Amount { get; set; }
        public string Currency { get; set; } = "USD";
        public PaymentType PaymentType { get; set; }
        public PaymentStatus Status { get; set; }
        public Guid ReferenceId { get; set; }
        public string ReferenceType { get; set; } = null!;
        public string? GatewayTransactionId { get; set; }
        public DateTimeOffset? ProcessedAt { get; set; }
        public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

        // Navigation
        public string PayerId { get; set; } = null!;
        public string PayeeId { get; set; } = null!;
        public ApplicationUser Payer { get; set; } = null!;
        public ApplicationUser Payee { get; set; } = null!;
        public Guid? RentalApplicationId { get; set; }
        public RentalApplication? RentalApplication { get; set; }
    }
}
