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
        private readonly IFileStorageService fileStorageService;

        public AuthController(UserManager<ApplicationUser> _userManager, 
            IConfiguration _config,
            AuthRepository _authRepo,
            IFileStorageService _fileStorageService)
        {
            userManager = _userManager; // To access User and Role Tables in Db
            config = _config; // To access appsettings.json
            authRepo = _authRepo; // To Access GetTokenAsync Method
            fileStorageService = _fileStorageService;
            // To save profile image file on server (or cloud) and return its path to store in DB
        }

        [HttpPost("Register")] // api/Auth/Register
        public async Task<IActionResult> Register([FromForm]RegisterDto UserDto)
        {
            if (ModelState.IsValid)
            {
                ApplicationUser user = new();
                user.UserName = UserDto.Username;
                user.Gender = UserDto.Gender;

                if (UserDto.ProfileImageFile != null)
                    ///<summary>
                    /// The Storing of ProfileImage now is completely generic.
                    /// - Maybe today you save ProfileImage in wwwroot
                    /// - Tomorrow in Azure Blob
                    /// - After that in AWS S3
                    ///</summary>
                    user.ProfileImage = await fileStorageService.SaveFileAsync(UserDto.ProfileImageFile);

                // Assign role to user
                await userManager.AddToRoleAsync(user, UserDto.Role.ToString());

                // Save in DB
                IdentityResult result = await userManager.CreateAsync(user, UserDto.Password);

                if (result.Succeeded)
                    return Ok("Created.");

                foreach (var error in result.Errors)
                    ModelState.AddModelError(error.Code, error.Description);

            }
            return BadRequest(ModelState);
        }


        [HttpPost("Login")] // api/Auth/Login
        public async Task<IActionResult> Login(LoginDto userFromRequest)
        {
            if (ModelState.IsValid)
            {
                // Check of account is exists
                ApplicationUser userFromDb = await userManager.FindByNameAsync(userFromRequest.Username);
                if (userFromDb != null)
                {
                    // Check password
                    bool isValid = await userManager.CheckPasswordAsync(userFromDb, userFromRequest.Password);

                    if (isValid)
                    {
                        // Generate Token
                        var MyToken = await authRepo.GetTokenAsync(userFromDb);

                        return Ok(new { token = MyToken });
                    }
                }
                ModelState.AddModelError("Username", "Username or Password Invalid.");
            }
            return BadRequest(ModelState);
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
