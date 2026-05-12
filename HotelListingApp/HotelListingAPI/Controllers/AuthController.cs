using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using HotelListingAPI.Domain;
using HotelListingAPI.Application.Contracts;
using HotelListingAPI.Application.DTOs.Auth;

namespace HotelListingAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    // [AllowAnonymous]
    public class AuthController : BaseApiController
    {

        #region fields
        //private readonly UserManager<ApplicationUser> _userManager;
        private readonly IUsersServices _usersServices;
        private readonly SignInManager<ApplicationUser> _signInManager;
        #endregion

        #region Constructors
        public AuthController(
          //  UserManager<ApplicationUser> userManager,
          IUsersServices usersServices,
            SignInManager<ApplicationUser> signInManager
            )
        {
            // _userManager = userManager;
            _usersServices = usersServices;
            _signInManager = signInManager;
        }
        #endregion
        #region MyRegion

        #endregion

        // POST:  "api/auth/register"
        [HttpPost("register")]
        [AllowAnonymous]
        // Or
        //[HttpPost]
        //[Route("register")]
        public async Task<ActionResult<RegisteredUserDto>> Register([FromBody] RegisterUserDto registerUserDto)
        {
            var result = await _usersServices.RegisterAsync(registerUserDto);
            return ToActionResult(result);
        }

        // GET:  "api/auth/login"
        [HttpGet]
        [Route("login")]
        [AllowAnonymous]
        public async Task<ActionResult<string>> Login([FromBody] LoginUserDto loginUserDto)
        {
            var result = await _usersServices.LoginAsync(loginUserDto);
            return ToActionResult(result);
        }

        // Delete: "api/auth/delete"
        [HttpDelete]
        [Route("delete")]
        [Authorize(Roles = "Administrator")]
        public async Task<ActionResult<string>> Delete([FromBody] DeleteUserDto deleteUseDto)
        {
            var result = await _usersServices.DeleteAsync(deleteUseDto);
            return ToActionResult(result);
        }

        // GET: "api/auth/logout"
        [HttpGet]
        [Route("logout")]
        [AllowAnonymous]
        public async Task<ActionResult<string>> Logout()
        {
            var result = await _usersServices.LogoutAsync();
            return ToActionResult(result);
        }

    }
}

