using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HumanResourcesManager.DAL.Repository
{
    using HumanResourcesManager.DAL.Data;
    using HumanResourcesManager.DAL.Interfaces;
    using HumanResourcesManager.DAL.Models;
    using HumanResourcesManager.DAL.Shared;
    using Microsoft.EntityFrameworkCore;

    public class OTRepository : IOTRepository
    {
        private readonly HumanManagerContext _context;

        public OTRepository(HumanManagerContext context)
        {
            _context = context;
        }

        // Lấy OT đã approved và employee accepted
        public async Task<List<OverTimeRequest>> GetApprovedOTsAsync(int employeeId, int month, int year)
        {
            return await _context.OverTimeRequests
                .Include(o => o.OTAttendance)
                .Where(o => o.EmployeeId == employeeId
                            && o.EmployeeAccepted == true
                            // Legacy dữ liệu có thể dùng 1/2 cho approved, constants mới dùng 11
                            && (o.Status == 1 || o.Status == 2 || o.Status == Constants.Approved)
                            && o.WorkDate.Month == month
                            && o.WorkDate.Year == year)
                .ToListAsync();
        }

        public async Task<List<OverTimeRequest>> GetEmployeeOTByDateAsync(int employeeId, DateTime workDate)
        {
            var date = workDate.Date;
            return await _context.OverTimeRequests
                .Include(o => o.OTAttendance)
                .Where(o => o.EmployeeId == employeeId && o.WorkDate.Date == date)
                .ToListAsync();
        }

        public async Task<OverTimeRequest?> GetByIdAsync(int id)
        {
            return await _context.OverTimeRequests
                .Include(o => o.OTAttendance)
                .FirstOrDefaultAsync(o => o.Id == id);
        }

        public async Task<(List<OverTimeRequest> Items, int TotalRecords)> GetEmployeeOTHistoryAsync(int employeeId, int page, int pageSize, int? month, int? year)
        {
            if (page < 1) page = 1;
            if (pageSize <= 0) pageSize = 10;

            var query = _context.OverTimeRequests
                .Include(o => o.OTAttendance)
                .Where(o => o.EmployeeId == employeeId
                            && o.EmployeeAccepted == true
                            // 3 = Cancelled in schedule module, 13 = Cancelled in shared Constants
                            && o.Status != 3
                            && o.Status != Constants.Cancelled);

            if (month.HasValue && month.Value > 0)
                query = query.Where(o => o.WorkDate.Month == month.Value);

            if (year.HasValue && year.Value > 0)
                query = query.Where(o => o.WorkDate.Year == year.Value);

            query = query.OrderByDescending(o => o.WorkDate).ThenByDescending(o => o.StartTime);

            var total = await query.CountAsync();

            var items = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (items, total);
        }
    }
}
