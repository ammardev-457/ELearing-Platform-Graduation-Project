using ELProject.Domain.Models;
using ELProject.Shared;
using ELProject.Shared.DTOs;
using Google.Apis.Auth;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace ELProject.DataAccess.Repositories.Repos
{
    public class AuthRepository
    {
        private readonly UserManager<ApplicationUser> userManager;
        private readonly IConfiguration _config;
        private readonly IFileStorageService fileStorageService;

        public AuthRepository(UserManager<ApplicationUser> _userManager,
            IConfiguration config,
            IFileStorageService _fileStorageService)
        {
            userManager = _userManager;
            _config = config;
            fileStorageService = _fileStorageService;
        }

        public async Task<IdentityResult> RegisterAsync(RegisterDto UserDto)
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
            return result;
        }

        public async Task<ApplicationUser> LoginAsync(LoginDto dto)
        {
            // Check of account is exists
            ApplicationUser userFromDb = await userManager.FindByNameAsync(dto.Username);

            if (userFromDb != null)
                await userManager.CheckPasswordAsync(userFromDb, dto.Password);

            return userFromDb;
        }

        public async Task<ApplicationUser> ExternalLoginAsync(ExternalLoginDto model)
        {
            var payload = await GoogleJsonWebSignature.ValidateAsync(model.IdToken);

            var email = payload.Email;

            var user = await userManager.FindByEmailAsync(email);

            if (user == null) // If the user register for the first time
            {
                user = new ApplicationUser
                {
                    UserName = email.Substring(0, email.IndexOf('@')), // To get the first letters before '@'
                    Email = email,
                    EmailConfirmed = true
                };

                await userManager.CreateAsync(user);
                await userManager.AddToRoleAsync(user, model.Role.ToString());
            }

            return user;
        }

        public async Task<IdentityResult> ChangePasswordAsync(ClaimsPrincipal userFromClaims, ChangePasswordDto dto)
        {
            var user = await userManager.GetUserAsync(userFromClaims);

            var result = await userManager.ChangePasswordAsync(user, dto.CurrentPassword, dto.NewPassword);

            return result;
        }

        public List<IdentityError> GetIdentityErrors(IEnumerable<IdentityError> Errors)
        {
            List<IdentityError> ListOfErrors = new();
            foreach (var error in Errors)
            {
                IdentityError ie = new();
                ie.Code = error.Code;
                ie.Description = error.Description;

                ListOfErrors.Add(ie);
            }
            return ListOfErrors;
        }

        public async Task<string> GetTokenAsync(ApplicationUser user)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim(ClaimTypes.Name, user.UserName)
            };

            var UserRoles = await userManager.GetRolesAsync(user);
            foreach (var role in UserRoles)
                claims.Add(new Claim(ClaimTypes.Role, role));

            var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(_config["JWT:Key"]!));

            var creds = new SigningCredentials(
                key,
                SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _config["JWT:Issuer"],
                audience: _config["JWT:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddHours(1),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
