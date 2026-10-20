using do_an_epu.Repositories.Interface;
using do_an_epu.Models;
using Microsoft.EntityFrameworkCore;
using do_an_epu.Data;
using do_an_epu.DTOs;
namespace do_an_epu.Repositories
{
    public class HotelRepository : IHotelRepository
    {
        private readonly ApplicationDbContext _context;
        public HotelRepository(ApplicationDbContext context)
        {
            _context = context;
        }
        public async Task<Hotel> AddAsync(Hotel hotel)
        {
            await _context.AddAsync(hotel);
            await _context.SaveChangesAsync();
            return hotel;
        }
        public async Task<List<Hotel>> GetAllAsync()
        {
            return await _context.Hotels
                .Include(h => h.Images)
                .Include(h => h.Location)
                .Where(h => !h.IsDeleted).ToListAsync();
        }
        public async Task<Hotel> GetHotelByIdAsync(int id)
        {
            return await _context.Hotels
                .Include(h => h.Images)
                .Include(h => h.Location)
                .Include(h => h.HotelAmenities)
                    .ThenInclude(ha => ha.Amenity)
                .FirstOrDefaultAsync(h => h.Id == id && !h.IsDeleted);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var hotel = await GetHotelByIdAsync(id);
            if (hotel == null)
                return false;

            hotel.IsDeleted = true;
            _context.Hotels.Update(hotel);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<Hotel> UpdateAsync(Hotel hotel)
        {
            _context.Hotels.Update(hotel);
            await _context.SaveChangesAsync();
            return hotel;
        }
    }
}
