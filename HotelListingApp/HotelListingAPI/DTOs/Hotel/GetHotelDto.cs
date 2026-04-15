using System.ComponentModel.DataAnnotations;

namespace HotelListingAPI.DTOs.Hotel;

public record GetHotelDto(
    int Id,
    string Name,
    string Address,
    double Rating,
    string Country
    );

