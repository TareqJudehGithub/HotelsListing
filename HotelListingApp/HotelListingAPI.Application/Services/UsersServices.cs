// Ignore Spelling: Dto

using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Options;
using HotelListingAPI.Domain;
using HotelListingAPI.Common.Constants;
using HotelListingAPI.Common.Results;
using Microsoft.AspNetCore.Http;
using HotelListingAPI.Application.Contracts;
using HotelListingAPI.Application.DTOs.Auth;
using HotelListingAPI.Common.Models.Config;
using Microsoft.Extensions.Logging;

namespace HotelListingAPI.Application.Services;

public class UsersServices : IUsersServices
{
    #region Fields
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly IOptions<JwtSettings> _jwtOptions;
    private readonly IHttpContextAccessor _contextAccessor;
    private readonly HotelListingsDbContext _dbContext;
    private readonly ILogger<UsersServices> _logger;

    //  string IUsersServices.UserId => throw new NotImplementedException();
    #endregion

    #region Constructors
    public UsersServices(
        UserManager<ApplicationUser> userManager,
          SignInManager<ApplicationUser> signInManager,
       IOptions<JwtSettings> jwtOptions,
        IHttpContextAccessor httpContextAccessor,
        HotelListingsDbContext dbContext,
        ILogger<UsersServices> logger
        )
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _jwtOptions = jwtOptions;
        _contextAccessor = httpContextAccessor;
        _dbContext = dbContext;
        _logger = logger;
    }
    #endregion

    #region Methods
    public async Task<Result<RegisteredUserDto>> RegisterAsync(RegisterUserDto registerUserDto)
    {
        // Create a new ApplicationUser
        var user = new ApplicationUser()
        {
            Email = registerUserDto.Email,
            UserName = registerUserDto.Email,
            FirstName = registerUserDto.FirstName,
            LastName = registerUserDto.LastName
        };

        // Add new created user using UserManager
        var result = await _userManager.CreateAsync(user: user, password: registerUserDto.Password);

        // Check result outcome
        if (!result.Succeeded)
        {
            var errors = result.Errors
                .Select(error => new Error(Code: ErrorCodes.BadRequest, Description: error.Description))
                .ToArray();

            // Logging errors
            _logger.LogError(message: $"User registration failed for Email: {registerUserDto.Email} with errors: {string.Join(", ", errors)}");

            return Result<RegisteredUserDto>.BadRequest(errors: errors);
        }

        // Assign a role to new user
        await _userManager.AddToRoleAsync(user: user, role: registerUserDto.Role);


        // If user is a Hotel Admin, add to HotelAdmins table
        if (registerUserDto.Role.Equals(RoleNames.HotelAdmin, StringComparison.OrdinalIgnoreCase))
        {
            var hotelAdmin = _dbContext.HotelAdmins.Add(
                new HotelAdmin
                {
                    UserId = user.Id,
                    HotelId = registerUserDto.AssociatedHotelId.GetValueOrDefault()
                });
            await _dbContext.SaveChangesAsync();
        }


        // map
        var registeredUser = new RegisteredUserDto()
        {
            Id = user.Id,
            Email = user.Email,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Role = registerUserDto.Role
        };

        // return result
        return Result<RegisteredUserDto>.Success(registeredUser);
    }

    public async Task<Result<string>> LoginAsync(LoginUserDto loginUserDto)
    {
        // Get logged in user
        var user = await _userManager.FindByEmailAsync(loginUserDto.Email);

        // Check for null
        if (user is null)
        {
            _logger.LogWarning(message: $"Failed login attempt for email: {loginUserDto.Email}");

            return Result<string>
                .NotFound(errors: new Error(Code: ErrorCodes.NotFound, Description: "User not found"));
        }

        // Check credentials
        var valid = await _userManager.CheckPasswordAsync(user: user, password: loginUserDto.Password);

        if (!valid)
        {
            return Result<string>
                .BadRequest(errors: new Error(Code: ErrorCodes.BadRequest, Description: "Invalid username or password"));
        }

        // Issue a token
        var token = await GenerateToken(user: user);

        // return success result
        return Result<string>.Success(token);

    }
    public async Task<Result<string>> LogoutAsync()
    {
        await _signInManager.SignOutAsync();
        return Result<string>.Success("User logged out successfully.");
    }

    public async Task<Result<string>> DeleteAsync(DeleteUserDto deleteUserDto)
    {
        var user = await _userManager.FindByEmailAsync(deleteUserDto.Email);
        if (user == null)
        {
            return Result<string>
                .NotFound(errors: new Error(Code: ErrorCodes.NotFound, Description: $"Invalid username."));
        }
        await _userManager.DeleteAsync(user);

        return Result<string>.Success($"Username: {user.UserName} was as successfully deleted.");
    }

    // JWT
    private async Task<string> GenerateToken(ApplicationUser user)
    {
        // Set basic user claims
        var claims = new List<Claim>
        {
            new (type: JwtRegisteredClaimNames.Sub, value: user.Id),
            new (type: JwtRegisteredClaimNames.Email, value: user.Email),
            new (type: JwtRegisteredClaimNames.Jti, value: Guid.NewGuid().ToString()),
            new(type: JwtRegisteredClaimNames.Name, value: $"{user.FullName}")
        };
        // Set user Role claims
        var roles = await _userManager.GetRolesAsync(user: user);

        // Convert user roles into claims list
        var roleClaims = roles.Select(q => new Claim(type: ClaimTypes.Role, value: q)).ToList();

        claims = claims.Union(roleClaims).ToList();

        // Set JWT key credentials
        // retrieve JWT security key
        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtOptions.Value.Key));
        // Hash the new securityKey with HmacSha256
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        // Create an encoded token
        var token = new JwtSecurityToken(
            issuer: _jwtOptions.Value.Issuer,
            audience: _jwtOptions.Value.Audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(Convert.ToInt32(_jwtOptions.Value.DurationInMinutes)),
            signingCredentials: credentials
            );

        // Return token value
        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    // IHttpContextAccessor
    public string UserId => _contextAccessor?
            .HttpContext?
            .User?
            .FindFirst(JwtRegisteredClaimNames.Sub)?.Value
        ?? _contextAccessor?
            .HttpContext?
            .User?
            .FindFirst(ClaimTypes.NameIdentifier)?.Value
        ?? string.Empty;
    #endregion
}
