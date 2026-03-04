using ELProject.DataAccess.Repositories.Repos;
using ELProject.Domain.Models;
using ELProject.Shared;
using ELProject.Shared.DTOs;
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
            authRepo = _authRepo; // To Access GetTokenAsync Method
            // To save profile image file on server (or cloud) and return its path to store in DB
        }

        [HttpPost("Register")] // api/Auth/Register
        public async Task<IActionResult> Register([FromForm]RegisterDto UserDto)
        {
            var result = await authRepo.RegisterAsync(UserDto);

            if (result.Succeeded)
                return Ok("Created.");

            List<IdentityError> ListOfErrors = new();
            foreach (var error in result.Errors)
            {
                IdentityError ie = new();
                ie.Code = error.Code;
                ie.Description = error.Description;

                ListOfErrors.Add(ie);
            }

            return BadRequest(ListOfErrors);
        }


        [HttpPost("Login")] // api/Auth/Login
        public async Task<IActionResult> Login(LoginDto userFromRequest)
        {
            ApplicationUser user = await authRepo.LoginAsync(userFromRequest);

            if (user == null)
                return BadRequest("Username or Password Invalid.");
            

            // Generate Token
            var MyToken = await authRepo.GetTokenAsync(user);

            return Ok(new { token = MyToken });
        }


        [HttpPost("External-Login")]
        public async Task<IActionResult> ExternalLogin(ExternalLoginDto model)
        {
            var user = await authRepo.ExternalLoginAsync(model);

            var token = await authRepo.GetTokenAsync(user);

            return Ok(new { token });
        }


    }
}
