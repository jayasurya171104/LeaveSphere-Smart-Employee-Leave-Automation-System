using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System;
using System.Collections.Generic;

namespace LeaveSphere.API.Models
{
    public class Employee
    {
        [Key]
        public int EmployeeId { get; set; }

        [Required]
        [MaxLength(100)]
        public string Name { get; set; }

        [Required]
        [MaxLength(100)]
        public string Email { get; set; }

        public string? PasswordHash { get; set; }

        [Required]
        public string Role { get; set; } = "Employee";

        public DateTime DateOfJoining { get; set; } = DateTime.Now;

        // Navigation Properties
        [System.Text.Json.Serialization.JsonIgnore]
        public ICollection<LeaveRequest>? LeaveRequests { get; set; }
        public LeaveBalance? LeaveBalance { get; set; }

        public int DepartmentId { get; set; }
        public Department? Department { get; set; }
    }
}