using ELProject.Domain.Models;
using ELProject.Shared;
using ELProject.Shared.DTOs;
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

        public async Task<string> GetTokenAsync(ApplicationUser user)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim(ClaimTypes.Name, user.UserName!)
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
