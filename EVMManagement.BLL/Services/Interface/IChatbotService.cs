using System;
using System.Threading.Tasks;
using EVMManagement.BLL.DTOs.Request.Chatbot;
using EVMManagement.BLL.DTOs.Response.Chatbot;

namespace EVMManagement.BLL.Services.Interface
{
    public interface IChatbotService
    {
        Task<ChatResponseDto> ProcessChatAsync(ChatRequestDto request);
    }
}
