using do_an_epu.Repositories.Interface;
using do_an_epu.DTOs;
using do_an_epu.Services.Interface;
using do_an_epu.Models;
using AutoMapper;
using do_an_epu.Services;
using CloudinaryDotNet;
namespace do_an_epu.Services
{
    public class AmenityService : IAmenityService
    {
        private readonly IAmenityRepository _amenityRepository;
        private readonly IImageService _imageService;
        private readonly IMapper _mapper;
        public AmenityService(IAmenityRepository amenityRepository, IMapper mapper, IImageService imageService)
        {
            _amenityRepository = amenityRepository;
            _mapper = mapper;
            _imageService = imageService;
        }
        public async Task<List<AmenityCategoryDto>> GetAllAmenitiesCategoryAsync()
        {
            return await _amenityRepository.GetAllAmenitiesCategoryAsync();
        }
        public async Task<AmenityCategory> AddAmenityCategoryAsync(AmenityCategoryCreateDto model)
        {
            var newAmenityCategory = new AmenityCategory
            {
                Name = model.Name,
                IsDeleted = false
            };
            return await _amenityRepository.AddAmenityCategoryAsync(newAmenityCategory);
        }
        public async Task<Amenity> AddAmenityAsync(AmenityCreateDto model)
        {
            string url = string.Empty;
            if (model.Image != null)
            {
                url = await _imageService
                   .UploadAsync(model.Image, "amenities");
            }
            model.IconUrl = url;
            var newAmenity = _mapper.Map<Amenity>(model);
            return await _amenityRepository.AddAmenityAsync(newAmenity);
        }

        public async Task<Amenity> GetAmenityByIdAsync(int id)
        {
            return await _amenityRepository.GetAmenityByIdAsync(id);
        }
        public async Task<Amenity> UpdateAmenityAsync(int id, AmenityCreateDto model)
        {
            var updateAmenity = _mapper.Map<Amenity>(model);
            return await _amenityRepository.UpdateAmenityAsync(id, updateAmenity);
        }

        public async Task<bool> DeleteAmenityAsync(int id)
        {
            return await _amenityRepository.DeleteAmenityAsync(id);
        }
    }
}
