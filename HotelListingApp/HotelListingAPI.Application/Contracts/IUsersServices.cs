using HotelListingAPI.Application.DTOs.Auth;
using HotelListingAPI.Common.Results;
using HotelListingAPI.Domain;

namespace HotelListingAPI.Application.Contracts;

public interface IUsersServices
{
    Task<Result<RegisteredUserDto>> RegisterAsync(RegisterUserDto registerUserDto);
    Task<Result<string>> LoginAsync(LoginUserDto loginUserDto);
    string UserId { get; }
    Task<Result<string>> DeleteAsync(DeleteUserDto deleteUserDto);
    Task<Result<string>> LogoutAsync();
}
