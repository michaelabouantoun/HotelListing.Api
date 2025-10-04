using HotelListing.Api.Application.DTOs.Hotel;
using HotelListing.Api.Common.Models.Paging;

namespace HotelListing.Api.Application.DTOs.Country;

public class GetCountryDto
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public required string ShortName { get; set; }
    public required List<GetHotelSlimDto> Hotels { get; set; }
}
public class GetCountryHotelsDto
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public required PagedResult<GetHotelSlimDto> Hotels { get; set; }
}
