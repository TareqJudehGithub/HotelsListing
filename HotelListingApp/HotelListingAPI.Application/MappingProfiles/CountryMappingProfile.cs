using AutoMapper;
using HotelListingAPI.Application.DTOs.Country;
using HotelListingAPI.Domain;

namespace HotelListingAPI.Application.MappingProfiles;

public class CountryMappingProfile : Profile
{
    public CountryMappingProfile()
    {
        CreateMap<Country, GetCountriesDto>()
             .ForMember(d => d.CountryId, opt => opt
            .MapFrom(s => s.Id));

        CreateMap<Country, GetCountryDto>()
            .ForMember(d => d.CountryId, opt => opt
            .MapFrom(s => s.Id));

        CreateMap<Country, CreateCountryDto>().ReverseMap();

        CreateMap<Country, UpdateCountryDto>()
            .ForMember(d => d.Id, opt => opt.MapFrom(s => s.Id))
            .ReverseMap()
            .ForMember(d => d.Id, opt => opt.MapFrom(s => s.Id));
    }
}
