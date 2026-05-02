using HotelListingAPI.Constants;
using HotelListingAPI.Contracts;
using HotelListingAPI.Data;
using HotelListingAPI.DTOs.Auth;
using HotelListingAPI.Results;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace HotelListingAPI.Services;

public class UsersServices : IUsersServices
{
    #region Fields
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly IConfiguration _config;
    #endregion

    #region Constructors
    public UsersServices(
        UserManager<ApplicationUser> userManager,
          SignInManager<ApplicationUser> signInManager,
        IConfiguration iconfiguration
        )
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _config = iconfiguration;
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

            return Result<RegisteredUserDto>.BadRequest(errors: errors);
        }
        // map
        var registeredUser = new RegisteredUserDto()
        {
            Id = user.Id,
            Email = user.Email,
            FirstName = user.FirstName,
            LastName = user.LastName
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
        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["JwtSettings:Key"]));
        // Hash the new securityKey with HmacSha256
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        // Create an encoded token
        var token = new JwtSecurityToken(
            issuer: _config["JwtSettings:Issuer"],
            audience: _config["JwtSettings:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(Convert.ToInt32(_config["JwtSettings:DurationInMinutes"])),
            signingCredentials: credentials
            );

        // Return token value
        return new JwtSecurityTokenHandler().WriteToken(token);
    }


    #endregion
}
