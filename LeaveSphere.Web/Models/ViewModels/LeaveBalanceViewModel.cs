namespace LeaveSphere.Web.Models.ViewModels
{
    public class LeaveBalanceViewModel
    {
        public int BalanceId { get; set; }
        public int EmployeeId { get; set; }
        public int TotalLeaves { get; set; }
        public int UsedLeaves { get; set; }
        public int RemainingLeaves { get; set; }
    }
}
