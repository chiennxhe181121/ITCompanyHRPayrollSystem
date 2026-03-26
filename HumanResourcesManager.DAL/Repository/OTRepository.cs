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
                            && o.Status == 2 // Approved
                            && o.WorkDate.Month == month
                            && o.WorkDate.Year == year)
                .ToListAsync();
        }
    }
}
