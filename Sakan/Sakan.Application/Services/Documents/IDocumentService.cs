using Microsoft.AspNetCore.Http;
using Sakan.Application.Common;
using Sakan.Application.DTO;
using Sakan.Domain.Enums;

namespace Sakan.Application.Services.Documents;

public interface IDocumentService
{
    Task<ServiceResult<DocumentDto>> UploadAsync(
        Guid relatedEntityId,
        string relatedEntityType,
        DocumentType documentType,
        string uploadedById,
        IFormFile file);

    Task<IEnumerable<DocumentDto>> GetForEntityAsync(Guid relatedEntityId, string relatedEntityType);
    Task<ServiceResult> VerifyAsync(Guid documentId);
}
