using Microsoft.AspNetCore.Mvc;
using do_an_epu.Services.Interface;
using do_an_epu.DTOs;
using do_an_epu.DTOs.Common;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace do_an_epu.Controllers.Client
{
    [Route("api/user")]
    [ApiController]
    [Authorize]
    public class UserController : ControllerBase
    {
        private readonly IAuthService _authService; 

        public UserController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpGet("profile")]
        public async Task<IActionResult> GetProfile()
        {
            try
            {
                var userId = GetUserId();
                var profile = await _authService.GetProfileAsync(userId);
                return Ok(ApiResponse<UserProfileDto>.Ok(profile, "Success"));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<object>.Fail(ex.Message));
            }
        }

        [HttpPut("profile")]
        public async Task<IActionResult> UpdateProfile([FromBody] UserProfileDto profileDto)
        {
            try
            {
                var userId = GetUserId();
                var result = await _authService.UpdateProfileAsync(userId, profileDto);
                if (result.Succeeded)
                {
                    return Ok(ApiResponse<object>.Ok(null, "Profile updated successfully"));
                }
                return BadRequest(ApiResponse<object>.Fail(string.Join(", ", result.Errors.Select(e => e.Description))));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<object>.Fail(ex.Message));
            }
        }

        [HttpPost("change-password")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto changePasswordDto)
        {
            try
            {
                var userId = GetUserId();
                var result = await _authService.ChangePasswordAsync(userId, changePasswordDto);
                if (result.Succeeded)
                {
                    return Ok(ApiResponse<object>.Ok(null, "Password changed successfully"));
                }
                return BadRequest(ApiResponse<object>.Fail(string.Join(", ", result.Errors.Select(e => e.Description))));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<object>.Fail(ex.Message));
            }
        }

        private int GetUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
            {
                 throw new Exception("Unauthorized or Invalid Token"); 
            }
            return userId;
        }
    }
}
