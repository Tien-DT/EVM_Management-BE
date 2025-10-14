using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace EVMManagement.DAL.Models.Entities
{
    public class UserProfile : BaseEntity
    {
        [Required]
        public Guid AccountId { get; set; }

        public Guid? DealerId { get; set; }

        [Required]
        [MaxLength(255)]
        public string FullName { get; set; } = string.Empty;

        [MaxLength(20)]
        public string? Phone { get; set; }

        [MaxLength(50)]
        public string? CardId { get; set; }
        public virtual Account Account { get; set; } 
        public virtual Dealer? Dealer { get; set; }
        public virtual ICollection<Quotation> CreatedQuotations { get; set; } = new HashSet<Quotation>();
        public virtual ICollection<Order> CreatedOrders { get; set; } = new HashSet<Order>();
        public virtual ICollection<Contract> CreatedContracts { get; set; } = new HashSet<Contract>();
        public virtual ICollection<DealerContract> SignedDealerContractsAsDealer { get; set; } = new HashSet<DealerContract>();
        public virtual ICollection<DealerContract> SignedDealerContractsAsEVM { get; set; } = new HashSet<DealerContract>();
        public virtual ICollection<TestDriveBooking> AssistedTestDriveBookings { get; set; } = new HashSet<TestDriveBooking>();
    }
}
