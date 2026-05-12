using AutoMapper;
using HotelListingAPI.Application.DTOs.Booking;
using HotelListingAPI.Domain;

namespace HotelListingAPI.Application.MappingProfiles;

public class HotelBookingMappingProfile : Profile
{
    public HotelBookingMappingProfile()
    {
        CreateMap<Booking, GetBookingDto>()
            .ForMember(d => d.HotelName, opt => opt.MapFrom(s => s.Hotel!.Name))
            .ForMember(d => d.Status, opt => opt.MapFrom(s => s.Status.ToString()));

        CreateMap<CreateBookingDto, Booking>()
            .ForMember(d => d.Id, opt => opt.Ignore())
            .ForMember(d => d.UserId, opt => opt.Ignore())
            .ForMember(d => d.TotalPrice, opt => opt.Ignore())
            .ForMember(d => d.Status, opt => opt.Ignore())
            .ForMember(d => d.CreateAtUtc, opt => opt.Ignore())
            .ForMember(d => d.UpdateAtUtc, opt => opt.Ignore())
            .ForMember(d => d.Hotel, opt => opt.Ignore())
            .ReverseMap();

        CreateMap<UpdateBookingDto, Booking>()
            .ForMember(d => d.Id, opt => opt.Ignore())
            .ForMember(d => d.UserId, opt => opt.Ignore())
            .ForMember(d => d.TotalPrice, opt => opt.Ignore())
            .ForMember(d => d.Status, opt => opt.Ignore())
            .ForMember(d => d.CreateAtUtc, opt => opt.Ignore())
            .ForMember(d => d.UpdateAtUtc, opt => opt.Ignore())
            .ForMember(d => d.Hotel, opt => opt.Ignore())
            .ReverseMap();
    }
}