using HotelListing.Api.DTOs.Hotel;
using HotelListing.Api.Results;
namespace HotelListing.Api.Contracts;

public interface IHotelsService
{
    Task<Result<GetHotelDto>> CreateHotelAsync(CreateHotelDto createDto);
    Task<Result> DeleteHotelAsync(int id);
    Task<Result<GetHotelDto>> GetHotelAsync(int id);
    Task<Result<IEnumerable<GetHotelDto>>> GetHotelsAsync();
    Task<bool> HotelExistsAsync(int id);
    Task<Result> UpdateHotelAsync(int id, UpdateHotelDto updateDto);
}