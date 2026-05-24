using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Sakan.Domain.Constants;
using Sakan.Domain.Entities;

namespace Sakan.Infrastructure.Data.Seeding;

public static class IdentityDataSeeder
{
    public static async Task SeedAsync(IServiceProvider serviceProvider)
    {
        var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        var context = serviceProvider.GetRequiredService<ApplicationDbContext>();
        foreach (var roleName in ApplicationRoles.All)
        {
            if (!await roleManager.RoleExistsAsync(roleName))
                await roleManager.CreateAsync(new IdentityRole(roleName));
        }
        if (!await context.Users.AnyAsync())

            return;
        await SeedUsersAsync(context);

    }
    private static async Task SeedUsersAsync(ApplicationDbContext context)
    {
        if (!context.Set<ApplicationUser>().Any())
        {
            var jsonUsers = File.ReadAllText(@"../Sakan.Infrastructure\Data\Seeding\JsonData\users.json");

            var users = JsonSerializer.Deserialize<List<ApplicationUser>>(jsonUsers, JsonOptions);

            await context.AddRangeAsync(users!);
        }


    }

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,

        Converters = { new JsonStringEnumConverter() }

    };
}
