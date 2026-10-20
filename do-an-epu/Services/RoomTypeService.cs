using do_an_epu.Repositories.Interface;
using do_an_epu.DTOs;
using do_an_epu.Models;
using do_an_epu.Services.Interface;
using AutoMapper;
using System.Text.Json;

namespace do_an_epu.Services
{
    public class RoomTypeService : IRoomTypeService
    {
        private readonly IRoomTypeRepository _roomTypeRepository;
        private readonly IImageService _imageService;
        private readonly IInventoryService _inventoryService;
        private readonly IMapper _mapper;

        public RoomTypeService(IRoomTypeRepository roomTypeRepository, IInventoryService inventoryService, IImageService imageService, IMapper mapper)
        {
            _roomTypeRepository = roomTypeRepository;
            _imageService = imageService;
            _inventoryService = inventoryService;
            _mapper = mapper;
        }

        public async Task<RoomTypeResponseDto> AddAsync(RoomTypeCreateDto roomTypeDto)
        {
            var roomType = new RoomType
            {
                HotelId = roomTypeDto.HotelId,
                RoomTypeCategoryId = roomTypeDto.RoomTypeCategoryId,
                Description = roomTypeDto.Description,
                PricePerNight = roomTypeDto.PricePerNight,
                Area = roomTypeDto.Area,
                Capacity = roomTypeDto.Capacity,
                MaxAdults = roomTypeDto.MaxAdults,
                MaxChildren = roomTypeDto.MaxChildren,
                ExtraAdultPrice = roomTypeDto.ExtraAdultFee,
                ExtraChildPrice = roomTypeDto.ExtraChildFee,
                Images = new List<Image>(),
            };

            if (roomTypeDto.Images != null && roomTypeDto.Images.Any())
            {
                var urls = await _imageService
                    .UploadMultipleAsync(roomTypeDto.Images, "roomtypes");

                foreach (var url in urls)
                {
                    roomType.Images.Add(new Image
                    {
                        ImageUrl = url,
                    });
                }
            }

            // Add amenities
            var amenityIds = string.IsNullOrEmpty(roomTypeDto.AmenityIds)
                ? new List<int>()
                : JsonSerializer.Deserialize<List<int>>(roomTypeDto.AmenityIds);

            if (amenityIds != null && amenityIds.Any())
            {
                foreach (var amenityId in amenityIds)
                {
                    roomType.RoomAmenities.Add(new RoomAmenity
                    {
                        AmenityId = amenityId,
                        IsFree = true,
                        IsDefault = false
                    });
                }
            }


            var createdRoomType = await _roomTypeRepository.AddAsync(roomType);

            DateOnly startDate = DateOnly.FromDateTime(DateTime.UtcNow);

            await _inventoryService.UpInventoryAsync(createdRoomType.Id, startDate);
            return new RoomTypeResponseDto
            {
                Id = createdRoomType.Id
            };
        }

        public async Task<List<RoomTypeResponseDto>> GetAllAsync(int hotelId)
        {
            var roomTypes = await _roomTypeRepository.GetAllAsync(hotelId);

            return _mapper.Map<List<RoomTypeResponseDto>>(roomTypes);
        }

        public async Task<List<RoomTypeResponseDto>> GetRoomTypesByHotelAsync(int hotelId)
        {
            var roomTypes = await _roomTypeRepository.GetRoomTypesByHotelAsync(hotelId);
            return _mapper.Map<List<RoomTypeResponseDto>>(roomTypes);
        }

        public async Task<RoomTypeResponseDto> GetByIdAsync(int id)
        {
            var roomType = await _roomTypeRepository.GetByIdAsync(id);
            if (roomType == null)
                return null;

            return _mapper.Map<RoomTypeResponseDto>(roomType);
        }

