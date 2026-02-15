using HumanResourcesManager.DAL.Models;

public interface ILeaveRequestRepository
{
    List<LeaveRequest> GetAll();
    LeaveRequest? GetById(int id);
    bool ExistsActiveRequest(int employeeId, DateTime fromDate, DateTime toDate);
    void Add(LeaveRequest entity);
    void Update(LeaveRequest entity);
    void Save();
}
