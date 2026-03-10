using LeaveSphere.API.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LeaveSphere.API.Services
{
    public interface ILeaveService
    {
        Task<IEnumerable<LeaveRequest>> GetLeavesAsync();
        Task ApplyLeaveAsync(LeaveRequest leave);
        Task ApproveLeaveAsync(int id);
        Task RejectLeaveAsync(int id);
        Task<bool> HasLeaveConflictAsync(int departmentId, DateTime startDate, DateTime endDate);
    }
}