using System;
using System.ComponentModel.DataAnnotations;

namespace EVMManagement.BLL.DTOs.Request.Transport
{
    public class AddTransportToWarehouseDto
    {
        [Required(ErrorMessage = "Mã vận chuyển là bắt buộc")]
        public Guid TransportId { get; set; }

        [Required(ErrorMessage = "Mã kho hàng là bắt buộc")]
        public Guid WarehouseId { get; set; }
    }
}
