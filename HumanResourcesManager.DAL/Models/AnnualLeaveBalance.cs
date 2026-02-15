namespace HumanResourcesManager.DAL.Models
{
    public class AnnualLeaveBalance
    {
        public int AnnualLeaveBalanceId { get; set; }

        public int EmployeeId { get; set; }
        public Employee Employee { get; set; } = null!;

        public int Year { get; set; }

        // Số ngày được cấp trong năm (ví dụ 12, 14…)
        public double EntitledDays { get; set; }

        // Số ngày đã sử dụng
        public double UsedDays { get; set; }

        // Số ngày còn lại
        public double RemainingDays { get; set; }

        // Ngày reset quota
        public DateTime CreatedDate { get; set; }
    }
}
