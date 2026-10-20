using Microsoft.AspNetCore.Mvc;
using do_an_epu.Services.Interface;
using do_an_epu.DTOs;
using do_an_epu.DTOs.Common;

namespace do_an_epu.Controllers.Client
{
    [Route("api/client/search")]
    [ApiController]
    public class SearchController : ControllerBase
    {
        private readonly ISearchService _searchService;

        public SearchController(ISearchService searchService)
        {
            _searchService = searchService;
        }

        /// <summary>
        /// Tìm kiếm phòng theo Inventory - dành cho khách hàng
        /// </summary>
        [HttpGet("hotels")]
        public async Task<IActionResult> SearchAvailableHotels([FromQuery] HotelSearchModel model)
        {
            var result = await _searchService.GetAvailableRoomByInventoryAsync(model);
            return Ok(ApiResponse<object>.Ok(result, "Success"));
        }
    }
}
