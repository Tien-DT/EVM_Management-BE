using System;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using EVMManagement.DAL.Models.Enums;

namespace EVMManagement.BLL.DTOs.Request.Chatbot
{
    public class ChatRequestDto
    {
        [Required(ErrorMessage = "Nội dung tin nhắn là bắt buộc")]
        public string Message { get; set; } = string.Empty;

        [JsonIgnore]
        public Guid? DealerId { get; set; }

        [JsonIgnore]
        public Guid? UserId { get; set; }

        [JsonIgnore]
        public AccountRole? UserRole { get; set; }
    }
}
