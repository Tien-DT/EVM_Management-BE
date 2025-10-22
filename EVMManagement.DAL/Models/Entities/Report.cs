using System;
using System.ComponentModel.DataAnnotations;

namespace EVMManagement.DAL.Models.Entities
{
    public class Report : BaseEntity
    {
        [Required]
        public Guid AccountId { get; set; }

        public Guid? DealerId { get; set; }

        [MaxLength(100)]
        public string? Type { get; set; }

        [MaxLength(255)]
        public string? Title { get; set; }

        public string? Content { get; set; }

        public Guid? OrderId { get; set; }

        public Guid? TransportId { get; set; }

        public virtual Account Account { get; set; } = null!;
        public virtual Dealer? Dealer { get; set; }
        public virtual Order? Order { get; set; }
        public virtual Transport? Transport { get; set; }
    }
}
