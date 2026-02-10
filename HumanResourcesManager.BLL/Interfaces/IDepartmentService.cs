using HumanResourcesManager.BLL.DTOs.Common;
using HumanResourcesManager.BLL.DTOs.Department;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

// HoangDH
namespace HumanResourcesManager.BLL.Interfaces
{
    public interface IDepartmentService
    {
        List<DepartmentListDTO> GetAll(string? keyword);
        DepartmentCreateUpdateDTO? GetById(int id);

        bool Create(DepartmentCreateUpdateDTO dto);
        bool Update(DepartmentCreateUpdateDTO dto);
        bool Delete(int id); // soft delete
        void SetActive(int id);

        PagedResult<DepartmentListDTO> Search(
            string? keyword,
            int? status,
            int page,
            int pageSize
        );

        int Count();
    }
}
