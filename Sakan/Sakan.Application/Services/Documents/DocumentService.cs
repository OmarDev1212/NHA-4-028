using System.Security.Cryptography;
using Microsoft.AspNetCore.Http;
using Sakan.Application.Common;
using Sakan.Application.DTO;
using Sakan.Domain.Contracts;
using Sakan.Domain.Entities;
using Sakan.Domain.Enums;

namespace Sakan.Application.Services.Documents;

public class DocumentService(IUnitOfWork unitOfWork) : IDocumentService
{
    private static readonly string[] AllowedExtensions = [".pdf", ".png", ".jpg", ".jpeg"];
    private const int MaxSize = 10 * 1024 * 1024;
    private const string FolderName = "Documents";

    public async Task<ServiceResult<DocumentDto>> UploadAsync(
        Guid relatedEntityId,
        string relatedEntityType,
        DocumentType documentType,
        string uploadedById,
        IFormFile file)
    {
        if (file is null || file.Length == 0)
            return ServiceResult<DocumentDto>.Fail("File is required.");

        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!AllowedExtensions.Contains(extension))
            return ServiceResult<DocumentDto>.Fail("Invalid file type. Allowed: PDF, PNG, JPG.");

        if (file.Length > MaxSize)
            return ServiceResult<DocumentDto>.Fail("File exceeds 10 MB limit.");

        var folderPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Uploads", FolderName);
        Directory.CreateDirectory(folderPath);

        var storedFileName = $"{Guid.NewGuid()}_{file.FileName}";
        var filePath = Path.Combine(folderPath, storedFileName);

        await using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await file.CopyToAsync(stream);
        }

        string hash;
        await using (var readStream = File.OpenRead(filePath))
        {
            var hashBytes = await SHA256.HashDataAsync(readStream);
            hash = Convert.ToHexString(hashBytes);
        }

        var document = new Document
        {
            Id = Guid.NewGuid(),
            DocumentType = documentType,
            FileName = file.FileName,
            StorageUrl = storedFileName,
            FileHash = hash,
            FileSizeBytes = file.Length,
            RelatedEntityId = relatedEntityId,
            RelatedEntityType = relatedEntityType,
            UploadedById = uploadedById,
            IsVerified = false,
            UploadedAt = DateTimeOffset.UtcNow,
            IsDeleted = false
        };

        await unitOfWork.GetRepository<Document, Guid>().AddAsync(document);
        await unitOfWork.SaveChangesAsync();

        return ServiceResult<DocumentDto>.Ok(DocumentDto.FromEntity(document));
    }

    public async Task<IEnumerable<DocumentDto>> GetForEntityAsync(Guid relatedEntityId, string relatedEntityType)
    {
        var documents = await unitOfWork.GetRepository<Document, Guid>()
            .GetAllAsync(d => !d.IsDeleted
                && d.RelatedEntityId == relatedEntityId
                && d.RelatedEntityType == relatedEntityType);
        return documents.OrderByDescending(d => d.UploadedAt).Select(DocumentDto.FromEntity);
    }

    public async Task<ServiceResult> VerifyAsync(Guid documentId)
    {
        var repo = unitOfWork.GetRepository<Document, Guid>();
        var document = await repo.GetByIdAsync(documentId);
        if (document is null || document.IsDeleted)
            return ServiceResult.Fail("Document not found.");

        document.IsVerified = true;
        repo.Update(document);
        await unitOfWork.SaveChangesAsync();
        return ServiceResult.Ok();
    }
}
