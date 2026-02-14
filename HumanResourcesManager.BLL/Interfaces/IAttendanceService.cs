using HumanResourcesManager.BLL.DTOs.Common;
using HumanResourcesManager.BLL.DTOs.Employee;
using HumanResourcesManager.DAL.Enum;
using Microsoft.AspNetCore.Http;

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
        Task<ServiceResult> CheckIn(int userId, CheckInDTO dto);
        Task<ServiceResult> CheckOut(int userId, CheckOutDTO dto);
    }
}
