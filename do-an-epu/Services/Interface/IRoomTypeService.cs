using do_an_epu.DTOs;
using do_an_epu.Models;

namespace do_an_epu.Services.Interface
{
    public interface IRoomTypeService
    {
        Task<RoomTypeResponseDto> AddAsync(RoomTypeCreateDto roomTypeDto);
        Task<List<RoomTypeResponseDto>> GetAllAsync(int hotelId);
        Task<List<RoomTypeResponseDto>> GetRoomTypesByHotelAsync(int hotelId);
        Task<RoomTypeResponseDto> GetByIdAsync(int id);
        Task<RoomTypeResponseDto> UpdateAsync(int id, RoomTypeUpdateDto roomTypeDto);
        Task<bool> DeleteAsync(int id);
        Task<List<RoomTypeCategory>> GetAllCategoriesAsync();
        
        // RoomTypeCategory methods
        Task<RoomTypeCategoryResponseDto> AddCategoryAsync(RoomTypeCategoryCreateDto categoryDto);
        Task<RoomTypeCategoryResponseDto?> GetCategoryByIdAsync(int id);
        Task<RoomTypeCategoryResponseDto> UpdateCategoryAsync(int id, RoomTypeCategoryUpdateDto categoryDto);
        Task<bool> DeleteCategoryAsync(int id);
    }
}
