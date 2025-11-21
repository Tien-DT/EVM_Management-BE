using System;
using System.Collections.Generic;

namespace EVMManagement.BLL.DTOs.Response.Chatbot
{
    public class ChatResponseDto
    {
        public string Response { get; set; } = string.Empty;

        public List<string>? FunctionsCalled { get; set; }

        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }
}
