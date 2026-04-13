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
        /// <summary>
        /// هنا؟ [FromForm] ليه بنستخدم 🧠 
        /// : لأن
        /// JSON مينفعش ييجي في IFormFile
        /// multipart/form-data يكون request لازم ال
        /// 
        public async Task<IActionResult> RegisterAsync([FromForm]RegisterDto UserDto)
        {
            var result = await authRepo.RegisterAsync(UserDto);

            if (!result.IsAuthenticated)
                return BadRequest(result.Message);

            SetRefreshTokenInCookie(result.RefreshToken, result.RefreshTokenExpiration);

            return Ok(result);
        }


        [HttpPost("login")] // api/Auth/login
        public async Task<IActionResult> LoginAsync(LoginDto userFromRequest)
        {
            var authResult = await authRepo.LoginAsync(userFromRequest);

            if (!authResult.IsAuthenticated)
                return BadRequest(authResult.Message);

            if (!string.IsNullOrEmpty(authResult.RefreshToken))
                SetRefreshTokenInCookie(authResult.RefreshToken, authResult.RefreshTokenExpiration);

            return Ok(authResult);
        }


        [HttpGet("refresh-token")]
        public async Task<IActionResult> RefreshToken()
        {
            var refreshToken = Request.Cookies["refreshToken"];

            var result = await authRepo.RefreshTokenAsync(refreshToken);

            if (!result.IsAuthenticated)
                return BadRequest(result);

            SetRefreshTokenInCookie(result.RefreshToken, result.RefreshTokenExpiration);

            return Ok(result);
        }

        [HttpPost("revoke-token")]
        public async Task<IActionResult> RevokeToken([FromBody] RevokeToken model)
        {
            var token = model.Token ?? Request.Cookies["refreshToken"];

            if (string.IsNullOrEmpty(token))
                return BadRequest("Token is required!");

            var result = await authRepo.RevokeTokenAsync(token);

            if (!result)
                return BadRequest("Token is invalid!");

            return Ok();
        }


        [HttpPost("external-login")]
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

            if (result.Message is not null)
                return BadRequest(result);

            return Ok("Password changed successfully");
        }


        private void SetRefreshTokenInCookie(string refreshToken, DateTime refreshTokenExpiration)
        {
            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Expires = refreshTokenExpiration
            };

            Response.Cookies.Append("refreshToken", refreshToken, cookieOptions);
        }
    }
}