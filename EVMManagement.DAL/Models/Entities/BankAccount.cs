using System;
using System.ComponentModel.DataAnnotations;

namespace EVMManagement.DAL.Models.Entities
{
    public class BankAccount : BaseEntity
    {
        public Guid? DealerId { get; set; }

        [MaxLength(255)]
        public string? BankName { get; set; }

        [MaxLength(50)]
        public string? AccountNumber { get; set; }

        [MaxLength(255)]
        public string? AccountOwner { get; set; }

        public bool IsActive { get; set; } = true;

        public virtual Dealer? Dealer { get; set; }
    }
}
