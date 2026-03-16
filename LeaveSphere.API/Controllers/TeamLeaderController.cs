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
    public class TeamLeaderController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public TeamLeaderController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult GetTeamLeaders()
        {
            var leaders = _context.TeamLeaders
                .Include(t => t.Department)
                .Include(t => t.LeaveBalance)
                .ToList();
            return Ok(leaders);
        }

        [HttpGet("{id}")]
        public IActionResult GetTeamLeader(int id)
        {
            var leader = _context.TeamLeaders
                .Include(t => t.Department)
                .Include(t => t.LeaveBalance)
                .FirstOrDefault(t => t.TeamLeaderId == id);
            
            if (leader == null) return NotFound();
            return Ok(leader);
        }

        [HttpPost]
        public IActionResult CreateTeamLeader([FromBody] TeamLeader leader)
        {
            if (_context.TeamLeaders.Any(e => e.Email == leader.Email) || _context.Employees.Any(e => e.Email == leader.Email))
                return BadRequest("Email already exists");

            // Check if department already has a team leader
            var existingTL = _context.TeamLeaders.FirstOrDefault(t => t.DepartmentId == leader.DepartmentId);
            if (existingTL != null)
                return BadRequest("Department already has a Team Leader");

            leader.PasswordHash = BCrypt.Net.BCrypt.HashPassword(leader.PasswordHash ?? "Password@123");
            _context.TeamLeaders.Add(leader);
            _context.SaveChanges();

            // Initialize LeaveBalance for new team leader
            var leaveBalance = new LeaveBalance
            {
                TeamLeaderId = leader.TeamLeaderId,
                TotalLeaves = 25, // Give 25 leaves to Team Leaders
                UsedLeaves = 0,
                RemainingLeaves = 25
            };
            _context.LeaveBalances.Add(leaveBalance);
            _context.SaveChanges();

            return Ok("Team Leader Created Successfully");
        }

        [HttpPut("{id}")]
        public IActionResult UpdateTeamLeader(int id, [FromBody] TeamLeader leader)
        {
            if (id != leader.TeamLeaderId) return BadRequest("ID mismatch");

            var existingLeader = _context.TeamLeaders.Find(id);
            if (existingLeader == null) return NotFound();

            // Check if changing department to one that already has a TL
            if (existingLeader.DepartmentId != leader.DepartmentId)
            {
                var deptTL = _context.TeamLeaders.FirstOrDefault(t => t.DepartmentId == leader.DepartmentId);
                if (deptTL != null)
                    return BadRequest("New Department already has a Team Leader");
            }

            existingLeader.Name = leader.Name;
            existingLeader.Email = leader.Email;
            existingLeader.DepartmentId = leader.DepartmentId;

            if (!string.IsNullOrEmpty(leader.PasswordHash))
            {
                existingLeader.PasswordHash = BCrypt.Net.BCrypt.HashPassword(leader.PasswordHash);
            }

            _context.Entry(existingLeader).State = EntityState.Modified;
            _context.SaveChanges();

            return Ok("Team Leader Updated Successfully");
        }

        [HttpDelete("{id}")]
        public IActionResult DeleteTeamLeader(int id)
        {
            var leader = _context.TeamLeaders.Find(id);
            if (leader == null) return NotFound();

            _context.TeamLeaders.Remove(leader);
            _context.SaveChanges();
            return Ok("Team Leader Deleted Successfully");
        }
    }
}
