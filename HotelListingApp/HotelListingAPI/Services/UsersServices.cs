using HotelListingAPI.Constants;
using HotelListingAPI.Contracts;
using HotelListingAPI.Data;
using HotelListingAPI.DTOs.Auth;
using HotelListingAPI.Results;
using Microsoft.AspNetCore.Identity;

namespace HotelListingAPI.Services;

public class UsersServices : IUsersServices
{
    #region Fields
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    #endregion

    #region Constructors
    public UsersServices(
        UserManager<ApplicationUser> userManager,
          SignInManager<ApplicationUser> signInManager
        )
    {
        _userManager = userManager;
        _signInManager = signInManager;
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

        // return success result
        return Result<string>.Success($"User {user.Email} logged in successfully!");

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
    #endregion
}
