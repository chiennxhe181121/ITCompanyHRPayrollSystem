using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HumanResourcesManager.DAL.Models;

namespace HumanResourcesManager.DAL.Interfaces
{
    public interface IOTRepository
    {
        Task<List<OverTimeRequest>> GetApprovedOTsAsync(int employeeId, int month, int year);
        Task<List<OverTimeRequest>> GetEmployeeOTByDateAsync(int employeeId, DateTime workDate);
        Task<OverTimeRequest?> GetByIdAsync(int id);
        Task<(List<OverTimeRequest> Items, int TotalRecords)> GetEmployeeOTHistoryAsync(int employeeId, int page, int pageSize, int? month, int? year);
    }
}
