using HumanResourcesManager.DAL.Data;
using HumanResourcesManager.DAL.Interfaces;
using HumanResourcesManager.DAL.Models;

namespace HumanResourcesManager.DAL.Repository
{
    public class PayrollRepository : IPayrollRepository
    {
        private readonly HumanManagerContext _context;

        public PayrollRepository(HumanManagerContext context)
        {
            _context = context;
        }

        public IQueryable<Payroll> GetQueryableByEmployee(int employeeId)
        {
            return _context.Payrolls
                .Where(x => x.EmployeeId == employeeId)
                .OrderByDescending(x => x.Year)
                .ThenByDescending(x => x.Month);
        }
    }
}
