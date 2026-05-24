using Microsoft.Extensions.DependencyInjection;
using Sakan.Application.Services;
using Sakan.Application.Services.Documents;
using Sakan.Application.Services.MaintenanceRequests;
using Sakan.Application.Services.Notifications;
using Sakan.Application.Services.OwnershipTransfers;
using Sakan.Application.Services.PropertyListingService;
using Sakan.Application.Services.PropertyService;
using Sakan.Application.Services.RentalApplications;
using Sakan.Application.Services.ViewingAppointments;
using Sakan.Domain.Contracts;

namespace Sakan.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplicationService(this IServiceCollection services)
    {
        services.AddScoped<IAttachmentService, AttachmentService>();
        services.AddScoped<IPropertyService, PropertyService>();
        services.AddScoped<IPropertyListingService, ListingService>();
        services.AddScoped<IViewingAppointmentService, ViewingAppointmentService>();
        services.AddScoped<IRentalApplicationService, RentalApplicationService>();
        services.AddScoped<IOwnershipTransferService, OwnershipTransferService>();
        services.AddScoped<IMaintenanceRequestService, MaintenanceRequestService>();
        services.AddScoped<INotificationService, NotificationService>();
        services.AddScoped<IDocumentService, DocumentService>();
        return services;
    }
}
