using AutoMapper;
using AutoMapper.QueryableExtensions;
using HotelListing.Api.Application.Contracts;
using HotelListing.Api.Application.DTOs.Hotel;
using HotelListing.Api.Common.Constants;
using HotelListing.Api.Common.Results;
using HotelListing.Api.Domain;
using Microsoft.EntityFrameworkCore;

namespace HotelListing.Api.Application.Services;

public class HotelsService(HotelListingDbContext context, ICountriesService countriesService, IMapper mapper) : IHotelsService
{
    public async Task<Result<IEnumerable<GetHotelDto>>> GetHotelsAsync()
    {
        var hotels = await context.Hotels
            .AsNoTracking()
            .ProjectTo<GetHotelDto>(mapper.ConfigurationProvider)
            .ToListAsync();
        return Result<IEnumerable<GetHotelDto>>.Success(hotels);
    }
    public async Task<Result<GetHotelDto>> GetHotelAsync(int id)
    {
        var hotel = await context.Hotels
            .AsNoTracking()
           .Where(h => h.Id == id)
           .ProjectTo<GetHotelDto>(mapper.ConfigurationProvider)
           .FirstOrDefaultAsync();
        return hotel is null
           ? Result<GetHotelDto>.NotFound(new Error(ErrorCodes.NotFound, $"Hotel '{id}' was not found."))
           : Result<GetHotelDto>.Success(hotel);
    }
    public async Task<Result> UpdateHotelAsync(int id, UpdateHotelDto updateDto) //can be handled by properties means updating only the modified properties
    {
        try
        {
            if (id != updateDto.Id)
            {
                return Result.BadRequest(new Error(ErrorCodes.Validation, "Id route value does not match payload Id"));
            }
            var countryExists = await countriesService.CountryExistsAsync(updateDto.CountryId);
            if (!countryExists)
            {
                return Result.NotFound(new Error(ErrorCodes.NotFound, $"Country {updateDto.CountryId} was not found"));
            }
            var affected = await context.Hotels
           .Where(h => h.Id == id)
           .ExecuteUpdateAsync(setters => setters
                              .SetProperty(h => h.Name, updateDto.Name)
                              .SetProperty(h => h.Address, updateDto.Address)
                              .SetProperty(h => h.Rating, updateDto.Rating)
                              .SetProperty(h => h.CountryId, updateDto.CountryId));
            if (affected == 0)
            {
                return Result.NotFound(new Error(
                    ErrorCodes.NotFound,
                    $"Hotel {id} was not found"));
            }

            return Result.Success();
        }
        catch (Exception) // Wrap in try/catch to handle unexpected DB errors (e.g. country deleted after validation ,causing a foreign key violation)
        {
            return Result.Failure(new Error(ErrorCodes.Failure, "An unexpected derror occurred while updating the hotel"));
        }
    }
    public async Task<Result<GetHotelDto>> CreateHotelAsync(CreateHotelDto createDto)
    {
        try
        {
            var countryExists = await countriesService.CountryExistsAsync(createDto.CountryId);
            if (!countryExists)
            {
                return Result<GetHotelDto>.NotFound(new Error(ErrorCodes.NotFound, $"Country {createDto.CountryId} was not found"));
            }
            var hotel = mapper.Map<Hotel>(createDto);
            context.Hotels.Add(hotel);
            await context.SaveChangesAsync();
            var returnObj = mapper.Map<GetHotelDto>(hotel);
            return Result<GetHotelDto>.Success(returnObj);
        }
        catch (Exception) //wrap in try/catch to handle unexpected DB errors (e.g. country deleted after validation ,causing a foreign key violation)
        {
            return Result<GetHotelDto>.Failure(new Error(ErrorCodes.Failure, "An unexpected derror occurred while creating the hotel"));
        }
    }
    public async Task<Result> DeleteHotelAsync(int id)
    {
        var affected = await context.Hotels
                    .Where(h => h.Id == id)
                    .ExecuteDeleteAsync();
        if (affected == 0)
        {
            return Result.NotFound(new Error(ErrorCodes.NotFound, $"Hotel {id} was not found"));
        }
        return Result.Success();

    }
    public async Task<bool> HotelExistsAsync(int id)
    {
        return await context.Hotels.AnyAsync(e => e.Id == id);
    }

}
