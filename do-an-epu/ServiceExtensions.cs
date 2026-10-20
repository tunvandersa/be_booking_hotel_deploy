using Microsoft.Extensions.DependencyInjection;
using do_an_epu.Repositories.Interface;
using do_an_epu.Repositories;
using do_an_epu.Services.Interface;
using do_an_epu.Services;
namespace do_an_epu.ServicesExtensions
{
    public static class ServiceExtensions
    {
        public static void ConfigureRepositories(this IServiceCollection services)
        {
            services.AddScoped<IHotelRepository, HotelRepository>();
            services.AddScoped<ISearchRepository, SearchRepository>();
            services.AddScoped<IAuthRepository, AuthRepository>();
            services.AddScoped<IAmenityRepository, AmenityRepository>();
            services.AddScoped<IRoomRepository, RoomRepository>();
            services.AddScoped<IRoomTypeRepository, RoomTypeRepository>();
            services.AddScoped<IInventoryRepository, InventoryRepository>();
            services.AddScoped<IBookingRepository, BookingRepository>();
            services.AddScoped<IFloorRepository, FloorRepository>();
            services.AddScoped<ILocationRepository, LocationRepository>();
        }
        public static void ConfigureServices(this IServiceCollection services)
        {
            services.AddScoped<IHotelService, HotelService>();
            services.AddScoped<ISearchService, SearchService>();
            services.AddScoped<IAuthService, AuthService>();
            services.AddScoped<IAmenityService, AmenityService>();
            services.AddScoped<IBookingService, BookingService>();
            services.AddScoped<IImageService, ImageService>();
            services.AddScoped<IRoomTypeService, RoomTypeService>();
            services.AddScoped<IRoomService, RoomService>();
            services.AddScoped<IInventoryService, InventoryService>();
            services.AddScoped<IFloorService, FloorService>();
            services.AddScoped<ILocationService, LocationService>();
            services.AddScoped<IVnPayService, VnPayService>();
            services.AddScoped<IChatService, GeminiChatService>();
            services.AddHttpClient<IChatService, GeminiChatService>();
            services.AddScoped<IAdminUserService, AdminUserService>();
            services.AddScoped<IAdminBookingService, AdminBookingService>();
            services.AddScoped<IStatisticsService, StatisticsService>();
        }

    }
}