        public async Task<RoomTypeResponseDto> UpdateAsync(int id, RoomTypeUpdateDto roomTypeDto)
        {
            var roomType = await _roomTypeRepository.GetByIdAsync(id);
            if (roomType == null)
                return null;

            // Cập nhật các trường cơ bản
            roomType.RoomTypeCategoryId = roomTypeDto.RoomTypeCategoryId;
            roomType.Description = roomTypeDto.Description ?? roomType.Description;
            roomType.PricePerNight = roomTypeDto.PricePerNight;
            roomType.Area = roomTypeDto.Area;
            roomType.Capacity = roomTypeDto.Capacity;
            roomType.MaxAdults = roomTypeDto.MaxAdults;
            roomType.MaxChildren = roomTypeDto.MaxChildren;
            roomType.ExtraAdultPrice = roomTypeDto.ExtraAdultPrice;
            roomType.ExtraChildPrice = roomTypeDto.ExtraChildPrice;
            roomType.IsActive = true;
            roomType.UpdatedAt = DateTime.UtcNow;

            // Xử lý ảnh cũ - xóa ảnh không còn trong danh sách
            var existingUrls = new HashSet<string>();
            if (!string.IsNullOrEmpty(roomTypeDto.ExistingImageUrls))
            {
                var parsedUrls = JsonSerializer.Deserialize<List<string>>(roomTypeDto.ExistingImageUrls);
                if (parsedUrls != null) existingUrls = new HashSet<string>(parsedUrls);
            }

            // Tìm và xóa ảnh không còn tồn tại trong danh sách giữ lại
            var imagesToRemove = roomType.Images.Where(img => !existingUrls.Contains(img.ImageUrl)).ToList();
            foreach (var img in imagesToRemove)
            {
                // Note: Nên thêm logic xóa ảnh vật lý trên Cloud tại đây nếu cần
                roomType.Images.Remove(img);
            }

            // Xử lý upload ảnh mới
            if (roomTypeDto.Images != null && roomTypeDto.Images.Any())
            {
                var urls = await _imageService.UploadMultipleAsync(roomTypeDto.Images, "roomtypes");

                foreach (var url in urls)
                {
                    roomType.Images.Add(new Image
                    {
                        ImageUrl = url,
                    });
                }
            }

            var newAmenityIds = new HashSet<int>();
            if (!string.IsNullOrEmpty(roomTypeDto.AmenityIds))
            {
                var parsedIds = JsonSerializer.Deserialize<List<int>>(roomTypeDto.AmenityIds);
                if (parsedIds != null) newAmenityIds = new HashSet<int>(parsedIds);
            }

            var amenitiesToRemove = roomType.RoomAmenities
                .Where(ra => !newAmenityIds.Contains(ra.AmenityId))
                .ToList();

            foreach (var amenity in amenitiesToRemove)
            {
                roomType.RoomAmenities.Remove(amenity);
            }

            // 2. Thêm Amenities mới chưa có
            var currentAmenityIds = roomType.RoomAmenities.Select(ra => ra.AmenityId).ToHashSet();
            foreach (var amenityId in newAmenityIds)
            {
                if (!currentAmenityIds.Contains(amenityId))
                {
                    roomType.RoomAmenities.Add(new RoomAmenity
                    {
                        AmenityId = amenityId,
                        IsFree = true,
                        IsDefault = false
                    });
                }
            }

            var updatedRoomType = await _roomTypeRepository.UpdateAsync(roomType);
            DateOnly startDate = DateOnly.FromDateTime(DateTime.UtcNow);

            await _inventoryService.UpInventoryAsync((int)updatedRoomType.Id, startDate);

            return _mapper.Map<RoomTypeResponseDto>(updatedRoomType);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var roomType = await _roomTypeRepository.GetByIdAsync(id);
            if (roomType == null)
                return false;

            roomType.IsDeleted = true;
            roomType.UpdatedAt = DateTime.UtcNow;


            await _roomTypeRepository.UpdateAsync(roomType);
            DateOnly startDate = DateOnly.FromDateTime(DateTime.UtcNow);

            await _inventoryService.UpInventoryAsync(id, startDate);
            return true;
        }
        public async Task<List<RoomTypeCategory>> GetAllCategoriesAsync()
        {
            return await _roomTypeRepository.GetRoomTypeCategoriesAsync();
        }

        // RoomTypeCategory methods
        public async Task<RoomTypeCategoryResponseDto> AddCategoryAsync(RoomTypeCategoryCreateDto categoryDto)
        {
            // Kiểm tra tên đã tồn tại chưa
            var exists = await _roomTypeRepository.CategoryExistsByNameAsync(categoryDto.Name);
            if (exists)
            {
                throw new Exception($"Danh mục '{categoryDto.Name}' đã tồn tại");
            }

            var category = new RoomTypeCategory
            {
                Name = categoryDto.Name
            };

            var createdCategory = await _roomTypeRepository.AddCategoryAsync(category);

            return new RoomTypeCategoryResponseDto
            {
                Id = createdCategory.Id,
                Name = createdCategory.Name
            };
        }

        public async Task<RoomTypeCategoryResponseDto?> GetCategoryByIdAsync(int id)
        {
            var category = await _roomTypeRepository.GetCategoryByIdAsync(id);
            if (category == null)
                return null;

            return new RoomTypeCategoryResponseDto
            {
                Id = category.Id,
                Name = category.Name
            };
        }

        public async Task<RoomTypeCategoryResponseDto> UpdateCategoryAsync(int id, RoomTypeCategoryUpdateDto categoryDto)
        {
            var category = await _roomTypeRepository.GetCategoryByIdAsync(id);
            if (category == null)
            {
                throw new Exception($"Không tìm thấy danh mục với ID {id}");
            }

            // Kiểm tra tên mới đã tồn tại chưa (ngoại trừ chính nó)
            if (category.Name.ToLower() != categoryDto.Name.ToLower())
            {
                var exists = await _roomTypeRepository.CategoryExistsByNameAsync(categoryDto.Name);
                if (exists)
                {
                    throw new Exception($"Danh mục '{categoryDto.Name}' đã tồn tại");
                }
            }

            category.Name = categoryDto.Name;
            var updatedCategory = await _roomTypeRepository.UpdateCategoryAsync(category);

            return new RoomTypeCategoryResponseDto
            {
                Id = updatedCategory.Id,
                Name = updatedCategory.Name
            };
        }

        public async Task<bool> DeleteCategoryAsync(int id)
        {
            return await _roomTypeRepository.DeleteCategoryAsync(id);
        }
    }
}
