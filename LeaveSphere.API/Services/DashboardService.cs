using LeaveSphere.API.Data;
using LeaveSphere.API.Models.DTOs;
using System.Linq;
using Microsoft.EntityFrameworkCore;
namespace LeaveSphere.API.Services
{
    public class DashboardService
    {
        private readonly ApplicationDbContext _context;

        public DashboardService(ApplicationDbContext context)
        {
            _context = context;
        }

        public DashboardDTO GetSummary()
        {
            return new DashboardDTO
            {
                TotalEmployees = _context.Employees.Count(),
                TotalLeaves = _context.LeaveRequests.Count(),
                PendingLeaves = _context.LeaveRequests.Count(l => l.Status == "Pending"),
                ApprovedLeaves = _context.LeaveRequests.Count(l => l.Status == "Approved"),
                RejectedLeaves = _context.LeaveRequests.Count(l => l.Status == "Rejected")
            };
        }
    }
}