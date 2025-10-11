using System;
using System.ComponentModel.DataAnnotations;

namespace EVMManagement.DAL.Models.Entities
{
    public class HandoverRecord : BaseEntity
    {
        [Required]
        public Guid OrderId { get; set; }

        [Required]
        public Guid VehicleId { get; set; }

        public Guid? TransportDetailId { get; set; }

        public string? Notes { get; set; }

        public bool IsAccepted { get; set; } = false;

        public DateTime? HandoverDate { get; set; }

        public virtual Order Order { get; set; } = null!;
        public virtual Vehicle Vehicle { get; set; } = null!;
        public virtual TransportDetail? TransportDetail { get; set; }
    }
}
