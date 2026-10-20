using Microsoft.AspNetCore.Mvc;
using do_an_epu.Services.Interface;
using do_an_epu.DTOs;
using do_an_epu.DTOs.Common;
namespace do_an_epu.Controllers.API
{
    [Route("api/admin/search")]
    [ApiController]
    public class SearchController : Controller
    {
        private readonly ISearchService _searchService;
        public SearchController (ISearchService searchService)
        {
            _searchService = searchService;
        }

        [HttpGet("/booking/hotel")]
        public async Task<IActionResult> SearchAvailableHotel([FromQuery] HotelSearchModel model)
        {
            var result = await _searchService.GetAvailableRoomAsync(model);
            return Ok(ApiResponse<object>.Ok(result, "Success"));
        }

        [HttpGet("hotels")]
        public async Task<IActionResult> SearchAvailableHotelByInventory([FromQuery] HotelSearchModel model)
        {
            var result = await _searchService.GetAvailableRoomByInventoryAsync(model);
            return Ok(ApiResponse<object>.Ok(result, "Success"));
        }
    }
}
