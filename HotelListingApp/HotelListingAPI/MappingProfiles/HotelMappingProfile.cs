using AutoMapper;

using HotelListingAPI.Data;
using HotelListingAPI.DTOs;
using HotelListingAPI.DTOs.Hotel;
namespace HotelListingAPI.MappingProfiles;

public class HotelMappingProfile : Profile
{
    public HotelMappingProfile()
    {
        CreateMap<Hotel, GetHotelDto>()
           // For the destination Country (string), map from the source (Country entity: Name property)         
           .ForMember(d => d.CountryName, cfg => cfg
            .MapFrom(s => s.Country != null ? s.Country.Name : "Country Name field"));

        // In case we did not decide to use the  'Return newly created hotel along with country name' in HotelsServices
        //.ForMember(d => d.Country, cfg => cfg.MapFrom<CountryNameResolver>());

        CreateMap<CreateHotelDto, Hotel>().ReverseMap();
        CreateMap<UpdateHotelDto, Hotel>().ReverseMap();
    }
}

// Resolve Country Name in Hotels entity
public class CountryNameResolver : IValueResolver<Hotel, GetHotelDto, string>
{
    public string Resolve(Hotel source, GetHotelDto destination, string destMember, ResolutionContext context)
    {
        return source.Country?.Name ?? string.Empty;
    }
}
