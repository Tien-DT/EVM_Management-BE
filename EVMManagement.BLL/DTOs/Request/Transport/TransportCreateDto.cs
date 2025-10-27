using System;
using System.Collections.Generic;
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

        /// <summary>
        /// List of OrderIds to include in this transport
        /// </summary>
        [Required]
        [MinLength(1, ErrorMessage = "At least one order is required")]
        public List<Guid> OrderIds { get; set; } = new List<Guid>();
    }
}

