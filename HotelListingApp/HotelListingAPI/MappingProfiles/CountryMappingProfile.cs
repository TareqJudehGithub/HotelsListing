using AutoMapper;

using HotelListingAPI.Data;
using HotelListingAPI.DTOs.Country;

namespace HotelListingAPI.MappingProfiles;

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
        CreateMap<CreateCountryDto, Country>().ReverseMap();
        CreateMap<UpdateCountryDto, Country>().ReverseMap();
    }
}
