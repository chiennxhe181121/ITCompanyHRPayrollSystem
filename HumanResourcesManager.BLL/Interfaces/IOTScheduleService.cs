using System.Collections.Generic;
using HumanResourcesManager.BLL.DTOs.Manager;

namespace HumanResourcesManager.BLL.Interfaces
{
    public interface IOTScheduleService
    {
        bool CreateOTSchedule(int managerUserId, OTScheduleCreateDTO dto, out string message);
        IEnumerable<OTScheduleDTO> GetManagerSchedules(int managerUserId);
        IEnumerable<OTScheduleDTO> GetDepartmentOpenSchedules(int employeeUserId);
        bool RegisterOT(int employeeUserId, int scheduleId, out string message);
        bool CancelRegistration(int employeeUserId, int scheduleId, out string message);
        bool RemoveEmployeeFromSchedule(int managerUserId, int scheduleId, int employeeId, out string message, out (string Email, string Name)? removedEmployee);
        void CancelExpiredSchedules();
        
        bool CancelOTSchedule(int managerUserId, int scheduleId, out string message, out List<(string Email, string Name)> notifyList);
        bool ReopenOTSchedule(int managerUserId, int scheduleId, out string message);
        OTScheduleDetailDTO? GetOTScheduleDetails(int managerUserId, int scheduleId);
        bool UpdateOTSchedule(int managerUserId, int scheduleId, OTScheduleCreateDTO dto, out string message, out List<(string Email, string Name)> notifyList);
        OTScheduleCreateDTO? GetScheduleForEdit(int managerUserId, int scheduleId);
        bool MarkAsCompleted(int managerUserId, int scheduleId, out string message);
    }
}
