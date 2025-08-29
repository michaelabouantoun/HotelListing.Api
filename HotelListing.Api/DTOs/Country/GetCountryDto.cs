using HotelListing.Api.DTOs.Hotel;

namespace HotelListing.Api.DTOs.Country;

public class GetCountryDto
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public required string ShortName { get; set; }
    public required List<GetHotelSlimDto> Hotels { get; set; }
}
