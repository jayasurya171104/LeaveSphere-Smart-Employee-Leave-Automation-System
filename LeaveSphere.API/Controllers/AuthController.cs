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

            if (user == null)
            {
                Console.WriteLine("User not found in database.");
                return Unauthorized("Invalid Credentials");
            }

            bool passwordValid = BCrypt.Net.BCrypt.Verify(login.Password, user.PasswordHash);
            if (!passwordValid)
            {
                Console.WriteLine("Password verification failed.");
                return Unauthorized("Invalid Password");
            }

            Console.WriteLine("Login successful. Generating token...");

            var claims = new[]
{
    new Claim(ClaimTypes.Name, user.Email),
    new Claim(ClaimTypes.Role, user.Role),
    new Claim("EmployeeId", user.EmployeeId.ToString()),
    new Claim("EmployeeName", user.Name)
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
        public IActionResult GetProfile()
        {
            var email = User.FindFirst(ClaimTypes.Name)?.Value;
            var user = _context.Employees
                .Include(e => e.LeaveBalance)
                .Include(e => e.Department)
                .FirstOrDefault(e => e.Email == email);

            if (user == null) return NotFound("User not found");

            return Ok(user);
        }
    }
}