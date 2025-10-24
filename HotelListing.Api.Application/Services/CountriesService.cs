﻿using AutoMapper;
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

namespace HotelListing.Api.Application.Services;

public class CountriesService(HotelListingDbContext context, IMapper mapper) :BaseService, ICountriesService
{
    public async Task<Result<IEnumerable<GetCountriesDto>>> GetCountriesAsync(CountryFilterParameters filters)
    {
        var query = context.Countries.AsQueryable();
        if (!string.IsNullOrWhiteSpace(filters.Search))
        {
            var term = filters.Search.Trim();
            query = query.Where(c => EF.Functions.Like(c.Name, $"%{term}%")
            || EF.Functions.Like(c.ShortName, $"%{term}%"));
        }
        var countries = await query
        .ProjectTo<GetCountriesDto>(mapper.ConfigurationProvider) //error provider handled
        .ToListAsync();
        return Result<IEnumerable<GetCountriesDto>>.Success(countries);
    }
    public async Task<Result<GetCountryDto>> GetCountryAsync(int id)
    {
        var country = await context.Countries
            .Where(q => q.CountryId == id)
            .ProjectTo<GetCountryDto>(mapper.ConfigurationProvider) //error provider handled
            .FirstOrDefaultAsync();
        return country is null
            ? Result<GetCountryDto>.NotFound(new Error(ErrorCodes.NotFound, $"Country with this id = {id} not found"))
            : Result<GetCountryDto>.Success(country);
    }
    public async Task<Result> UpdateCountryAsync(int id, UpdateCountryDto updateDto)
    {

        if (id != updateDto.Id)
        {
            return Result.BadRequest(new Error(ErrorCodes.Validation, "Id route value does not match payload Id"));
        }
        var affected = await context.Countries
                       .Where(c => c.CountryId == id)
                       .ExecuteUpdateAsync(setters => setters
                                           .SetProperty(c => c.Name, updateDto.Name)
                                           .SetProperty(c => c.ShortName, updateDto.ShortName));

        if (affected == 0)
        {
            return Result.NotFound(new Error(ErrorCodes.NotFound, $"Country {id} was not found"));
        }

        return Result.Success();
    }

    public async Task<Result<GetCountryDto>> CreateCountryAsync(CreateCountryDto createDto)
    {
        var country = mapper.Map<Country>(createDto);
        context.Countries.Add(country);
        await context.SaveChangesAsync();
        var dto = mapper.Map<GetCountryDto>(country); //handled
        return Result<GetCountryDto>.Success(dto);
    }



    public async Task<Result> DeleteCountryAsync(int id) //doesnt provide error because while deleting a country by cascade deletes all its hotels no need to handle 
    {

        var affected = await context.Countries
                       .Where(c => c.CountryId == id)
                       .ExecuteDeleteAsync();

        if (affected == 0)
        {
            return Result.NotFound(new Error(ErrorCodes.NotFound, $"Country {id} was not found"));
        }

        return Result.Success();
    }

    public async Task<bool> CountryExistsAsync(int id)
    {
        return await context.Countries.AnyAsync(e => e.CountryId == id);
    }

    public async Task<Result<GetCountryHotelsDto>> GetCountryHotelsAsync(int countryId, PaginationParameters paginationParameters
        , CountryFilterParameters filters)
    {
        var countryName = await context.Countries
            .Where(q => q.CountryId == countryId)
            .Select(q => q.Name)
            .FirstOrDefaultAsync();
        if (countryName == null)
        {
            return Result<GetCountryHotelsDto>.Failure(new Error(ErrorCodes.NotFound, $"Country '{countryId}' was not found."));

        }
        //ghyrt
        var hotelsQuery = context.Hotels
            .Where(h => h.CountryId == countryId)
            .AsQueryable();
        if (!string.IsNullOrWhiteSpace(filters.Search))
        {
            var term = filters.Search.Trim();
            hotelsQuery = hotelsQuery.Where(h => EF.Functions.Like(h.Name, $"%{term}%"));

        }
        hotelsQuery = (filters.SortBy?.Trim().ToLowerInvariant()) switch
        {
            "rating" => filters.SortDescending ? hotelsQuery.OrderByDescending(q => q.Rating) :
            hotelsQuery.OrderBy(q => q.Rating),
            _ => hotelsQuery.OrderBy(q => q.PerNightRate)
        };
        var pagedHotels = await hotelsQuery
            .ProjectTo<GetHotelSlimDto>(mapper.ConfigurationProvider)
            .ToPagedResultAsync(paginationParameters);
        var result = new GetCountryHotelsDto
        {
            Id = countryId,
            Name = countryName,
            Hotels = pagedHotels
        };
        return Result<GetCountryHotelsDto>.Success(result);
    }

    public async Task<Result> PatchCountryAsync(int id, JsonPatchDocument<UpdateCountryDto> patchDoc)
    {
        var country=await context.Countries.FindAsync(id);
        if (country == null) {
            return Result.NotFound(new Error(ErrorCodes.NotFound, $"Country '{id}' was not found."));
        }
        var countryDto=mapper.Map<UpdateCountryDto>(country); //for security purpose
        patchDoc.ApplyTo(countryDto);
        if (countryDto.Id != id)
        {
            return Result.BadRequest(new Error(ErrorCodes.Validation, "Cannot modify the Id field."));
        }
        if (!ValidateDto(countryDto)) {
            return Result.BadRequest(new Error(ErrorCodes.Validation, "Invalid data."));
        }
        mapper.Map(countryDto, country);
        await context.SaveChangesAsync();
        return Result.Success();

    }
}


