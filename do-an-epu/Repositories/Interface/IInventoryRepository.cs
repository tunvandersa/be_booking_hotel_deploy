using do_an_epu.Models;
using System.Threading.Tasks;
namespace do_an_epu.Repositories.Interface

{
    public interface IInventoryRepository
    {
        Task<List<RoomInventory>> GetByDateRangeAndRoomTypeIdsAsync(DateOnly startDate,DateOnly endDate,List<int> roomTypeIds);
        Task<List<RoomInventory>> GetAllByRoomTypeIdAsync(int id);
        Task<List<RoomInventory>> GetByRoomTypeIdFromDateAsync(int roomTypeId, DateOnly startDate);
        
        /// <summary>
        /// Kiểm tra số phòng trống tối thiểu trong khoảng ngày cho một loại phòng
        /// </summary>
        Task<int> GetMinAvailableRoomsAsync(int roomTypeId, DateOnly startDate, DateOnly endDate);
        
        /// <summary>
        /// Cập nhật số phòng đã đặt cho một loại phòng trong khoảng ngày
        /// </summary>
        Task UpdateBookedRoomsAsync(int roomTypeId, DateOnly startDate, DateOnly endDate, int roomCount);
    }
}
