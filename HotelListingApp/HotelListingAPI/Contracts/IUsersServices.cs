using HotelListingAPI.DTOs.Auth;
using HotelListingAPI.Results;

namespace HotelListingAPI.Contracts;

public interface IUsersServices
{
    Task<Result<RegisteredUserDto>> RegisterAsync(RegisterUserDto registerUserDto);
    Task<Result<string>> LoginAsync(LoginUserDto loginUserDto);
    Task<Result<string>> DeleteAsync(DeleteUserDto deleteUserDto);
    Task<Result<string>> LogoutAsync(LoginUserDto loginUserDto);

}
