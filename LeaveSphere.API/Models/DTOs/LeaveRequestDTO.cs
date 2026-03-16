using System.ComponentModel.DataAnnotations;
using System;

namespace LeaveSphere.API.Models.DTOs
{
    public class LeaveRequestDTO
    {
        [Required]
        public int EmployeeId { get; set; }

        [Required]
        public required string LeaveType { get; set; }

        [Required]
        public DateTime StartDate { get; set; }

        [Required]
        public DateTime EndDate { get; set; }

        [Required]
        public required string Reason { get; set; }
    }
}