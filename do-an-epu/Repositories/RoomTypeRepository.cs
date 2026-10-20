using do_an_epu.Repositories.Interface;
using do_an_epu.Models;
using Microsoft.EntityFrameworkCore;
using do_an_epu.Data;

namespace do_an_epu.Repositories
{
    public class RoomTypeRepository : IRoomTypeRepository
    {
        private readonly ApplicationDbContext _context;

        public RoomTypeRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<RoomType> AddAsync(RoomType roomType)
        {
            await _context.AddAsync(roomType);
            await _context.SaveChangesAsync();
            return roomType;
        }

        public async Task<List<RoomType>> GetAllAsync(int hotelId)
        {
            return await _context.RoomTypes
                .Where(rt => !rt.IsDeleted && rt.HotelId == hotelId)
                .Include(rt => rt.Images)
                .Include(rt => rt.RoomTypeCategory)
                .Include(rt => rt.Rooms)
                .Include(rt => rt.RoomAmenities)
                    .ThenInclude(ra => ra.Amenity)
                .ToListAsync();
        }

        public async Task<List<RoomType>> GetRoomTypesByHotelAsync(int hotelId)
        {
            return await _context.RoomTypes
                .Where(rt => rt.HotelId == hotelId && !rt.IsDeleted)
                .Include(rt => rt.Images)
                .Include(rt => rt.RoomTypeCategory)
                .Include(rt => rt.RoomAmenities)
                    .ThenInclude(ra => ra.Amenity)
                .ToListAsync();
        }

        public async Task<RoomType> GetByIdAsync(int id)
        {
            return await _context.RoomTypes
                .Include(rt => rt.Images)
                .Include(rt => rt.Hotel)
                .Include(rt => rt.RoomTypeCategory)
                .Include(rt => rt.RoomAmenities)
                    .ThenInclude(ra => ra.Amenity)
                .FirstOrDefaultAsync(rt => rt.Id == id && !rt.IsDeleted);
        }

        public async Task<RoomType> UpdateAsync(RoomType roomType)
        {
            _context.RoomTypes.Update(roomType);
            await _context.SaveChangesAsync();
            
            // Reload với đầy đủ navigation properties để tránh lỗi khi mapping
            return await _context.RoomTypes
                .Include(rt => rt.Images)
                .Include(rt => rt.Hotel)
                .Include(rt => rt.RoomTypeCategory)
                .Include(rt => rt.RoomAmenities)
                    .ThenInclude(ra => ra.Amenity)
                .FirstOrDefaultAsync(rt => rt.Id == roomType.Id);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var roomType = await GetByIdAsync(id);
            if (roomType == null)
                return false;

            roomType.IsDeleted = true;
            await UpdateAsync(roomType);
            return true;
        }

        public async Task<List<RoomTypeCategory>> GetRoomTypeCategoriesAsync()
        {
            return await _context.RoomTypeCategory.ToListAsync();
        }
        public async Task<List<RoomType>> GetRoomTypesByIdsAsync(List<int> ids)
        {
            return await _context.RoomTypes
                .Include(rt => rt.RoomTypeCategory)
                .Where(rt => ids.Contains(rt.Id) && !rt.IsDeleted)
                .ToListAsync();
        }

        // RoomTypeCategory methods
        public async Task<RoomTypeCategory> AddCategoryAsync(RoomTypeCategory category)
        {
            await _context.RoomTypeCategory.AddAsync(category);
            await _context.SaveChangesAsync();
            return category;
        }

        public async Task<RoomTypeCategory?> GetCategoryByIdAsync(int id)
        {
            return await _context.RoomTypeCategory.FindAsync(id);
        }

        public async Task<RoomTypeCategory> UpdateCategoryAsync(RoomTypeCategory category)
        {
            _context.RoomTypeCategory.Update(category);
            await _context.SaveChangesAsync();
            return category;
        }

        public async Task<bool> DeleteCategoryAsync(int id)
        {
            var category = await _context.RoomTypeCategory.FindAsync(id);
            if (category == null)
                return false;

            _context.RoomTypeCategory.Remove(category);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> CategoryExistsByNameAsync(string name)
        {
            return await _context.RoomTypeCategory
                .AnyAsync(c => c.Name.ToLower() == name.ToLower());
        }
    }
}
