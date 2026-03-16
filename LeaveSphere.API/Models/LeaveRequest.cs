using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System;
using System.Collections.Generic;

namespace LeaveSphere.API.Models
{
    public class LeaveRequest
    {
        [Key]
        public int LeaveId { get; set; }

        public int? EmployeeId { get; set; }

        [ForeignKey("EmployeeId")]
        public Employee? Employee { get; set; }

        public int? TeamLeaderId { get; set; }

        [ForeignKey("TeamLeaderId")]
        public TeamLeader? TeamLeader { get; set; }

        public int DepartmentId { get; set; }

        [Required]
        [MaxLength(20)]
        public string Role { get; set; } = "Employee";

        [MaxLength(100)]
        public string? ApprovedBy { get; set; }

        [Required]
        [MaxLength(50)]
        public required string LeaveType { get; set; }

        [Required]
        public DateTime StartDate { get; set; }

        [Required]
        public DateTime EndDate { get; set; }

        [Required]
        [MaxLength(500)]
        public required string Reason { get; set; }

        public string? Status { get; set; } = "Pending";

        public DateTime AppliedDate { get; set; } = DateTime.Now;
    }
}