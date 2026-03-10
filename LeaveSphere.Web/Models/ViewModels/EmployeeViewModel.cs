using System;

namespace LeaveSphere.Web.Models.ViewModels
{
    public class EmployeeViewModel
    {
        public int EmployeeId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Role { get; set; } = "Employee";
        public DateTime DateOfJoining { get; set; }
        public int DepartmentId { get; set; }
        public DepartmentViewModel? Department { get; set; }
        public LeaveBalanceViewModel? LeaveBalance { get; set; }
    }
}
