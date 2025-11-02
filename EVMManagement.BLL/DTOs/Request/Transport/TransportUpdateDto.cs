using System;
using System.ComponentModel.DataAnnotations;
using EVMManagement.DAL.Models.Enums;

namespace EVMManagement.BLL.DTOs.Request.Transport
{
    public class TransportUpdateDto
    {
        [MaxLength(255)]
        public string? ProviderName { get; set; }

        [MaxLength(255)]
        public string? PickupLocation { get; set; }

        [MaxLength(255)]
        public string? DropoffLocation { get; set; }

        public TransportStatus? Status { get; set; }

        public DateTime? ScheduledPickupAt { get; set; }

        public DateTime? DeliveredAt { get; set; }

        public Guid? OrderId { get; set; }
    }
}

