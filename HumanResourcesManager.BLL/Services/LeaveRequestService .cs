public class LeaveRequestService : ILeaveRequestService
{
    private readonly ILeaveRequestRepository _repo;

    public LeaveRequestService(ILeaveRequestRepository repo)
    {
        _repo = repo;
    }

    public List<LeaveRequestDTO> GetAll()
    {
        return _repo.GetAll().Select(x => new LeaveRequestDTO
        {
            LeaveRequestId = x.LeaveRequestId,
            EmployeeId = x.EmployeeId,
            EmployeeName = x.Employee.FullName,
            LeaveTypeId = x.LeaveTypeId,
            LeaveTypeName = x.LeaveType.LeaveName,
            FromDate = x.FromDate,
            ToDate = x.ToDate,
            Status = x.Status
        }).ToList();
    }

    public LeaveRequestDTO? GetById(int id)
    {
        var x = _repo.GetById(id);
        if (x == null) return null;

        return new LeaveRequestDTO
        {
            LeaveRequestId = x.LeaveRequestId,
            EmployeeId = x.EmployeeId,
            LeaveTypeId = x.LeaveTypeId,
            FromDate = x.FromDate,
            ToDate = x.ToDate,
            Status = x.Status
        };
    }

    public void UpdateStatus(int leaveRequestId, long status)
    {
        var entity = _repo.GetById(leaveRequestId);
        if (entity == null) return;

        entity.Status = status;
        _repo.Update(entity);
        _repo.Save();
    }
}
