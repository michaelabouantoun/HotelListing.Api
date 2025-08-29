using AutoMapper;
using HotelListing.Api.Data;
using HotelListing.Api.DTOs.Country;
using HotelListing.Api.DTOs.Hotel;

namespace HotelListing.Api.MappingProfiles;

public class HotelMappingProfile : Profile
{
    public HotelMappingProfile()
    {
        CreateMap<Hotel, GetHotelDto>()
         .ForMember(d => d.Country, opt => opt.MapFrom(src => src.Country != null ? src.Country.Name : string.Empty)); //need to handle this
        CreateMap<CreateHotelDto, Hotel>();
        CreateMap<UpdateHotelDto, Hotel>();
        CreateMap<Hotel, GetHotelSlimDto>();



    }
}
public class CountryMappingProfile : Profile
{
    public CountryMappingProfile()
    {
        CreateMap<Country, GetCountryDto>()
       .ForMember(d => d.Id, cfg => cfg.MapFrom(src => src.CountryId)); //hotels is automatically mapped to hotelsslim because of create map  hotels to hotels slim
        CreateMap<Country, GetCountriesDto>()
        .ForMember(d => d.Id, cfg => cfg.MapFrom(src => src.CountryId));
        CreateMap<CreateCountryDto, Country>();
        CreateMap<UpdateCountryDto, Country>();


    }

}
/*public class CountryNameResolver : IValueResolver<Hotel, GetHotelDto, string>
{
    public string Resolve(Hotel source, GetHotelDto destination, string destMember, ResolutionContext context)
    {
        return source.Country?.Name ?? string.Empty;
    }
}*/    //bad idea in this case

