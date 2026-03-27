using HumanResourcesManager.DAL.Enum;

namespace HumanResourcesManager.BLL.DTOs.Employee
{
    public class OTTodayAttendanceViewDTO
    {
        public int OverTimeRequestId { get; set; }
        public DateTime WorkDate { get; set; }

        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }

        public TimeSpan? CheckInTime { get; set; }
        public TimeSpan? CheckOutTime { get; set; }

        public string? CheckInImagePath { get; set; }
        public string? CheckOutImagePath { get; set; }

        public AttendanceStatus? Status { get; set; }

        public bool HasCheckIn => CheckInTime.HasValue;
        public bool HasCheckOut => CheckOutTime.HasValue;
    }

    public class OTAttendanceRowDTO
    {
        public int OverTimeRequestId { get; set; }
        public DateTime WorkDate { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }

        public TimeSpan? CheckIn { get; set; }
        public TimeSpan? CheckOut { get; set; }

        public string? CheckInImagePath { get; set; }
        public string? CheckOutImagePath { get; set; }

        public AttendanceStatus? Status { get; set; }
        public string? Reason { get; set; }
        public int MissingMinutes { get; set; }
    }

    public class EmployeeOTAttendanceHistoryViewDTO
    {
        public List<OTAttendanceRowDTO> Records { get; set; } = new();

        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }
        public int PageSize { get; set; }
        public int TotalRecords { get; set; }

        public int? SelectedMonth { get; set; }
        public int? SelectedYear { get; set; }
    }
}

