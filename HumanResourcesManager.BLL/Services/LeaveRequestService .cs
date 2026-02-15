using HumanResourcesManager.BLL.DTOs.Common;
using HumanResourcesManager.DAL.Enum;
using HumanResourcesManager.DAL.Interfaces;
using HumanResourcesManager.DAL.Models;
using HumanResourcesManager.DAL.Repository;
namespace HumanResourcesManager.DAL.Shared;

public class LeaveRequestService : ILeaveRequestService
{
    private readonly ILeaveRequestRepository _leaveRequestRepo;
    private readonly IAttendanceRepository _attendanceRepository;
    private readonly IAnnualLeaveBalanceRepositry _annualLeaveBalanceRepositry;
    private readonly ILeaveTypeRepository _leaveTypeRepository;

    public LeaveRequestService(ILeaveRequestRepository leaveRequestRepository, IAttendanceRepository attendanceRepository, IAnnualLeaveBalanceRepositry annualLeaveBalanceRepositry, ILeaveTypeRepository leaveTypeRepository)
    {
        _leaveRequestRepo = leaveRequestRepository;
        _attendanceRepository = attendanceRepository;
        _annualLeaveBalanceRepositry = annualLeaveBalanceRepositry;
        _leaveTypeRepository = leaveTypeRepository;
    }

