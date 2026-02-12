using HumanResourcesManager.BLL.DTOs.ADEmployee;
using HumanResourcesManager.DAL.Models;

namespace HumanResourcesManager.BLL.Interfaces
{
    public interface IADEmployeeService
    {
        List<Employee> GetAllEmployees();

        bool CreateEmployee(ADEmployeeCreateDTO dto, out string message);
        List<DepartmentSelectDTO> GetDepartments();
        List<PositionSelectDTO> GetPositions();

        ADEmployeeUpdateDTO? GetById(int id);
        bool UpdateEmployee(ADEmployeeUpdateDTO dto, out string message);
        //void SetStatus(int id, int status); 
 
        void SetStatus(int id, int status, int? currentUserId);
    }
}