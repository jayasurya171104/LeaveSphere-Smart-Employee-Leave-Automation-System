using LeaveSphere.API.Data;
using LeaveSphere.API.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Hosting;
using System;
using System.Linq;

var builder = WebApplication.CreateBuilder(args);
var connectionString = builder.Configuration["ConnectionStrings:DefaultConnection"];
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseMySql(connectionString, new MySqlServerVersion(new Version(8, 0, 34))));

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    
    // Check Leave Requests
    var leaves = context.LeaveRequests.Include(l => l.Employee).ToList();
    Console.WriteLine($"Total Leave Requests in DB: {leaves.Count}");
    foreach (var l in leaves)
    {
        Console.WriteLine($"ID: {l.LeaveId}, Emp: {l.Employee?.Email ?? l.EmployeeId.ToString()}, Type: {l.LeaveType}, Status: {l.Status}, Date: {l.AppliedDate}");
    }
}
