using Sakan.Application.Common;
using Sakan.Application.DTO;
using Sakan.Application.Services.Notifications;
using Sakan.Domain.Constants;
using Sakan.Domain.Contracts;
using Sakan.Domain.Entities;
using Sakan.Domain.Enums;

namespace Sakan.Application.Services.OwnershipTransfers;

public class OwnershipTransferService(
    IUnitOfWork unitOfWork,
    INotificationService notificationService) : IOwnershipTransferService
{
    public async Task<ServiceResult<OwnershipTransferDto>> InitiateAsync(CreateOwnershipTransferDto dto, string sellerId)
    {
        var property = await unitOfWork.GetRepository<Property, Guid>().GetByIdAsync(dto.PropertyId);
        if (property is null || property.IsDeleted)
            return ServiceResult<OwnershipTransferDto>.Fail("Property not found.");
        if (property.CurrentOwnerId != sellerId)
            return ServiceResult<OwnershipTransferDto>.Fail("Only the current owner can initiate a transfer.");

        var transfer = new OwnershipTransfer
        {
            Id = Guid.NewGuid(),
            PropertyId = dto.PropertyId,
            FromOwnerId = sellerId,
            ToOwnerId = dto.ToOwnerId,
            AgentId = dto.AgentId,
            AgreedPrice = dto.AgreedPrice,
            Status = TransferStatus.Initiated.ToString(),
            InitiatedAt = DateTimeOffset.UtcNow,
            NotaryNotes = string.Empty,
            CancellationReason = string.Empty,
            IsDeleted = false
        };

        await unitOfWork.GetRepository<OwnershipTransfer, Guid>().AddAsync(transfer);
        await unitOfWork.SaveChangesAsync();

        await notificationService.NotifyAsync(
            dto.ToOwnerId,
            "Ownership transfer initiated",
            $"A transfer was initiated for {property.Title} at {dto.AgreedPrice:N2}.",
            transfer.Id,
            EntityReferenceTypes.OwnershipTransfer);

        return ServiceResult<OwnershipTransferDto>.Ok((await GetByIdAsync(transfer.Id))!);
    }

    public async Task<ServiceResult> SubmitDocumentsAsync(Guid transferId, string userId)
    {
        var transfer = await GetTransferAsync(transferId);
        if (transfer is null)
            return ServiceResult.Fail("Transfer not found.");
        if (transfer.FromOwnerId != userId && transfer.ToOwnerId != userId)
            return ServiceResult.Fail("You are not a party to this transfer.");

        if (!IsStatus(transfer, TransferStatus.Initiated, TransferStatus.DocumentsUploaded))
            return ServiceResult.Fail("Transfer is not accepting documents.");

        var docs = await unitOfWork.GetRepository<Document, Guid>()
            .GetAllAsync(d => !d.IsDeleted
                && d.RelatedEntityId == transferId
                && d.RelatedEntityType == EntityReferenceTypes.OwnershipTransfer);

        if (!docs.Any())
            return ServiceResult.Fail("Upload at least one document before submitting.");

        transfer.Status = TransferStatus.DocumentsUploaded.ToString();
        UpdateTransfer(transfer);
        await unitOfWork.SaveChangesAsync();

        await NotifyPartiesAsync(transfer, "Documents submitted", "Documents were submitted for legal review.");
        return ServiceResult.Ok();
    }

    public async Task<ServiceResult> LegalReviewAsync(Guid transferId, LegalReviewDto dto, string reviewerId)
    {
        var transfer = await GetTransferAsync(transferId);
        if (transfer is null)
            return ServiceResult.Fail("Transfer not found.");
        if (!IsStatus(transfer, TransferStatus.DocumentsUploaded, TransferStatus.LegalReview))
            return ServiceResult.Fail("Transfer is not ready for legal review.");

        if (!dto.Approved)
        {
            transfer.Status = TransferStatus.Cancelled.ToString();
            transfer.CancellationReason = dto.NotaryNotes;
            UpdateTransfer(transfer);
            await unitOfWork.SaveChangesAsync();
            await NotifyPartiesAsync(transfer, "Transfer rejected", dto.NotaryNotes);
            return ServiceResult.Ok();
        }

        transfer.Status = TransferStatus.LegalReview.ToString();
        transfer.NotaryNotes = dto.NotaryNotes;
        UpdateTransfer(transfer);
        await unitOfWork.SaveChangesAsync();

        await NotifyPartiesAsync(transfer, "Legal review passed", dto.NotaryNotes);
        return ServiceResult.Ok();
    }

    public async Task<ServiceResult> CreateEscrowPaymentAsync(Guid transferId, string buyerId)
    {
        var transfer = await GetTransferAsync(transferId);
        if (transfer is null)
            return ServiceResult.Fail("Transfer not found.");
        if (transfer.ToOwnerId != buyerId)
            return ServiceResult.Fail("Only the buyer can fund escrow.");
        if (!IsStatus(transfer, TransferStatus.LegalReview))
            return ServiceResult.Fail("Legal review must be completed first.");

        var payment = new Payment
        {
            Id = Guid.NewGuid(),
            Amount = transfer.AgreedPrice,
            Currency = "USD",
            PaymentType = PaymentType.Escrow,
            Status = PaymentStatus.Held,
            ReferenceId = transfer.Id,
            ReferenceType = EntityReferenceTypes.OwnershipTransfer,
            PayerId = buyerId,
            PayeeId = transfer.FromOwnerId,
            GatewayTransactionId = $"ESCROW-{Guid.NewGuid():N}"[..24],
            ProcessedAt = DateTimeOffset.UtcNow,
            CreatedAt = DateTimeOffset.UtcNow,
            IsDeleted = false
        };

        await unitOfWork.GetRepository<Payment, Guid>().AddAsync(payment);
        transfer.EscrowPaymentId = payment.Id;
        transfer.Status = TransferStatus.PaymentHeld.ToString();
        UpdateTransfer(transfer);
        await unitOfWork.SaveChangesAsync();

        await NotifyPartiesAsync(transfer, "Escrow funded", $"Escrow of {payment.Amount:N2} {payment.Currency} is held.");
        return ServiceResult.Ok();
    }

    public async Task<ServiceResult> MarkSignedAsync(Guid transferId, string userId)
    {
        var transfer = await GetTransferAsync(transferId);
        if (transfer is null)
            return ServiceResult.Fail("Transfer not found.");
        if (transfer.FromOwnerId != userId && transfer.ToOwnerId != userId)
            return ServiceResult.Fail("You are not a party to this transfer.");
        if (!IsStatus(transfer, TransferStatus.PaymentHeld))
            return ServiceResult.Fail("Escrow must be funded before signing.");

        var docs = await unitOfWork.GetRepository<Document, Guid>()
            .GetAllAsync(d => !d.IsDeleted
                && d.RelatedEntityId == transferId
                && d.RelatedEntityType == EntityReferenceTypes.OwnershipTransfer);

        foreach (var doc in docs)
        {
            doc.IsVerified = true;
            unitOfWork.GetRepository<Document, Guid>().Update(doc);
        }

        transfer.Status = TransferStatus.Signed.ToString();
        UpdateTransfer(transfer);
        await unitOfWork.SaveChangesAsync();

        await NotifyPartiesAsync(transfer, "Agreement signed", "All parties signed. Documents are now immutable.");
        return ServiceResult.Ok();
    }

    public async Task<ServiceResult> MarkRegisteredAsync(Guid transferId, string reviewerId)
    {
        var transfer = await GetTransferAsync(transferId);
        if (transfer is null)
            return ServiceResult.Fail("Transfer not found.");
        if (!IsStatus(transfer, TransferStatus.Signed))
            return ServiceResult.Fail("Transfer must be signed before registry confirmation.");

        transfer.Status = TransferStatus.Registered.ToString();
        UpdateTransfer(transfer);
        await unitOfWork.SaveChangesAsync();

        await NotifyPartiesAsync(transfer, "Registry confirmed", "Transfer was registered with the national registry.");
        return ServiceResult.Ok();
    }

    public async Task<ServiceResult> CompleteAsync(Guid transferId, string reviewerId)
    {
        var transfer = await GetTransferAsync(transferId);
        if (transfer is null)
            return ServiceResult.Fail("Transfer not found.");
        if (!IsStatus(transfer, TransferStatus.Registered))
            return ServiceResult.Fail("Transfer must be registered before completion.");
        if (transfer.EscrowPaymentId is null)
            return ServiceResult.Fail("Escrow payment is missing.");

        var property = await unitOfWork.GetRepository<Property, Guid>().GetByIdAsync(transfer.PropertyId);
        if (property is null)
            return ServiceResult.Fail("Property not found.");

        var ownershipRepo = unitOfWork.GetRepository<OwnershipRecord, Guid>();
        var activeRecords = await ownershipRepo.GetAllAsync(r =>
            r.PropertyId == transfer.PropertyId && r.IsActive && !r.IsDeleted);

        foreach (var record in activeRecords)
        {
            record.IsActive = false;
            record.ReleasedAt = DateTimeOffset.UtcNow;
            ownershipRepo.Update(record);
        }

        await ownershipRepo.AddAsync(new OwnershipRecord
        {
            Id = Guid.NewGuid(),
            PropertyId = transfer.PropertyId,
            OwnerId = transfer.ToOwnerId,
            AcquiredAt = DateTimeOffset.UtcNow,
            AcquisitionMethod = AcquisitionMethod.Purchase,
            PurchasePrice = transfer.AgreedPrice,
            IsActive = true,
            IsDeleted = false
        });

        property.CurrentOwnerId = transfer.ToOwnerId;
        property.UpdatedAt = DateTimeOffset.UtcNow;
        unitOfWork.GetRepository<Property, Guid>().Update(property);

        var escrow = await unitOfWork.GetRepository<Payment, Guid>().GetByIdAsync(transfer.EscrowPaymentId.Value);
        if (escrow is not null)
        {
            escrow.Status = PaymentStatus.Completed;
            escrow.ProcessedAt = DateTimeOffset.UtcNow;
            unitOfWork.GetRepository<Payment, Guid>().Update(escrow);
        }

        var listing = await unitOfWork.GetRepository<PropertyListing, Guid>()
            .GetAllAsync(l => l.PropertyId == transfer.PropertyId && !l.IsDeleted);
        foreach (var item in listing)
        {
            item.Status = ListingStatus.Sold;
            unitOfWork.GetRepository<PropertyListing, Guid>().Update(item);
        }

        transfer.Status = TransferStatus.Completed.ToString();
        transfer.CompletedAt = DateTimeOffset.UtcNow;
        UpdateTransfer(transfer);

        await unitOfWork.SaveChangesAsync();
        await NotifyPartiesAsync(transfer, "Transfer completed", "Ownership transfer completed and escrow released.");

        return ServiceResult.Ok();
    }

    public async Task<ServiceResult> CancelAsync(Guid transferId, string userId, string reason)
    {
        var transfer = await GetTransferAsync(transferId);
        if (transfer is null)
            return ServiceResult.Fail("Transfer not found.");
        if (transfer.FromOwnerId != userId && transfer.ToOwnerId != userId)
            return ServiceResult.Fail("You are not a party to this transfer.");
        if (IsStatus(transfer, TransferStatus.Completed))
            return ServiceResult.Fail("Completed transfers cannot be cancelled.");

        transfer.Status = TransferStatus.Cancelled.ToString();
        transfer.CancellationReason = reason;
        UpdateTransfer(transfer);
        await unitOfWork.SaveChangesAsync();

        await NotifyPartiesAsync(transfer, "Transfer cancelled", reason);
        return ServiceResult.Ok();
    }

    public async Task<IEnumerable<OwnershipTransferDto>> GetForUserAsync(string userId)
    {
        var transfers = await unitOfWork.GetRepository<OwnershipTransfer, Guid>()
            .GetAllAsync(t => !t.IsDeleted
                && (t.FromOwnerId == userId || t.ToOwnerId == userId || t.AgentId == userId));

        return await MapListAsync(transfers);
    }

    public async Task<OwnershipTransferDto?> GetByIdAsync(Guid id)
    {
        var transfer = await GetTransferAsync(id);
        if (transfer is null)
            return null;

        var property = await unitOfWork.GetRepository<Property, Guid>().GetByIdAsync(transfer.PropertyId);
        return OwnershipTransferDto.FromEntity(transfer, property?.Title ?? "Property");
    }

    private async Task<OwnershipTransfer?> GetTransferAsync(Guid id)
    {
        var transfer = await unitOfWork.GetRepository<OwnershipTransfer, Guid>().GetByIdAsync(id);
        return transfer is null || transfer.IsDeleted ? null : transfer;
    }

    private void UpdateTransfer(OwnershipTransfer transfer) =>
        unitOfWork.GetRepository<OwnershipTransfer, Guid>().Update(transfer);

    private static bool IsStatus(OwnershipTransfer transfer, params TransferStatus[] allowed) =>
        allowed.Any(s => string.Equals(transfer.Status, s.ToString(), StringComparison.OrdinalIgnoreCase));

    private async Task NotifyPartiesAsync(OwnershipTransfer transfer, string title, string body)
    {
        await notificationService.NotifyAsync(transfer.FromOwnerId, title, body, transfer.Id, EntityReferenceTypes.OwnershipTransfer);
        await notificationService.NotifyAsync(transfer.ToOwnerId, title, body, transfer.Id, EntityReferenceTypes.OwnershipTransfer);
        if (!string.IsNullOrEmpty(transfer.AgentId))
            await notificationService.NotifyAsync(transfer.AgentId!, title, body, transfer.Id, EntityReferenceTypes.OwnershipTransfer);
    }

    private async Task<IEnumerable<OwnershipTransferDto>> MapListAsync(IEnumerable<OwnershipTransfer> transfers)
    {
        var list = transfers.ToList();
        var propertyIds = list.Select(t => t.PropertyId).Distinct().ToList();
        var properties = (await unitOfWork.GetRepository<Property, Guid>()
            .GetAllAsync(p => propertyIds.Contains(p.Id)))
            .ToDictionary(p => p.Id);

        return list.Select(t => OwnershipTransferDto.FromEntity(
            t,
            properties.TryGetValue(t.PropertyId, out var p) ? p.Title : "Property"));
    }
}
