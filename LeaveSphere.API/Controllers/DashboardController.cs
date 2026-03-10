using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using LeaveSphere.API.Data;
using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

using System.Linq;

namespace LeaveSphere.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin")]
    public class DashboardController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public DashboardController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet("summary")]
        public IActionResult GetSummary()
        {
            var totalEmployees = _context.Employees.Count();
            var totalLeaves = _context.LeaveRequests.Count();
            var pending = _context.LeaveRequests.Count(l => l.Status == "Pending");
            var approved = _context.LeaveRequests.Count(l => l.Status == "Approved");
            var rejected = _context.LeaveRequests.Count(l => l.Status == "Rejected");

            return Ok(new
            {
                totalEmployees,
                totalLeaves,
                pending,
                approved,
                rejected
            });
        }
    }
}