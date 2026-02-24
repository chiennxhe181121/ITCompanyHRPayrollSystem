using HumanResourcesManager.DAL.Data;
using HumanResourcesManager.DAL.Interfaces;
using HumanResourcesManager.DAL.Models;
using Microsoft.EntityFrameworkCore;

namespace HumanResourcesManager.DAL.Repository
{
    public class AnnualLeaveBalanceRepository : IAnnualLeaveBalanceRepositry
    {
        private readonly HumanManagerContext _context;

        public AnnualLeaveBalanceRepository(HumanManagerContext context)
        {
            _context = context;
        }

        public void Add(AnnualLeaveBalance entity)
        {
            _context.AnnualLeaveBalance.Add(entity);
        }

        public void Update(AnnualLeaveBalance entity)
        {
            _context.AnnualLeaveBalance.Update(entity);
        }

        public void Delete(int id)
        {
            var entity = _context.AnnualLeaveBalance.Find(id);
            if (entity != null)
            {
                _context.AnnualLeaveBalance.Remove(entity);
            }
        }

        public IQueryable<AnnualLeaveBalance> GetAll()
        {
            return _context.AnnualLeaveBalance.AsQueryable();
        }

        public void Save()
        {
            _context.SaveChanges();
        }

        public AnnualLeaveBalance? GetByEmployeeAndYear(int employeeId, int year)
        {
            return _context.AnnualLeaveBalance
                .FirstOrDefault(x =>
                    x.EmployeeId == employeeId &&
                    x.Year == year);
        }

        public bool Exists(int employeeId, int year)
        {
            return _context.AnnualLeaveBalance
                .Any(x => x.EmployeeId == employeeId &&
                          x.Year == year);
        }

        public List<AnnualLeaveBalance>
    GetPreviousBalances(int employeeId, int year)
        {
            return _context.AnnualLeaveBalance
                .Where(x => x.EmployeeId == employeeId &&
                            x.Year < year)
                .OrderByDescending(x => x.Year)
                .Take(3)
                .ToList();
        }

        public async Task<double> GetRemainingDaysAsync(int employeeId, int year)
        {
            var balance = await _context.AnnualLeaveBalance
                .AsNoTracking()
                .FirstOrDefaultAsync(x =>
                    x.EmployeeId == employeeId
                    && x.Year == year
                    && !x.IsExpired);

            if (balance == null)
                return 0;

            return balance.RemainingDays;
        }
    }
}
