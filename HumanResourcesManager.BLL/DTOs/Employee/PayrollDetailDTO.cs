namespace HumanResourcesManager.BLL.DTOs.Employee
{
    public class EmployeePayrollDetailDTO
    {
        public int PayrollId { get; set; }
        public int Month { get; set; }
        public int Year { get; set; }

        public decimal BasicSalary { get; set; }
        public decimal TotalOT { get; set; }
        public decimal TotalAllowance { get; set; }
        public decimal MissingMinutesPenalty { get; set; }
        public decimal NetSalary { get; set; }

        public List<PayrollDetailDTO> Details { get; set; } = new();
    }
}
