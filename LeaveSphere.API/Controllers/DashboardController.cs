using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using LeaveSphere.API.Data;
using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace LeaveSphere.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin,TeamLeader")]
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
            var roleClaim = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value;
            var email = User.FindFirst(System.Security.Claims.ClaimTypes.Name)?.Value ?? User.Identity?.Name;

            if (roleClaim == "TeamLeader")
            {
                var tl = _context.TeamLeaders
                    .Include(t => t.Department)
                    .FirstOrDefault(t => t.Email == email);
                if (tl == null) return Unauthorized("Team Leader not found");

                var departmentId = tl.DepartmentId;
                
                // Statistics
                var totalEmployees = _context.Employees.Count(e => e.DepartmentId == departmentId);
                var totalLeaves = _context.LeaveRequests.Count(l => l.TeamLeaderId == tl.TeamLeaderId); // Personal leaves
                var pending = _context.LeaveRequests.Count(l => l.DepartmentId == departmentId && l.Role == "Employee" && l.Status == "Pending");
                var approved = _context.LeaveRequests.Count(l => l.DepartmentId == departmentId && l.Role == "Employee" && l.Status == "Approved");
                var rejected = _context.LeaveRequests.Count(l => l.DepartmentId == departmentId && l.Role == "Employee" && l.Status == "Rejected");

                // Employee List (Direct Dept Employees)
                var deptEmployees = _context.Employees
                    .Where(e => e.DepartmentId == departmentId)
                    .Select(e => new {
                        e.Name,
                        e.Email,
                        Department = e.Department != null ? e.Department.DepartmentName : "N/A",
                        e.Role
                    }).ToList();

                // Recent Leave Requests
                var recentLeaves = _context.LeaveRequests
                    .Include(l => l.Employee)
                    .Where(l => l.DepartmentId == departmentId && l.Role == "Employee")
                    .OrderByDescending(l => l.AppliedDate)
                    .Take(10)
                    .Select(l => new {
                        EmployeeName = l.Employee != null ? l.Employee.Name : "Unknown",
                        l.StartDate,
                        l.EndDate,
                        l.Reason,
                        l.Status
                    }).ToList();

                return Ok(new { 
                    totalEmployees, 
                    totalLeaves, 
                    pending, 
                    approved, 
                    rejected,
                    deptEmployees,
                    recentLeaves
                });
            }
            else // Admin
            {
                var totalEmployees = _context.Employees.Count();
                var totalLeaves = _context.LeaveRequests.Count();
                var pending = _context.LeaveRequests.Count(l => l.Status == "Pending");
                var approved = _context.LeaveRequests.Count(l => l.Status == "Approved");
                var rejected = _context.LeaveRequests.Count(l => l.Status == "Rejected");

                return Ok(new { totalEmployees, totalLeaves, pending, approved, rejected });
            }
        }
    }
}