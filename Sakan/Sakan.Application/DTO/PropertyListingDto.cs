using Sakan.Domain.Entities;
using Sakan.Domain.Enums;

namespace Sakan.Application.DTO;

public class PropertyListingDto
{
    public Guid Id { get; set; }
    public Guid PropertyId { get; set; }
    public string PropertyTitle { get; set; } = default!;
    public string City { get; set; } = default!;
    public string Country { get; set; } = default!;
    public string ListingType { get; set; } = default!;
    public decimal Price { get; set; }
    public string Currency { get; set; } = "USD";
    public DateOnly AvailableFrom { get; set; }
    public DateOnly? AvailableTo { get; set; }
    public string Status { get; set; } = default!;
    public bool IsFeatured { get; set; }
    public int ViewCount { get; set; }
    public DateTimeOffset? PublishedAt { get; set; }
    public string? ListedById { get; set; }
    public string? PrimaryPhotoUrl { get; set; }
    public decimal AreaSqm { get; set; }
    public int? Bedrooms { get; set; }
    public int? Bathrooms { get; set; }

    public static PropertyListingDto FromEntities(
        PropertyListing listing,
        Property property,
        string? primaryPhotoUrl)
    {
        return new PropertyListingDto
        {
            Id = listing.Id,
            PropertyId = property.Id,
            PropertyTitle = property.Title,
            City = property.Address.City,
            Country = property.Address.Country,
            ListingType = listing.ListingType.ToString(),
            Price = listing.Price,
            Currency = listing.Currency,
            AvailableFrom = listing.AvailableFrom,
            AvailableTo = listing.AvailableTo,
            Status = listing.Status.ToString(),
            IsFeatured = listing.IsFeatured,
            ViewCount = listing.ViewCount,
            PublishedAt = listing.PublishedAt,
            ListedById = listing.ListedById,
            PrimaryPhotoUrl = primaryPhotoUrl,
            AreaSqm = property.AreaSqm,
            Bedrooms = property.Bedrooms,
            Bathrooms = property.Bathrooms
        };
    }
}

public class CreatePropertyListingDto
{
    public Guid PropertyId { get; set; }
    public ListingType ListingType { get; set; }
    public decimal Price { get; set; }
    public string Currency { get; set; } = "USD";
    public DateOnly AvailableFrom { get; set; }
    public DateOnly? AvailableTo { get; set; }
    public bool PublishNow { get; set; }
}
