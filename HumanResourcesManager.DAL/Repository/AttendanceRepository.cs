using HumanResourcesManager.DAL.Data;
using HumanResourcesManager.DAL.Enum;
using HumanResourcesManager.DAL.Interfaces;
using HumanResourcesManager.DAL.Models;
using Microsoft.EntityFrameworkCore;

namespace HumanResourcesManager.DAL.Repository
{
    public class AttendanceRepository : IAttendanceRepository
    {
        private readonly HumanManagerContext _context;

        public AttendanceRepository(HumanManagerContext context)
        {
            _context = context;
        }

        public void Add(Attendance attendance)
        {
            _context.Attendances.Add(attendance);
        }

        public void Update(Attendance attendance)
        {
            _context.Attendances.Update(attendance);
        }

        public void Delete(int id)
        {
            var entity = _context.Attendances.Find(id);
            if (entity != null)
            {
                _context.Attendances.Remove(entity);
            }
        }

        public void Save()
        {
            _context.SaveChanges();
        }

        public IQueryable<Attendance> GetQueryableByEmployeeId(int employeeId)
        {
            return _context.Attendances
                .Where(a => a.EmployeeId == employeeId)
                .AsNoTracking();
        }

        public int CountWorkingDays(int employeeId)
        {
            return _context.Attendances
                .Count(a => a.EmployeeId == employeeId
                         && a.Status != AttendanceStatus.ApprovedLeave
                         && a.Status != AttendanceStatus.AWOL);
        }
        public Attendance? GetByEmployeeAndWorkDate(int employeeId, DateTime workDate)
        {
            var start = workDate.Date;
            var end = start.AddDays(1);

            return _context.Attendances
                .FirstOrDefault(x => x.EmployeeId == employeeId
                                  && x.WorkDate >= start
                                  && x.WorkDate < end);
        }
    }
}