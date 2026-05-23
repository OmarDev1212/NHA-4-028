using Sakan.Domain.Enums;
using Sakan.Domain.ValueObjects;

namespace Sakan.Domain.Entities
{
    public class Property:BaseEntity<Guid>
    {
        public string Title { get; set; } = default!;

        public PropertyType PropertyType { get; set; }

        public Address Address { get; set; } = default!;


        public decimal AreaSqm { get; set; }

        public int? Bedrooms { get; set; }

        public int? Bathrooms { get; set; }

        public int? YearBuilt { get; set; }

        public LegalStatus LegalStatus { get; set; }

        public DateTimeOffset CreatedAt { get; set; }

        public DateTimeOffset UpdatedAt { get; set; }


        //relations

        public  ApplicationUser CurrentOwner { get; set; }
        public string CurrentOwnerId { get; set; }

        public  ICollection<MaintanceRequest> MaintenanceRequests { get; set; } = new List<MaintanceRequest>();

        public ICollection<OwnershipRecord> OwnershipRecords { get; set; } = new List<OwnershipRecord>();

        public  ICollection<OwnershipTransfer> OwnershipTransfers { get; set; } = new List<OwnershipTransfer>();

        public  ICollection<PropertyFeature> PropertyFeatures { get; set; } = new List<PropertyFeature>();

        public PropertyListing PropertyListing { get; set; }    

        public  ICollection<PropertyPhoto> PropertyPhotos { get; set; } = new List<PropertyPhoto>();
        public ICollection<Document> Documents { get; set; } = new List<Document>();

    }
}
