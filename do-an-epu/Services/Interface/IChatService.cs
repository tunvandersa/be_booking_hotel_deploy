namespace do_an_epu.Services.Interface
{
    public interface IChatService
    {
        Task<string> GetResponseAsync(string message, List<do_an_epu.DTOs.Chat.ChatMessageDto> history);
    }
}
