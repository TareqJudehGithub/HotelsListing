using System.ComponentModel.DataAnnotations;

namespace HotelListingAPI.DTOs.Hotel;

public record GetHotelsDto(
    int Id,
    string Name,
    string Address,
    double Rating,
    int CountryId
    );

