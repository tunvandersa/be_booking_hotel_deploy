using do_an_epu.Repositories.Interface;
using do_an_epu.Data;
using do_an_epu.DTOs;
using do_an_epu.Models;
using Microsoft.EntityFrameworkCore;
using do_an_epu.Exceptions;
namespace do_an_epu.Repositories
{
    public class AmenityRepository : IAmenityRepository
    {
        private readonly ApplicationDbContext _context;
        public AmenityRepository(ApplicationDbContext context)
        {
            _context = context;
        }
        public async Task<AmenityCategory> AddAmenityCategoryAsync(AmenityCategory model)
        {
            _context.AmenityCategories.Add(model);
            await _context.SaveChangesAsync();
            return model;
        }
        public async Task<List<AmenityCategoryDto>> GetAllAmenitiesCategoryAsync()
        {
            var result = await _context.AmenityCategories
         .AsNoTracking()
         .Where(c => !c.IsDeleted)
         .Where(c => c.IsActive == true)
         .Select(c => new AmenityCategoryDto
         {
             Id = c.Id,
             Name = c.Name,
             IsActive = c.IsActive,
             Amenities = c.Amenities
                 .Where(a => !a.IsDeleted) // nếu Amenity có IsDeleted
                 .Where(a => a.IsActive == true)
                 .Select(a => new AmenityResponseDto
                 {
                     Id = a.Id,
                     Name = a.Name,
                     IsActive = a.IsActive,
                     IconUrl = a.IconUrl
                 })
                 .ToList()
         })
         .ToListAsync();

            return result;
        }
        public async Task<Amenity> AddAmenityAsync(Amenity model)
        {
            _context.Amenities.Add(model);
            await _context.SaveChangesAsync();
            return model;
        }

        public async Task<Amenity> GetAmenityByIdAsync(int id)
        {
            var amenity = await _context.Amenities
                .AsNoTracking()
                .FirstOrDefaultAsync(a => a.Id == id && !a.IsDeleted);
            return amenity;
        }

        public async Task<Amenity> UpdateAmenityAsync(int id, Amenity model)
        {
            var existingAmenity = await _context.Amenities
                .FirstOrDefaultAsync(a => a.Id == id && !a.IsDeleted);

            if (existingAmenity == null)
            {
                throw new NotFoundException("Không tìm thấy tiện nghi");
            }

            existingAmenity.Name = model.Name;
            existingAmenity.AmenityCategoryId = model.AmenityCategoryId;
            if (!string.IsNullOrEmpty(model.IconUrl))
            {
                existingAmenity.IconUrl = model.IconUrl;
            }
            existingAmenity.IsActive = model.IsActive;
            existingAmenity.UpdatedAt = DateTime.UtcNow;

            _context.Amenities.Update(existingAmenity);
            await _context.SaveChangesAsync();
            return existingAmenity;
        }

        public async Task<bool> DeleteAmenityAsync(int id)
        {
            var amenity = await _context.Amenities
                .FirstOrDefaultAsync(a => a.Id == id && !a.IsDeleted);

            if (amenity == null)
            {
                return false;
            }

            amenity.IsDeleted = true;
            _context.Amenities.Update(amenity);
            await _context.SaveChangesAsync();
            return true;
        }

    }
}
