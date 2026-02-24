using ELProject.Domain.Models;
using ELProject.Shared;
using ELProject.Shared.DTOs;
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
        private readonly IFileStorageService fileStorageService;

        public AuthController(UserManager<ApplicationUser> _userManager, 
            IConfiguration _config, 
            IFileStorageService _fileStorageService)
        {
            userManager = _userManager; // To access User and Role Tables in Db
            config = _config; // To access appsettings.json
            fileStorageService = _fileStorageService;
            // To save profile image file on server (or cloud) and return its path to store in DB
        }

        [HttpPost("Register")] // api/Account/Register
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


        [HttpPost("Login")] // api/Account/Login
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
                        //--------------Generate Token (JWT)---------------
                        // Add Claims										
                        List<Claim> UserClaims = new List<Claim>();
                        UserClaims.Add(new Claim(ClaimTypes.NameIdentifier, userFromDb.Id));
                        UserClaims.Add(new Claim(ClaimTypes.Name, userFromDb.UserName));

                        IList<string> UserRoles = await userManager.GetRolesAsync(userFromDb);
                        foreach (var roleName in UserRoles)
                            UserClaims.Add(new Claim(ClaimTypes.Role, roleName));

                        UserClaims.Add(new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()));

                        // Generate Key for Signature														
                        SymmetricSecurityKey key
                            = new(Encoding.UTF8.GetBytes(config["JWT:Key"]));

                        SigningCredentials signCred
                            = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

                        // Design Token
                        JwtSecurityToken myToken = new JwtSecurityToken(
                            issuer: config["JWT:Issuer"],
                            audience: config["JWT:Audience"],
                            expires: DateTime.Now.AddHours(1),
                            claims: UserClaims,
                            signingCredentials: signCred
                        );

                        // Generate Token and return it(as a string) in response
                        return Ok(new
                        {
                            token = new JwtSecurityTokenHandler().WriteToken(myToken), // Token returned as a string
                            expiration = DateTime.Now.AddHours(1) // or myToken.ValidTo
                        });

                    }
                }
                ModelState.AddModelError("Username", "Username or Password Invalid.");
            }
            return BadRequest(ModelState);
        }


    }
}
