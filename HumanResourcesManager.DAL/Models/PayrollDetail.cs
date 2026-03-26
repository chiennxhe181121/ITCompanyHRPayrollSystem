using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HumanResourcesManager.DAL.Enum;

namespace HumanResourcesManager.DAL.Models
{
    public class PayrollDetail
    {
        public int PayrollDetailId { get; set; }
        public int PayrollId { get; set; }
        public Payroll Payroll { get; set; } = null!;
        public PayrollDetailType Type { get; set; }
        public string Description { get; set; } = null!;
        public decimal Amount { get; set; }
    }

}
