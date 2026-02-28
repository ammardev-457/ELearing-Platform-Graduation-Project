using ELProject.DataAccess.Repositories.Repos;
using ELProject.Domain.Models;
using ELProject.Shared;
using ELProject.Shared.DTOs;
using Google.Apis.Auth;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.Diagnostics.Metrics;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace ELProject.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> userManager;
        private readonly IConfiguration config;
        private readonly AuthRepository authRepo;

        public AuthController(UserManager<ApplicationUser> _userManager, 
            IConfiguration _config,
            AuthRepository _authRepo)
        {
            userManager = _userManager; // To access User and Role Tables in Db
            config = _config; // To access appsettings.json
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
            var payload = await GoogleJsonWebSignature.ValidateAsync(model.IdToken);

            var email = payload.Email;

            var user = await userManager.FindByEmailAsync(email);

            if (user == null)
            {
                user = new ApplicationUser
                {
                    Email = email,
                    EmailConfirmed = true
                };

                await userManager.CreateAsync(user);
                await userManager.AddToRoleAsync(user, model.Role.ToString());
            }

            var token = await authRepo.GetTokenAsync(user);

            return Ok(new { token });
        }


    }
}
