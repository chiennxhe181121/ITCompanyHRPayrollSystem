using HumanResourcesManager.DAL.Models;

namespace HumanResourcesManager.DAL.Interfaces
{
    public interface IOTAttendanceRepository
    {
        OTAttendance? GetByOverTimeRequestId(int overTimeRequestId);
        void Add(OTAttendance attendance);
        void Update(OTAttendance attendance);
        void Save();
    }
}

