using Microsoft.EntityFrameworkCore;
using LeaveSphere.API.Models;

namespace LeaveSphere.API.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Employee> Employees { get; set; }
        public DbSet<LeaveRequest> LeaveRequests { get; set; }
        public DbSet<Department> Departments { get; set; }
        public DbSet<LeaveBalance> LeaveBalances { get; set; }
        public DbSet<TeamLeader> TeamLeaders { get; set; }
    }
}