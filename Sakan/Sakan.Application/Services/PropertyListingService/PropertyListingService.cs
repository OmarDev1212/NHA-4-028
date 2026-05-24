using Sakan.Application.Common;
using Sakan.Application.DTO;
using Sakan.Domain.Contracts;
using Sakan.Domain.Entities;
using Sakan.Domain.Enums;

namespace Sakan.Application.Services.PropertyListingService;

public class ListingService(IUnitOfWork unitOfWork) : IPropertyListingService
{
    public async Task<IEnumerable<PropertyListingDto>> SearchAsync(ListingSearchCriteria criteria)
    {
        var listingRepo = unitOfWork.GetRepository<PropertyListing, Guid>();
        var listings = (await listingRepo.GetAllAsync(l => !l.IsDeleted)).ToList();

        if (criteria.Status.HasValue)
            listings = listings.Where(l => l.Status == criteria.Status.Value).ToList();
        if (criteria.ListingType.HasValue)
            listings = listings.Where(l => l.ListingType == criteria.ListingType.Value).ToList();
        if (criteria.MinPrice.HasValue)
            listings = listings.Where(l => l.Price >= criteria.MinPrice.Value).ToList();
        if (criteria.MaxPrice.HasValue)
            listings = listings.Where(l => l.Price <= criteria.MaxPrice.Value).ToList();
        if (criteria.AvailableFrom.HasValue)
            listings = listings.Where(l => l.AvailableTo == null || l.AvailableTo >= criteria.AvailableFrom.Value).ToList();
        if (criteria.AvailableTo.HasValue)
            listings = listings.Where(l => l.AvailableFrom <= criteria.AvailableTo.Value).ToList();

        if (listings.Count == 0)
            return [];

        var propertyIds = listings.Select(l => l.PropertyId).Distinct().ToList();
        var properties = (await unitOfWork.GetRepository<Property, Guid>()
            .GetAllAsync(p => propertyIds.Contains(p.Id) && !p.IsDeleted))
            .ToDictionary(p => p.Id);

        if (!string.IsNullOrWhiteSpace(criteria.City))
        {
            listings = listings
                .Where(l => properties.TryGetValue(l.PropertyId, out var p)
                    && p.Address.City.Contains(criteria.City, StringComparison.OrdinalIgnoreCase))
                .ToList();
        }

        if (!string.IsNullOrWhiteSpace(criteria.Country))
        {
            listings = listings
                .Where(l => properties.TryGetValue(l.PropertyId, out var p)
                    && p.Address.Country.Contains(criteria.Country, StringComparison.OrdinalIgnoreCase))
                .ToList();
        }

        var photos = (await unitOfWork.GetRepository<PropertyPhoto, Guid>()
            .GetAllAsync(p => propertyIds.Contains(p.PropertyId)))
            .GroupBy(p => p.PropertyId)
            .ToDictionary(g => g.Key, g => g.OrderBy(p => p.SortOrder).FirstOrDefault()?.PhotoUrl);

        return listings
            .Where(l => properties.ContainsKey(l.PropertyId))
            .OrderByDescending(l => l.IsFeatured)
            .ThenByDescending(l => l.PublishedAt)
            .Select(l => PropertyListingDto.FromEntities(
                l,
                properties[l.PropertyId],
                photos.GetValueOrDefault(l.PropertyId)));
    }

    public async Task<PropertyListingDto?> GetByIdAsync(Guid id)
    {
        var listing = await unitOfWork.GetRepository<PropertyListing, Guid>().GetByIdAsync(id);
        if (listing is null || listing.IsDeleted)
            return null;

        var property = await unitOfWork.GetRepository<Property, Guid>().GetByIdAsync(listing.PropertyId);
        if (property is null || property.IsDeleted)
            return null;

        var photos = await unitOfWork.GetRepository<PropertyPhoto, Guid>()
            .GetAllAsync(p => p.PropertyId == property.Id);
        var primaryPhoto = photos.OrderBy(p => p.SortOrder).FirstOrDefault()?.PhotoUrl;

        return PropertyListingDto.FromEntities(listing, property, primaryPhoto);
    }

    public async Task<ServiceResult<PropertyListingDto>> CreateAsync(CreatePropertyListingDto dto, string listedById)
    {
        var property = await unitOfWork.GetRepository<Property, Guid>().GetByIdAsync(dto.PropertyId);
        if (property is null || property.IsDeleted)
            return ServiceResult<PropertyListingDto>.Fail("Property not found.");

        var existingListings = await unitOfWork.GetRepository<PropertyListing, Guid>()
            .GetAllAsync(l => l.PropertyId == dto.PropertyId && !l.IsDeleted
                && l.Status != ListingStatus.Withdrawn
                && l.Status != ListingStatus.Sold
                && l.Status != ListingStatus.Rented);

        if (existingListings.Any())
            return ServiceResult<PropertyListingDto>.Fail("Property already has an active listing.");

        var listing = new PropertyListing
        {
            Id = Guid.NewGuid(),
            PropertyId = dto.PropertyId,
            ListedById = listedById,
            ListingType = dto.ListingType,
            Price = dto.Price,
            Currency = dto.Currency,
            AvailableFrom = dto.AvailableFrom,
            AvailableTo = dto.AvailableTo,
            Status = dto.PublishNow ? ListingStatus.Active : ListingStatus.Draft,
            PublishedAt = dto.PublishNow ? DateTimeOffset.UtcNow : null,
            CreatedAt = DateTimeOffset.UtcNow,
            IsDeleted = false
        };

        await unitOfWork.GetRepository<PropertyListing, Guid>().AddAsync(listing);
        await unitOfWork.SaveChangesAsync();

        var result = await GetByIdAsync(listing.Id);
        return result is null
            ? ServiceResult<PropertyListingDto>.Fail("Listing created but could not be loaded.")
            : ServiceResult<PropertyListingDto>.Ok(result);
    }

    public async Task<ServiceResult> PublishAsync(Guid listingId, string userId)
    {
        var repo = unitOfWork.GetRepository<PropertyListing, Guid>();
        var listing = await repo.GetByIdAsync(listingId);
        if (listing is null || listing.IsDeleted)
            return ServiceResult.Fail("Listing not found.");
        if (listing.ListedById != userId)
            return ServiceResult.Fail("You are not allowed to publish this listing.");

        listing.Status = ListingStatus.Active;
        listing.PublishedAt = DateTimeOffset.UtcNow;
        repo.Update(listing);
        await unitOfWork.SaveChangesAsync();
        return ServiceResult.Ok();
    }

    public async Task<ServiceResult> WithdrawAsync(Guid listingId, string userId)
    {
        var repo = unitOfWork.GetRepository<PropertyListing, Guid>();
        var listing = await repo.GetByIdAsync(listingId);
        if (listing is null || listing.IsDeleted)
            return ServiceResult.Fail("Listing not found.");
        if (listing.ListedById != userId)
            return ServiceResult.Fail("You are not allowed to withdraw this listing.");

        listing.Status = ListingStatus.Withdrawn;
        repo.Update(listing);
        await unitOfWork.SaveChangesAsync();
        return ServiceResult.Ok();
    }

    public async Task IncrementViewCountAsync(Guid listingId)
    {
        var repo = unitOfWork.GetRepository<PropertyListing, Guid>();
        var listing = await repo.GetByIdAsync(listingId);
        if (listing is null || listing.IsDeleted)
            return;

        listing.ViewCount++;
        repo.Update(listing);
        await unitOfWork.SaveChangesAsync();
    }
}
