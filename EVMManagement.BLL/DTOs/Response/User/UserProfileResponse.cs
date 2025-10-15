using EVMManagement.DAL.Models.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EVMManagement.BLL.DTOs.Response.User
{
    public class AccountDto
    {
        public AccountRole Role { get; set; }
        public bool IsActive { get; set; }
    }
    public class DealerDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }
    public class UserProfileResponse
    {
        public Guid Id { get; set; }
        public Guid AccountId { get; set; }
        public Guid? DealerId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string? Phone { get; set; }
        public string? CardId { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public DateTime? DeletedDate { get; set; }
        public bool IsDeleted { get; set; }
        public DealerDto? Dealer { get; set; }
        public AccountDto? Account { get; set; }
    }
}
