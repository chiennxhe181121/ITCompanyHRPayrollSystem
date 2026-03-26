using HumanResourcesManager.DAL.Enum;

namespace HumanResourcesManager.BLL.DTOs.Employee
{
    public class LeaveRowDTO
    {
        public int LeaveRequestId { get; set; }
        public string LeaveTypeName { get; set; }
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public double TotalDays { get; set; }
        public string? Reason { get; set; }
        public RequestStatus Status { get; set; }
    }

    public class EmployeeLeaveViewDTO
    {
        public List<LeaveRowDTO> Records { get; set; }

        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }
        public int PageSize { get; set; }
        public int TotalRecords { get; set; }

        public int? SelectedYear { get; set; }
        public RequestStatus? SelectedStatus { get; set; }
    }
}
