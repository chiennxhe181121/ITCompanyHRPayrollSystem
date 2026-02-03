public interface ILeaveRequestService
{
    List<LeaveRequestDTO> GetAll();
    LeaveRequestDTO? GetById(int id);
    void UpdateStatus(int leaveRequestId, long status);
}
