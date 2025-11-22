using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EVMManagement.DAL.Models.Entities
{
    public class SystemConfiguration : BaseEntity
    {
        [Required]
        [MaxLength(100)]
        public string Key { get; set; } = string.Empty;

        [Required]
        [MaxLength(500)]
        public string Value { get; set; } = string.Empty;

        [MaxLength(255)]
        public string? Description { get; set; }

        [Required]
        [MaxLength(50)]
        public string DataType { get; set; } = "String";

        [Required]
        public bool IsActive { get; set; } = true;
    }
}
