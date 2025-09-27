using HotelListing.Api.Application.DTOs.Hotel;

namespace HotelListing.Api.Application.DTOs.Country;

public class GetCountryDto
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public required string ShortName { get; set; }
    public required List<GetHotelSlimDto> Hotels { get; set; }
}
