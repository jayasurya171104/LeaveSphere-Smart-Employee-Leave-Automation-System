using LeaveSphere.API.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LeaveSphere.API.Repositories
{
    public interface ILeaveRepository
    {
        Task<IEnumerable<LeaveRequest>> GetAllAsync();
        Task<LeaveRequest?> GetByIdAsync(int id);
        Task AddAsync(LeaveRequest leave);
        Task UpdateAsync(LeaveRequest leave);
        Task SaveAsync();
    }
}