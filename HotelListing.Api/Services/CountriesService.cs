using AutoMapper;
using AutoMapper.QueryableExtensions;
using HotelListing.Api.Constants;
using HotelListing.Api.Contracts;
using HotelListing.Api.Data;
using HotelListing.Api.DTOs.Country;
using HotelListing.Api.Results;
using Microsoft.EntityFrameworkCore;

namespace HotelListing.Api.Services;

public class CountriesService(HotelListingDbContext context, IMapper mapper) : ICountriesService
{
    public async Task<Result<IEnumerable<GetCountriesDto>>> GetCountriesAsync()
    {
        var countries = await context.Countries
        .AsNoTracking() //perfomance booster
        .ProjectTo<GetCountriesDto>(mapper.ConfigurationProvider) //error provider handled
        .ToListAsync();
        return Result<IEnumerable<GetCountriesDto>>.Success(countries);
    }
    public async Task<Result<GetCountryDto>> GetCountryAsync(int id)
    {
        var country = await context.Countries
            .AsNoTracking()
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

}

