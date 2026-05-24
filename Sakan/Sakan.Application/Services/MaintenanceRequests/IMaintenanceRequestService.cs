using Sakan.Application.Common;
using Sakan.Application.DTO;

namespace Sakan.Application.Services.MaintenanceRequests;

public interface IMaintenanceRequestService
{
    Task<ServiceResult<MaintenanceRequestDto>> CreateAsync(CreateMaintenanceRequestDto dto, string userId);
    Task<ServiceResult<MaintenanceRequestDto>> AssignAsync(Guid requestId, AssignMaintenanceRequestDto dto, string landlordId);
    Task<ServiceResult> ResolveAsync(Guid requestId, decimal? actualCost, string userId);
    Task<IEnumerable<MaintenanceRequestDto>> GetForPropertyAsync(Guid propertyId);
    Task<IEnumerable<MaintenanceRequestDto>> GetForUserAsync(string userId);
    Task<MaintenanceRequestDto?> GetByIdAsync(Guid id);
}
