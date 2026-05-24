using Sakan.Application.Common;
using Sakan.Application.DTO;

namespace Sakan.Application.Services.ViewingAppointments;

public interface IViewingAppointmentService
{
    Task<ServiceResult<ViewingAppointmentDto>> RequestAsync(CreateViewingAppointmentDto dto, string userId);
    Task<ServiceResult<ViewingAppointmentDto>> ConfirmAsync(Guid appointmentId, ConfirmViewingAppointmentDto dto);
    Task<ServiceResult> CompleteAsync(Guid appointmentId, string agentId);
    Task<ServiceResult> CancelAsync(Guid appointmentId, string userId);
    Task<IEnumerable<ViewingAppointmentDto>> GetForUserAsync(string userId, bool asAgent = false);
    Task<ViewingAppointmentDto?> GetByIdAsync(Guid id);
}
