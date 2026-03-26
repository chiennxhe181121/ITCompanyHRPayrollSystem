namespace HumanResourcesManager.BLL.DTOs.Employee
{
    public class PayrollRowDTO
    {
        public int PayrollId { get; set; }
        public int Month { get; set; }
        public int Year { get; set; }

        public decimal BasicSalary { get; set; }
        public decimal TotalOT { get; set; }
        public decimal TotalAllowance { get; set; }
        public decimal MissingMinutesPenalty { get; set; }
        public decimal NetSalary { get; set; }

        public DateTime CreatedDate { get; set; }
    }

    public class EmployeePayrollViewDTO
    {
        public List<PayrollRowDTO> Records { get; set; } = new();

        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }
        public int PageSize { get; set; }
        public int TotalRecords { get; set; }

        public int? SelectedMonth { get; set; }
        public int? SelectedYear { get; set; }
    }
}