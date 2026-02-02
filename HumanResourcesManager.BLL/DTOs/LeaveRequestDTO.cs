public class LeaveRequestDTO
{
    public int LeaveRequestId { get; set; }

    public int EmployeeId { get; set; }
    public string EmployeeName { get; set; } = "";

    public int LeaveTypeId { get; set; }
    public string LeaveTypeName { get; set; } = "";

    public DateTime FromDate { get; set; }
    public DateTime ToDate { get; set; }

    // 0 = Pending, 1 = Approved, -1 = Rejected
    public long Status { get; set; }
}
