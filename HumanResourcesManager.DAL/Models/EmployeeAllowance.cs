using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HumanResourcesManager.DAL.Models
{
    public class EmployeeAllowance
    {
        public int EmployeeId { get; set; }
        public Employee Employee { get; set; } = null!;

        public int AllowanceId { get; set; }
        public Allowance Allowance { get; set; } = null!;
    }

}
