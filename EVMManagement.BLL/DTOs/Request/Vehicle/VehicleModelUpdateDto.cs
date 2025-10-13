using EVMManagement.DAL.Models.Enums;
using System;
using System.ComponentModel.DataAnnotations;

namespace EVMManagement.BLL.DTOs.Request.Vehicle
{
    public class VehicleModelUpdateDto
    {
        [Required]
        [MaxLength(50)]
        public string Code { get; set; } = string.Empty;

        [Required]
        [MaxLength(255)]
        public string Name { get; set; } = string.Empty;

        public DateTime? LaunchDate { get; set; }

        [MaxLength(1000)]
        public string? Description { get; set; }

        public bool Status { get; set; } = true;
        public VehicleModelRanking? Ranking { get; set; }
    }
}
