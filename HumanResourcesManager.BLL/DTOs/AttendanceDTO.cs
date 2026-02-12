namespace HumanResourcesManager.BLL.DTOs
{
    public class AttendanceRowDTO
    {
        public DateTime WorkDate { get; set; }

        public TimeSpan? CheckIn { get; set; }
        public TimeSpan? CheckOut { get; set; }

        public int? MissingMinutes { get; set; }
        public string? CheckInImagePath { get; set; }
        public string? CheckOutImagePath { get; set; }

        public string Status { get; set; } = null!;
    }

    public class EmployeeAttendanceViewDTO
    {
        public List<AttendanceRowDTO> Records { get; set; } = new();

        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }

        public int PageSize { get; set; }
        public int TotalRecords { get; set; }
    }
}
