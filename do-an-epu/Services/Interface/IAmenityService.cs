using do_an_epu.DTOs;
using do_an_epu.Models;
namespace do_an_epu.Services.Interface
{
    public interface IAmenityService
    {
        Task<List<AmenityCategoryDto>> GetAllAmenitiesCategoryAsync();
        Task<AmenityCategory> AddAmenityCategoryAsync(AmenityCategoryCreateDto model);
        Task<Amenity> AddAmenityAsync(AmenityCreateDto model);
        Task<Amenity> GetAmenityByIdAsync(int id);
        Task<Amenity> UpdateAmenityAsync(int id, AmenityCreateDto model);
        Task<bool> DeleteAmenityAsync(int id);
    }
}
