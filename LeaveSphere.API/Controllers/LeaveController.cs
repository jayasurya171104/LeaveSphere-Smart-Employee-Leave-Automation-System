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
            var leaves = await _service.GetLeavesAsync();
            return Ok(leaves);
        }

        [HttpGet("my")]
        [Authorize(Roles = "Employee")]
        public async Task<IActionResult> GetMyLeaves()
        {
            var email = User.FindFirst(System.Security.Claims.ClaimTypes.Name)?.Value 
                        ?? User.Identity?.Name;
            
            Console.WriteLine($"[MyLeaves] Fetching for: {email}");

            var emp = _context.Employees.FirstOrDefault(e => e.Email == email);
            if (emp == null) 
            {
                Console.WriteLine("[MyLeaves] User not found in database.");
                return Unauthorized("User not found");
            }

            var allLeaves = await _service.GetLeavesAsync();
            var myLeaves = allLeaves.Where(l => l.EmployeeId == emp.EmployeeId).ToList();
            
            Console.WriteLine($"[MyLeaves] Found {myLeaves.Count} records for {email}");
            return Ok(myLeaves);
        }

        [HttpPost("apply")]
        [Authorize(Roles = "Employee")]
        public async Task<IActionResult> ApplyLeave([FromBody] LeaveRequest? leave)
        {
            Console.WriteLine("ApplyLeave reached!");
            
            if (leave == null)
            {
                Console.WriteLine("ApplyLeave: Received null leave object.");
                return BadRequest(new { Message = "Empty or invalid leave request body." });
            }

            if (!ModelState.IsValid)
            {
                var errors = string.Join(", ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage));
                Console.WriteLine($"ModelState is invalid: {errors}");
                return BadRequest(new { Message = "Validation failed", Errors = errors });
            }

            if (leave.StartDate.Date < DateTime.Today || leave.EndDate.Date < DateTime.Today)
            {
                return BadRequest(new { Message = "You cannot apply leave for past dates." });
            }

            if (leave.EndDate.Date < leave.StartDate.Date)
            {
                return BadRequest(new { Message = "End date cannot be earlier than start date." });
            }

            var email = User.FindFirst(System.Security.Claims.ClaimTypes.Name)?.Value 
                        ?? User.Identity?.Name;
            Console.WriteLine($"ApplyLeave User Identity: {email}");

            var emp = _context.Employees.FirstOrDefault(e => e.Email == email);
            if (emp == null) 
            {
                Console.WriteLine("User not found in DB.");
                return Unauthorized("User not found in database");
            }

            leave.EmployeeId = emp.EmployeeId;
            leave.AppliedDate = DateTime.Now;
            leave.Status = "Pending";

            try
            {
                await _service.ApplyLeaveAsync(leave);
                Console.WriteLine($"Leave Saved successfully for EmployeeId: {emp.EmployeeId}");
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
            else
            {
                var email = User.FindFirst(System.Security.Claims.ClaimTypes.Name)?.Value
                            ?? User.Identity?.Name;
                var emp = _context.Employees.FirstOrDefault(e => e.Email == email);
                if (emp == null) return Unauthorized("User not found");
                leaves = allLeaves.Where(l => l.EmployeeId == emp.EmployeeId);
            }

            var events = leaves.Select(l => new
            {
                id = l.LeaveId,
                title = (l.Employee?.Name ?? "Unknown") + " - " + l.LeaveType,
                start = l.StartDate.ToString("yyyy-MM-dd"),
                end = l.EndDate.AddDays(1).ToString("yyyy-MM-dd"), // FullCalendar end is exclusive
                color = l.Status == "Approved" ? "#28a745" :
                        l.Status == "Rejected" ? "#dc3545" : "#fd7e14",
                extendedProps = new
                {
                    employeeName = l.Employee?.Name ?? "Unknown",
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
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ApproveLeave(int id)
        {
            await _service.ApproveLeaveAsync(id);
            return Ok("Leave Approved");
        }

        [HttpPut("reject/{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> RejectLeave(int id)
        {
            await _service.RejectLeaveAsync(id);
            return Ok("Leave Rejected");
        }

        [HttpGet("report")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetLeaveReport([FromQuery] DateTime date)
        {
            var day = date.Date;
            var leaves = await _context.LeaveRequests
                .Include(l => l.Employee)
                    .ThenInclude(e => e.Department)
                .Where(l => l.StartDate.Date <= day && l.EndDate.Date >= day)
                .ToListAsync();

            var reportData = leaves.Select(l => new
            {
                DepartmentName = l.Employee?.Department?.DepartmentName ?? "N/A",
                EmployeeName = l.Employee?.Name ?? "Unknown",
                LeaveDate = day,
                LeaveType = l.LeaveType,
                Status = l.Status
            });

            return Ok(reportData);
        }

        [HttpGet("check-conflict")]
        [Authorize(Roles = "Employee")]
        public async Task<IActionResult> CheckConflict([FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
        {
            var email = User.FindFirst(System.Security.Claims.ClaimTypes.Name)?.Value ?? User.Identity?.Name;
            var emp = _context.Employees.FirstOrDefault(e => e.Email == email);
            if (emp == null) return Unauthorized("User not found");

            bool hasConflict = await _service.HasLeaveConflictAsync(emp.DepartmentId, startDate, endDate);
            
            return Ok(new { Conflict = hasConflict, Message = hasConflict ? "Too many employees from your department are already on leave for these dates." : "" });
        }
    }
}