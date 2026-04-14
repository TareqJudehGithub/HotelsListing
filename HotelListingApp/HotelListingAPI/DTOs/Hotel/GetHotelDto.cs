// Ignore Spelling: Dto

using System.ComponentModel.DataAnnotations;

namespace HotelListingAPI.DTOs.Hotel;

public record GetHotelDto(
    int Id,
    [Display(Name = "Hotel Name")]
    string Name,
    string Address,
    double Rating,
    string Country
    );

