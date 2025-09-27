namespace HotelListing.Api.Application.DTOs.Country;

public class GetCountriesDto
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public required string ShortName { get; set; }
}
