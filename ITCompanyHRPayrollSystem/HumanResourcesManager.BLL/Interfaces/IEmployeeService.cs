using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HumanResourcesManager.BLL.DTOs;

namespace HumanResourcesManager.BLL.Interfaces
{
    public interface IEmployeeService
    {
        IEnumerable<EmployeeDTO> GetAll();
        EmployeeDTO? GetById(int id);
        void Create(EmployeeDTO dto);
        void Update(EmployeeDTO dto);
        void Delete(int id);
    }
}
