using HumanResourcesManager.DAL.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HumanResourcesManager.DAL.Interfaces
{
    public interface IAttendanceRepository
    {
        void Add(Attendance attendance);
        void Update(Attendance attendance);
        void Delete(int id);
        void Save();

        List<Attendance> GetByEmployeeId(int employeeId, int page, int pageSize);
        int CountByEmployeeId(int employeeId);
    }
}
