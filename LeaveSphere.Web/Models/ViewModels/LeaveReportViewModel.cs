using System;

namespace LeaveSphere.Web.Models.ViewModels
{
    public class LeaveReportViewModel
    {
        public string DepartmentName { get; set; } = string.Empty;
        public string EmployeeName { get; set; } = string.Empty;
        public DateTime LeaveDate { get; set; }
        public string LeaveType { get; set; } = string.Empty;
        public string Status { get; set; } = "Pending";
    }
}
