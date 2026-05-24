using Sakan.Domain.Enums;

namespace Sakan.Application.DTO;

public class ListingSearchCriteria
{
    public ListingType? ListingType { get; set; }
    public ListingStatus? Status { get; set; } = ListingStatus.Active;
    public decimal? MinPrice { get; set; }
    public decimal? MaxPrice { get; set; }
    public DateOnly? AvailableFrom { get; set; }
    public DateOnly? AvailableTo { get; set; }
    public string? City { get; set; }
    public string? Country { get; set; }
}
