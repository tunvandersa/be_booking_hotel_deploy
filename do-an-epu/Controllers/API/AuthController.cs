using Microsoft.AspNetCore.Mvc;
using do_an_epu.Models;
using Microsoft.Extensions.Logging;
using do_an_epu.Services.Interface;
using do_an_epu.DTOs.Common;
using do_an_epu.DTOs;
namespace do_an_epu.Controllers.API
{
    [Route("api/auth")]
    [ApiController]
    public class AuthController : Controller
    {
        private readonly ILogger<AuthController> _logger;
        private readonly IAuthService _authService;
        public AuthController(ILogger<AuthController> logger, IAuthService authService)
        {
            _logger = logger;
            _authService = authService;
        }
        [HttpPost("signup")]
        public async Task<ActionResult<ApiResponse<string>>> SignUp([FromBody] RegisterUserDto  registerUserDto)
        {
            if(!ModelState.IsValid)
            {
                return BadRequest(new ApiResponse<string>
                {
                    Success = false,
                    Message = "Invalid input data.",
                    Data = null
                });
            }
            var user = new AppUser
            {
                UserName = registerUserDto.UserName,
                Email = registerUserDto.Email,
                LastName = registerUserDto.LastName,
                FirstName = registerUserDto.FirstName
            };

            var result = await _authService.SignUpAsync(user, registerUserDto.Password);

            if (result.Succeeded)
            {
                return Ok(new ApiResponse<string>
                {
                    Success = true,
                    Message = "User registered successfully.",
                    Data = null
                });
            }
            else
            {
                var errors = string.Join("; ", result.Errors.Select(e => e.Description));
                return BadRequest(new ApiResponse<string>
                {
                    Success = false,
                    Message = "User registration failed.",
                    Data = errors
                });
            }
        }
        [HttpPost("login")]
        public async Task<ActionResult<ApiResponse<LoginResponseDto>>> Login([FromBody] LoginUserDto loginUserDto)
        {
            var loginResponse = await _authService.LoginAsync(loginUserDto);

            return ApiResponse<LoginResponseDto>.Ok(loginResponse, "Đăng nhập thành công!!");
        }

        [HttpPost("google-login")]
        public async Task<ActionResult<ApiResponse<LoginResponseDto>>> GoogleLogin([FromBody] GoogleLoginDto googleLoginDto)
        {
            try
            {
                var loginResponse = await _authService.GoogleLoginAsync(googleLoginDto.IdToken);
                return ApiResponse<LoginResponseDto>.Ok(loginResponse, "Đăng nhập bằng Google thành công!");
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<LoginResponseDto>.Fail(ex.Message));
            }
        }
    }
}
