using LeaveSphere.API.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LeaveSphere.API.Repositories
{
    public interface IEmployeeRepository
    {
        Task<IEnumerable<Employee>> GetAllAsync();
        Task<Employee?> GetByIdAsync(int id);
        Task<Employee?> GetByEmailAsync(string email);
        Task AddAsync(Employee employee);
        Task SaveAsync();
    }
}