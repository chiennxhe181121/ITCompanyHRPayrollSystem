using HumanResourcesManager.DAL.Enum;


    public class LeaveRequestDTO
{
    public int LeaveRequestId { get; set; }

    public int EmployeeId { get; set; }
    public string EmployeeName { get; set; } = "";

    public int LeaveTypeId { get; set; }
    public string LeaveTypeName { get; set; } = "";

    public DateTime FromDate { get; set; }
    public DateTime ToDate { get; set; }
    public RequestStatus Status { get; set; }
}
public class ApproveRejectLeaveDTO
{
    public int LeaveRequestId { get; set; }
    public string? Comments { get; set; }   // tương ứng với trường Reply trong model
}

public class LeaveRequestViewDTO
{
    public List<LeaveRequestDTO> Records { get; set; } = new();
    public int CurrentPage { get; set; }
    public int TotalPages { get; set; }
    public int PageSize { get; set; }
    public int TotalRecords { get; set; }
    public RequestStatus? SelectedStatus { get; set; }
}