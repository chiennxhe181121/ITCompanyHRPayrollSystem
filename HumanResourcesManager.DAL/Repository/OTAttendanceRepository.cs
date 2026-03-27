using HumanResourcesManager.DAL.Data;
using HumanResourcesManager.DAL.Interfaces;
using HumanResourcesManager.DAL.Models;

namespace HumanResourcesManager.DAL.Repository
{
    public class OTAttendanceRepository : IOTAttendanceRepository
    {
        private readonly HumanManagerContext _context;

        public OTAttendanceRepository(HumanManagerContext context)
        {
            _context = context;
        }

        public OTAttendance? GetByOverTimeRequestId(int overTimeRequestId)
        {
            return _context.OTAttendances.FirstOrDefault(a => a.OverTimeRequestId == overTimeRequestId);
        }

        public void Add(OTAttendance attendance)
        {
            _context.OTAttendances.Add(attendance);
        }

        public void Update(OTAttendance attendance)
        {
            _context.OTAttendances.Update(attendance);
        }

        public void Save()
        {
            _context.SaveChanges();
        }
    }
}

