using HumanResourcesManager.BLL.DTOs.Common;
using HumanResourcesManager.BLL.DTOs.Employee;
using HumanResourcesManager.DAL.Enum;

public interface ILeaveRequestService
{
    EmployeeLeaveViewDTO GetEmployeeLeaves(
    int currentUserId,
    int page,
    int pageSize,
    int? year,
    RequestStatus? status);
    void CancelLeave(int leaveId, int userId);
    List<LeaveRequestDTO> GetAll();
    LeaveRequestDTO? GetById(int id);
    void UpdateStatus(int leaveRequestId, RequestStatus status);
    ServiceResult CreateLeaveRequest(int employeeId, CreateLeaveRequestDTO dto);
    ServiceResult ApproveLeaveRequest(int leaveRequestId, int approverId);
    ServiceResult RejectLeaveRequest(int leaveRequestId, int approverId);
}
