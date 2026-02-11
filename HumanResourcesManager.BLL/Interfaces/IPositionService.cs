using HumanResourcesManager.BLL.DTOs.Position;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

// HoangDH
namespace HumanResourcesManager.BLL.Interfaces
{
    public interface IPositionService
    {
        IEnumerable<PositionListDTO> GetAll();
        PositionCreateUpdateDTO? GetById(int id);
        bool Create(PositionCreateUpdateDTO dto);
        bool Update(PositionCreateUpdateDTO dto);
        //void SoftDelete(int id);
        bool SoftDelete(int id); 
        void SetActive(int id);
        int Count();
    }

}
