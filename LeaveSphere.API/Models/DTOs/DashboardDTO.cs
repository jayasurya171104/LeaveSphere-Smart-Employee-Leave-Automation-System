namespace LeaveSphere.API.Models.DTOs
{
    public class DashboardDTO
    {
        public int TotalEmployees { get; set; }
        public int TotalLeaves { get; set; }
        public int PendingLeaves { get; set; }
        public int ApprovedLeaves { get; set; }
        public int RejectedLeaves { get; set; }
    }
}