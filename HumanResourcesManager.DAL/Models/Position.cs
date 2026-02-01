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
        public decimal BaseSalary { get; set; }

        public ICollection<Employee> Employees { get; set; } = new List<Employee>();
    }

}
