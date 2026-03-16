using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LeaveSphere.API.Models.DTOs;
using LeaveSphere.API.Data;
using LeaveSphere.API.Models;
using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

using System.Linq;

namespace LeaveSphere.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin,TeamLeader")]
    public class EmployeeController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public EmployeeController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet] // GET /api/employee
        public IActionResult GetEmployees()
        {
            var roleClaim = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value;
            var email = User.FindFirst(System.Security.Claims.ClaimTypes.Name)?.Value ?? User.Identity?.Name;

            var query = _context.Employees
                .Include(e => e.Department)
                .Include(e => e.LeaveBalance)
                .AsQueryable();

            if (roleClaim == "TeamLeader")
            {
                var tl = _context.TeamLeaders.FirstOrDefault(t => t.Email == email);
                if (tl == null) return Unauthorized("Team Leader not found");
                query = query.Where(e => e.DepartmentId == tl.DepartmentId);
            }

            return Ok(query.ToList());
        }

        [HttpGet("all")] // GET /api/employee/all
        [Authorize(Roles = "Admin")]
        public IActionResult GetAll()
        {
            var employees = _context.Employees
                .Include(e => e.Department)
                .Include(e => e.LeaveBalance)
                .ToList();
            return Ok(employees);
        }

        [HttpGet("{id}")]
        public IActionResult GetEmployee(int id)
        {
            var emp = _context.Employees
                .Include(e => e.Department)
                .Include(e => e.LeaveBalance)
                .FirstOrDefault(e => e.EmployeeId == id);
            if (emp == null) return NotFound();
            return Ok(emp);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public IActionResult UpdateEmployee(int id, [FromBody] Employee emp)
        {
            if (id != emp.EmployeeId) return BadRequest("ID mismatch");

            var existingEmp = _context.Employees.Find(id);
            if (existingEmp == null) return NotFound();

            existingEmp.Name = emp.Name;
            existingEmp.Email = emp.Email;
            existingEmp.Role = emp.Role;
            existingEmp.DepartmentId = emp.DepartmentId;

            _context.Entry(existingEmp).State = EntityState.Modified;
            _context.SaveChanges();

            return Ok("Employee Updated Successfully");
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteEmployee(int id)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var emp = await _context.Employees
                    .Include(e => e.LeaveRequests)
                    .Include(e => e.LeaveBalance)
                    .FirstOrDefaultAsync(e => e.EmployeeId == id);

                if (emp == null) return NotFound();

                // 1. Remove Leave Requests
                if (emp.LeaveRequests != null)
                {
                    _context.LeaveRequests.RemoveRange(emp.LeaveRequests);
                }

                // 2. Remove Leave Balance
                if (emp.LeaveBalance != null)
                {
                    _context.LeaveBalances.Remove(emp.LeaveBalance);
                }

                // 3. Remove Employee
                _context.Employees.Remove(emp);

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return Ok("Employee Deleted Successfully");
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
    }
}