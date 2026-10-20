using do_an_epu.Data;
using do_an_epu.Models;
using do_an_epu.Repositories.Interface;
using Microsoft.EntityFrameworkCore;

namespace do_an_epu.Repositories
{
    public class FloorRepository : IFloorRepository
    {
        private readonly ApplicationDbContext _context;

        public FloorRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Floor> AddAsync(Floor floor)
        {
            await _context.Floors.AddAsync(floor);
            await _context.SaveChangesAsync();
            return floor;
        }

        public async Task<List<Floor>> GetAllAsync()
        {
            return await _context.Floors
                .Include(f => f.Rooms)
                .ToListAsync();
        }

        public async Task<List<Floor>> GetFloorsByHotelIdAsync(int hotelId)
        {
            return await _context.Floors
                .Where(f => f.HotelId == hotelId)
                .Include(f => f.Rooms)
                .OrderBy(f => f.FloorNumber)
                .ToListAsync();
        }

        public async Task<Floor?> GetByIdAsync(int id)
        {
            return await _context.Floors
                .Include(f => f.Rooms)
                .FirstOrDefaultAsync(f => f.Id == id);
        }

        public async Task<Floor> UpdateAsync(Floor floor)
        {
            _context.Floors.Update(floor);
            await _context.SaveChangesAsync();
            return floor;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var floor = await _context.Floors.FindAsync(id);
            if (floor == null)
                return false;

            _context.Floors.Remove(floor);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ExistsAsync(int hotelId, int floorNumber)
        {
            return await _context.Floors
                .AnyAsync(f => f.HotelId == hotelId && f.FloorNumber == floorNumber);
        }
    }
}