    public List<LeaveRequestDTO> GetAll()
    {
        return _leaveRequestRepo.GetAll().Select(x => new LeaveRequestDTO
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
        var x = _leaveRequestRepo.GetById(id);
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

    public void UpdateStatus(int leaveRequestId, RequestStatus status)
    {
        var entity = _leaveRequestRepo.GetById(leaveRequestId);
        if (entity == null) return;

        entity.Status = status;
        _leaveRequestRepo.Update(entity);
        _leaveRequestRepo.Save();
    }

    private DateTime GetVietnamNow()
    {
        return TimeZoneInfo.ConvertTimeFromUtc(
            DateTime.UtcNow,
            TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time")
        );
    }

    private double CalculateLeaveDays(DateTime fromDate, DateTime toDate)
    {
        double totalDays = 0;

        for (var date = fromDate.Date; date <= toDate.Date; date = date.AddDays(1))
        {
            bool isFixedHoliday = Constants.FixedHolidays
                .Any(h => h.Day == date.Day && h.Month == date.Month);

            bool isTetHoliday = LunarHelper
                .GetTetHolidayDates(date.Year)
                .Contains(date);

            if (!isFixedHoliday && !isTetHoliday)
            {
                totalDays += 1;
            }
        }

        return totalDays;
    }

    public ServiceResult CreateLeaveRequest(int employeeId, CreateLeaveRequestDTO dto)
    {
        if (dto.FromDate.Date > dto.ToDate.Date)
        {
            return ServiceResult.Failure("Ngày bắt đầu phải nhỏ hơn hoặc bằng ngày kết thúc.");
        }

        var vietnamNow = GetVietnamNow();
        var deadline = dto.FromDate.Date;

        if (dto.FromDate.Year != dto.ToDate.Year)
        {
            return ServiceResult.Failure("Không thể tạo đơn nghỉ xuyên năm.");
        }

        if (vietnamNow >= deadline)
        {
            return ServiceResult.Failure("Bạn chỉ có thể tạo đơn nghỉ trước 0h của ngày bắt đầu nghỉ.");
        }

        bool exists = _leaveRequestRepo.ExistsActiveRequest(
            employeeId,
            dto.FromDate,
            dto.ToDate
        );

        if (exists)
        {
            return ServiceResult.Failure("Bạn đã có đơn nghỉ đang chờ duyệt hoặc đã được duyệt trong khoảng thời gian này.");
        }

        // 🔥 Lấy LeaveType
        var leaveType = _leaveTypeRepository.GetById(dto.LeaveTypeId);

        if (leaveType == null)
        {
            return ServiceResult.Failure("Loại nghỉ không tồn tại.");
        }

        // 🔥 Tính số ngày nghỉ thực tế
        double requestedDays = CalculateLeaveDays(dto.FromDate, dto.ToDate);

        // 🔥 Không cho tạo đơn quá 12 ngày

        if (requestedDays <= 0)
        {
            return ServiceResult.Failure("Khoảng thời gian này không có ngày làm việc hợp lệ.");
        }

        // ======================================================
        // 🔥 CHỈ Annual Leave mới check quota
        // ======================================================
        if (leaveType.LeaveName == "Annual Leave")
        {
            int year = dto.FromDate.Year;

            var balance = _annualLeaveBalanceRepositry
                .GetByEmployeeAndYear(employeeId, year);

            if (balance == null)
            {
                return ServiceResult.Failure("Không tìm thấy thông tin số ngày phép năm.");
            }

            if (balance.RemainingDays < requestedDays)
            {
                return ServiceResult.Failure(
                    $"Số ngày phép còn lại không đủ. Bạn còn {balance.RemainingDays} ngày.");
            }
        }

        // ======================================================
        // ❌ Unpaid Leave → không cần check quota
        // ❌ Maternity Leave → xử lý policy riêng sau
        // ======================================================

        var leave = new LeaveRequest
        {
            EmployeeId = employeeId,
            LeaveTypeId = dto.LeaveTypeId,
            FromDate = dto.FromDate,
            ToDate = dto.ToDate,
            Reason = dto.Reason,
            CreatedDate = vietnamNow,
            Status = RequestStatus.Pending
        };

        _leaveRequestRepo.Add(leave);
        _leaveRequestRepo.Save();

        return ServiceResult.Success(
            $"Tạo đơn nghỉ thành công. Số ngày yêu cầu: {requestedDays}.");
    }

    public ServiceResult ApproveLeaveRequest(int leaveRequestId, int approverId)
    {
        var leave = _leaveRequestRepo.GetById(leaveRequestId);

        if (leave == null)
            return ServiceResult.Failure("Không tìm thấy đơn nghỉ.");

        if (leave.Status != RequestStatus.Pending)
            return ServiceResult.Failure("Chỉ có thể duyệt đơn đang chờ.");

        leave.Status = RequestStatus.Approved;
        leave.ApprovedBy = approverId;
        leave.ApprovedDate = GetVietnamNow();

        _leaveRequestRepo.Update(leave);
        _leaveRequestRepo.Save();

        // 👉 Tạo / cập nhật Attendance sau khi duyệt
        for (var date = leave.FromDate.Date; date <= leave.ToDate.Date; date = date.AddDays(1))
        {
            var attendance = _attendanceRepository
                .GetByEmployeeAndWorkDate(leave.EmployeeId, date);

            if (attendance == null)
            {
                var newAttendance = new Attendance
                {
                    EmployeeId = leave.EmployeeId,
                    WorkDate = date,
                    Status = AttendanceStatus.ApprovedLeave,
                    MissingMinutes = 0
                };

                _attendanceRepository.Add(newAttendance);
            }
            else
            {
                attendance.Status = AttendanceStatus.ApprovedLeave;
                attendance.MissingMinutes = 0;
                _attendanceRepository.Update(attendance);
            }
        }

        _attendanceRepository.Save();

        // Cập nhật AnnualLeaveBalance của cronjob tạo sau khi duyệt
        // TODO: Đang thiếu cronjob tạo record 12 nghỉ/ năm, chạy theo ngày rồi tạo hoặc cập nhật thay vì chạy theo năm

        return ServiceResult.Success("Duyệt đơn nghỉ thành công.");
    }

    public ServiceResult RejectLeaveRequest(int leaveRequestId, int approverId)
    {
        var leave = _leaveRequestRepo.GetById(leaveRequestId);

        if (leave == null)
            return ServiceResult.Failure("Không tìm thấy đơn nghỉ.");

        if (leave.Status != RequestStatus.Pending)
            return ServiceResult.Failure("Chỉ có thể từ chối đơn đang chờ.");

        leave.Status = RequestStatus.Rejected;
        leave.ApprovedBy = approverId;
        leave.ApprovedDate = GetVietnamNow();

        _leaveRequestRepo.Update(leave);
        _leaveRequestRepo.Save();

        return ServiceResult.Success("Đã từ chối đơn nghỉ.");
    }
}
