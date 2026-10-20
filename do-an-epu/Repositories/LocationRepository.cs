using do_an_epu.Data;
using do_an_epu.Models;
using do_an_epu.Repositories.Interface;
using Microsoft.EntityFrameworkCore;

namespace do_an_epu.Repositories
{
    public class LocationRepository : ILocationRepository
    {
        private readonly ApplicationDbContext _context;

        public LocationRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Location> AddAsync(Location location)
        {
            await _context.Set<Location>().AddAsync(location);
            await _context.SaveChangesAsync();
            return location;
        }

        public async Task<List<Location>> GetAllAsync()
        {
            return await _context.Set<Location>()
                .Include(l => l.Hotels)
                .ToListAsync();
        }

        public async Task<Location?> GetByIdAsync(int id)
        {
            return await _context.Set<Location>()
                .Include(l => l.Hotels)
                .FirstOrDefaultAsync(l => l.Id == id);
        }

        public async Task<Location> UpdateAsync(Location location)
        {
            _context.Set<Location>().Update(location);
            await _context.SaveChangesAsync();
            return location;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var location = await _context.Set<Location>().FindAsync(id);
            if (location == null)
                return false;

            _context.Set<Location>().Remove(location);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ExistsByNameAsync(string name)
        {
            return await _context.Set<Location>()
                .AnyAsync(l => l.Name.ToLower() == name.ToLower());
        }
    }
}
