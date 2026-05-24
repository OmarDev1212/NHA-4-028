using Sakan.Application.Common;
using Sakan.Application.DTO;
using Sakan.Application.Services.Notifications;
using Sakan.Domain.Constants;
using Sakan.Domain.Contracts;
using Sakan.Domain.Entities;
using Sakan.Domain.Enums;

namespace Sakan.Application.Services.RentalApplications;

public class RentalApplicationService(
    IUnitOfWork unitOfWork,
    INotificationService notificationService) : IRentalApplicationService
{
    public async Task<ServiceResult<RentalApplicationDto>> SubmitAsync(CreateRentalApplicationDto dto, string applicantId)
    {
        var listing = await unitOfWork.GetRepository<PropertyListing, Guid>().GetByIdAsync(dto.ListingId);
        if (listing is null || listing.IsDeleted || listing.ListingType != ListingType.ForRent)
            return ServiceResult<RentalApplicationDto>.Fail("Rental listing not found.");
        if (listing.Status != ListingStatus.Active)
            return ServiceResult<RentalApplicationDto>.Fail("Listing is not accepting applications.");

        if (dto.RequestedTo <= dto.RequestedFrom)
            return ServiceResult<RentalApplicationDto>.Fail("Requested end date must be after start date.");

        var application = new RentalApplication
        {
            Id = Guid.NewGuid(),
            ListingId = dto.ListingId,
            ApplicantId = applicantId,
            RequestedFrom = dto.RequestedFrom,
            RequestedTo = dto.RequestedTo,
            MonthlyBudget = dto.MonthlyBudget,
            Message = dto.Message,
            Status = ApplicationStatus.Submitted,
            IsDeleted = false
        };

        await unitOfWork.GetRepository<RentalApplication, Guid>().AddAsync(application);
        await unitOfWork.SaveChangesAsync();

        await notificationService.NotifyAsync(
            listing.ListedById,
            "New rental application",
            $"A renter applied for {dto.RequestedFrom:d} – {dto.RequestedTo:d}.",
            application.Id,
            EntityReferenceTypes.RentalApplication);

        return ServiceResult<RentalApplicationDto>.Ok((await GetByIdAsync(application.Id))!);
    }

    public async Task<ServiceResult<RentalApplicationDto>> ApproveAsync(Guid applicationId, string landlordId)
    {
        var appRepo = unitOfWork.GetRepository<RentalApplication, Guid>();
        var application = await appRepo.GetByIdAsync(applicationId);
        if (application is null || application.IsDeleted)
            return ServiceResult<RentalApplicationDto>.Fail("Application not found.");

        var listing = await unitOfWork.GetRepository<PropertyListing, Guid>().GetByIdAsync(application.ListingId);
        if (listing is null || listing.ListedById != landlordId)
            return ServiceResult<RentalApplicationDto>.Fail("You are not allowed to approve this application.");

        if (application.Status is ApplicationStatus.Approved or ApplicationStatus.Rejected)
            return ServiceResult<RentalApplicationDto>.Fail("Application has already been reviewed.");

        application.Status = ApplicationStatus.Approved;
        application.ReviewedAt = DateTimeOffset.UtcNow;

        var contractDoc = new Document
        {
            Id = Guid.NewGuid(),
            DocumentType = DocumentType.RentalContract,
            FileName = $"rental-contract-{application.Id}.pdf",
            StorageUrl = $"generated/rental-contract-{application.Id}.pdf",
            FileHash = Convert.ToHexString(new byte[32]),
            FileSizeBytes = 0,
            RelatedEntityId = application.Id,
            RelatedEntityType = EntityReferenceTypes.RentalApplication,
            UploadedById = landlordId,
            IsVerified = true,
            UploadedAt = DateTimeOffset.UtcNow,
            IsDeleted = false
        };
        await unitOfWork.GetRepository<Document, Guid>().AddAsync(contractDoc);
        application.ContractDocumentId = contractDoc.Id;

        var otherApplications = await appRepo.GetAllAsync(a =>
            a.ListingId == application.ListingId
            && a.Id != application.Id
            && !a.IsDeleted
            && a.Status == ApplicationStatus.Submitted);

        foreach (var other in otherApplications)
        {
            other.Status = ApplicationStatus.Rejected;
            other.ReviewedAt = DateTimeOffset.UtcNow;
            appRepo.Update(other);
        }

        listing.Status = ListingStatus.Rented;
        unitOfWork.GetRepository<PropertyListing, Guid>().Update(listing);
        appRepo.Update(application);

        await GenerateRentPaymentScheduleAsync(application, listing);
        await unitOfWork.SaveChangesAsync();

        await notificationService.NotifyAsync(
            application.ApplicantId,
            "Rental application approved",
            "Your rental application was approved. A contract and payment schedule were created.",
            application.Id,
            EntityReferenceTypes.RentalApplication);

        return ServiceResult<RentalApplicationDto>.Ok((await GetByIdAsync(applicationId))!);
    }

    public async Task<ServiceResult> RejectAsync(Guid applicationId, string landlordId)
    {
        var appRepo = unitOfWork.GetRepository<RentalApplication, Guid>();
        var application = await appRepo.GetByIdAsync(applicationId);
        if (application is null || application.IsDeleted)
            return ServiceResult.Fail("Application not found.");

        var listing = await unitOfWork.GetRepository<PropertyListing, Guid>().GetByIdAsync(application.ListingId);
        if (listing is null || listing.ListedById != landlordId)
            return ServiceResult.Fail("You are not allowed to reject this application.");

        application.Status = ApplicationStatus.Rejected;
        application.ReviewedAt = DateTimeOffset.UtcNow;
        appRepo.Update(application);
        await unitOfWork.SaveChangesAsync();

        await notificationService.NotifyAsync(
            application.ApplicantId,
            "Rental application rejected",
            "Your rental application was not approved.",
            application.Id,
            EntityReferenceTypes.RentalApplication);

        return ServiceResult.Ok();
    }

    public async Task<IEnumerable<RentalApplicationDto>> GetForListingAsync(Guid listingId, string landlordId)
    {
        var listing = await unitOfWork.GetRepository<PropertyListing, Guid>().GetByIdAsync(listingId);
        if (listing is null || listing.ListedById != landlordId)
            return [];

        var applications = await unitOfWork.GetRepository<RentalApplication, Guid>()
            .GetAllAsync(a => a.ListingId == listingId && !a.IsDeleted);
        return await MapListAsync(applications);
    }

    public async Task<IEnumerable<RentalApplicationDto>> GetForApplicantAsync(string applicantId)
    {
        var applications = await unitOfWork.GetRepository<RentalApplication, Guid>()
            .GetAllAsync(a => a.ApplicantId == applicantId && !a.IsDeleted);
        return await MapListAsync(applications);
    }

    public async Task<RentalApplicationDto?> GetByIdAsync(Guid id)
    {
        var application = await unitOfWork.GetRepository<RentalApplication, Guid>().GetByIdAsync(id);
        if (application is null || application.IsDeleted)
            return null;

        var title = await GetPropertyTitleAsync(application.ListingId);
        return RentalApplicationDto.FromEntity(application, title);
    }

    public async Task<ServiceResult> RecordRentPaymentAsync(Guid paymentId, string payerId)
    {
        var payment = await unitOfWork.GetRepository<Payment, Guid>().GetByIdAsync(paymentId);
        if (payment is null || payment.IsDeleted || payment.PayerId != payerId)
            return ServiceResult.Fail("Payment not found.");

        if (payment.Status == PaymentStatus.Completed)
            return ServiceResult.Fail("Payment is already completed.");

        payment.Status = PaymentStatus.Completed;
        payment.ProcessedAt = DateTimeOffset.UtcNow;
        payment.GatewayTransactionId ??= $"SIM-{Guid.NewGuid():N}"[..20];
        unitOfWork.GetRepository<Payment, Guid>().Update(payment);
        await unitOfWork.SaveChangesAsync();

        await notificationService.NotifyAsync(
            payment.PayeeId,
            "Rent payment received",
            $"Payment of { payment.Amount} {payment.Currency} was completed.",
            payment.Id,
            EntityReferenceTypes.RentalApplication);

        return ServiceResult.Ok();
    }

    private async Task GenerateRentPaymentScheduleAsync(RentalApplication application, PropertyListing listing)
    {
        var paymentRepo = unitOfWork.GetRepository<Payment, Guid>();
        var month = new DateOnly(application.RequestedFrom.Year, application.RequestedFrom.Month, 1);

        while (month < application.RequestedTo)
        {
            await paymentRepo.AddAsync(new Payment
            {
                Id = Guid.NewGuid(),
                Amount = listing.Price,
                Currency = listing.Currency,
                PaymentType = PaymentType.RentPayment,
                Status = PaymentStatus.Pending,
                ReferenceId = application.Id,
                ReferenceType = EntityReferenceTypes.RentalApplication,
                RentalApplicationId = application.Id,
                PayerId = application.ApplicantId,
                PayeeId = listing.ListedById,
                CreatedAt = new DateTimeOffset(month.ToDateTime(TimeOnly.MinValue), TimeSpan.Zero),
                IsDeleted = false
            });

            month = month.AddMonths(1);
        }
    }

    private async Task<IEnumerable<RentalApplicationDto>> MapListAsync(IEnumerable<RentalApplication> applications)
    {
        var list = applications.ToList();
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
            return RentalApplicationDto.FromEntity(a, title);
        });
    }

    private async Task<string> GetPropertyTitleAsync(Guid listingId)
    {
        var listing = await unitOfWork.GetRepository<PropertyListing, Guid>().GetByIdAsync(listingId);
        if (listing is null) return "Property";
        var property = await unitOfWork.GetRepository<Property, Guid>().GetByIdAsync(listing.PropertyId);
        return property?.Title ?? "Property";
    }
}
