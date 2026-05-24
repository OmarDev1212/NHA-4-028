using Sakan.Application.Common;
using Sakan.Application.DTO;

namespace Sakan.Application.Services.OwnershipTransfers;

public interface IOwnershipTransferService
{
    Task<ServiceResult<OwnershipTransferDto>> InitiateAsync(CreateOwnershipTransferDto dto, string sellerId);
    Task<ServiceResult> SubmitDocumentsAsync(Guid transferId, string userId);
    Task<ServiceResult> LegalReviewAsync(Guid transferId, LegalReviewDto dto, string reviewerId);
    Task<ServiceResult> CreateEscrowPaymentAsync(Guid transferId, string buyerId);
    Task<ServiceResult> MarkSignedAsync(Guid transferId, string userId);
    Task<ServiceResult> MarkRegisteredAsync(Guid transferId, string reviewerId);
    Task<ServiceResult> CompleteAsync(Guid transferId, string reviewerId);
    Task<ServiceResult> CancelAsync(Guid transferId, string userId, string reason);
    Task<IEnumerable<OwnershipTransferDto>> GetForUserAsync(string userId);
    Task<OwnershipTransferDto?> GetByIdAsync(Guid id);
}
