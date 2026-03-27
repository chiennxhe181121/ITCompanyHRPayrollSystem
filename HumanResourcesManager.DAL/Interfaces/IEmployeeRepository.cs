using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HumanResourcesManager.DAL.Models;

namespace HumanResourcesManager.DAL.Interfaces
{
    public interface IEmployeeRepository
    {
        IEnumerable<Employee> GetAll();
        IEnumerable<Employee> GetByDepartment(int departmentId, int? excludeEmployeeId = null);
        Employee? GetById(int id);
        Employee? GetByUserId(int id);
        int CountByDepartment(int departmentId, int? excludeEmployeeId = null);
        int CountPendingOvertimeRequestsForManager(int managerEmployeeId);
        int CountOvertimeSchedulesForManager(int managerEmployeeId);
        int CountCompletedOTSchedulesForManager(int managerEmployeeId);
        IEnumerable<Employee> GetEmployeesWithoutDepartment();
        bool AssignEmployeeToDepartment(int employeeId, int departmentId);
        bool ExistsByEmail(string email, int excludeUserId);
        bool ExistsByPhone(string phone, int excludeUserId);
        void Add(Employee employee);
        void Update(Employee employee);
        bool SetStatus(int employeeId, int status);
        void SoftDelete(int id);
        void Save();
    }
}
