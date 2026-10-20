using do_an_epu.DTOs;
using do_an_epu.Models;
using Microsoft.AspNetCore.Identity;
namespace do_an_epu.Services.Interface
{
    public interface IAuthService
    {
        Task<IdentityResult> SignUpAsync(AppUser user, string password);
        //Task<AppUser> GetUserByEmailAsync(string email);
        Task<LoginResponseDto> LoginAsync (LoginUserDto loginDto);
        Task<LoginResponseDto> GoogleLoginAsync(string idToken);
        Task<UserProfileDto> GetProfileAsync(int userId);
        Task<IdentityResult> UpdateProfileAsync(int userId, UserProfileDto profileDto);
        Task<IdentityResult> ChangePasswordAsync(int userId, ChangePasswordDto changePasswordDto);
    }
}
