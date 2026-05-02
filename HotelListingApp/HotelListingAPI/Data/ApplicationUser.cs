using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations.Schema;

namespace HotelListingAPI.Data;

public class ApplicationUser : IdentityUser
{
    #region Properties
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    [NotMapped]
    public string FullName => $"{LastName}, {FirstName}";
    #endregion
}
