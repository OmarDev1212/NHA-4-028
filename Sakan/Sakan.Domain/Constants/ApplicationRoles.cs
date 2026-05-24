namespace Sakan.Domain.Constants;

public static class ApplicationRoles
{
    public const string Member = "Member";
    public const string Buyer = "Buyer";
    public const string Seller = "Seller";
    public const string Renter = "Renter";
    public const string Landlord = "Landlord";
    public const string Admin = "Admin";
    public const string Agent = "Agent";

    public static readonly IReadOnlyList<string> All =
    [
        Member,
        Buyer,
        Seller,
        Renter,
        Landlord,
        Admin,
        Agent,
    ];
}
