using Sakan.Domain.Entities;
using Sakan.Domain.Enums;

namespace Sakan.Application.DTO;

public class MaintenanceRequestDto
{
    public Guid Id { get; set; }
    public Guid PropertyId { get; set; }
    public string PropertyTitle { get; set; } = default!;
    public string Category { get; set; } = default!;
    public string Title { get; set; } = default!;
    public string Description { get; set; } = default!;
    public string Priority { get; set; } = default!;
    public string Status { get; set; } = default!;
    public decimal? EstimatedCostUSD { get; set; }
    public decimal? ActualCostUSD { get; set; }
    public string RequestedById { get; set; } = default!;
    public string? AssignedToId { get; set; }
    public DateTimeOffset RequestedAt { get; set; }
    public DateTimeOffset? ResolvedAt { get; set; }

    public static MaintenanceRequestDto FromEntity(MaintanceRequest request, string propertyTitle)
    {
        return new MaintenanceRequestDto
        {
            Id = request.Id,
            PropertyId = request.PropertyId,
            PropertyTitle = propertyTitle,
            Category = request.Category.ToString(),
            Title = request.Title,
            Description = request.Description,
            Priority = request.Priority.ToString(),
            Status = request.Status.ToString(),
            EstimatedCostUSD = request.EstimatedCostUSD,
            ActualCostUSD = request.ActualCostUSD,
            RequestedById = request.RequestedById,
            AssignedToId = request.AssignedToId,
            RequestedAt = request.RequestedAt,
            ResolvedAt = request.ResolvedAt
        };
    }
}

public class CreateMaintenanceRequestDto
{
    public Guid PropertyId { get; set; }
    public MaintenanceCategory Category { get; set; }
    public string Title { get; set; } = default!;
    public string Description { get; set; } = default!;
    public Priority Priority { get; set; } = Priority.Medium;
}

public class AssignMaintenanceRequestDto
{
    public string AssignedToId { get; set; } = default!;
    public decimal? EstimatedCostUSD { get; set; }
}
