using AutoMapper;
using HotelListing.Api.Application.DTOs.Booking;
using HotelListing.Api.Application.DTOs.Country;
using HotelListing.Api.Application.DTOs.Hotel;
using HotelListing.Api.Domain;

namespace HotelListing.Api.Application.MappingProfiles;

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
public sealed class BookingMappingProfile : Profile
{
    public BookingMappingProfile()
    {
        CreateMap<Booking, GetBookingDto>()
        .ForMember(d => d.HotelName, o => o.MapFrom(s => s.Hotel!.Name))
        .ForMember(d => d.Status, o => o.MapFrom(s => s.Status.ToString()));
        CreateMap<CreateBookingDto, Booking>()
          .ForMember(d => d.Id, o => o.Ignore())         //if i dont specify this automapper will automatically
          .ForMember(d => d.UserId, o => o.Ignore())    //will look at every property in booking and then try and map every property over to the target which is in this case createDto
          .ForMember(d => d.TotalPrice, o => o.Ignore())
          .ForMember(d => d.Status, o => o.Ignore())
          .ForMember(d => d.CreatedAtUtc, o => o.Ignore())
          .ForMember(d => d.UpdatedAtUtc, o => o.Ignore())
          .ForMember(d => d.User, o => o.Ignore())
          .ForMember(d => d.HotelId, o => o.Ignore())
          .ForMember(d => d.Hotel, o => o.Ignore());

        CreateMap<UpdateBookingDto, Booking>()
            .ForMember(d => d.Id, o => o.Ignore())
            .ForMember(d => d.UserId, o => o.Ignore())
            .ForMember(d => d.TotalPrice, o => o.Ignore())
            .ForMember(d => d.Status, o => o.Ignore())
            .ForMember(d => d.CreatedAtUtc, o => o.Ignore())
            .ForMember(d => d.UpdatedAtUtc, o => o.Ignore())
            .ForMember(d => d.User, o => o.Ignore())
            .ForMember(d => d.HotelId, o => o.Ignore())
            .ForMember(d => d.Hotel, o => o.Ignore());













    }
}