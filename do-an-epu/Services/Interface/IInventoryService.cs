using do_an_epu.Models;
using do_an_epu.DTOs;
using do_an_epu.Models;

namespace do_an_epu.Services.Interface
{
    public interface IInventoryService
    {
        Task<bool> CreateInventoryAsync(DateOnly startDate, DateOnly endDate, int id);
        Task<bool> UpInventoryAsync(int id, DateOnly startDate);
        Task<bool> UpdateInventoryAsync(int id, UpdateInventoryDto dto);
        Task<IEnumerable<RoomInventoryDto>> GetInventoryAsync(DateOnly startDate, DateOnly endDate, int hotelId);
    }
}
