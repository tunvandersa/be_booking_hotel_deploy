using do_an_epu.Repositories.Interface;
using do_an_epu.DTOs;
using do_an_epu.Models;
using do_an_epu.Services.Interface;
using System.Text.Json;
using AutoMapper;

namespace do_an_epu.Services
{
    public class HotelService : IHotelService
    {
        private readonly IHotelRepository _hotelRepository;
        private readonly IImageService _imageService;
        private readonly IMapper _mapper;
        public HotelService(IHotelRepository hotelRepository, IImageService imageService, IMapper mapper)
        {
            _hotelRepository = hotelRepository;
            _imageService = imageService;
            _mapper = mapper;
        }
        public async Task<HotelResponseDto> AddAsync(HotelCreateDto hotelDto)
        {
            var hotel = new Hotel
            {
                Name = hotelDto.Name,
                Address = hotelDto.Address,
                Description = hotelDto.Description,
                PhoneNumber = hotelDto.PhoneNumber,
                Email = hotelDto.Email,
                LocationId = hotelDto.LocationId ?? 1,
                CheckOutTime = hotelDto.CheckOutTime,
                CheckInTime = hotelDto.CheckInTime,
                Images = new List<Image>()
            };
            var amenityIds = string.IsNullOrEmpty(hotelDto.AmenityIds)
                ? new List<int>()
                : JsonSerializer.Deserialize<List<int>>(hotelDto.AmenityIds);

            if (amenityIds != null && amenityIds.Any())
            {
                foreach (var amenityId in amenityIds)
                {
                    hotel.HotelAmenities.Add(new HotelAmenity
                    {
                        AmenityId = amenityId,
                        IsFree = true,
                        IsActive = true
                    });
                }
            }
            var floors = string.IsNullOrEmpty(hotelDto.Floors)
                    ? new List<FloorHotelDto>()
                    : JsonSerializer.Deserialize<List<FloorHotelDto>>(
                        hotelDto.Floors,
                        new JsonSerializerOptions
                        {
                            PropertyNameCaseInsensitive = true
                        }
                    );

            foreach (var floorDto in floors)
            {
                var floor = new Floor
                {
                    FloorNumber = floorDto.Id ?? 0,
                    Rooms = new List<Room>()
                };

                if (floorDto.Rooms.Count > 0)
                {
                    foreach (var room in floorDto.Rooms)
                    {
                        var newRoom = new Room
                        {
                            RoomNumber = room,
                        };
                        floor.Rooms.Add(newRoom);
                    }
                }
                hotel.Floors.Add(floor);
            }

            if (hotelDto.Images != null && hotelDto.Images.Any())
            {
                var urls = await _imageService
                    .UploadMultipleAsync(hotelDto.Images, "hotels");

                foreach (var url in urls)
                {
                    hotel.Images.Add(new Image
                    {
                        ImageUrl = url,
                    });
                }
            }
            await _hotelRepository.AddAsync(hotel);

            return new HotelResponseDto
            {
                Id = hotel.Id,
                Name = hotel.Name
            };
        }
        public async Task<List<HotelResponseDto>> GetAllAsync()
        {
            var hotels = await _hotelRepository.GetAllAsync();
            return _mapper.Map<List<HotelResponseDto>>(hotels);
        }

        public async Task<List<HotelResponseDto>> GetActiveHotelsAsync()
        {
            var hotels = await _hotelRepository.GetAllAsync();
            var activeHotels = hotels.Where(h => h.Status == HotelStatus.Active).ToList();
            return _mapper.Map<List<HotelResponseDto>>(activeHotels);
        }

        public async Task<HotelResponseDto> GetByIdAsync(int id)
        {
            var hotel = await _hotelRepository.GetHotelByIdAsync(id);
            if (hotel == null)
                return null;
            return _mapper.Map<HotelResponseDto>(hotel);
        }
        public async Task<bool> DeleteAsync(int id)
        {
            return await _hotelRepository.DeleteAsync(id);
        }

        public async Task<HotelResponseDto> UpdateAsync(int id, HotelUpdateDto hotelDto)
        {
            var hotel = await _hotelRepository.GetHotelByIdAsync(id);
            if (hotel == null)
                return null;

            hotel.Name = hotelDto.Name ?? hotel.Name;
            hotel.Address = hotelDto.Address ?? hotel.Address;
            hotel.Description = hotelDto.Description ?? hotel.Description;
            hotel.PhoneNumber = hotelDto.PhoneNumber ?? hotel.PhoneNumber;
            hotel.Email = hotelDto.Email ?? hotel.Email;
            hotel.CheckInTime = hotelDto.CheckInTime ?? hotel.CheckInTime;
            hotel.CheckOutTime = hotelDto.CheckOutTime ?? hotel.CheckOutTime;
            if (hotelDto.LocationId.HasValue)
            {
                hotel.LocationId = hotelDto.LocationId.Value;
            }
            if (hotelDto.Status.HasValue)
            {
                hotel.Status = (HotelStatus)hotelDto.Status.Value;
            }

            // Update Amenities
            if (hotelDto.AmenityIds != null)
            {
                var amenityIds = JsonSerializer.Deserialize<List<int>>(hotelDto.AmenityIds) ?? new List<int>();
                
                // Remove existing amenities
                hotel.HotelAmenities.Clear();

                // Add new amenities
                foreach (var amenityId in amenityIds)
                {
                    hotel.HotelAmenities.Add(new HotelAmenity
                    {
                        AmenityId = amenityId,
                        IsFree = true,
                        IsActive = true
                    });
                }
            }
       
            // Xử lý ảnh cũ - xóa ảnh không còn trong danh sách
            var existingUrls = new HashSet<string>();
            if (!string.IsNullOrEmpty(hotelDto.ExistingImageUrls))
            {
                var parsedUrls = JsonSerializer.Deserialize<List<string>>(hotelDto.ExistingImageUrls);
                if (parsedUrls != null) existingUrls = new HashSet<string>(parsedUrls);
            }

            // Tìm và xóa ảnh không còn tồn tại trong danh sách giữ lại
            var imagesToRemove = hotel.Images.Where(img => !existingUrls.Contains(img.ImageUrl)).ToList();
            foreach (var img in imagesToRemove)
            {
                hotel.Images.Remove(img);
            }

            // Xử lý upload ảnh mới
            if (hotelDto.Images != null && hotelDto.Images.Any())
            {
                var urls = await _imageService.UploadMultipleAsync(hotelDto.Images, "hotels");
                foreach (var url in urls)
                {
                    hotel.Images.Add(new Image { ImageUrl = url });
                }
            }
            hotel.UpdatedAt = DateTime.UtcNow;
            await _hotelRepository.UpdateAsync(hotel);

            return _mapper.Map<HotelResponseDto>(hotel);
        }
    }
}
