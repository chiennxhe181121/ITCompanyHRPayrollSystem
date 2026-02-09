using HumanResourcesManager.DAL.Models;

public interface ILeaveRequestRepository
{
    List<LeaveRequest> GetAll();
    LeaveRequest? GetById(int id);
    void Update(LeaveRequest entity);
    void Save();
}
