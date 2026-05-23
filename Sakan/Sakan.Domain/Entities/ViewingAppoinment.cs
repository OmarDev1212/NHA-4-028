using Sakan.Domain.Enums;

namespace Sakan.Domain.Entities
{
    public class ViewingAppoinment:BaseEntity<Guid>
    {
        public DateTimeOffset ScheduledAt { get; set; }
        public int DurationMinutes { get; set; } = 30;
        public AppointmentStatus Status { get; set; } = AppointmentStatus.Pending;
        public MeetingType MeetingType { get; set; }
        public string? VirtualMeetingUrl { get; set; }
        public string? Notes { get; set; }
        public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;


        // Navigation
        public PropertyListing Listing { get; set; } = null!;
        public ApplicationUser RequestedBy { get; set; } = null!;
        public ApplicationUser? Agent { get; set; }
        public Guid ListingId { get; set; }
        public string RequestedById { get; set; }
        public string? AgentId { get; set; }
    }
}
