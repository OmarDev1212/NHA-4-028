using Sakan.Domain.Enums;

namespace Sakan.Domain.Entities
{
    public class Document:BaseEntity<Guid>
    {
        public DocumentType DocumentType { get; set; }
        public string FileName { get; set; } = null!;
        public string StorageUrl { get; set; } = null!;
        public string FileHash { get; set; } = null!;
        public long FileSizeBytes { get; set; }
        public Guid RelatedEntityId { get; set; }
        public string RelatedEntityType { get; set; } = null!;
        public bool IsVerified { get; set; }
        public DateTimeOffset UploadedAt { get; set; } = DateTimeOffset.UtcNow;


        public ApplicationUser UploadedBy { get; set; } = null!;

        public string UploadedById { get; set; }
    }
}
