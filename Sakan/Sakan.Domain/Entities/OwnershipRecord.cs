using Sakan.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;

namespace Sakan.Domain.Entities
{
    public class OwnershipRecord:BaseEntity<Guid>
    {
        public DateTimeOffset AcquiredAt { get; set; }
        public DateTimeOffset? ReleasedAt { get; set; }
        public AcquisitionMethod AcquisitionMethod { get; set; }
        public decimal? PurchasePrice { get; set; }
        public bool IsActive { get; set; }
        



        public Property Property { get; set; } = null!;
        public Guid PropertyId { get; set; }
        public ApplicationUser Owner { get; set; } = null!;
        public string OwnerId { get; set; }
        public Document? DeedDocument { get; set; }
        public Guid? DeedDocumentId { get; set; }


    }
}
