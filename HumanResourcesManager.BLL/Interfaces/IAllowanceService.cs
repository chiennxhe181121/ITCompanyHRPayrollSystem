using HumanResourcesManager.BLL.DTOs.Allowance;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

// HoangDH
namespace HumanResourcesManager.BLL.Interfaces
{
    public interface IAllowanceService
    {
        List<AllowanceDTO> GetAll();
        AllowanceDTO? GetById(int id);
        bool Create(AllowanceDTO dto);
        bool Update(AllowanceDTO dto);
        bool ToggleStatus(int id); 
    }
}
