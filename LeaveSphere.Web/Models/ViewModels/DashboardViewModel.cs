using System.Collections.Generic;

namespace LeaveSphere.Web.Models.ViewModels
{
    public class DashboardViewModel
    {
        public int TotalEmployees { get; set; }
        public int TotalLeaves { get; set; }
        public int Pending { get; set; }
        public int Approved { get; set; }
        public int Rejected { get; set; }

        public List<EmployeeSummary>? DeptEmployees { get; set; }
        public List<RecentLeaveSummary>? RecentLeaves { get; set; }
    }

    public class EmployeeSummary
    {
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Department { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
    }

    public class RecentLeaveSummary
    {
        public string EmployeeName { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string Reason { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
    }
}