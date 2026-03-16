using System;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LeaveSphere.API.Data;
using LeaveSphere.API.Models;
using System.Linq;

namespace LeaveSphere.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin")]
    public class DepartmentController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public DepartmentController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult GetDepartments()
        {
            var departments = _context.Departments.ToList();
            return Ok(departments);
        }

        [HttpGet("{id}")]
        public IActionResult GetDepartment(int id)
        {
            var department = _context.Departments.Find(id);
            if (department == null) return NotFound("Department not found");
            return Ok(department);
        }

        [HttpPost]
        public IActionResult CreateDepartment([FromBody] Department department)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            _context.Departments.Add(department);
            _context.SaveChanges();

            return Ok(new { message = "Department Created Successfully" });
        }

        [HttpPut("{id}")]
        public IActionResult UpdateDepartment(int id, [FromBody] Department department)
        {
            if (id != department.DepartmentId) return BadRequest("ID mismatch");

            var existingDept = _context.Departments.Find(id);
            if (existingDept == null) return NotFound("Department not found");

            existingDept.DepartmentName = department.DepartmentName;

            _context.Entry(existingDept).State = EntityState.Modified;
            _context.SaveChanges();

            return Ok(new { message = "Department Updated Successfully" });
        }

        [HttpDelete("{id}")]
        public IActionResult DeleteDepartment(int id)
        {
            try
            {
                var dept = _context.Departments
                    .Include(d => d.Employees)
                    .Include(d => d.TeamLeader)
                    .FirstOrDefault(d => d.DepartmentId == id);
                if (dept == null) return NotFound("Department not found");

                if (dept.Employees != null && dept.Employees.Any())
                {
                    return BadRequest("Cannot delete department because it contains employees. Please reassignment them first.");
                }

                if (dept.TeamLeader != null)
                {
                    return BadRequest("Cannot delete department because it has a Team Leader. Please remove the Team Leader first.");
                }

                _context.Departments.Remove(dept);
                _context.SaveChanges();
                return Ok(new { message = "Department Deleted Successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
    }
}
