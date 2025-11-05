using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using EVMManagement.DAL.Models.Enums;

namespace EVMManagement.DAL.Models.Entities
{
    public class Account : BaseEntity
    {
        [Required]
        [MaxLength(255)]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        [MaxLength(255)]
        public string PasswordHash { get; set; } = string.Empty;

        public bool IsPasswordChange { get; set; }

        public bool IsActive { get; set; } = true;

        [Required]
        public AccountRole Role { get; set; }

        public virtual UserProfile? UserProfile { get; set; }
        public virtual ICollection<Deposit> ReceivedDeposits { get; set; } = new HashSet<Deposit>();
        public virtual ICollection<Report> Reports { get; set; } = new HashSet<Report>();
    }
}
