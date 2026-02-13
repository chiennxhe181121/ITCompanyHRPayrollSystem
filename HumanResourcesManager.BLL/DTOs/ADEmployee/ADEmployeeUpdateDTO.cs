using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

// HoangDH
namespace HumanResourcesManager.BLL.DTOs.ADEmployee
{
    public class ADEmployeeUpdateDTO : ADEmployeeCreateDTO
    {
        public int EmployeeId { get; set; }

        public bool HasAccount { get; set; }
    }
}
