using HumanResourcesManager.BLL.DTOs;

namespace HumanResourcesManager.BLL.Interfaces
{
    public interface IAttendanceService
    {
        EmployeeAttendanceViewDTO GetEmployeeAttendance(int currentUserId, int page, int pageSize);
    }
}
