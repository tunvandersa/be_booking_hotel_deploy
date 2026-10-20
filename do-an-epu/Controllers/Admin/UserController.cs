using do_an_epu.DTOs;
using do_an_epu.Services.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace do_an_epu.Controllers.Admin
{
    [Route("api/admin/users")]
    [ApiController]
    [Authorize(Roles = "Admin")]
    public class UserController : ControllerBase
    {
        private readonly IAdminUserService _userService;

        public UserController(IAdminUserService userService)
        {
            _userService = userService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllUsers()
        {
            var users = await _userService.GetAllUsersAsync();
            return Ok(users);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetUserById(int id)
        {
            var user = await _userService.GetUserByIdAsync(id);
            if (user == null) return NotFound();
            return Ok(user);
        }
       
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateUser(int id, [FromBody] UserDto userDto)
        {
            try {
                var result = await _userService.UpdateUserAsync(id, userDto);
                if (result.Succeeded) return Ok(new { message = "User updated successfully" });
                return BadRequest(result.Errors);
            } catch (Exception ex) {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            try {
                var result = await _userService.DeleteUserAsync(id);
                if (result.Succeeded) return Ok(new { message = "User deleted successfully" });
                return BadRequest(result.Errors);
            } catch (Exception ex) {
                return StatusCode(500, new { message = ex.Message });
            }
        }
    }
}
