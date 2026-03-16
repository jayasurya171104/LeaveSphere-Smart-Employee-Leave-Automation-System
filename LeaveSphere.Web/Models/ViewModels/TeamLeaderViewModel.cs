using System;

namespace LeaveSphere.Web.Models.ViewModels
{
    public class TeamLeaderViewModel
    {
        public int TeamLeaderId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Role { get; set; } = "TeamLeader";
        public DateTime DateOfJoining { get; set; }
        public int DepartmentId { get; set; }
        public DepartmentViewModel? Department { get; set; }
        public LeaveBalanceViewModel? LeaveBalance { get; set; }
        
        // For UI purposes
        public string? Password { get; set; }
        public string? PasswordHash { get; set; }
    }
}
