using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HumanResourcesManager.BLL.DTOs;
using HumanResourcesManager.DAL.Models;
using HumanResourcesManager.BLL.DTOs.Employee;

namespace HumanResourcesManager.BLL.Interfaces
{
    public interface IPayrollService
    {
        Task<List<PayrollDTO>> GetAllAsync();
        Task<PayrollDTO?> GetByIdAsync(int id);

        Task CreateAsync(int employeeId, int month, int year);
        Task UpdateAsync(PayrollDTO dto);
        Task DeleteAsync(int id);
        Task<PayrollDTO> GeneratePayrollForEmployeeAsync(int employeeId, int month, int year);
        Task<List<EmployeeDTO>> GetAllEmployeesWithoutPayrollAsync(int month, int year);
        EmployeePayrollViewDTO GetPayrolls(int employeeId, int page, int pageSize, int? month, int? year);
        Task<List<PayrollAuditSimpleDTO>> AuditPayrollSimpleAsync(int id);
    }
}
