namespace HumanResourcesManager.DAL.Models
{
    public class Employee
    {
        public int EmployeeId { get; set; }
        public int? UserId { get; set; }
        public UserAccount? UserAccount { get; set; }
        public string EmployeeCode { get; set; } = null!;
        public string FullName { get; set; } = null!;

        // true = Male, false = Female
        public bool Gender { get; set; }

        public DateTime DateOfBirth { get; set; }

        public string Email { get; set; } = null!;
        public string Phone { get; set; } = null!;

        public string? Address { get; set; }

        // ===== AVATAR =====
        // Lưu path hoặc URL ảnh đại diện
        // Có thể null → user update sau
        public string? ImgAvatar { get; set; }

        public DateTime HireDate { get; set; }

        // ===== EMPLOYEE STATUS =====
        // 1 = Working
        // 0 = Inactive / Resigned
        // -1 = Deleted (xóa mềm)
        public int Status { get; set; }

        // ===== RELATION =====
        public int DepartmentId { get; set; }
        public Department Department { get; set; } = null!;
        public int PositionId { get; set; }
        public Position Position { get; set; } = null!;

        // ===== NAVIGATION =====
        public ICollection<Contract> Contracts { get; set; } = new List<Contract>();
        public ICollection<Attendance> Attendances { get; set; } = new List<Attendance>();
        public ICollection<LeaveRequest> LeaveRequests { get; set; } = new List<LeaveRequest>();
        public ICollection<Payroll> Payrolls { get; set; } = new List<Payroll>();
        public ICollection<EmployeeAllowance> EmployeeAllowances { get; set; } = new List<EmployeeAllowance>();
        public ICollection<OverTimeRequest> OverTimeRequests { get; set; } = new List<OverTimeRequest>();
        public ICollection<AnnualLeaveBalance> AnnualLeaveBalances { get; set; } = new List<AnnualLeaveBalance>();
    }
}
