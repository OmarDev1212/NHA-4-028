using Sakan.Domain.Entities;
using Sakan.Domain.Enums;

namespace Sakan.Application.DTO;

public class ViewingAppointmentDto
{
    public Guid Id { get; set; }
    public Guid ListingId { get; set; }
    public string PropertyTitle { get; set; } = default!;
    public DateTimeOffset ScheduledAt { get; set; }
    public int DurationMinutes { get; set; }
    public string Status { get; set; } = default!;
    public string MeetingType { get; set; } = default!;
    public string? VirtualMeetingUrl { get; set; }
    public string? Notes { get; set; }
    public string RequestedById { get; set; } = default!;
    public string? AgentId { get; set; }

    public static ViewingAppointmentDto FromEntity(
        ViewingAppoinment appointment,
        string propertyTitle)
    {
        return new ViewingAppointmentDto
        {
            Id = appointment.Id,
            ListingId = appointment.ListingId,
            PropertyTitle = propertyTitle,
            ScheduledAt = appointment.ScheduledAt,
            DurationMinutes = appointment.DurationMinutes,
            Status = appointment.Status.ToString(),
            MeetingType = appointment.MeetingType.ToString(),
            VirtualMeetingUrl = appointment.VirtualMeetingUrl,
            Notes = appointment.Notes,
            RequestedById = appointment.RequestedById,
            AgentId = appointment.AgentId
        };
    }
}

public class CreateViewingAppointmentDto
{
    public Guid ListingId { get; set; }
    public DateTimeOffset ScheduledAt { get; set; }
    public MeetingType MeetingType { get; set; } = MeetingType.Virtual;
    public string? Notes { get; set; }
}

public class ConfirmViewingAppointmentDto
{
    public string AgentId { get; set; } = default!;
    public string? VirtualMeetingUrl { get; set; }
}
