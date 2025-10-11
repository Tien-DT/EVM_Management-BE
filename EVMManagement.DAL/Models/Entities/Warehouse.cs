using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using EVMManagement.DAL.Models.Enums;

namespace EVMManagement.DAL.Models.Entities
{
    public class Warehouse : BaseEntity
    {
        public Guid? DealerId { get; set; }

        [Required]
        [MaxLength(255)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(255)]
        public string? Address { get; set; }

        public int? Capacity { get; set; }

        public WarehouseType Type { get; set; }
        public virtual Dealer? Dealer { get; set; }
        public virtual ICollection<Vehicle> Vehicles { get; set; } = new HashSet<Vehicle>();
    }
}
