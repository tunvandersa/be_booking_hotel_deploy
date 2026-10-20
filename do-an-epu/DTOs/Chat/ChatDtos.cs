namespace do_an_epu.DTOs.Chat
{
    public class ChatRequest
    {
        public string Message { get; set; }
        public List<ChatMessageDto> History { get; set; }
    }

    public class ChatMessageDto
    {
        public string Role { get; set; } // "user" or "model"
        public string Content { get; set; }
    }

    public class GeminiResponse
    {
        public string Answer { get; set; }
    }
}
