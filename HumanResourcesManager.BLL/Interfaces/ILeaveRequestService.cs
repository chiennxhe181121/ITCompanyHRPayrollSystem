using HumanResourcesManager.BLL.DTOs.Common;
using HumanResourcesManager.DAL.Enum;

public interface ILeaveRequestService
{
    List<LeaveRequestDTO> GetAll();
    LeaveRequestDTO? GetById(int id);
    void UpdateStatus(int leaveRequestId, RequestStatus status);
    ServiceResult CreateLeaveRequest(int employeeId, CreateLeaveRequestDTO dto);
}
