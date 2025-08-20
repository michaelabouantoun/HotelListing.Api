namespace HotelListing.Api.Data;

public class Country
{
    public int CountryId { get; set; }
    public required string Name { get; set; }
    public required string ShortName { get; set; }
    public IList<Hotel> Hotels { get; set; } = []; //not a traditional data base type //called a navigation property //anytime there's a relationship between two entities you can add a navigation property

}

