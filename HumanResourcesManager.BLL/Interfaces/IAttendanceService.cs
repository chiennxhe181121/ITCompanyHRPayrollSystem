using HumanResourcesManager.BLL.DTOs;
using HumanResourcesManager.DAL.Enum;

namespace HumanResourcesManager.BLL.Interfaces
{
    public interface IAttendanceService
    {
        EmployeeAttendanceViewDTO GetEmployeeAttendance(
            int currentUserId,
            int page,
            int pageSize,
            int? month,
            int? year,
            AttendanceStatus? status);
    }
}
