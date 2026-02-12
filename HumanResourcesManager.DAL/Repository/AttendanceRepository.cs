using HumanResourcesManager.DAL.Data;
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

        public List<Attendance> GetByEmployeeId(int employeeId, int page, int pageSize)
        {
            return _context.Attendances
                .Where(a => a.EmployeeId == employeeId)
                .OrderByDescending(a => a.WorkDate) // sắp xếp mới nhất trước
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .AsNoTracking()
                .ToList();
        }

        public int CountByEmployeeId(int employeeId)
        {
            return _context.Attendances
                .Count(a => a.EmployeeId == employeeId);
        }

        public IQueryable<Attendance> GetQueryableByEmployeeId(int employeeId)
        {
            return _context.Attendances
                .Where(a => a.EmployeeId == employeeId)
                .AsNoTracking();
        }
    }
}