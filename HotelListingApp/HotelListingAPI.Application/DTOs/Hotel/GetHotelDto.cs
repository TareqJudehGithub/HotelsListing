using System.ComponentModel.DataAnnotations;

namespace HotelListingAPI.Application.DTOs.Hotel;

public record GetHotelDto(
    int Id,
    string Name,
    string Address,
    double Rating,
    int CountryId,
    string CountryName
    );

