using AutoMapper;
using AutoMapper.QueryableExtensions;
using HotelListing.Api.Constants;
using HotelListing.Api.Contracts;
using HotelListing.Api.Data;
using HotelListing.Api.Data.Enums;
using HotelListing.Api.DTOs.Booking;
using HotelListing.Api.Results;
using Microsoft.EntityFrameworkCore;

namespace HotelListing.Api.Services;

public class BookingService(HotelListingDbContext context, IUsersService usersService, IMapper mapper) : IBookingService
{
    public async Task<Result<IEnumerable<GetBookingDto>>> GetBookingsForHotelAsync(int hotelId)
    {
        var hotelExists = await context.Hotels.AnyAsync(h => h.Id == hotelId); // This hotel existence check is only necessary for System Administrators.
                                                                               // HotelAdmins are guaranteed by the FK constraint to be linked to a valid Hotel.
        if (!hotelExists)
        {
            return Result<IEnumerable<GetBookingDto>>.NotFound(new Error(ErrorCodes.NotFound, $"Hotel with id '{hotelId}' was not found."));
        }
        var bookings = await context.Bookings
                        .Where(b => b.HotelId == hotelId)
                        .OrderBy(b => b.CheckIn)
                        .AsNoTracking()
                        .ProjectTo<GetBookingDto>(mapper.ConfigurationProvider)
                        .ToListAsync();
        return Result<IEnumerable<GetBookingDto>>.Success(bookings);



    }
    public async Task<Result<GetBookingDto>> CreateBookingAsync(CreateBookingDto dto, int hotelId)
    {
        var userId = usersService.UserId;
        bool overlaps = await IsOverlap(hotelId, userId, dto.CheckIn, dto.CheckOut);
        if (overlaps)
        {
            return Result<GetBookingDto>.Failure(new Error(ErrorCodes.Conflict, "The selected dates overlap with a existing booking."));
        }

        var hotel = await context.Hotels
            .Where(h => h.Id == hotelId)
            .AsNoTracking()
            .FirstOrDefaultAsync();
        if (hotel == null)
        {
            return Result<GetBookingDto>.Failure(new Error(ErrorCodes.NotFound, $"Hotel '{hotelId}' was not found."));
        }

        var nights = dto.CheckOut.DayNumber - dto.CheckIn.DayNumber;
        var totalPrice = hotel.PerNightRate * nights;
        var booking = mapper.Map<Booking>(dto);
        booking.UserId = userId;
        booking.HotelId = hotelId;
        booking.TotalPrice = totalPrice;
        context.Bookings.Add(booking);
        await context.SaveChangesAsync();
        var result = mapper.Map<GetBookingDto>(booking);
        return Result<GetBookingDto>.Success(result);

    }

    public async Task<Result<GetBookingDto>> UpdateBookingAsync(int hotelId, int bookingId, UpdateBookingDto dto)
    {
        //in the update i am changing info only not status
        var userId = usersService.UserId;
        bool overlaps = await IsOverlap(hotelId, userId, dto.CheckIn, dto.CheckOut, bookingId);
        if (overlaps)
        {
            return Result<GetBookingDto>.Failure(new Error(ErrorCodes.Conflict, "The selected dates overlap with a existing booking."));
        }

        var booking = await context.Bookings //since the booking exists that mean i dont need to check if the hotel exists again right now
            .Include(b => b.Hotel)
            .FirstOrDefaultAsync(b =>
            b.Id == bookingId
            && b.HotelId == hotelId
            && b.UserId == userId //this checking ensures that this is the booking for this user on this specific hotel //authorization
            );
        if (booking is null)
        {
            return Result<GetBookingDto>.Failure(new Error(ErrorCodes.NotFound, $"Booking '{bookingId}' was not found."));
        }
        if (booking.Status == BookingStatus.Cancelled)
        {
            return Result<GetBookingDto>.Failure(new Error(ErrorCodes.Conflict, "Cancelled bookings cannot be modified."));
        }

        mapper.Map(dto, booking);
        var perNight = booking.Hotel!.PerNightRate;
        var nights = dto.CheckOut.DayNumber - dto.CheckIn.DayNumber;
        booking.TotalPrice = perNight * nights; //the checkin and check out changed so i need to recalulate
        booking.UpdatedAtUtc = DateTime.UtcNow;
        await context.SaveChangesAsync();
        var updated = mapper.Map<GetBookingDto>(booking);
        return Result<GetBookingDto>.Success(updated);
    }
    private async Task<bool> IsOverlap(int hotelId, string userId, DateOnly checkIn, DateOnly checkOut, int? bookingId = null)
    {
        var query = context.Bookings.Where(
            b => b.HotelId == hotelId
            && b.Status != BookingStatus.Cancelled
            && checkIn < b.CheckOut
            && checkOut > b.CheckIn
            && b.UserId == userId
            ).AsQueryable();
        if (bookingId.HasValue)
        {
            query = query.Where(q => q.Id != bookingId.Value);
        }
        return await query.AnyAsync();
    }

