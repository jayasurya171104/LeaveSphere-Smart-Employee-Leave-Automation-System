using LeaveSphere.API.Models;
using LeaveSphere.API.Repositories;
using LeaveSphere.API.Data;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.EntityFrameworkCore;

namespace LeaveSphere.API.Services
{
    public class LeaveService : ILeaveService
    {
        private readonly ILeaveRepository _leaveRepo;
        private readonly ApplicationDbContext _context;

        public LeaveService(ILeaveRepository leaveRepo, ApplicationDbContext context)
        {
            _leaveRepo = leaveRepo;
            _context = context;
        }

        public async Task<IEnumerable<LeaveRequest>> GetLeavesAsync()
        {
            return await _leaveRepo.GetAllAsync();
        }

        public async Task ApplyLeaveAsync(LeaveRequest leave)
        {
            await _leaveRepo.AddAsync(leave);
            await _leaveRepo.SaveAsync();
        }

        public async Task ApproveLeaveAsync(int id)
        {
            var leave = await _leaveRepo.GetByIdAsync(id);
            if (leave == null) return;

            leave.Status = "Approved";

            LeaveBalance? balance = null;
            if (leave.EmployeeId.HasValue) 
            {
               balance = _context.LeaveBalances.FirstOrDefault(lb => lb.EmployeeId == leave.EmployeeId.Value);
            }
            else if (leave.TeamLeaderId.HasValue)
            {
               balance = _context.LeaveBalances.FirstOrDefault(lb => lb.TeamLeaderId == leave.TeamLeaderId.Value);
            }

            if (balance != null)
            {
                int days = (leave.EndDate - leave.StartDate).Days + 1;
                balance.UsedLeaves += days;
                balance.RemainingLeaves -= days;
            }

            await _leaveRepo.UpdateAsync(leave);
            await _leaveRepo.SaveAsync();
        }

        public async Task RejectLeaveAsync(int id)
        {
            var leave = await _leaveRepo.GetByIdAsync(id);
            if (leave == null) return;

            leave.Status = "Rejected";

            await _leaveRepo.UpdateAsync(leave);
            await _leaveRepo.SaveAsync();
        }

        public async Task<bool> HasLeaveConflictAsync(int departmentId, DateTime startDate, DateTime endDate)
        {
            var overlappingLeavesCount = await _context.LeaveRequests
                .Where(l => l.DepartmentId == departmentId && 
                            l.Status == "Approved" &&
                            l.StartDate <= endDate && 
                            l.EndDate >= startDate)
                .CountAsync();

            return overlappingLeavesCount >= 3;
        }
    }
}