using do_an_epu.Models;
using do_an_epu.DTOs;
namespace do_an_epu.Repositories.Interface
{
    public interface IHotelRepository
    {
        Task<Hotel> AddAsync(Hotel hotel);
        Task<List<Hotel>> GetAllAsync();
        Task<Hotel> GetHotelByIdAsync(int id);
        Task<Hotel> UpdateAsync(Hotel hotel);
        Task<bool> DeleteAsync(int id);
    }
}
