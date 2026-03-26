using HumanResourcesManager.BLL.DTOs.Employee;

namespace HumanResourcesManager.BLL.Interfaces
{
    public interface IPayrollService
    {
        EmployeePayrollViewDTO GetPayrolls(int employeeId, int page, int pageSize, int? month, int? year);
    }
}
