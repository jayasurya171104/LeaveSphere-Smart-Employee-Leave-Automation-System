using System;

namespace LeaveSphere.Web.Models.ViewModels
{
    public class LeaveViewModel
    {
        public int LeaveId { get; set; }     // 🔥 Important
        public int EmployeeId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }

        public string LeaveType { get; set; } = string.Empty;
        public string Reason { get; set; } = string.Empty;

        public int? TeamLeaderId { get; set; }
        public TeamLeaderViewModel? TeamLeader { get; set; }

        public int DepartmentId { get; set; }

        public string Role { get; set; } = "Employee";

        public string? ApprovedBy { get; set; }
        public string Status { get; set; } = string.Empty;   // 🔥 Important
        public EmployeeViewModel? Employee { get; set; }
    }
}