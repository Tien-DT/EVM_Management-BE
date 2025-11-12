using System;
using System.ComponentModel.DataAnnotations;
using EVMManagement.DAL.Models.Enums;

namespace EVMManagement.BLL.DTOs.Request.Chatbot
{
    public class ChatRequestDto
    {
        [Required(ErrorMessage = "Nội dung tin nhắn là bắt buộc")]
        public string Message { get; set; } = string.Empty;

        public string? SessionId { get; set; }

        public Guid? DealerId { get; set; }

        public Guid? UserId { get; set; }

        public AccountRole? UserRole { get; set; }
    }
}
