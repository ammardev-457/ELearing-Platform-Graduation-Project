using Azure.Core;
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

        [HttpPost("register")] // api/Auth/register
        public async Task<IActionResult> RegisterAsync(RegisterDto UserDto)
        {
            var result = await authRepo.RegisterAsync(UserDto);

            if (!result.IsAuthenticated)
                return Unauthorized(result.Message);

            SetRefreshTokenInCookie(result.RefreshToken, result.RefreshTokenExpiration);

            return Ok(new
            {
                token = result.Token,
                expiresAt = result.ExpiresAt,
                email = result.Email,
                name = result.Name,
                roles = result.Roles
            });
        }


        [HttpPost("login")] // api/Auth/login
        public async Task<IActionResult> LoginAsync(LoginDto userFromRequest)
        {
            var authResult = await authRepo.LoginAsync(userFromRequest);

            if (!authResult.IsAuthenticated)
                return Unauthorized(authResult.Message);

            SetRefreshTokenInCookie(authResult.RefreshToken!, authResult.RefreshTokenExpiration);

            return Ok(new
            {
                token = authResult.Token,
                expiresAt = authResult.ExpiresAt,
                email = authResult.Email,
                name = authResult.Name,
                roles = authResult.Roles
            });
        }


        // When access token expires, the client send request with refresh token that is stored in cookie.
        [HttpPost("refresh-token")]
        public async Task<IActionResult> RefreshToken()
        {
            var refreshToken = Request.Cookies["refreshToken"];

            if (string.IsNullOrEmpty(refreshToken))
                return Unauthorized("No refresh token found");

            var result = await authRepo.RefreshTokenAsync(refreshToken);

            if (!result.IsAuthenticated)
                return Unauthorized(result.Message);

            SetRefreshTokenInCookie(result.RefreshToken, result.RefreshTokenExpiration);

            return Ok(new
            {
                token = result.Token,
                expiresAt = result.ExpiresAt,
                email = result.Email,
                name = result.Name,
                roles = result.Roles
            });
        }


        [Authorize]
        [HttpPost("logout")]
        public async Task<IActionResult> LogoutAsync()
        {
            var token = Request.Cookies["refreshToken"];

            if (string.IsNullOrEmpty(token))
                return Unauthorized("No token found");

            var result = await authRepo.RevokeTokenAsync(token);

            if (!result)
                return Unauthorized("Invalid or inactive token");

            Response.Cookies.Delete("refreshToken");

            return Ok();
        }


        [HttpPost("external-login")]
        public async Task<IActionResult> ExternalLoginAsync(ExternalLoginDto model)
        {
            var result = await authRepo.ExternalLoginAsync(model);

            if (!result.IsAuthenticated)
                return Unauthorized(result.Message);

            SetRefreshTokenInCookie(result.RefreshToken!, result.RefreshTokenExpiration);

            return Ok(new
            {
                token = result.Token,
                expiresAt = result.ExpiresAt,
                name = result.Name,
                roles = result.Roles
            });
        }


        [Authorize]
        [HttpPost("change-password")]
        public async Task<IActionResult> ChangePasswordAsync(ChangePasswordDto dto)
        {
            var result = await authRepo.ChangePasswordAsync(User, dto);

            if (result.Message is not null)
                return Unauthorized(result.Message);

            return Ok("Password changed successfully");
        }

        private void SetRefreshTokenInCookie(string refreshToken, DateTime refreshTokenExpiration)
        {
            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict,
                Expires = refreshTokenExpiration
            };

            Response.Cookies.Append("refreshToken", refreshToken, cookieOptions);
        }
    }
}