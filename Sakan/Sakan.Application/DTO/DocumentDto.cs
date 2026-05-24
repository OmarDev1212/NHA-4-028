using Sakan.Domain.Entities;

namespace Sakan.Application.DTO;

public class DocumentDto
{
    public Guid Id { get; set; }
    public string DocumentType { get; set; } = default!;
    public string FileName { get; set; } = default!;
    public string StorageUrl { get; set; } = default!;
    public bool IsVerified { get; set; }
    public DateTimeOffset UploadedAt { get; set; }
    public Guid RelatedEntityId { get; set; }
    public string RelatedEntityType { get; set; } = default!;

    public static DocumentDto FromEntity(Document document) => new()
    {
        Id = document.Id,
        DocumentType = document.DocumentType.ToString(),
        FileName = document.FileName,
        StorageUrl = document.StorageUrl,
        IsVerified = document.IsVerified,
        UploadedAt = document.UploadedAt,
        RelatedEntityId = document.RelatedEntityId,
        RelatedEntityType = document.RelatedEntityType
    };
}
