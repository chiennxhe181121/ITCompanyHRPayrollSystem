using HumanResourcesManager.DAL.Enum;
using Microsoft.AspNetCore.Http;

namespace HumanResourcesManager.BLL.DTOs.Employee
{
    public class CheckInDTO
    {
        public DateTime WorkDate { get; set; }
        public TimeSpan? CheckInTime { get; set; }
        public IFormFile? CheckInImage { get; set; }
    }

    public class CheckOutDTO
    {
        public DateTime WorkDate { get; set; }
        public TimeSpan? CheckOutTime { get; set; }
        public IFormFile? CheckOutImage { get; set; }
    }

    // ================= HISTORY =================

    public class AttendanceRowDTO
    {
        public DateTime WorkDate { get; set; }

        public TimeSpan? CheckIn { get; set; }
        public TimeSpan? CheckOut { get; set; }

        public int? MissingMinutes { get; set; }
        public string? CheckInImagePath { get; set; }
        public string? CheckOutImagePath { get; set; }
        public AttendanceStatus Status { get; set; }
    }

    public class EmployeeAttendanceViewDTO
    {
        public List<AttendanceRowDTO> Records { get; set; } = new();

        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }
        public int PageSize { get; set; }
        public int TotalRecords { get; set; }

        public int? SelectedMonth { get; set; }
        public int? SelectedYear { get; set; }
        public AttendanceStatus? SelectedStatus { get; set; }
    }

    // ================= TODAY (INDEX PAGE) =================

    public class TodayAttendanceViewDTO
    {
        public DateTime WorkDate { get; set; }

        public TimeSpan? CheckInTime { get; set; }
        public TimeSpan? CheckOutTime { get; set; }

        public AttendanceStatus? Status { get; set; }

        // ================= DISPLAY =================

        public bool IsLeave =>
            Status == AttendanceStatus.ApprovedLeave;

        public bool CanCheckIn =>
            Status == AttendanceStatus.Pending
            && !CheckInTime.HasValue;

        public bool CanCheckOut =>
            Status == AttendanceStatus.Pending
            && CheckInTime.HasValue
            && !CheckOutTime.HasValue;
    }
}
