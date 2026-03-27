using HumanResourcesManager.BLL.DTOs.Employee;
using HumanResourcesManager.BLL.DTOs.Manager;

namespace HumanResourcesManager.Models.Employee
{
    public class EmployeeOvertimeIndexViewModel
    {
        public IEnumerable<OTScheduleDTO> AvailableSchedules { get; set; } = Enumerable.Empty<OTScheduleDTO>();
        public EmployeeOTAttendanceHistoryViewDTO History { get; set; } = new();

        public int HistoryPageSize => History.PageSize;
        public int HistoryCurrentPage => History.CurrentPage;
        public int HistoryTotalPages => History.TotalPages;
    }
}

