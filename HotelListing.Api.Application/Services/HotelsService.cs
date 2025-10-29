using AutoMapper;
using AutoMapper.QueryableExtensions;
using HotelListing.Api.Application.Contracts;
using HotelListing.Api.Application.DTOs.Country;
using HotelListing.Api.Application.DTOs.Hotel;
using HotelListing.Api.Common.Constants;
using HotelListing.Api.Common.Models.Extensions;
using HotelListing.Api.Common.Models.Filtering;
using HotelListing.Api.Common.Models.Paging;
using HotelListing.Api.Common.Results;
using HotelListing.Api.Domain;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics.Metrics;

namespace HotelListing.Api.Application.Services;

public class HotelsService(HotelListingDbContext context, ICountriesService countriesService, IMapper mapper) : BaseService, IHotelsService
{
    public async Task<Result<PagedResult<GetHotelDto>>> GetHotelsAsync(PaginationParameters paginationParameters
        , HotelFilterParameters filters)
    {
        var query = context.Hotels
            .AsNoTracking()
            .AsQueryable();
        if (filters.CountryId.HasValue)
        {
            query = query.Where(q => q.CountryId == filters.CountryId);
        }
        if (filters.MinRating.HasValue)
            query = query.Where(h => h.Rating >= filters.MinRating.Value);
        if (filters.MaxRating.HasValue)
            query = query.Where(h => h.Rating <= filters.MaxRating.Value);
        if (filters.MinPrice.HasValue)
            query = query.Where(h => h.PerNightRate >= filters.MinPrice.Value);
        if (filters.MaxPrice.HasValue)
            query = query.Where(h => h.PerNightRate <= filters.MaxPrice.Value);
        if (!string.IsNullOrWhiteSpace(filters.Location))
        {
            var location = filters.Location.Trim();

            query = query.Where(h => EF.Functions.Like(h.Address, $"%{location}%"));
        }
        //generic search param
        if (!string.IsNullOrWhiteSpace(filters.Search))
        {
            var search=filters.Search.Trim();
            query = query.Where(h => EF.Functions.Like(h.Name, $"%{search}%") ||
                                   EF.Functions.Like(h.Address, $"%{search}%"));
        }
        query = filters.SortBy?.ToLower() switch
        {
            "rating" => filters.SortDescending ?
                query.OrderByDescending(h => h.Rating) : query.OrderBy(h => h.Rating),

            "price" => filters.SortDescending ?
                query.OrderByDescending(h => h.PerNightRate) : query.OrderBy(h => h.PerNightRate),
            _ => query.OrderBy(h => h.PerNightRate),
        };

        var hotels = await query
            .ProjectTo<GetHotelDto>(mapper.ConfigurationProvider)
            .ToPagedResultAsync(paginationParameters);
        return Result<PagedResult<GetHotelDto>>.Success(hotels);
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
    public async Task<Hotel?> HotelObjectExistsAsync(int id)
    {
        return await context.Hotels
            .AsNoTracking()
            .FirstOrDefaultAsync(h => h.Id == id);
    }

    public async Task<Result> PatchHotelAsync(int id, JsonPatchDocument<UpdateHotelDto> patchDoc)
    {
        var hotel = await context.Hotels.FindAsync(id);
        if (hotel == null)
        {
            return Result.NotFound(new Error(ErrorCodes.NotFound, $"Hotel '{id}' was not found."));
        }
        var hotelDto = mapper.Map<UpdateHotelDto>(hotel); //for security purpose
        patchDoc.ApplyTo(hotelDto);
        if (hotelDto.Id != id)
        {
            return Result.BadRequest(new Error(ErrorCodes.Validation, "Cannot modify the Id field."));
        }
        if (!ValidateDto(hotelDto))
        {
            return Result.BadRequest(new Error(ErrorCodes.Validation, "Invalid data."));
        }
        var countryExists = await countriesService.CountryExistsAsync(hotelDto.CountryId);
        if (!countryExists)
        {
            return Result.NotFound(new Error(ErrorCodes.NotFound, $"Country {hotelDto.CountryId} was not found"));
        }
        mapper.Map(hotelDto, hotel);
        await context.SaveChangesAsync();
        return Result.Success();
    }
}
