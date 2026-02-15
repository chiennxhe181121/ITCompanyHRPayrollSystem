using HumanResourcesManager.DAL.Enum;

namespace HumanResourcesManager.DAL.Models
{
    public class LeaveRequest
    {
        public int LeaveRequestId { get; set; }
        public int EmployeeId { get; set; }
        public Employee Employee { get; set; } = null!;
        public int LeaveTypeId { get; set; }
        public LeaveType LeaveType { get; set; } = null!;
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public string? Reason { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? ApprovedDate { get; set; }
        public int? ApprovedBy { get; set; }
        public string? Reply { get; set; }
        public RequestStatus Status { get; set; }
    }
}
