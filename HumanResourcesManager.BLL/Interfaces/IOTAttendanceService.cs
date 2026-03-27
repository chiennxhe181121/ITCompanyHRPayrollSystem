using HumanResourcesManager.BLL.DTOs.Common;
using HumanResourcesManager.BLL.DTOs.Employee;
using HumanResourcesManager.DAL.Models;

namespace HumanResourcesManager.BLL.Interfaces
{
    public interface IOTAttendanceService
    {
        Task<ServiceResult> CheckIn(int userId, OTCheckInDTO dto);
        Task<ServiceResult> CheckOut(int userId, OTCheckOutDTO dto);
        Task<List<OverTimeRequest>> GetTodayOTs(int userId);
        Task<OTTodayAttendanceViewDTO?> GetTodayOTAttendance(int userId, int overTimeRequestId);
        Task<EmployeeOTAttendanceHistoryViewDTO> GetHistory(int userId, int page, int pageSize, int? month, int? year);
    }
}

