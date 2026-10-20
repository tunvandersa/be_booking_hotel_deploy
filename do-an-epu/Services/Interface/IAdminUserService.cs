using do_an_epu.DTOs;
using Microsoft.AspNetCore.Identity;

namespace do_an_epu.Services.Interface
{
    public interface IAdminUserService
    {
        Task<IEnumerable<UserDto>> GetAllUsersAsync();
        Task<UserDto> GetUserByIdAsync(int id);
        Task<IdentityResult> UpdateUserAsync(int id, UserDto userDto);
        Task<IdentityResult> DeleteUserAsync(int id);
    }
}
