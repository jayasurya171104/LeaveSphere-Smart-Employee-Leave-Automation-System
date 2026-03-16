using LeaveSphere.API.Data;
using LeaveSphere.API.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LeaveSphere.API.Repositories
{
    public class LeaveRepository : ILeaveRepository
    {
        private readonly ApplicationDbContext _context;

        public LeaveRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<LeaveRequest>> GetAllAsync()
        {
            return await _context.LeaveRequests
                .Include(l => l.Employee)
                    .ThenInclude(e => e!.LeaveBalance)
                .Include(l => l.Employee)
                    .ThenInclude(e => e!.Department)
                .Include(l => l.TeamLeader)
                    .ThenInclude(t => t!.Department)
                .ToListAsync();
        }

        public async Task<LeaveRequest?> GetByIdAsync(int id)
        {
            return await _context.LeaveRequests
                .Include(l => l.Employee)
                    .ThenInclude(e => e!.LeaveBalance)
                .Include(l => l.Employee)
                    .ThenInclude(e => e!.Department)
                .Include(l => l.TeamLeader)
                    .ThenInclude(t => t!.Department)
                .FirstOrDefaultAsync(l => l.LeaveId == id);
        }

        public async Task AddAsync(LeaveRequest leave)
        {
            await _context.LeaveRequests.AddAsync(leave);
        }

        public async Task UpdateAsync(LeaveRequest leave)
        {
            _context.LeaveRequests.Update(leave);
            await Task.CompletedTask;
        }

        public async Task SaveAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}