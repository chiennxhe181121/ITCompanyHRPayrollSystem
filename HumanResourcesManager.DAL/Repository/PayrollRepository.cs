using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HumanResourcesManager.DAL.Data;
using HumanResourcesManager.DAL.Interfaces;
using HumanResourcesManager.DAL.Models;
using Microsoft.EntityFrameworkCore;

namespace HumanResourcesManager.DAL.Repository
{
    public class PayrollRepository : IPayrollRepository
    {
        private readonly HumanManagerContext _context;

        public PayrollRepository(HumanManagerContext context)
        {
            _context = context;
        }

        public async Task<List<Payroll>> GetAllAsync()
        {
            return await _context.Payrolls
                .Include(p => p.Employee)
                .Include(p => p.PayrollDetails)
                .ToListAsync();
        }

        public async Task<Payroll?> GetByIdAsync(int id)
        {
            return await _context.Payrolls
                .Include(p => p.Employee)
                .Include(p => p.PayrollDetails)
                .FirstOrDefaultAsync(p => p.PayrollId == id);
        }

        public async Task AddAsync(Payroll payroll)
        {
            await _context.Payrolls.AddAsync(payroll);
        }

        public async Task UpdateAsync(Payroll payroll)
        {
            _context.Payrolls.Update(payroll);
        }

        public async Task DeleteAsync(int id)
        {
            var payroll = await GetByIdAsync(id);
            if (payroll != null)
            {
                _context.Payrolls.Remove(payroll);
            }
        }
       
        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }

        public IQueryable<Payroll> GetQueryableByEmployee(int employeeId)
        {
            return _context.Payrolls
                .Where(x => x.EmployeeId == employeeId)
                .OrderByDescending(x => x.Year)
                .ThenByDescending(x => x.Month);
        }

        public Payroll GetPayrollDetail(int payrollId, int employeeId)
        {
            return _context.Payrolls
                .Where(p => p.PayrollId == payrollId && p.EmployeeId == employeeId)
                .Select(p => new Payroll
                {
                    PayrollId = p.PayrollId,
                    Month = p.Month,
                    Year = p.Year,
                    BasicSalary = p.BasicSalary,
                    TotalOT = p.TotalOT,
                    TotalAllowance = p.TotalAllowance,
                    MissingMinutesPenalty = p.MissingMinutesPenalty,
                    NetSalary = p.NetSalary,
                    PayrollDetails = p.PayrollDetails.ToList()
                })
                .FirstOrDefault();
        }

        public Payroll? GetLatestPayroll(int employeeId)
        {
            return _context.Payrolls
                .Where(p => p.EmployeeId == employeeId)
                .OrderByDescending(p => p.Year)
                .ThenByDescending(p => p.Month)
                .FirstOrDefault();
        }
    }
}
