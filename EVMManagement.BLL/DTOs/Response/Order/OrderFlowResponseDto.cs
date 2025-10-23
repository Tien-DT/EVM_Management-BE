using System;
using EVMManagement.DAL.Models.Enums;

namespace EVMManagement.BLL.DTOs.Response.Order
{
    public class OrderFlowResponseDto
    {
        public Guid OrderId { get; set; }

        public OrderStatus Status { get; set; }

        public string Message { get; set; } = string.Empty;

        public bool Success { get; set; }

        public object? Data { get; set; }
    }
}

