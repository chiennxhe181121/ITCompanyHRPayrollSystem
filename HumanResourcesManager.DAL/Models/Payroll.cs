using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HumanResourcesManager.DAL.Models
{
    public class Payroll
    {
        public int PayrollId { get; set; }
        public int EmployeeId { get; set; }
        public Employee Employee { get; set; } = null!;

        public int Month { get; set; }
        public int Year { get; set; }

        public decimal BasicSalary { get; set; }
        public decimal TotalOT { get; set; }
        public decimal TotalAllowance { get; set; }
        public decimal MissingMinutesPenalty { get; set; }
        public decimal NetSalary { get; set; }
        public DateTime CreatedDate { get; set; }
        public ICollection<PayrollDetail> PayrollDetails { get; set; } = new List<PayrollDetail>();
    }

}
