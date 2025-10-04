using HotelListing.Api.Common.Enums;

namespace HotelListing.Api.Domain;

public class Booking
{
    public int Id { get; set; }
    public Hotel? Hotel { get; set; }
    public int HotelId { get; set; }
    public ApplicationUser? User { get; set; }
    public required string UserId { get; set; }
    public DateOnly CheckIn { get; set; }
    public DateOnly CheckOut { get; set; }
    public int Guests { get; set; }
    public decimal TotalPrice { get; set; }
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;                //important
    public DateTime? UpdatedAtUtc { get; set; }                                //important also why nullable here to if i dont update it be null not a default date
    public BookingStatus Status { get; set; } = BookingStatus.Pending;
}
