using Microsoft.AspNetCore.Identity;
using Sakan.Domain.Constants;
using Sakan.Domain.Entities;
using Sakan.Domain.Enums;

namespace Sakan.Infrastructure.Identity;

public class UserRoleAssignmentService(UserManager<ApplicationUser> userManager)
{
    public async Task EnsureMemberRoleAsync(ApplicationUser user)
    {
        if (!await userManager.IsInRoleAsync(user, ApplicationRoles.Member))
            await userManager.AddToRoleAsync(user, ApplicationRoles.Member);
    }

    public async Task EnsureListingRoleAsync(ApplicationUser user, ListingType listingType)
    {
        var role = listingType is ListingType.ForRent or ListingType.ShortTermRent
            ? ApplicationRoles.Landlord
            : ApplicationRoles.Seller;

        if (!await userManager.IsInRoleAsync(user, role))
            await userManager.AddToRoleAsync(user, role);
    }

    public async Task EnsureRenterRoleAsync(ApplicationUser user)
    {
        if (!await userManager.IsInRoleAsync(user, ApplicationRoles.Renter))
            await userManager.AddToRoleAsync(user, ApplicationRoles.Renter);
    }

    public async Task EnsureBuyerRoleAsync(ApplicationUser user)
    {
        if (!await userManager.IsInRoleAsync(user, ApplicationRoles.Buyer))
            await userManager.AddToRoleAsync(user, ApplicationRoles.Buyer);
    }
}
