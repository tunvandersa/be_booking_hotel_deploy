using do_an_epu.DTOs.Chat;
using do_an_epu.DTOs.Common;
using do_an_epu.Services.Interface;
using Microsoft.AspNetCore.Mvc;

namespace do_an_epu.Controllers.Client
{
    [Route("api/client/chat")]
    [ApiController]
    public class ChatController : ControllerBase
    {
        private readonly IChatService _chatService;

        public ChatController(IChatService chatService)
        {
            _chatService = chatService;
        }

        [HttpPost("ask")]
        public async Task<IActionResult> Ask([FromBody] ChatRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Message))
            {
                return BadRequest(ApiResponse<object>.Fail("Tin nhắn không được để trống"));
            }

            var response = await _chatService.GetResponseAsync(request.Message, request.History ?? new List<ChatMessageDto>());
            
            return Ok(ApiResponse<GeminiResponse>.Ok(new GeminiResponse { Answer = response }));
        }
    }
}
