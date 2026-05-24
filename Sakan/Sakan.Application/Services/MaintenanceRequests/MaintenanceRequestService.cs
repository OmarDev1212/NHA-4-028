using Sakan.Application.Common;
using Sakan.Application.DTO;
using Sakan.Application.Services.Notifications;
using Sakan.Domain.Constants;
using Sakan.Domain.Contracts;
using Sakan.Domain.Entities;
using Sakan.Domain.Enums;

namespace Sakan.Application.Services.MaintenanceRequests;

public class MaintenanceRequestService(
    IUnitOfWork unitOfWork,
    INotificationService notificationService) : IMaintenanceRequestService
{
    public async Task<ServiceResult<MaintenanceRequestDto>> CreateAsync(CreateMaintenanceRequestDto dto, string userId)
    {
        var property = await unitOfWork.GetRepository<Property, Guid>().GetByIdAsync(dto.PropertyId);
        if (property is null || property.IsDeleted)
            return ServiceResult<MaintenanceRequestDto>.Fail("Property not found.");

        var request = new MaintanceRequest
        {
            Id = Guid.NewGuid(),
            PropertyId = dto.PropertyId,
            RequestedById = userId,
            Category = dto.Category,
            Title = dto.Title,
            Description = dto.Description,
            Priority = dto.Priority,
            Status = RequestStatus.Open,
            RequestedAt = DateTimeOffset.UtcNow,
            IsDeleted = false
        };

        await unitOfWork.GetRepository<MaintanceRequest, Guid>().AddAsync(request);
        await unitOfWork.SaveChangesAsync();

        await notificationService.NotifyAsync(
            property.CurrentOwnerId,
            "Maintenance request",
            $"{dto.Title} was reported for {property.Title}.",
            request.Id,
            EntityReferenceTypes.MaintenanceRequest);

        return ServiceResult<MaintenanceRequestDto>.Ok((await GetByIdAsync(request.Id))!);
    }

    public async Task<ServiceResult<MaintenanceRequestDto>> AssignAsync(
        Guid requestId,
        AssignMaintenanceRequestDto dto,
        string landlordId)
    {
        var repo = unitOfWork.GetRepository<MaintanceRequest, Guid>();
        var request = await repo.GetByIdAsync(requestId);
        if (request is null || request.IsDeleted)
            return ServiceResult<MaintenanceRequestDto>.Fail("Request not found.");

        var property = await unitOfWork.GetRepository<Property, Guid>().GetByIdAsync(request.PropertyId);
        if (property is null || property.CurrentOwnerId != landlordId)
            return ServiceResult<MaintenanceRequestDto>.Fail("Only the property owner can assign contractors.");

        request.AssignedToId = dto.AssignedToId;
        request.EstimatedCostUSD = dto.EstimatedCostUSD;
        request.Status = RequestStatus.InProgress;
        repo.Update(request);
        await unitOfWork.SaveChangesAsync();

        await notificationService.NotifyAsync(
            dto.AssignedToId,
            "Maintenance assignment",
            $"You were assigned: {request.Title}.",
            request.Id,
            EntityReferenceTypes.MaintenanceRequest);

        await notificationService.NotifyAsync(
            request.RequestedById,
            "Contractor assigned",
            $"A contractor was assigned to '{request.Title}'.",
            request.Id,
            EntityReferenceTypes.MaintenanceRequest);

        return ServiceResult<MaintenanceRequestDto>.Ok((await GetByIdAsync(requestId))!);
    }

    public async Task<ServiceResult> ResolveAsync(Guid requestId, decimal? actualCost, string userId)
    {
        var repo = unitOfWork.GetRepository<MaintanceRequest, Guid>();
        var request = await repo.GetByIdAsync(requestId);
        if (request is null || request.IsDeleted)
            return ServiceResult.Fail("Request not found.");

        var property = await unitOfWork.GetRepository<Property, Guid>().GetByIdAsync(request.PropertyId);
        if (property is null)
            return ServiceResult.Fail("Property not found.");

        if (request.AssignedToId != userId && property.CurrentOwnerId != userId)
            return ServiceResult.Fail("You are not allowed to resolve this request.");

        request.Status = RequestStatus.Resolved;
        request.ActualCostUSD = actualCost;
        request.ResolvedAt = DateTimeOffset.UtcNow;
        repo.Update(request);
        await unitOfWork.SaveChangesAsync();

        await notificationService.NotifyAsync(
            request.RequestedById,
            "Maintenance resolved",
            $"'{request.Title}' was marked resolved.",
            request.Id,
            EntityReferenceTypes.MaintenanceRequest);

        return ServiceResult.Ok();
    }

    public async Task<IEnumerable<MaintenanceRequestDto>> GetForPropertyAsync(Guid propertyId)
    {
        var requests = await unitOfWork.GetRepository<MaintanceRequest, Guid>()
            .GetAllAsync(r => r.PropertyId == propertyId && !r.IsDeleted);
        return await MapListAsync(requests);
    }

    public async Task<IEnumerable<MaintenanceRequestDto>> GetForUserAsync(string userId)
    {
        var requests = await unitOfWork.GetRepository<MaintanceRequest, Guid>()
            .GetAllAsync(r => !r.IsDeleted
                && (r.RequestedById == userId || r.AssignedToId == userId));
        return await MapListAsync(requests);
    }

    public async Task<MaintenanceRequestDto?> GetByIdAsync(Guid id)
    {
        var request = await unitOfWork.GetRepository<MaintanceRequest, Guid>().GetByIdAsync(id);
        if (request is null || request.IsDeleted)
            return null;

        var property = await unitOfWork.GetRepository<Property, Guid>().GetByIdAsync(request.PropertyId);
        return MaintenanceRequestDto.FromEntity(request, property?.Title ?? "Property");
    }

    private async Task<IEnumerable<MaintenanceRequestDto>> MapListAsync(IEnumerable<MaintanceRequest> requests)
    {
        var list = requests.ToList();
        var propertyIds = list.Select(r => r.PropertyId).Distinct().ToList();
        var properties = (await unitOfWork.GetRepository<Property, Guid>()
            .GetAllAsync(p => propertyIds.Contains(p.Id)))
            .ToDictionary(p => p.Id);

        return list.Select(r => MaintenanceRequestDto.FromEntity(
            r,
            properties.TryGetValue(r.PropertyId, out var p) ? p.Title : "Property"));
    }
}
