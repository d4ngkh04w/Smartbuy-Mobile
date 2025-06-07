using api.DTOs.Chatbot;

namespace api.Interfaces.Services
{
    public interface IChatbotService
    {
        Task<ChatResponseDTO> ProcessMessageAsync(ChatMessageDTO messageDTO);
        List<string> GetQuickQuestions();
        Task<ProductContextDTO> GetProductContextAsync();
        Task<string> GenerateResponseAsync(string message, ProductContextDTO? context = null);
    }
}
