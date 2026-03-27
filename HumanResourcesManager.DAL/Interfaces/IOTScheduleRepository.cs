using System.Collections.Generic;
using HumanResourcesManager.DAL.Models;

namespace HumanResourcesManager.DAL.Interfaces
{
    public interface IOTScheduleRepository
    {
        void Add(OTSchedule schedule);
        void Update(OTSchedule schedule);
        void RemoveRegistrations(IEnumerable<OverTimeRequest> registrations);
        OTSchedule? GetById(int id);
        IEnumerable<OTSchedule> GetByDepartment(int departmentId);
        IEnumerable<OTSchedule> GetByManager(int managerId);
        int CountByManager(int managerId);
        IEnumerable<OTSchedule> GetExpiredOpenSchedules(DateTime now);
        IEnumerable<OTSchedule> GetStartedOpenSchedules(DateTime now);
        IEnumerable<OTSchedule> GetUpcomingToActivateSchedules(DateTime now);
        IEnumerable<OTSchedule> GetSchedulesToDeactivate(DateTime now);
        IEnumerable<OverTimeRequest> GetEmployeeOverTimeRequestsInRange(int employeeId, DateTime fromDate, DateTime toDate);
        bool HasEmployeeOvertimeOverlap(int employeeId, DateTime fromDate, DateTime toDate, TimeSpan startTime, TimeSpan endTime);
        void Save();
    }
}
