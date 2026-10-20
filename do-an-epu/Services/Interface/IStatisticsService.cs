using do_an_epu.DTOs;

namespace do_an_epu.Services.Interface
{
    public interface IStatisticsService
    {
        Task<AdminDashboardDataDto> GetAdminDashboardDataAsync(int year, int? month = null);
    }
}
