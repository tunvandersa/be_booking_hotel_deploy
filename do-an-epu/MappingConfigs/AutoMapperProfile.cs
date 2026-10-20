using AutoMapper;
using do_an_epu.Models;
using do_an_epu.DTOs;

namespace do_an_epu.MappingConfigs
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            CreateMap<HotelCreateDto, Hotel>();
            CreateMap<Hotel, HotelCreateDto>();
            CreateMap<AmenityCreateDto, Amenity>();
            CreateMap<AppUser, UserDto>();
            CreateMap<RoomTypeCreateDto, RoomType>();
            CreateMap<ImageDto, Image>();
            CreateMap<Image, ImageDto>();
            
            CreateMap<RoomType, RoomTypeResponseDto>()
                .ForMember(dest => dest.RoomTypeCategoryName, opt => opt.MapFrom(src => src.RoomTypeCategory.Name))
                .ForMember(dest => dest.ImageUrls, opt => opt.MapFrom(src => src.Images))
                .ForMember(dest => dest.Amenities, opt => opt.MapFrom(src => src.RoomAmenities.Select(ra => new AmenityResponseDto { Id = ra.AmenityId, Name = ra.Amenity.Name, IconUrl = ra.Amenity.IconUrl }).ToList()));

            CreateMap<RoomCreateDto, Room>();
            CreateMap<Room, RoomResponseDto>()
                .ForMember(dest => dest.FloorName, opt => opt.MapFrom(src => src.Floor.FloorNumber))
                .ForMember(dest => dest.RoomTypeName, opt => opt.MapFrom(src => src.RoomType.RoomTypeCategory.Name))
                .ForMember(dest => dest.StatusName, opt => opt.MapFrom(src => src.Status.ToString()));

            CreateMap<RoomUpdateDto, Room>()
                .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));

            CreateMap<Hotel, HotelResponseDto>()
                .ForMember(dest => dest.ImageUrls, opt => opt.MapFrom(src => src.Images.Select(img => img.ImageUrl).ToList()))
                .ForMember(dest => dest.LocationId, opt => opt.MapFrom(src => src.LocationId))
                .ForMember(dest => dest.LocationName, opt => opt.MapFrom(src => src.Location != null ? src.Location.Name : null))
                .ForMember(dest => dest.Amenities, opt => opt.MapFrom(src => src.HotelAmenities.Select(ha => new AmenityResponseDto {   Id = ha.AmenityId, Name = ha.Amenity.Name, IconUrl = ha.Amenity.IconUrl }).ToList()))
                .ForMember(dest => dest.RoomTypes, opt => opt.MapFrom(src => src.Floors.SelectMany(f => f.Rooms).Select(r => r.RoomType).Distinct()));
        }
    }
}