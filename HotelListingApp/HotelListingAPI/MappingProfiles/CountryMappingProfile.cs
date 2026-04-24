using AutoMapper;

using HotelListingAPI.Data;
using HotelListingAPI.DTOs;
using HotelListingAPI.DTOs.Country;

namespace HotelListingAPI.MappingProfiles;

public class CountryMappingProfile : Profile
{
    public CountryMappingProfile()
    {
        CreateMap<Country, GetCountriesDto>();
        CreateMap<Country, GetCountryDto>();
        CreateMap<CreateCountryDto, Country>().ReverseMap();
        CreateMap<UpdateCountryDto, Country>().ReverseMap();
    }
}
