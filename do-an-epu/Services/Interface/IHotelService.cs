using do_an_epu.Models;
using do_an_epu.DTOs;
namespace do_an_epu.Services.Interface
{
    public interface IHotelService
    {
        Task<HotelResponseDto> AddAsync(HotelCreateDto hotel);
        Task<List<HotelResponseDto>> GetAllAsync();
        Task<List<HotelResponseDto>> GetActiveHotelsAsync();
        Task<HotelResponseDto> GetByIdAsync(int id);
        Task<HotelResponseDto> UpdateAsync(int id, HotelUpdateDto hotelDto);
        Task<bool> DeleteAsync(int id);
    }
}
