using Azure.Core;
using ELProject.DataAccess.Results;
using ELProject.Domain.Enums;
using ELProject.Domain.Models;
using ELProject.ExternalServices;
using ELProject.Shared.DTOs.Auth;
using Google.Apis.Auth;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace ELProject.DataAccess.Repositories.Repos
{
    public class AuthRepository
    {
        private readonly UserManager<ApplicationUser> userManager;
        private readonly IConfiguration _config;

        public AuthRepository(UserManager<ApplicationUser> _userManager, IConfiguration config)
        {
            userManager = _userManager;
            _config = config;
        }

        public async Task<AuthResult> RegisterAsync(RegisterDto UserDto)
        {
            var email = UserDto.Email.Trim().ToLower();

            if (await userManager.FindByEmailAsync(email) is not null)
                return new AuthResult { Message = "Email is already exists!" };

            if (!CheckRole(UserDto.Role))
                return new AuthResult { IsAuthenticated = false, Message = "Invalid role" };

            ApplicationUser user = new()
            {
                Name = UserDto.Name,
                UserName = $"{UserDto.Email.Split('@')[0]}-{Guid.NewGuid()}",
                Email = UserDto.Email,
                Gender = UserDto.Gender,
                Bio = UserDto.Bio,
                EmailConfirmed = true
            };

            // Save in DB
            var result = await userManager.CreateAsync(user, UserDto.Password);

            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                return new AuthResult { Message = errors };
            }

            var roleResult = await userManager.AddToRoleAsync(user, UserDto.Role.ToString());

            if (!roleResult.Succeeded)
            {
                await userManager.DeleteAsync(user); // rollback, because there is no user without role
                var errors = string.Join(", ", roleResult.Errors.Select(e => e.Description));
                return new AuthResult { Message = errors };
            }

            var accessToken = await GetTokenAsync(user);

            var refreshToken = GenerateRefreshToken();
            user.RefreshTokens?.Add(refreshToken);
            await userManager.UpdateAsync(user);

            var roles = await userManager.GetRolesAsync(user);

            return new AuthResult
            {
                IsAuthenticated = true,
                Name = user.Name,
                Roles = roles.ToList(),
                Token = new JwtSecurityTokenHandler().WriteToken(accessToken),
                ExpiresAt = accessToken.ValidTo,
                RefreshToken = refreshToken.Token,
                RefreshTokenExpiration = refreshToken.ExpiresAt
            };
        }

        public async Task<AuthResult> LoginAsync(LoginDto dto)
        {
            var authResult = new AuthResult();

            var email = dto.Email.Trim().ToLower();

            // Check if account exists
            ApplicationUser userFromDb = await userManager.FindByEmailAsync(email);

            if (userFromDb is null || !await userManager.CheckPasswordAsync(userFromDb, dto.Password))
            {
                authResult.Message = "Email or Password is incorrect!";
                return authResult;
            }

            var accessToken = await GetTokenAsync(userFromDb);

            var rolesList = await userManager.GetRolesAsync(userFromDb);

            authResult.IsAuthenticated = true;
            authResult.Email = userFromDb.Email;
            authResult.Name = userFromDb.Name;
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
            var authResult = new AuthResult();

            var user = await userManager.Users.SingleOrDefaultAsync(u =>
            u.RefreshTokens.Any(t => t.Token == token));
            // token بتاعه refreshToken اللي ال user يعني عايز ال

            if (user == null)
            {
                authResult.Message = "Invalid token";
                return authResult;
            }

            // Generate a new JWT token because the old one is expired
            var jwtToken = await GetTokenAsync(user);
            authResult.Token = new JwtSecurityTokenHandler().WriteToken(jwtToken);
            authResult.ExpiresAt = jwtToken.ValidTo;

            // Check if the refresh token is expired or not
            var refreshToken = user.RefreshTokens.Single(t => t.Token == token);

            if (!refreshToken.IsActive)
            {
                authResult.Message = "Inactive token";
                return authResult;
            }

            var remainingTime = refreshToken.ExpiresAt - DateTime.UtcNow;

            if (remainingTime <= TimeSpan.FromDays(1))
            {
                // if remainingTime less than 1 day, revoke refresh token
                refreshToken.RevokedAt = DateTime.UtcNow;

                // create a new one
                var newRefreshToken = GenerateRefreshToken();
                user.RefreshTokens.Add(newRefreshToken);

                authResult.RefreshToken = newRefreshToken.Token;
                authResult.RefreshTokenExpiration = newRefreshToken.ExpiresAt;
            }
            else
            {
                authResult.RefreshToken = refreshToken.Token;
                authResult.RefreshTokenExpiration = refreshToken.ExpiresAt;
            }

            await userManager.UpdateAsync(user);

            authResult.IsAuthenticated = true;
            authResult.Email = user.Email;
            authResult.Name = user.Name;

            var roles = await userManager.GetRolesAsync(user);
            authResult.Roles = roles.ToList();

            return authResult;
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

        public async Task<AuthResult> ExternalLoginAsync(ExternalLoginDto model)
        {
            var settings = new GoogleJsonWebSignature.ValidationSettings()
            {
                Audience = new List<string> { _config["GoogleAuth:ClientId"]! }
            };

            GoogleJsonWebSignature.Payload payload;

            try
            {
                payload = await GoogleJsonWebSignature.ValidateAsync(model.IdToken, settings);
            }
            catch
            {
                return new AuthResult { IsAuthenticated = false, Message = "Invalid Google token" };
            }

            var email = payload.Email;

            var user = await userManager.FindByEmailAsync(email);

            if (user == null) // If the user register for the first time
            {
                user = new ApplicationUser
                {
                    Name = payload.Name,
                    UserName = $"{email.Split('@')[0]}-{Guid.NewGuid()}",
                    Email = email,
                    EmailConfirmed = true
                };

                var result = await userManager.CreateAsync(user);

                if (!result.Succeeded)
                {
                    var errors = string.Empty;

                    foreach (var error in result.Errors)
                        errors += $"{error.Description},";

                    return new AuthResult { Message = errors };
                }

                if (!CheckRole(model.Role))
                    return new AuthResult { IsAuthenticated = false, Message = "Invalid role" };

                await userManager.AddToRoleAsync(user, model.Role.ToString());

                await userManager.AddLoginAsync(user,
                    new UserLoginInfo("Google", payload.Subject, "Google"));
            }
            else
            /// <summary>
            /// سيناريو مهم:
            /// email/ password سجل قبل كده بـ user
            /// بنفس الايميل Google Login دلوقتي بيعمل
            /// 
            {
                // Link Google login if not already linked
                var logins = await userManager.GetLoginsAsync(user);

                if (!logins.Any(l => l.LoginProvider == "Google"))
                {
                    await userManager.AddLoginAsync(user,
                        new UserLoginInfo("Google", payload.Subject, "Google"));
                }
            }

            var jwt = await GetTokenAsync(user);
            var refreshToken = GenerateRefreshToken();
            user.RefreshTokens.Add(refreshToken);
            await userManager.UpdateAsync(user);

            var roles = await userManager.GetRolesAsync(user);
            return new AuthResult
            {
                IsAuthenticated = true,
                Name = user.Name,
                Roles = roles.ToList(),
                Token = new JwtSecurityTokenHandler().WriteToken(jwt),
                ExpiresAt = jwt.ValidTo,
                RefreshToken = refreshToken.Token,
                RefreshTokenExpiration = refreshToken.ExpiresAt
            };
        }

        public async Task<AuthResult> ChangePasswordAsync(ClaimsPrincipal userFromClaims, ChangePasswordDto dto)
        {
            var user = await userManager.GetUserAsync(userFromClaims);

            if (user == null)
                return new AuthResult { Message = "User not found" };

            var result = await userManager.ChangePasswordAsync(user, dto.CurrentPassword, dto.NewPassword);

            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                return new AuthResult { Message = errors };
            }

            return new AuthResult { IsAuthenticated = true };
        }

        public async Task<JwtSecurityToken> GetTokenAsync(ApplicationUser user)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim(ClaimTypes.Name, user.Name)
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

            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomNumber);
            return new RefreshToken
            {
                Token = Convert.ToBase64String(randomNumber),
                CreatedAt = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.AddDays(7)
            };

        }

        private bool CheckRole(UserRole role)
        {
            return role == UserRole.Student || role == UserRole.Instructor;
        }
    }
}