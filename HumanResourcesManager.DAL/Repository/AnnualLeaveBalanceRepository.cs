using HumanResourcesManager.DAL.Data;
using HumanResourcesManager.DAL.Interfaces;
using HumanResourcesManager.DAL.Models;

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
    }
}
