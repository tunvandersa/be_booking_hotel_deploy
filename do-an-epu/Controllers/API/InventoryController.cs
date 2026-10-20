using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using do_an_epu.Services.Interface;
using do_an_epu.DTOs;
using Microsoft.AspNetCore.Authorization;
using do_an_epu.Exceptions;

namespace do_an_epu.Controllers.API
{
    [Route("api/admin/inventory")]
    [ApiController]
    [Authorize(Roles = "Admin")]
    public class InventoryController : ControllerBase
    {
        private readonly ILogger<InventoryController> _logger;
        private readonly IInventoryService _inventoryService;

        public InventoryController(ILogger<InventoryController> logger, IInventoryService inventoryService)
        {
            _logger = logger;
            _inventoryService = inventoryService;
        }

        [HttpGet("create/{id}")]
        public async Task<IActionResult> CreateInventory([FromQuery] DateOnly startDate,[FromQuery] DateOnly endDate, int id)
        {
            try
            {
                await _inventoryService.CreateInventoryAsync(startDate, endDate, id);
                return Ok(new { message = "Inventory created successfully for the next six months." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating inventory for the next six months.");
                return StatusCode(500, new { message = "An error occurred while creating inventory." });
            }
        }

        [HttpGet("view/{hotelId}")]
        public async Task<IActionResult> GetInventory([FromQuery] DateOnly startDate, [FromQuery] DateOnly endDate, int hotelId)
        {
            try
            {
                var inventory = await _inventoryService.GetInventoryAsync(startDate, endDate, hotelId);
                return Ok(inventory);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting inventory.");
                return StatusCode(500, new { message = "An error occurred while getting inventory." });
            }
        }

        [HttpPut("update/{id}")]
        public async Task<IActionResult> UpdateInventory(int id, [FromBody] UpdateInventoryDto dto)
        {
            try
            {
                await _inventoryService.UpdateInventoryAsync(id, dto);
                return Ok(new { message = "Inventory updated successfully." });
            }
            catch (NotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating inventory.");
                return StatusCode(500, new { message = "An error occurred while updating inventory." });
            }
        }
    }
}
