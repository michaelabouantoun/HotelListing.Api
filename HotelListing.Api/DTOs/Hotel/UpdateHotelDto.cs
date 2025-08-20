using System.ComponentModel.DataAnnotations;

namespace HotelListing.Api.DTOs.Hotel;

public class UpdateHotelDto:CreateHotelDto //to avoid repetition
{
    [Required]
    public int Id { get; set; }

}