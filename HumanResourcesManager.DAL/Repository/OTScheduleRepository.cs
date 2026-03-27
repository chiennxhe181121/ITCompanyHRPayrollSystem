using System;
using System.Collections.Generic;
using System.Linq;
using HumanResourcesManager.DAL.Data;
using HumanResourcesManager.DAL.Interfaces;
using HumanResourcesManager.DAL.Models;
using HumanResourcesManager.DAL.Shared;
using Microsoft.EntityFrameworkCore;

namespace HumanResourcesManager.DAL.Repositories
{
    public class OTScheduleRepository : IOTScheduleRepository
    {
        private readonly HumanManagerContext _context;

        public OTScheduleRepository(HumanManagerContext context)
        {
            _context = context;
        }

        // Tính tại C# (sau khi materialize) để tránh EF dịch sai .Date / AddDays lên SQL.
        private static DateTime GetScheduleStartDateTime(OTSchedule s)
            => s.StartDate.Date.Add(s.StartTime);

        private static DateTime GetEffectiveEndDateTime(OTSchedule s)
        {
            if (s.StartDate.Date == s.EndDate.Date && s.EndTime <= s.StartTime)
                return s.StartDate.Date.AddDays(1).Add(s.EndTime);
            return s.EndDate.Date.Add(s.EndTime);
        }

        public void Add(OTSchedule schedule)
        {
            _context.OTSchedules.Add(schedule);
        }

        public void Update(OTSchedule schedule)
        {
            _context.OTSchedules.Update(schedule);
        }

        public void RemoveRegistrations(IEnumerable<OverTimeRequest> registrations)
        {
            _context.OverTimeRequests.RemoveRange(registrations);
        }

        public OTSchedule? GetById(int id)
        {
            return _context.OTSchedules
                .Include(s => s.Registrations)
                    .ThenInclude(r => r.Employee)
                .Include(s => s.Manager)
                .FirstOrDefault(s => s.Id == id);
        }

        public IEnumerable<OTSchedule> GetByDepartment(int departmentId)
        {
            return _context.OTSchedules
                .Include(s => s.Registrations)
                .Include(s => s.Manager)
                .Where(s => s.DepartmentId == departmentId)
                .OrderByDescending(s => s.CreatedAt)
                .ToList();
        }

        public IEnumerable<OTSchedule> GetByManager(int managerId)
        {
            return _context.OTSchedules
                .Include(s => s.Registrations)
                .Where(s => s.ManagerId == managerId)
                .OrderByDescending(s => s.CreatedAt)
                .ToList();
        }

        public int CountByManager(int managerId)
        {
            return _context.OTSchedules.Count(s => s.ManagerId == managerId);
        }

        public IEnumerable<OTSchedule> GetExpiredOpenSchedules(DateTime now)
        {
            var open = _context.OTSchedules
                .Include(s => s.Registrations)
                .Where(s => s.Status == 0 || s.Status == 5)
                .ToList();
            return open.Where(s => now >= GetEffectiveEndDateTime(s));
        }

        public IEnumerable<OTSchedule> GetStartedOpenSchedules(DateTime now)
        {
            var open = _context.OTSchedules
                .Include(s => s.Registrations)
                .Where(s => s.Status == 0 || s.Status == 5)
                .ToList();
            return open.Where(s =>
                now >= GetScheduleStartDateTime(s) &&
                now < GetEffectiveEndDateTime(s));
        }

        public IEnumerable<OTSchedule> GetUpcomingToActivateSchedules(DateTime now)
        {
            var upcoming = _context.OTSchedules
                .Include(s => s.Registrations)
                .Where(s => s.Status == 5)
                .ToList();
            return upcoming.Where(s =>
                s.StartDate.Date < now.Date ||
                (s.StartDate.Date == now.Date && s.StartTime <= now.TimeOfDay));
        }

        public IEnumerable<OTSchedule> GetSchedulesToDeactivate(DateTime now)
        {
            var active = _context.OTSchedules
                .Where(s => s.Status == 0)
                .ToList();
            return active.Where(s =>
                s.StartDate.Date > now.Date ||
                (s.StartDate.Date == now.Date && s.StartTime > now.TimeOfDay));
        }

        public IEnumerable<OTSchedule> GetSchedulesExpiredInDbBeforeOvertimeStart(DateTime now)
        {
            var candidates = _context.OTSchedules
                .Include(s => s.Registrations)
                .Where(s => s.Status == 3 || s.Status == 1)
                .ToList();
            return candidates.Where(s => now < GetScheduleStartDateTime(s));
        }

        public IEnumerable<OverTimeRequest> GetEmployeeOverTimeRequestsInRange(int employeeId, DateTime fromDate, DateTime toDate)
        {
            var from = fromDate.Date;
            // inclusive end-of-day
            var toInclusive = toDate.Date.AddDays(1).AddTicks(-1);

            return _context.OverTimeRequests
                .Where(r => r.EmployeeId == employeeId
                            && r.WorkDate >= from
                            && r.WorkDate <= toInclusive
                            // 3 = Cancelled (đang được dùng nhất quán trong module schedule)
                            && r.Status != 3)
                .AsNoTracking()
                .ToList();
        }

        public bool HasEmployeeOvertimeOverlap(int employeeId, DateTime fromDate, DateTime toDate, TimeSpan startTime, TimeSpan endTime)
        {
            var from = fromDate.Date;
            var toInclusive = toDate.Date.AddDays(1).AddTicks(-1);

            return _context.OverTimeRequests.Any(r =>
                r.EmployeeId == employeeId
                && r.WorkDate >= from
                && r.WorkDate <= toInclusive
                && r.Status != 3
                && r.Status != Constants.Cancelled
                && r.StartTime < endTime
                && startTime < r.EndTime
            );
        }

        public void Save()
        {
            _context.SaveChanges();
        }
    }
}
