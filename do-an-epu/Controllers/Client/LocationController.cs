using Microsoft.AspNetCore.Mvc;
using do_an_epu.Services.Interface;
using do_an_epu.DTOs.Common;

namespace do_an_epu.Controllers.Client
{
    [Route("api/client/location")]
    [ApiController]
    public class LocationController : ControllerBase
    {
        private readonly ILocationService _locationService;

        public LocationController(ILocationService locationService)
        {
            _locationService = locationService;
        }

        /// <summary>
        /// Lấy danh sách địa điểm
        /// </summary>
        [HttpGet("list")]
        public async Task<IActionResult> GetLocations()
        {
            var result = await _locationService.GetAllLocationsAsync();
            return Ok(ApiResponse<object>.Ok(result, "Success"));
        }
    }
}
