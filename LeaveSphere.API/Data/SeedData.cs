using LeaveSphere.API.Models;
using BCrypt.Net;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System;
using System.Collections.Generic;

namespace LeaveSphere.API.Data
{
    public static class SeedData
    {
        public static void Initialize(ApplicationDbContext context)
        {
            Console.WriteLine("Initializing SeedData...");
            context.Database.EnsureCreated();

            // 1. Ensure Departments exist
            if (!context.Departments.Any())
            {
                var departments = new List<Department>
                {
                    new Department { DepartmentName = "HR" },
                    new Department { DepartmentName = "IT" },
                    new Department { DepartmentName = "Finance" }
                };
                context.Departments.AddRange(departments);
                context.SaveChanges();
            }

            var hrDept = context.Departments.FirstOrDefault(d => d.DepartmentName == "HR");
            int hrDeptId = hrDept?.DepartmentId ?? 1;

            // 2. Ensure Admin exists
            var adminUser = context.Employees.FirstOrDefault(e => e.Email == "admin@leavesphere.com");
            if (adminUser == null)
            {
                adminUser = new Employee
                {
                    Name = "System Admin",
                    Email = "admin@leavesphere.com",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin@123"),
                    Role = "Admin",
                    DepartmentId = hrDeptId,
                    DateOfJoining = DateTime.Now
                };
                context.Employees.Add(adminUser);
                context.SaveChanges();
            }
            else
            {
                // Update existing admin to ensure password and role are correct
                adminUser.PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin@123");
                adminUser.Role = "Admin";
                context.Employees.Update(adminUser);
                context.SaveChanges();
            }

            // 3. Ensure Sample Employees exist
            if (context.Employees.Count() <= 1)
            {
                Console.WriteLine("Seeding sample employees...");
                var allDepts = context.Departments.ToList();
                var random = new Random();

                for (int i = 1; i <= 10; i++)
                {
                    var testEmp = new Employee
                    {
                        Name = $"Test Employee {i}",
                        Email = $"employee{i}@leavesphere.com",
                        PasswordHash = BCrypt.Net.BCrypt.HashPassword("Employee@123"),
                        Role = "Employee",
                        DepartmentId = allDepts[random.Next(allDepts.Count)].DepartmentId,
                        DateOfJoining = DateTime.Now.AddDays(-random.Next(1, 365))
                    };
                    context.Employees.Add(testEmp);
                }
                context.SaveChanges();
            }

            // 4. Ensure ALL employees have a LeaveBalance
            var allEmployees = context.Employees.ToList();
            foreach (var e in allEmployees)
            {
                if (!context.LeaveBalances.Any(lb => lb.EmployeeId == e.EmployeeId))
                {
                    context.LeaveBalances.Add(new LeaveBalance
                    {
                        EmployeeId = e.EmployeeId,
                        TotalLeaves = e.Role == "Admin" ? 30 : 20,
                        UsedLeaves = 0,
                        RemainingLeaves = e.Role == "Admin" ? 30 : 20
                    });
                }
            }
            context.SaveChanges();
            Console.WriteLine("Verified LeaveBalances for all employees.");
        }
    }
}