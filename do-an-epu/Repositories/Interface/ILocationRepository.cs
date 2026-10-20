using do_an_epu.Models;

namespace do_an_epu.Repositories.Interface
{
    public interface ILocationRepository
    {
        Task<Location> AddAsync(Location location);
        Task<List<Location>> GetAllAsync();
        Task<Location?> GetByIdAsync(int id);
        Task<Location> UpdateAsync(Location location);
        Task<bool> DeleteAsync(int id);
        Task<bool> ExistsByNameAsync(string name);
    }
}
