using HumanResourcesManager.BLL.DTOs;
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
        EmployeeOwnerProfileDTO? GetOwnProfile(int userId);
        Task<Employee?> UpdateOwnProfile(
               int userId,
               EmployeeOwnerProfileDTO dto,
               IFormFile? avatarFile
           );
    }
}
