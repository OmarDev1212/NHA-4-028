using Microsoft.EntityFrameworkCore;

using Microsoft.Extensions.DependencyInjection;

using Sakan.Domain.Entities;

using System.Text.Json;

using System.Text.Json.Serialization;

namespace Sakan.Infrastructure.Data.Seeding;



public static class DataSeeder

{

    private static readonly JsonSerializerOptions JsonOptions = new()

    {

        PropertyNameCaseInsensitive = true,

        Converters = { new JsonStringEnumConverter() }

    };



    public static async Task SeedAsync(ApplicationDbContext context)

    {

        if (context.Database.GetPendingMigrations().Any())
        {
            await context.Database.MigrateAsync();
        }

        //await SeedPropertiesAsync(context);

        //await SeedPropertyFeaturesAsync(context);

        //await SeedPropertyPhotosAsync(context);

        ////await SeedDocumentsAsync(context);

        //await SeedPropertyListingsAsync(context);

        //await SeedOwnershipRecordsAsync(context);

        //await SeedPaymentsAsync(context);

        //await SeedOwnershipTransfersAsync(context);

        //await SeedMaintenanceRequestsAsync(context);

        //await SeedRentalApplicationsAsync(context);

        //await SeedViewingAppointmentsAsync(context);



        //await context.SaveChangesAsync();

    }

    private static async Task SeedPropertiesAsync(ApplicationDbContext context)

    {
        if (!context.Set<Property>().Any())
        {

            var jsonProperties = File.ReadAllText(@"../Sakan.Infrastructure\Data\Seeding\JsonData\properties.json");

            var properties = JsonSerializer.Deserialize<List<Property>>(jsonProperties, JsonOptions);

            await context.AddRangeAsync(properties!);
            
        }
    }



    private static async Task SeedPropertyFeaturesAsync(ApplicationDbContext context)
    {
        if (!context.Set<PropertyFeature>().Any())
        {

            var jsonPropertiesFeatures = File.ReadAllText(@"../Sakan.Infrastructure\Data\Seeding\JsonData\property-features.json");

            var features = JsonSerializer.Deserialize<List<PropertyFeature>>(jsonPropertiesFeatures, JsonOptions);

            await context.AddRangeAsync(features!);
        }
    }



    private static async Task SeedPropertyPhotosAsync(ApplicationDbContext context)

    {
        if (!context.Set<PropertyPhoto>().Any())
        {

            var jsonPropertiesPhotos = File.ReadAllText(@"../Sakan.Infrastructure\Data\Seeding\JsonData\property-photos.json");

            var propertiesPhotos = JsonSerializer.Deserialize<List<PropertyPhoto>>(jsonPropertiesPhotos, JsonOptions);

            await context.AddRangeAsync(propertiesPhotos!);
        }

    }



    private static async Task SeedDocumentsAsync(ApplicationDbContext context)
    {
        if (!context.Set<Document>().Any())
        {

            var jsonDocuments = File.ReadAllText(@"../Sakan.Infrastructure\Data\Seeding\JsonData\documents.json");

            var documents = JsonSerializer.Deserialize<List<Document>>(jsonDocuments, JsonOptions);

            await context.AddRangeAsync(documents!);

        }
        await context.SaveChangesAsync();

    }



    private static async Task SeedPropertyListingsAsync(ApplicationDbContext context)
    {
        if (!context.Set<PropertyListing>().Any())
        {
            var jsonPropertyListings = File.ReadAllText(@"../Sakan.Infrastructure\Data\Seeding\JsonData\property-listings.json");

            var propertyListings = JsonSerializer.Deserialize<List<PropertyListing>>(jsonPropertyListings, JsonOptions);

            await context.AddRangeAsync(propertyListings!);

        await context.SaveChangesAsync();
        }
    }



    private static async Task SeedOwnershipRecordsAsync(ApplicationDbContext context)

    {
        if (!context.Set<OwnershipRecord>().Any())
        {

            var jsonOwnershipRecords = File.ReadAllText(@"../Sakan.Infrastructure\Data\Seeding\JsonData\ownership-records.json");

            var ownershipRecords = JsonSerializer.Deserialize<List<OwnershipRecord>>(jsonOwnershipRecords, JsonOptions);

            await context.AddRangeAsync(ownershipRecords!);
        }

    }
    private static async Task SeedPaymentsAsync(ApplicationDbContext context)

    {
        if (!context.Set<Payment>().Any())
        {

            var jsonPayments = File.ReadAllText(@"../Sakan.Infrastructure\Data\Seeding\JsonData\payments.json");

            var payments = JsonSerializer.Deserialize<List<Payment>>(jsonPayments, JsonOptions);

            await context.AddRangeAsync(payments!);
        }

    }
    private static async Task SeedOwnershipTransfersAsync(ApplicationDbContext context)
    {
        if (!context.Set<OwnershipTransfer>().Any())
        {

            var jsonOwnershipTransfers = File.ReadAllText(@"../Sakan.Infrastructure\Data\Seeding\JsonData\ownership-transfers.json");

            var ownershipTransfers = JsonSerializer.Deserialize<List<OwnershipTransfer>>(jsonOwnershipTransfers, JsonOptions);

            await context.AddRangeAsync(ownershipTransfers!);
        }

    }
    private static async Task SeedMaintenanceRequestsAsync(ApplicationDbContext context)
    {
        if (!context.Set<MaintanceRequest>().Any())
        {

            var jsonMaintenanceRequests = File.ReadAllText(@"../Sakan.Infrastructure\Data\Seeding\JsonData\maintenance-requests.json");

            var maintenanceRequests = JsonSerializer.Deserialize<List<MaintanceRequest>>(jsonMaintenanceRequests, JsonOptions);

            await context.AddRangeAsync(maintenanceRequests!);

        }
    }
    private static async Task SeedRentalApplicationsAsync(ApplicationDbContext context)

    {
        if (!context.Set<Property>().Any())
        {
            var jsonRentalApplications = File.ReadAllText(@"../Sakan.Infrastructure\Data\Seeding\JsonData\rental-applications.json");

            var rentalApplications = JsonSerializer.Deserialize<List<RentalApplication>>(jsonRentalApplications, JsonOptions);

            await context.AddRangeAsync(rentalApplications!);
        }


    }
    private static async Task SeedViewingAppointmentsAsync(ApplicationDbContext context)

    {
        if (!context.Set<ViewingAppoinment>().Any())
        {

            var jsonViewingAppointments = File.ReadAllText(@"../Sakan.Infrastructure\Data\Seeding\JsonData\viewing-appointments.json");

            var viewingAppointments = JsonSerializer.Deserialize<List<ViewingAppoinment>>(jsonViewingAppointments, JsonOptions);

            await context.AddRangeAsync(viewingAppointments!);
        }
    }

}


