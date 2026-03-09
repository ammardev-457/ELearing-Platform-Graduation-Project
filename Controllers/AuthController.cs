using ELProject.DataAccess.Repositories.Repos;
using ELProject.Domain.Models;
using ELProject.Shared;
using ELProject.Shared.DTOs.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
namespace ELProject.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly AuthRepository authRepo;

        public AuthController(AuthRepository _authRepo)
        {
            authRepo = _authRepo;
        }

        [HttpPost("Register")] // api/Auth/Register
        public async Task<IActionResult> RegisterAsync([FromForm]RegisterDto UserDto)
        {
            var result = await authRepo.RegisterAsync(UserDto);

            if (result.Succeeded)
                return Ok(new { message = "User created successfully" });

            return BadRequest(new { errors = result.Errors });
        }


        [HttpPost("Login")] // api/Auth/Login
        public async Task<IActionResult> LoginAsync(LoginDto userFromRequest)
        {
            ApplicationUser user = await authRepo.LoginAsync(userFromRequest);

            if (user == null)
                return BadRequest("Username or Password Invalid.");
            

            // Generate Token
            var MyToken = await authRepo.GetTokenAsync(user);

            return Ok(new { token = MyToken });
        }


        [HttpPost("External-Login")]
        public async Task<IActionResult> ExternalLoginAsync(ExternalLoginDto model)
        {
            var user = await authRepo.ExternalLoginAsync(model);

            var token = await authRepo.GetTokenAsync(user);

            return Ok(new { token });
        }


        [Authorize]
        [HttpPost("change-password")]
        public async Task<IActionResult> ChangePasswordAsync(ChangePasswordDto dto)
        {
            var result = await authRepo.ChangePasswordAsync(User, dto);

            if (result.Succeeded)
                return Ok(new { message = "User created successfully" });

            return BadRequest(new { errors = result.Errors });
        }
    }
}