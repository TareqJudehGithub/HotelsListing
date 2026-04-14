// Ignore Spelling: Dto
using System.ComponentModel.DataAnnotations;

namespace HotelListingAPI.DTOs.Hotel;

public record GetHotelsDto(
    int Id,
    [Display(Name = "Hotel Name")]
    string Name,
    string Address,
    string Rating,
    [Display(Name = "Country Id")]
    int CountryId
    );

