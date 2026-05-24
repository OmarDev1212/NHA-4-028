using Sakan.Application.Common;
using Sakan.Application.DTO;

namespace Sakan.Application.Services.PropertyListingService;

public interface IPropertyListingService
{
    Task<IEnumerable<PropertyListingDto>> SearchAsync(ListingSearchCriteria criteria);
    Task<PropertyListingDto?> GetByIdAsync(Guid id);
    Task<ServiceResult<PropertyListingDto>> CreateAsync(CreatePropertyListingDto dto, string listedById);
    Task<ServiceResult> PublishAsync(Guid listingId, string userId);
    Task<ServiceResult> WithdrawAsync(Guid listingId, string userId);
    Task IncrementViewCountAsync(Guid listingId);
}
