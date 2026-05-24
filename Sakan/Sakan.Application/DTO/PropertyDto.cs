using Microsoft.AspNetCore.Http;
using Sakan.Domain.Entities;
using Sakan.Domain.Enums;
using Sakan.Domain.ValueObjects;

namespace Sakan.Application.DTO;

public class PropertyDto
{
    public Guid? Id { get; set; }
    public string Title { get; set; } = default!;
    public string PropertyType { get; set; } = default!;
    public string Street { get; set; } = default!;
    public string City { get; set; } = default!;
    public string Country { get; set; } = default!;
    public string State { get; set; } = default!;
    public string PostalCode { get; set; } = default!;
    public decimal AreaSqm { get; set; }
    public int? Bedrooms { get; set; }
    public int? Bathrooms { get; set; }
    public int? YearBuilt { get; set; }
    public string LegalStatus { get; set; } = default!;
    public string? CurrentOwnerId { get; set; }
    public string? CurrentOwnerEmail { get; set; }
    public List<string>? ImagesNames { get; set; } = [];
    public List<IFormFile>? Images { get; set; } = [];
    public List<string> Features { get; set; } = [];

    public static PropertyDto FromEntity(
        Property entity,
        IEnumerable<PropertyPhoto>? photos = null,
        IEnumerable<PropertyFeature>? features = null)
    {
        return new PropertyDto
        {
            Id = entity.Id,
            Title = entity.Title,
            PropertyType = entity.PropertyType.ToString(),
            Street = entity.Address.Street,
            City = entity.Address.City,
            State = entity.Address.State,
            Country = entity.Address.Country,
            PostalCode = entity.Address.PostalCode,
            AreaSqm = entity.AreaSqm,
            Bedrooms = entity.Bedrooms,
            Bathrooms = entity.Bathrooms,
            YearBuilt = entity.YearBuilt,
            LegalStatus = entity.LegalStatus.ToString(),
            CurrentOwnerId = entity.CurrentOwnerId,
            ImagesNames = photos?.OrderBy(p => p.SortOrder).Select(p => p.PhotoUrl).ToList() ?? [],
            Features = features?.Select(f => f.Feature).ToList() ?? [],
            CurrentOwnerEmail = entity.CurrentOwner?.Email ?? entity.CurrentOwner?.UserName,
           
        };
    }

    public static Property ToEntity(PropertyDto dto, string currentOwnerId)
    {
        var now = DateTimeOffset.UtcNow;
        return new Property
        {
            Id = dto.Id ?? Guid.NewGuid(),
            Title = dto.Title,
            PropertyType = Enum.Parse<PropertyType>(dto.PropertyType, ignoreCase: true),
            Address = new Address
            {
                Street = dto.Street,
                City = dto.City,
                State = dto.State,
                Country = dto.Country,
                PostalCode = dto.PostalCode
            },
            AreaSqm = dto.AreaSqm,
            Bedrooms = dto.Bedrooms,
            Bathrooms = dto.Bathrooms,
            YearBuilt = dto.YearBuilt,
            LegalStatus = Enum.Parse<LegalStatus>(dto.LegalStatus, ignoreCase: true),
            CurrentOwnerId = currentOwnerId,
            CreatedAt = now,
            UpdatedAt = now,
            IsDeleted = false
        };
    }

    public static void MapOntoEntity(Property entity, PropertyDto dto)
    {
        entity.Title = dto.Title;
        entity.PropertyType = Enum.Parse<PropertyType>(dto.PropertyType, ignoreCase: true);
        entity.Address = new Address
        {
            Street = dto.Street,
            City = dto.City,
            State = dto.State,
            Country = dto.Country,
            PostalCode = dto.PostalCode
        };
        entity.AreaSqm = dto.AreaSqm;
        entity.Bedrooms = dto.Bedrooms;
        entity.Bathrooms = dto.Bathrooms;
        entity.YearBuilt = dto.YearBuilt;
        entity.LegalStatus = Enum.Parse<LegalStatus>(dto.LegalStatus, ignoreCase: true);
        if (!string.IsNullOrEmpty(dto.CurrentOwnerId))
            entity.CurrentOwnerId = dto.CurrentOwnerId;
        entity.UpdatedAt = DateTimeOffset.UtcNow;
    }
}
