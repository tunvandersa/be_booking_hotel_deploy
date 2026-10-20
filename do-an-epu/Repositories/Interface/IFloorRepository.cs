using do_an_epu.Models;

namespace do_an_epu.Repositories.Interface
{
    public interface IFloorRepository
    {
        Task<Floor> AddAsync(Floor floor);
        Task<List<Floor>> GetAllAsync();
        Task<List<Floor>> GetFloorsByHotelIdAsync(int hotelId);
        Task<Floor?> GetByIdAsync(int id);
        Task<Floor> UpdateAsync(Floor floor);
        Task<bool> DeleteAsync(int id);
        Task<bool> ExistsAsync(int hotelId, int floorNumber);
    }
}
