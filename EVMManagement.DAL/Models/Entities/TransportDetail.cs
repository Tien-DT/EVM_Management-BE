using System;
using System.ComponentModel.DataAnnotations;

namespace EVMManagement.DAL.Models.Entities
{
    public class TransportDetail : BaseEntity
    {
        [Required]
        public Guid TransportId { get; set; }

        [Required]
        public Guid VehicleId { get; set; }

        public Guid? OrderId { get; set; }
        public virtual Transport Transport { get; set; } = null!;
        public virtual Vehicle Vehicle { get; set; } = null!;
        public virtual Order? Order { get; set; }
        public virtual HandoverRecord? HandoverRecord { get; set; }
    }
}
