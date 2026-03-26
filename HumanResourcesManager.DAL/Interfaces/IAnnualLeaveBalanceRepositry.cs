using HumanResourcesManager.DAL.Models;

namespace HumanResourcesManager.DAL.Interfaces
{
    public interface IAnnualLeaveBalanceRepositry
    {
        void Add(AnnualLeaveBalance attendance);
        void Update(AnnualLeaveBalance attendance);
        void Delete(int id);
        IQueryable<AnnualLeaveBalance> GetAll();

        void Save();
        AnnualLeaveBalance? GetByEmployeeAndYear(int employeeId, int year);
        bool Exists(int employeeId, int year);

        List<AnnualLeaveBalance> GetPreviousBalances(int employeeId, int year);
        Task<double> GetRemainingDaysAsync(int employeeId, int year);
    }
}
