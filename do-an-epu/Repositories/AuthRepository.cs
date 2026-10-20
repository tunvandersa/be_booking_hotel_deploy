using do_an_epu.Models;
using Microsoft.AspNetCore.Identity;
using do_an_epu.Repositories.Interface;
namespace do_an_epu.Repositories
{
    public class AuthRepository : IAuthRepository
    {
        private readonly UserManager<AppUser> _userManager;
        public AuthRepository(UserManager<AppUser> userManager)
        {
            _userManager = userManager;
        }
        public async Task<IdentityResult> SignUpAsync(AppUser user, string password)
        {
            var result = await _userManager.CreateAsync(user, password);
            return result;
        }
        public async Task<AppUser> GetUserByEmailAsync(string email)
        {
            var user = await _userManager.FindByEmailAsync(email);
            return user;
        }
        public async Task<bool> CheckUserPasswordAsync(AppUser user, string password)
        {
            return await _userManager.CheckPasswordAsync(user, password);
        }
        public async Task<IList<string>> GetUserRoleAsync(AppUser user)
        {
            return await _userManager.GetRolesAsync(user);
        }
    }
}
