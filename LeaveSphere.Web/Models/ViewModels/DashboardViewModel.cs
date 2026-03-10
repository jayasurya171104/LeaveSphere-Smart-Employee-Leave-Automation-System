namespace LeaveSphere.Web.Models.ViewModels
{
    public class DashboardViewModel
    {
        public int TotalEmployees { get; set; }
        public int TotalLeaves { get; set; }
        public int Pending { get; set; }
        public int Approved { get; set; }
        public int Rejected { get; set; }
    }
}