using Sakan.Domain.Entities;
using Sakan.Domain.Enums;

namespace Sakan.Application.DTO;

public class RentalApplicationDto
{
    public Guid Id { get; set; }
    public Guid ListingId { get; set; }
    public string PropertyTitle { get; set; } = default!;
    public string ApplicantId { get; set; } = default!;
    public DateOnly RequestedFrom { get; set; }
    public DateOnly RequestedTo { get; set; }
    public decimal MonthlyBudget { get; set; }
    public string? Message { get; set; }
    public string Status { get; set; } = default!;
    public DateTimeOffset? ReviewedAt { get; set; }
    public Guid? ContractDocumentId { get; set; }

    public static RentalApplicationDto FromEntity(RentalApplication application, string propertyTitle)
    {
        return new RentalApplicationDto
        {
            Id = application.Id,
            ListingId = application.ListingId,
            PropertyTitle = propertyTitle,
            ApplicantId = application.ApplicantId,
            RequestedFrom = application.RequestedFrom,
            RequestedTo = application.RequestedTo,
            MonthlyBudget = application.MonthlyBudget,
            Message = application.Message,
            Status = application.Status.ToString(),
            ReviewedAt = application.ReviewedAt,
            ContractDocumentId = application.ContractDocumentId
        };
    }
}

public class CreateRentalApplicationDto
{
    public Guid ListingId { get; set; }
    public DateOnly RequestedFrom { get; set; }
    public DateOnly RequestedTo { get; set; }
    public decimal MonthlyBudget { get; set; }
    public string? Message { get; set; }
}
