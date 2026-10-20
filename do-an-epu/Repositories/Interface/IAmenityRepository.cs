using do_an_epu.DTOs;
using do_an_epu.Models;
namespace do_an_epu.Repositories.Interface
{
    public interface IAmenityRepository
    {
        Task<AmenityCategory> AddAmenityCategoryAsync(AmenityCategory model);
        Task<List<AmenityCategoryDto>> GetAllAmenitiesCategoryAsync();
        Task<Amenity> AddAmenityAsync(Amenity model);
        Task<Amenity> GetAmenityByIdAsync(int id);
        Task<Amenity> UpdateAmenityAsync(int id, Amenity model);
        Task<bool> DeleteAmenityAsync(int id);
    }
}
