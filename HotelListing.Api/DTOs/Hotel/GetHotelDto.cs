namespace HotelListing.Api.DTOs.Hotel;

public class GetHotelDto
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public required string Address { get; set; }
    public double Rating { get; set; }
    public int CountryId { get; set; }
    public required string Country { get; set; }

}
public record GetHotelSlimDto(
    int Id,
    string Name,
    string Address,
    double Rating
    );

