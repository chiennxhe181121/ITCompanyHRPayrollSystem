using HumanResourcesManager.DAL.Models;

namespace HumanResourcesManager.DAL.Interfaces
{
    public interface IPayrollRepository
    {
        IQueryable<Payroll> GetQueryableByEmployee(int employeeId);
    }
}
