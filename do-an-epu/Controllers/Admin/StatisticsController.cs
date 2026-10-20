using do_an_epu.Services.Interface;
using do_an_epu.DTOs.Common;
using Microsoft.AspNetCore.Mvc;

namespace do_an_epu.Controllers.Admin
{
    [Route("api/admin/statistics")]
    [ApiController]
    public class StatisticsController : ControllerBase
    {
        private readonly IStatisticsService _statisticsService;

        public StatisticsController(IStatisticsService statisticsService)
        {
            _statisticsService = statisticsService;
        }

        [HttpGet("dashboard")]
        public async Task<IActionResult> GetDashboardData([FromQuery] int? year, [FromQuery] int? month)
        {
            int selectedYear = year ?? DateTime.UtcNow.Year;
            var data = await _statisticsService.GetAdminDashboardDataAsync(selectedYear, month);
            return Ok(ApiResponse<object>.Ok(data, "Success"));
        }
    }
}
