using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HumanResourcesManager.DAL.Models
{
    public class Allowance
    {
        public int AllowanceId { get; set; }
        public string AllowanceName { get; set; } = null!;
        public decimal Amount { get; set; }
        
        public int Status { get; set; } // Hoàng thêm 
        public ICollection<EmployeeAllowance> EmployeeAllowances { get; set; } = new List<EmployeeAllowance>();
    }

}
