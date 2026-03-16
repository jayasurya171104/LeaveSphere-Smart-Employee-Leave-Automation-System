using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LeaveSphere.API.Models.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using LeaveSphere.API.Data;
using LeaveSphere.API.Models;
using BCrypt.Net;
using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

using System.Linq;
using System.Collections.Generic;



namespace LeaveSphere.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _configuration;

        public AuthController(ApplicationDbContext context, IConfiguration configuration)
{
    _context = context;
    _configuration = configuration;
}

        [HttpPost("register")]
        public IActionResult Register(Employee model)
        {
            if (_context.Employees.Any(e => e.Email == model.Email))
                return BadRequest("Email already exists");

            model.PasswordHash = BCrypt.Net.BCrypt.HashPassword(model.PasswordHash);
            _context.Employees.Add(model);
            _context.SaveChanges();

            // 🔥 Initialize LeaveBalance for new employee
            var leaveBalance = new LeaveBalance
            {
                EmployeeId = model.EmployeeId,
                TotalLeaves = 20,
                UsedLeaves = 0,
                RemainingLeaves = 20
            };
            _context.LeaveBalances.Add(leaveBalance);
            _context.SaveChanges();

            return Ok("Employee Registered Successfully");
        }

        [HttpPost("login")]
        public IActionResult Login([FromBody] LoginDTO login)
        {
            Console.WriteLine($"Login attempt for: {login.Email}");
            var user = _context.Employees.FirstOrDefault(e => e.Email == login.Email);
            var tlUser = _context.TeamLeaders.FirstOrDefault(e => e.Email == login.Email);

            if (user == null && tlUser == null)
            {
                Console.WriteLine("User not found in database.");
                return Unauthorized("Invalid Credentials");
            }

            string? passwordHashToVerify = user != null ? user.PasswordHash : tlUser?.PasswordHash;
            if (string.IsNullOrEmpty(passwordHashToVerify)) return Unauthorized("Invalid account configuration");

            bool passwordValid = BCrypt.Net.BCrypt.Verify(login.Password, passwordHashToVerify);
            if (!passwordValid)
            {
                Console.WriteLine("Password verification failed.");
                return Unauthorized("Invalid Password");
            }

            Console.WriteLine("Login successful. Generating token...");

            string role = user != null ? user.Role : (tlUser?.Role ?? "TeamLeader");
            string name = user != null ? user.Name : (tlUser?.Name ?? "User");
            string employeeId = user != null ? user.EmployeeId.ToString() : (tlUser?.TeamLeaderId.ToString() ?? "0");

            var claims = new[]
{
    new Claim(ClaimTypes.Name, login.Email),
    new Claim(ClaimTypes.Role, role),
    new Claim("EmployeeId", employeeId),
    new Claim("EmployeeName", name)
};

var jwtKey = _configuration["Jwt:Key"] ?? "VERY_SECRET_KEY_PLACEHOLDER_12345";
var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

var token = new JwtSecurityToken(
    issuer: _configuration["Jwt:Issuer"] ?? "LeaveSphereAPI",
    audience: _configuration["Jwt:Audience"] ?? "LeaveSphereClient",
    claims: claims,
    expires: DateTime.Now.AddHours(2),
    signingCredentials: creds
);

            return Ok(new
            {
                token = new JwtSecurityTokenHandler().WriteToken(token)
            });
        }

        [HttpGet("profile")]
        [Authorize]
        public async Task<IActionResult> GetProfile()
        {
            var email = User.FindFirst(ClaimTypes.Name)?.Value;
            var user = await _context.Employees
                .Include(e => e.LeaveBalance)
                .Include(e => e.Department)
                .FirstOrDefaultAsync(e => e.Email == email);

            if (user != null)
            {
                var tlName = "N/A";
                if (user.Role == "Employee")
                {
                    var tl = await _context.TeamLeaders.FirstOrDefaultAsync(t => t.DepartmentId == user.DepartmentId);
                    tlName = tl?.Name ?? "N/A";
                }

                // 🔥 Dynamic Calculation for Leave Balance
                if (user.Role != "Admin" && user.LeaveBalance != null)
                {
                    var approvedLeaves = await _context.LeaveRequests
                        .Where(l => l.EmployeeId == user.EmployeeId && l.Status == "Approved")
                        .ToListAsync();
                    
                    int usedDays = approvedLeaves.Sum(l => (l.EndDate - l.StartDate).Days + 1);
                    user.LeaveBalance.UsedLeaves = usedDays;
                    user.LeaveBalance.RemainingLeaves = user.LeaveBalance.TotalLeaves - usedDays;
                }

                return Ok(new Dictionary<string, object>
                {
                    ["EmployeeId"] = user.EmployeeId,
                    ["Name"] = user.Name,
                    ["Email"] = user.Email,
                    ["Role"] = user.Role,
                    ["DateOfJoining"] = user.DateOfJoining,
                    ["DepartmentName"] = user.Department?.DepartmentName ?? "N/A",
                    ["TeamLeaderName"] = tlName,
                    ["LeaveBalance"] = user.Role == "Admin" ? null : user.LeaveBalance
                });
            }

            var tlUser = await _context.TeamLeaders
                .Include(t => t.LeaveBalance)
                .Include(t => t.Department)
                .FirstOrDefaultAsync(t => t.Email == email);

            if (tlUser != null)
            {
                // 🔥 Dynamic Calculation for Leave Balance
                if (tlUser.LeaveBalance != null)
                {
                    var approvedLeaves = await _context.LeaveRequests
                        .Where(l => l.TeamLeaderId == tlUser.TeamLeaderId && l.Status == "Approved")
                        .ToListAsync();

                    int usedDays = approvedLeaves.Sum(l => (l.EndDate - l.StartDate).Days + 1);
                    tlUser.LeaveBalance.UsedLeaves = usedDays;
                    tlUser.LeaveBalance.RemainingLeaves = tlUser.LeaveBalance.TotalLeaves - usedDays;
                }

                return Ok(new Dictionary<string, object>
                {
                    ["TeamLeaderId"] = tlUser.TeamLeaderId,
                    ["Name"] = tlUser.Name,
                    ["Email"] = tlUser.Email,
                    ["Role"] = tlUser.Role,
                    ["DateOfJoining"] = tlUser.DateOfJoining,
                    ["DepartmentName"] = tlUser.Department?.DepartmentName ?? "N/A",
                    ["LeaveBalance"] = tlUser.LeaveBalance
                });
            }

            return NotFound("User not found");
        }
    }
}