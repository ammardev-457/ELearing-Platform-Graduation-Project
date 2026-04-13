using ELProject.DataAccess.Results;
using ELProject.Domain.Models;
using ELProject.ExternalServices;
using ELProject.Shared.DTOs.Auth;
using Google.Apis.Auth;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
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

        public async Task<AuthResult> RegisterAsync(RegisterDto UserDto)
        {
            if (await userManager.FindByNameAsync(UserDto.Username) is not null)
                return new AuthResult { Message = "Username is already registered!" };

            ApplicationUser user = new();
            user.UserName = UserDto.Username;
            user.Email = UserDto.Email;
            user.Gender = UserDto.Gender;
            user.Bio = UserDto.Bio;

            if (UserDto.ProfileImageFile != null)
                ///<summary>
                /// The Storing of ProfileImage now is completely generic.
                /// - Maybe today you save ProfileImage in wwwroot
                /// - Tomorrow in Azure Blob
                /// - After that in AWS S3
                ///</summary>
                user.PathOfProfileImageInDb = await fileStorageService.SaveImageAsync(UserDto.ProfileImageFile);

            // Save in DB
            var result = await userManager.CreateAsync(user, UserDto.Password);

            if (!result.Succeeded)
            {
                var errors = string.Empty;

                foreach (var error in result.Errors)
                    errors += $"{error.Description},";

                return new AuthResult { Message = errors };
            }

            var roleResult = await userManager.AddToRoleAsync(user, UserDto.Role.ToString());

            if (!roleResult.Succeeded)
            {
                var errors = string.Empty;

                foreach (var error in result.Errors)
                    errors += $"{error.Description},";

                return new AuthResult { Message = errors };
            }

            var accessToken = await GetTokenAsync(user);

            var refreshToken = GenerateRefreshToken();
            user.RefreshTokens?.Add(refreshToken);
            await userManager.UpdateAsync(user);

            return new AuthResult
            {
                IsAuthenticated = true,
                Username = user.UserName,
                Roles = UserDto.Role.ToString().Split(',').ToList(),
                Token = new JwtSecurityTokenHandler().WriteToken(accessToken),
                ExpiresAt = accessToken.ValidTo,
                RefreshToken = refreshToken.Token,
                RefreshTokenExpiration = refreshToken.ExpiresAt
            };
        }

        public async Task<AuthResult> LoginAsync(LoginDto dto)
        {
            var authResult = new AuthResult();

            // Check of account is exists
            ApplicationUser userFromDb = await userManager.FindByEmailAsync(dto.Email);

            if (userFromDb is null || !await userManager.CheckPasswordAsync(userFromDb, dto.Password))
            {
                authResult.Message = "Email or Password is incorrect!";
                return authResult;
            }

            var accessToken = await GetTokenAsync(userFromDb);
            var refreshToken = GenerateRefreshToken();
            
            var rolesList = await userManager.GetRolesAsync(userFromDb);

            authResult.IsAuthenticated = true;
            authResult.Email = userFromDb.Email;
            authResult.Username = userFromDb.UserName;
            authResult.Roles = rolesList.ToList();
            authResult.Token = new JwtSecurityTokenHandler().WriteToken(accessToken);
            authResult.ExpiresAt = accessToken.ValidTo;

            var activeRefreshToken = userFromDb.RefreshTokens.FirstOrDefault(t => t.IsActive);

            if (activeRefreshToken is null)
            {
                activeRefreshToken = GenerateRefreshToken();
                userFromDb.RefreshTokens.Add(activeRefreshToken);
                await userManager.UpdateAsync(userFromDb);
            }

            authResult.RefreshToken = activeRefreshToken.Token;
            authResult.RefreshTokenExpiration = activeRefreshToken.ExpiresAt;

            return authResult;
        }

        public async Task<AuthResult> RefreshTokenAsync(string token)
        {
            var authModel = new AuthResult();

            var user = await userManager.Users.SingleOrDefaultAsync(u => u.RefreshTokens.Any(t => t.Token == token));
            // token بتاعه refreshToken اللي ال user يعني عايز ال

            if (user == null)
            {
                authModel.Message = "Invalid token";
                return authModel;
            }

            var refreshToken = user.RefreshTokens.Single(t => t.Token == token);

            if (!refreshToken.IsActive)
            {
                authModel.Message = "Inactive token";
                return authModel;
            }

            refreshToken.RevokedAt = DateTime.UtcNow;

            var newRefreshToken = GenerateRefreshToken();
            user.RefreshTokens.Add(newRefreshToken);
            await userManager.UpdateAsync(user);

            var jwtToken = await GetTokenAsync(user);

            authModel.IsAuthenticated = true;
            authModel.Token = new JwtSecurityTokenHandler().WriteToken(jwtToken);
            authModel.Email = user.Email;
            authModel.Username = user.UserName;
            var roles = await userManager.GetRolesAsync(user);
            authModel.Roles = roles.ToList();
            authModel.RefreshToken = newRefreshToken.Token;
            authModel.RefreshTokenExpiration = newRefreshToken.ExpiresAt;

            return authModel;
        }

        public async Task<bool> RevokeTokenAsync(string token)
        {
            var user = await userManager.Users.SingleOrDefaultAsync(u => u.RefreshTokens.Any(t => t.Token == token));

            if (user == null)
                return false;

            var refreshToken = user.RefreshTokens.Single(t => t.Token == token);

            if (!refreshToken.IsActive)
                return false;

            refreshToken.RevokedAt = DateTime.UtcNow;

            await userManager.UpdateAsync(user);

            return true;
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

        public async Task<AuthResult> ChangePasswordAsync(ClaimsPrincipal userFromClaims, ChangePasswordDto dto)
        {
            var user = await userManager.GetUserAsync(userFromClaims);

            var result = await userManager.ChangePasswordAsync(user, dto.CurrentPassword, dto.NewPassword);

            if (!result.Succeeded)
            {
                var errors = string.Empty;

                foreach (var error in result.Errors)
                    errors += $"{error.Description},";

                return new AuthResult { Message = errors };
            }

            return new AuthResult { IsAuthenticated = true };
        }

        public async Task<JwtSecurityToken> GetTokenAsync(ApplicationUser user)
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

            var jwtSecurityToken = new JwtSecurityToken(
                issuer: _config["JWT:Issuer"],
                audience: _config["JWT:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddHours(1),
                signingCredentials: creds
            );

            return jwtSecurityToken;
        }

        private RefreshToken GenerateRefreshToken()
        {
            var randomNumber = new byte[32];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(randomNumber);
                return new RefreshToken
                {
                    Token = Convert.ToBase64String(randomNumber),
                    CreatedAt = DateTime.UtcNow,
                    ExpiresAt = DateTime.UtcNow.AddDays(7)
                };
            }

        }
    }
}