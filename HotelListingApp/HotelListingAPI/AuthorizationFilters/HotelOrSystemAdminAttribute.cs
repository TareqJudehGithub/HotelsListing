using HotelListingAPI.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace HotelListingAPI.AuthorizationFilters;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false)]
public sealed class HotelOrSystemAdminAttribute : TypeFilterAttribute
{
    public HotelOrSystemAdminAttribute() : base(typeof(HotelOrSystemAdminFilter))
    {
    }
}
public class HotelOrSystemAdminFilter : IAsyncAuthorizationFilter
{
    private readonly HotelListingsDbContext _context;

    public HotelOrSystemAdminFilter(HotelListingsDbContext context)
    {
        _context = context;
    }

    public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
    {
        // This method confirms the request is authorized, and context provides information about the request coming in.

        // Get User from HttpContext
        var httpUser = context.HttpContext.User;

        // Check if user is authorized
        if (httpUser?.Identity?.IsAuthenticated == false)
        {
            context.Result = new UnauthorizedResult();
        }

        // Check if user is global admin 'Administrator' - then return
        if (httpUser!.IsInRole("Administrator"))
        {
            return;
        }

        // User Identity - if user is Hotel Admin
        var userId = httpUser
            .FindFirst(JwtRegisteredClaimNames.Sub)?
            .Value
            ??
            httpUser
            .FindFirst(ClaimTypes.NameIdentifier)?
            .Value;

        if (string.IsNullOrWhiteSpace(userId))
        {
            context.Result = new ForbidResult();
        }

        // Try and get hotelId from route values, and output the result to hotelIdObj
        context.RouteData.Values.TryGetValue(key: "hotelId", value: out var hotelIdObj);

        int.TryParse(hotelIdObj?.ToString(), out int hotelId);

        // Check if parsing failed
        if (hotelId == 0)
        {
            context.Result = new ForbidResult();
            return;
        }

        // Check if user is a hotel admin for this specific hotel
        var isHotelAdmin = await _context.HotelAdmins
            .AnyAsync(h => h.UserId == userId && h.HotelId == hotelId);

        if (!isHotelAdmin)
        {
            context.Result = new ForbidResult();
            return;
        }
    }
}
