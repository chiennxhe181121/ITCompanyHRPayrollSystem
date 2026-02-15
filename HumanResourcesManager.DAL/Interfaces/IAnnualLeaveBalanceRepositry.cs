using HumanResourcesManager.DAL.Models;

namespace HumanResourcesManager.DAL.Interfaces
{
    public interface IAnnualLeaveBalanceRepositry
    {
        void Add(AnnualLeaveBalance attendance);
        void Update(AnnualLeaveBalance attendance);
        void Delete(int id);
        void Save();
        AnnualLeaveBalance? GetByEmployeeAndYear(int employeeId, int year);
    }
}
