using HumanResourcesManager.BLL.DTOs;
using HumanResourcesManager.BLL.DTOs.Employee;
using HumanResourcesManager.DAL.Models;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HumanResourcesManager.BLL.Interfaces
{
    public interface IEmployeeService
    {
        IEnumerable<EmployeeDTO> GetAll();
        EmployeeDTO? GetById(int id);
        void Create(EmployeeDTO dto);
        void Update(EmployeeDTO dto);
        void Delete(int id);
        IEnumerable<EmployeeDTO> GetTeamMembers(int managerUserId);
        int CountManagedEmployees(int managerUserId);
        int CountPendingOvertimeRequests(int managerUserId);
        int CountOvertimeSchedules(int managerUserId);
        int CountCompletedOTSchedules(int managerUserId);
        IEnumerable<EmployeeDTO> GetEmployeesWithoutDepartment();
        bool AddEmployeeToManagerTeam(int managerUserId, int employeeId, out string message);
        bool ChangeTeamMemberStatus(int managerUserId, int employeeId, int status, out string message);
        EmployeeOwnerProfileDTO? GetOwnProfile(int userId);
        Task<Employee?> UpdateOwnProfile(
               int userId,
               EmployeeOwnerProfileDTO dto,
               IFormFile? avatarFile
           );
    }
}
