using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using EVMManagement.DAL.Models.Enums;

namespace EVMManagement.DAL.Models.Entities
{
    public class Transport : BaseEntity
    {
        [MaxLength(255)]
        public string? ProviderName { get; set; }

        [MaxLength(255)]
        public string? PickupLocation { get; set; }

        [MaxLength(255)]
        public string? DropoffLocation { get; set; }

        [Required]
        public TransportStatus Status { get; set; }

        public DateTime? ScheduledPickupAt { get; set; }

        public DateTime? DeliveredAt { get; set; }

        public Guid? OrderId { get; set; }

        public virtual Order? Order { get; set; }
        public virtual ICollection<TransportDetail> TransportDetails { get; set; } = new HashSet<TransportDetail>();
        public virtual ICollection<Report> Reports { get; set; } = new HashSet<Report>();
    }
}
