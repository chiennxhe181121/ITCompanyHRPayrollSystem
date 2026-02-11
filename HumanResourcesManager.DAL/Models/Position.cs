using HumanResourcesManager.DAL.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HumanResourcesManager.DAL.Models
{
    public class Position
    {
        public int PositionId { get; set; }
        public string PositionName { get; set; } = null!;
        public string? Description { get; set; } // Hoàng thêm 
        public decimal BaseSalary { get; set; }
        public int Status { get; set; } = Constants.Active; // 1 = Active, 0 = Inactive

        public DateTime CreatedAt { get; set; } = DateTime.Now; // Hoàng thêm
        public DateTime? UpdatedAt { get; set; } // Hoàng thêm
        public ICollection<Employee> Employees { get; set; } = new List<Employee>();
    }

}
