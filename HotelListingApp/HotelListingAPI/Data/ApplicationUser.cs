using Microsoft.AspNetCore.Identity;

namespace HotelListingAPI.Data;

public class ApplicationUser : IdentityUser
{
    #region Properties
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    #endregion
}
