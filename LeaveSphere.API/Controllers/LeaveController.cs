using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using LeaveSphere.API.Services;
using LeaveSphere.API.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

using LeaveSphere.API.Data;
using System.Linq;
using Microsoft.EntityFrameworkCore;

namespace LeaveSphere.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class LeaveController : ControllerBase
    {
        private readonly ILeaveService _service;
        private readonly ApplicationDbContext _context;

        public LeaveController(ILeaveService service, ApplicationDbContext context)
        {
            _service = service;
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllLeaves()
        {
            var roleClaim = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value    
                            ?? User.Claims.FirstOrDefault(c => c.Type == "role")?.Value;
            var email = User.FindFirst(System.Security.Claims.ClaimTypes.Name)?.Value ?? User.Identity?.Name;

            var allLeaves = await _service.GetLeavesAsync();

            if (roleClaim == "Admin")
            {
                return Ok(allLeaves);
            }
            else if (roleClaim == "TeamLeader")
            {
                var tl = _context.TeamLeaders.FirstOrDefault(t => t.Email == email);
                if (tl == null) return Unauthorized("Team Leader not found");

                // Filter: Employees in their department
                var filteredLeaves = allLeaves.Where(l => l.DepartmentId == tl.DepartmentId && l.Role == "Employee").ToList();
                return Ok(filteredLeaves);
            }
            
            return Ok(new List<LeaveRequest>());
        }

        [HttpGet("my")]
        [Authorize(Roles = "Employee,TeamLeader")]
        public async Task<IActionResult> GetMyLeaves()
        {
            var email = User.FindFirst(System.Security.Claims.ClaimTypes.Name)?.Value 
                        ?? User.Identity?.Name;
            var roleClaim = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value;
            
            Console.WriteLine($"[MyLeaves] Fetching for: {email}, Role: {roleClaim}");

            var allLeaves = await _service.GetLeavesAsync();

            if (roleClaim == "TeamLeader")
            {
                var tl = _context.TeamLeaders.FirstOrDefault(e => e.Email == email);
                if (tl == null) return Unauthorized("User not found");
                
                var myLeaves = allLeaves.Where(l => l.TeamLeaderId == tl.TeamLeaderId).ToList();
                return Ok(myLeaves);
            }
            else 
            {
                var emp = _context.Employees.FirstOrDefault(e => e.Email == email);
                if (emp == null) return Unauthorized("User not found");

                var myLeaves = allLeaves.Where(l => l.EmployeeId == emp.EmployeeId).ToList();
                return Ok(myLeaves);
            }
        }

        [HttpPost("apply")]
        [Authorize(Roles = "Employee,TeamLeader")]
        public async Task<IActionResult> ApplyLeave([FromBody] LeaveRequest? leave)
        {
            Console.WriteLine("ApplyLeave reached!");
            
            if (leave == null) return BadRequest(new { Message = "Empty or invalid leave request body." });
            if (!ModelState.IsValid) return BadRequest(new { Message = "Validation failed" });

            if (leave.StartDate.Date < DateTime.Today || leave.EndDate.Date < DateTime.Today)
                return BadRequest(new { Message = "You cannot apply leave for past dates." });

            if (leave.EndDate.Date < leave.StartDate.Date)
                return BadRequest(new { Message = "End date cannot be earlier than start date." });

            var email = User.FindFirst(System.Security.Claims.ClaimTypes.Name)?.Value ?? User.Identity?.Name;
            var roleClaim = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value;

            if (roleClaim == "TeamLeader")
            {
                var tl = _context.TeamLeaders.FirstOrDefault(e => e.Email == email);
                if (tl == null) return Unauthorized("User not found");
                
                leave.TeamLeaderId = tl.TeamLeaderId;
                leave.DepartmentId = tl.DepartmentId;
                leave.Role = "TeamLeader";
            }
            else
            {
                var emp = _context.Employees.FirstOrDefault(e => e.Email == email);
                if (emp == null) return Unauthorized("User not found");

                leave.EmployeeId = emp.EmployeeId;
                leave.DepartmentId = emp.DepartmentId;
                leave.Role = "Employee";
            }

            leave.AppliedDate = DateTime.Now;
            leave.Status = "Pending";

            try
            {
                await _service.ApplyLeaveAsync(leave);
                Console.WriteLine($"Leave Saved successfully for {(leave.Role == "TeamLeader" ? "TeamLeaderId: " + leave.TeamLeaderId : "EmployeeId: " + leave.EmployeeId)}");
            }
            catch(Exception ex)
            {
                Console.WriteLine($"Error saving leave: {ex.Message}");
                if (ex.InnerException != null) Console.WriteLine($"Inner Ex: {ex.InnerException.Message}");
                return StatusCode(500, new { Message = "Error saving to database", Details = ex.Message });
            }

            return Ok(new { Message = "Leave Applied Successfully" });
        }

        [HttpGet("calendar")]
        public async Task<IActionResult> GetCalendarData()
        {
            var allLeaves = await _service.GetLeavesAsync();

            // Check role
            var roleClaim = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value
                            ?? User.Claims.FirstOrDefault(c => c.Type == "role")?.Value;

            IEnumerable<LeaveRequest> leaves;

            if (roleClaim == "Admin")
            {
                leaves = allLeaves;
            }
            else if (roleClaim == "TeamLeader")
            {
                var email = User.FindFirst(System.Security.Claims.ClaimTypes.Name)?.Value ?? User.Identity?.Name;
                var tl = _context.TeamLeaders.FirstOrDefault(e => e.Email == email);
                if (tl == null) return Unauthorized("User not found");
                
                // TL sees their own leaves + their department's employee leaves
                leaves = allLeaves.Where(l => l.TeamLeaderId == tl.TeamLeaderId || (l.DepartmentId == tl.DepartmentId && l.Role == "Employee"));
            }
            else // Employee
            {
                var email = User.FindFirst(System.Security.Claims.ClaimTypes.Name)?.Value ?? User.Identity?.Name;
                var emp = _context.Employees.FirstOrDefault(e => e.Email == email);
                if (emp == null) return Unauthorized("Employee not found");
                
                leaves = allLeaves.Where(l => l.EmployeeId == emp.EmployeeId);
            }

            var events = leaves.Select(l => new
            {
                id = l.LeaveId,
                title = (l.Employee != null ? l.Employee.Name : (l.TeamLeader != null ? l.TeamLeader.Name : "Unknown")) + " - " + l.LeaveType,
                start = l.StartDate.ToString("yyyy-MM-dd"),
                end = l.EndDate.AddDays(1).ToString("yyyy-MM-dd"), // FullCalendar end is exclusive
                color = l.Status == "Approved" ? "#28a745" :
                        l.Status == "Rejected" ? "#dc3545" : "#fd7e14",
                extendedProps = new
                {
                    employeeName = (l.Employee?.Name ?? l.TeamLeader?.Name) ?? "Unknown",
                    leaveType = l.LeaveType,
                    startDate = l.StartDate.ToString("dd MMM yyyy"),
                    endDate = l.EndDate.ToString("dd MMM yyyy"),
                    status = l.Status,
                    reason = l.Reason
                }
            });

            return Ok(events);
        }

        [HttpPut("approve/{id}")]
        [Authorize(Roles = "Admin,TeamLeader")]
        public async Task<IActionResult> ApproveLeave(int id)
        {
            var leave = _context.LeaveRequests.Find(id);
            if (leave == null) return NotFound();

            var roleClaim = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value;
            var email = User.FindFirst(System.Security.Claims.ClaimTypes.Name)?.Value ?? User.Identity?.Name;

            if (roleClaim == "TeamLeader")
            {
                var tl = _context.TeamLeaders.FirstOrDefault(t => t.Email == email);
                if (tl == null) return Unauthorized("Team Leader not found");

                if (leave.Role == "TeamLeader") return Forbid("Team Leaders cannot approve other Team Leaders' leaves.");
                if (leave.DepartmentId != tl.DepartmentId) return Forbid("You can only approve leaves for employees in your department.");
            }

            await _service.ApproveLeaveAsync(id);
            return Ok("Leave Approved");
        }

        [HttpPut("reject/{id}")]
        [Authorize(Roles = "Admin,TeamLeader")]
        public async Task<IActionResult> RejectLeave(int id)
        {
            var leave = _context.LeaveRequests.Find(id);
            if (leave == null) return NotFound();

            var roleClaim = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value;
            var email = User.FindFirst(System.Security.Claims.ClaimTypes.Name)?.Value ?? User.Identity?.Name;

            if (roleClaim == "TeamLeader")
            {
                var tl = _context.TeamLeaders.FirstOrDefault(t => t.Email == email);
                if (tl == null) return Unauthorized("Team Leader not found");

                if (leave.Role == "TeamLeader") return Forbid("Team Leaders cannot reject other Team Leaders' leaves.");
                if (leave.DepartmentId != tl.DepartmentId) return Forbid("You can only reject leaves for employees in your department.");
            }

            await _service.RejectLeaveAsync(id);
            return Ok("Leave Rejected");
        }

        [HttpGet("report")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetLeaveReport(
            [FromQuery] string? role, 
            [FromQuery] int? departmentId, 
            [FromQuery] DateTime? startDate, 
            [FromQuery] DateTime? endDate, 
            [FromQuery] string? status)
        {
            var query = _context.LeaveRequests
                .Include(l => l.Employee)
                    .ThenInclude(e => e!.Department)
                .Include(l => l.TeamLeader)
                    .ThenInclude(t => t!.Department)
                .AsQueryable();

            if (!string.IsNullOrEmpty(role))
            {
                query = query.Where(l => l.Role == role);
            }

            if (departmentId.HasValue && departmentId > 0)
            {
                query = query.Where(l => l.DepartmentId == departmentId);
            }

            if (startDate.HasValue)
            {
                query = query.Where(l => l.StartDate >= startDate.Value);
            }

            if (endDate.HasValue)
            {
                query = query.Where(l => l.EndDate <= endDate.Value);
            }

            if (!string.IsNullOrEmpty(status))
            {
                // Status is case-insensitive usually, but let's be careful
                query = query.Where(l => l.Status == status);
            }

            var reportData = await query
                .OrderByDescending(l => l.AppliedDate)
                .ToListAsync();

            return Ok(reportData);
        }

        [HttpGet("check-conflict")]
        [Authorize(Roles = "Employee,TeamLeader")]
        public async Task<IActionResult> CheckConflict([FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
        {
            var email = User.FindFirst(System.Security.Claims.ClaimTypes.Name)?.Value ?? User.Identity?.Name;
            var roleClaim = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value;

            int departmentId = 0;
            if (roleClaim == "TeamLeader")
            {
                var tl = _context.TeamLeaders.FirstOrDefault(e => e.Email == email);
                if (tl == null) return Unauthorized("User not found");
                departmentId = tl.DepartmentId;
            }
            else
            {
                var emp = _context.Employees.FirstOrDefault(e => e.Email == email);
                if (emp == null) return Unauthorized("User not found");
                departmentId = emp.DepartmentId;
            }

            bool hasConflict = await _service.HasLeaveConflictAsync(departmentId, startDate, endDate);
            
            return Ok(new { Conflict = hasConflict, Message = hasConflict ? "Too many employees from your department are already on leave for these dates." : "" });
        }
    }
}