using Sakan.Application.Common;
using Sakan.Application.DTO;
using Sakan.Application.Services.Notifications;
using Sakan.Domain.Constants;
using Sakan.Domain.Contracts;
using Sakan.Domain.Entities;
using Sakan.Domain.Enums;

namespace Sakan.Application.Services.ViewingAppointments;

public class ViewingAppointmentService(
    IUnitOfWork unitOfWork,
    INotificationService notificationService) : IViewingAppointmentService
{
    public async Task<ServiceResult<ViewingAppointmentDto>> RequestAsync(CreateViewingAppointmentDto dto, string userId)
    {
        var listing = await unitOfWork.GetRepository<PropertyListing, Guid>().GetByIdAsync(dto.ListingId);
        if (listing is null || listing.IsDeleted || listing.Status != ListingStatus.Active)
            return ServiceResult<ViewingAppointmentDto>.Fail("Listing is not available for viewing.");

        var appointment = new ViewingAppoinment
        {
            Id = Guid.NewGuid(),
            ListingId = dto.ListingId,
            RequestedById = userId,
            ScheduledAt = dto.ScheduledAt,
            MeetingType = dto.MeetingType,
            Notes = dto.Notes,
            Status = AppointmentStatus.Pending,
            CreatedAt = DateTimeOffset.UtcNow,
            IsDeleted = false
        };

        await unitOfWork.GetRepository<ViewingAppoinment, Guid>().AddAsync(appointment);
        await unitOfWork.SaveChangesAsync();

        await notificationService.NotifyAsync(
            listing.ListedById,
            "New viewing request",
            $"A traveler requested a {dto.MeetingType} viewing on {dto.ScheduledAt:u}.",
            appointment.Id,
            EntityReferenceTypes.ViewingAppointment);

        return ServiceResult<ViewingAppointmentDto>.Ok(
            (await GetByIdAsync(appointment.Id))!);
    }

    public async Task<ServiceResult<ViewingAppointmentDto>> ConfirmAsync(Guid appointmentId, ConfirmViewingAppointmentDto dto)
    {
        var repo = unitOfWork.GetRepository<ViewingAppoinment, Guid>();
        var appointment = await repo.GetByIdAsync(appointmentId);
        if (appointment is null || appointment.IsDeleted)
            return ServiceResult<ViewingAppointmentDto>.Fail("Appointment not found.");
        if (appointment.Status != AppointmentStatus.Pending)
            return ServiceResult<ViewingAppointmentDto>.Fail("Only pending appointments can be confirmed.");

        appointment.Status = AppointmentStatus.Confirmed;
        appointment.AgentId = dto.AgentId;
        appointment.VirtualMeetingUrl = dto.VirtualMeetingUrl
            ?? $"https://meet.sakan.local/{appointment.Id:N}";

        repo.Update(appointment);
        await unitOfWork.SaveChangesAsync();

        await notificationService.NotifyAsync(
            appointment.RequestedById,
            "Viewing confirmed",
            $"Your viewing is confirmed. Join: {appointment.VirtualMeetingUrl}",
            appointment.Id,
            EntityReferenceTypes.ViewingAppointment);

        return ServiceResult<ViewingAppointmentDto>.Ok((await GetByIdAsync(appointmentId))!);
    }

    public async Task<ServiceResult> CompleteAsync(Guid appointmentId, string agentId)
    {
        var repo = unitOfWork.GetRepository<ViewingAppoinment, Guid>();
        var appointment = await repo.GetByIdAsync(appointmentId);
        if (appointment is null || appointment.IsDeleted)
            return ServiceResult.Fail("Appointment not found.");
        if (appointment.AgentId != agentId)
            return ServiceResult.Fail("Only the assigned agent can complete this appointment.");

        appointment.Status = AppointmentStatus.Completed;
        repo.Update(appointment);
        await unitOfWork.SaveChangesAsync();
        return ServiceResult.Ok();
    }

    public async Task<ServiceResult> CancelAsync(Guid appointmentId, string userId)
    {
        var repo = unitOfWork.GetRepository<ViewingAppoinment, Guid>();
        var appointment = await repo.GetByIdAsync(appointmentId);
        if (appointment is null || appointment.IsDeleted)
            return ServiceResult.Fail("Appointment not found.");
        if (appointment.RequestedById != userId && appointment.AgentId != userId)
            return ServiceResult.Fail("You are not allowed to cancel this appointment.");

        appointment.Status = AppointmentStatus.Cancelled;
        repo.Update(appointment);
        await unitOfWork.SaveChangesAsync();
        return ServiceResult.Ok();
    }

    public async Task<IEnumerable<ViewingAppointmentDto>> GetForUserAsync(string userId, bool asAgent = false)
    {
        var repo = unitOfWork.GetRepository<ViewingAppoinment, Guid>();
        var appointments = await repo.GetAllAsync(a => !a.IsDeleted
            && (asAgent ? a.AgentId == userId : a.RequestedById == userId));

        return await MapListAsync(appointments);
    }

    public async Task<ViewingAppointmentDto?> GetByIdAsync(Guid id)
    {
        var appointment = await unitOfWork.GetRepository<ViewingAppoinment, Guid>().GetByIdAsync(id);
        if (appointment is null || appointment.IsDeleted)
            return null;

        var title = await GetPropertyTitleForListingAsync(appointment.ListingId);
        return ViewingAppointmentDto.FromEntity(appointment, title);
    }

    private async Task<IEnumerable<ViewingAppointmentDto>> MapListAsync(IEnumerable<ViewingAppoinment> appointments)
    {
        var list = appointments.ToList();
        if (list.Count == 0)
            return [];

        var listingIds = list.Select(a => a.ListingId).Distinct().ToList();
        var listings = (await unitOfWork.GetRepository<PropertyListing, Guid>()
            .GetAllAsync(l => listingIds.Contains(l.Id)))
            .ToDictionary(l => l.Id);

        var propertyIds = listings.Values.Select(l => l.PropertyId).Distinct().ToList();
        var properties = (await unitOfWork.GetRepository<Property, Guid>()
            .GetAllAsync(p => propertyIds.Contains(p.Id)))
            .ToDictionary(p => p.Id);

        return list.Select(a =>
        {
            var title = "Property";
            if (listings.TryGetValue(a.ListingId, out var listing)
                && properties.TryGetValue(listing.PropertyId, out var property))
                title = property.Title;
            return ViewingAppointmentDto.FromEntity(a, title);
        });
    }

    private async Task<string> GetPropertyTitleForListingAsync(Guid listingId)
    {
        var listing = await unitOfWork.GetRepository<PropertyListing, Guid>().GetByIdAsync(listingId);
        if (listing is null)
            return "Property";

        var property = await unitOfWork.GetRepository<Property, Guid>().GetByIdAsync(listing.PropertyId);
        return property?.Title ?? "Property";
    }
}
