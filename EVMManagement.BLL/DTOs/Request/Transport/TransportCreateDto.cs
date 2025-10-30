using System;
using System.ComponentModel.DataAnnotations;

namespace EVMManagement.BLL.DTOs.Request.Transport
{
    public class TransportCreateDto
    {
        [MaxLength(255)]
        public string? ProviderName { get; set; }

        [MaxLength(255)]
        public string? PickupLocation { get; set; }

        [MaxLength(255)]
        public string? DropoffLocation { get; set; }

        public DateTime? ScheduledPickupAt { get; set; }

        public Guid? OrderId { get; set; }
    }
}