    public async Task<Result> CancelBookingAsync(int hotelId, int bookingId)
    {
        //a user should be able to cancel a request 
        //we dont want to delete any bookings, in this case its a sort of a soft delete where you are changing the status and using the status to dictate how current the record is
        //why could't have used the update method to do both operation? you could have but its all a matter of how segregated do you want your operations to be
        var userId = usersService.UserId;
        var booking = await context.Bookings
            .FirstOrDefaultAsync(b =>
            b.Id == bookingId
            && b.HotelId == hotelId
            && b.UserId == userId
            );
        if (booking is null)
        {
            return Result.Failure(new Error(ErrorCodes.NotFound, $"Booking '{bookingId}' was not found."));
        }
        if (booking.Status == BookingStatus.Cancelled)
        {
            return Result.Failure(new Error(ErrorCodes.Conflict, "This booking has already been cancelled."));
        }
        booking.Status = BookingStatus.Cancelled;
        booking.UpdatedAtUtc = DateTime.UtcNow;
        await context.SaveChangesAsync();
        return Result.Success();
    }
    public async Task<Result> AdminConfirmBookingAsync(int hotelId, int bookingId) //the extent to which admins have power over the bookings is relative to business rules build as you need to
    {
        var booking = await context.Bookings
          .FirstOrDefaultAsync(b =>
          b.Id == bookingId
          && b.HotelId == hotelId);
        if (booking is null)
        {
            return Result.Failure(new Error(ErrorCodes.NotFound, $"Booking '{bookingId}' was not found."));
        }
        if (booking.Status == BookingStatus.Cancelled)
        {
            return Result.Failure(new Error(ErrorCodes.Conflict, "This booking has already been cancelled."));
        }
        booking.Status = BookingStatus.Confirmed;
        booking.UpdatedAtUtc = DateTime.UtcNow;
        await context.SaveChangesAsync();
        return Result.Success();


    }
    public async Task<Result> AdminCancelBookingAsync(int hotelId, int bookingId)
    {
        var booking = await context.Bookings
           .FirstOrDefaultAsync(b =>
           b.Id == bookingId
           && b.HotelId == hotelId);
        if (booking is null)
        {
            return Result.Failure(new Error(ErrorCodes.NotFound, $"Booking '{bookingId}' was not found."));
        }
        if (booking.Status == BookingStatus.Cancelled)
        {
            return Result.Failure(new Error(ErrorCodes.Conflict, "This booking has already been cancelled."));
        }
        booking.Status = BookingStatus.Cancelled;
        booking.UpdatedAtUtc = DateTime.UtcNow;
        await context.SaveChangesAsync();
        return Result.Success();

    }

    public async Task<Result<IEnumerable<GetBookingDto>>> GetUserBookingsForHotelAsync(int hotelId)
    {
        var userId = usersService.UserId;
        var hotelExists = await context.Hotels.AnyAsync(h => h.Id == hotelId);
        if (!hotelExists)
        {
            return Result<IEnumerable<GetBookingDto>>.NotFound(new Error(ErrorCodes.NotFound, $"Hotel with id '{hotelId}' was not found."));
        }
        var bookings = await context.Bookings
                        .Where(b => b.HotelId == hotelId && b.UserId == userId)
                        .OrderBy(b => b.CheckIn)
                        .AsNoTracking()
                        .ProjectTo<GetBookingDto>(mapper.ConfigurationProvider)
                        .ToListAsync();
        return Result<IEnumerable<GetBookingDto>>.Success(bookings);


    }
}
