using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace EVMManagement.DAL.Models.Entities
{
    public class Dealer : BaseEntity
    {
        [Required]
        [MaxLength(255)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(255)]
        public string? Address { get; set; }

        [MaxLength(20)]
        public string? Phone { get; set; }

        [Required]
        [MaxLength(255)]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        public DateTime? EstablishedAt { get; set; }

        public bool IsActive { get; set; } = true;

        public virtual ICollection<UserProfile> UserProfiles { get; set; } = new HashSet<UserProfile>();
        public virtual ICollection<Warehouse> Warehouses { get; set; } = new HashSet<Warehouse>();
        public virtual ICollection<Order> Orders { get; set; } = new HashSet<Order>();
        public virtual DealerContract? DealerContract { get; set; }
        public virtual ICollection<BankAccount> BankAccounts { get; set; } = new HashSet<BankAccount>();
        public virtual ICollection<VehicleTimeSlot> VehicleTimeSlots { get; set; } = new HashSet<VehicleTimeSlot>();
        public virtual ICollection<Report> Reports { get; set; } = new HashSet<Report>();
    }
}
