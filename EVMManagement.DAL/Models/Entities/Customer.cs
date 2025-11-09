using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace EVMManagement.DAL.Models.Entities
{
    public class Customer : BaseEntity
    {
        [MaxLength(255)]
        public string? FullName { get; set; }

        [Required]
        [MaxLength(20)]
        public string Phone { get; set; } = string.Empty;

        [MaxLength(255)]
        [EmailAddress]
        public string? Email { get; set; }

        [MaxLength(20)]
        public string? Gender { get; set; }

        [MaxLength(255)]
        public string? Address { get; set; }

        public DateTime? Dob { get; set; }

        [MaxLength(50)]
        public string? CardId { get; set; }

        public Guid? DealerId { get; set; }
        public Guid? ManagedBy { get; set; }

        public virtual Dealer? Dealer { get; set; }
        public virtual Account? ManagedByAccount { get; set; }
        public virtual ICollection<Quotation> Quotations { get; set; } = new HashSet<Quotation>();
        public virtual ICollection<Order> Orders { get; set; } = new HashSet<Order>();
        public virtual ICollection<Contract> Contracts { get; set; } = new HashSet<Contract>();
        public virtual ICollection<TestDriveBooking> TestDriveBookings { get; set; } = new HashSet<TestDriveBooking>();
    }
}
