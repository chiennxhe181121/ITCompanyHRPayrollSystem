using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HumanResourcesManager.DAL.Models;

namespace HumanResourcesManager.DAL.Interfaces
{
    public interface IPayrollRepository
    {
        Task<List<Payroll>> GetAllAsync();
        Task<Payroll?> GetByIdAsync(int id);

        Task AddAsync(Payroll payroll);
        Task UpdateAsync(Payroll payroll);
        Task DeleteAsync(int id);
     
        Task SaveChangesAsync();
    }
}
