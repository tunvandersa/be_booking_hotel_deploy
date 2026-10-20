using do_an_epu.Models;
using Microsoft.AspNetCore.Identity;

namespace do_an_epu.Repositories.Interface
{
    public interface IAuthRepository
    {
        Task<IdentityResult>SignUpAsync(AppUser user, string password);
        Task<AppUser>GetUserByEmailAsync(string email);
        Task<bool>CheckUserPasswordAsync(AppUser user, string password);
        Task<IList<string>>GetUserRoleAsync(AppUser user);
    }
}
