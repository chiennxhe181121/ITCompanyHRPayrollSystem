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
    }
}
