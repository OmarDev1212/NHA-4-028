using Sakan.Application.Common;
using Sakan.Application.DTO;

namespace Sakan.Application.Services.RentalApplications;

public interface IRentalApplicationService
{
    Task<ServiceResult<RentalApplicationDto>> SubmitAsync(CreateRentalApplicationDto dto, string applicantId);
    Task<ServiceResult<RentalApplicationDto>> ApproveAsync(Guid applicationId, string landlordId);
    Task<ServiceResult> RejectAsync(Guid applicationId, string landlordId);
    Task<IEnumerable<RentalApplicationDto>> GetForListingAsync(Guid listingId, string landlordId);
    Task<IEnumerable<RentalApplicationDto>> GetForApplicantAsync(string applicantId);
    Task<RentalApplicationDto?> GetByIdAsync(Guid id);
    Task<ServiceResult> RecordRentPaymentAsync(Guid paymentId, string payerId);
}
