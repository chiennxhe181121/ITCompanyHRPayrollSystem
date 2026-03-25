using HumanResourcesManager.BLL.DTOs.Common;
using HumanResourcesManager.DAL.Enum;
using HumanResourcesManager.DAL.Interfaces;
using HumanResourcesManager.DAL.Models;
using HumanResourcesManager.DAL.Repositories;
using HumanResourcesManager.DAL.Repository;
namespace HumanResourcesManager.DAL.Shared;

public class LeaveRequestService : ILeaveRequestService
{
    private readonly ILeaveRequestRepository _leaveRequestRepo;
    private readonly IAttendanceRepository _attendanceRepository;
    private readonly IAnnualLeaveBalanceRepositry _annualLeaveBalanceRepositry;
    private readonly ILeaveTypeRepository _leaveTypeRepository;
    private readonly IEmployeeRepository _employeeRepository;


    public LeaveRequestService(ILeaveRequestRepository leaveRequestRepository,
        IAttendanceRepository attendanceRepository,
        IAnnualLeaveBalanceRepositry annualLeaveBalanceRepositry,
        ILeaveTypeRepository leaveTypeRepository,
        IEmployeeRepository employeeRepository)
    {
        _leaveRequestRepo = leaveRequestRepository;
        _attendanceRepository = attendanceRepository;
        _annualLeaveBalanceRepositry = annualLeaveBalanceRepositry;
        _leaveTypeRepository = leaveTypeRepository;
        _employeeRepository = employeeRepository;
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

    private void EnsureAnnualLeaveBalance(int employeeId, int year)
    {
        if (_annualLeaveBalanceRepositry.Exists(employeeId, year))
            return;

        var now = GetVietnamNow();

        double carryOver = 0;

        var previousBalances = _annualLeaveBalanceRepositry
            .GetAll()
            .Where(x => x.EmployeeId == employeeId
                        && x.Year < year
                        && !x.IsExpired)
            .OrderByDescending(x => x.Year)
            .Take(3)
            .ToList();

        foreach (var prev in previousBalances)
        {
            if (prev.RemainingDays > 0)
                carryOver += prev.RemainingDays;
        }

        var employee = _employeeRepository.GetById(employeeId);
        if (employee == null)
            return;

        double entitled;

        if (employee.HireDate.Year < year)
        {
            entitled = Constants.AnnualLeavePerYear;
        }
        else if (employee.HireDate.Year == year)
        {
            int monthsRemaining =
                Constants.MonthsInYear - employee.HireDate.Month + 1;

            double leavePerMonth =
                Constants.AnnualLeavePerYear / Constants.MonthsInYear;

            entitled = Math.Round(leavePerMonth * monthsRemaining, 1);
        }
        else
        {
            return;
        }

        var balance = new AnnualLeaveBalance
        {
            EmployeeId = employeeId,
            Year = year,
            EntitledDays = entitled,
            UsedDays = 0,
            RemainingDays = entitled + carryOver,
            CreatedDate = now,
            IsExpired = false
        };

        _annualLeaveBalanceRepositry.Add(balance);

        foreach (var prev in previousBalances)
        {
            prev.IsExpired = true;
            _annualLeaveBalanceRepositry.Update(prev);
        }

        _annualLeaveBalanceRepositry.Save();
    }

    public ServiceResult CreateLeaveRequest(int employeeId, CreateLeaveRequestDTO dto)
    {
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

        // 🔥 Lấy LeaveType
        var leaveType = _leaveTypeRepository.GetById(dto.LeaveTypeId);

        if (leaveType == null)
        {
            return ServiceResult.Failure("Loại nghỉ không tồn tại.");
        }

        // 🔥 Maternity auto set ToDate
        if (leaveType.LeaveName == "Maternity Leave")
        {
            dto.ToDate = dto.FromDate.AddMonths(Constants.MATERNITY_MONTHS);
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

        // 👉 giờ mới validate
        if (dto.FromDate.Date > dto.ToDate.Date)
        {
            return ServiceResult.Failure("Ngày bắt đầu phải nhỏ hơn hoặc bằng ngày kết thúc.");
        }

        if (leaveType.LeaveName == "Maternity Leave")
        {
            var employee = _employeeRepository.GetById(employeeId);

            if (employee == null)
                return ServiceResult.Failure("Không tìm thấy nhân viên.");

            if (employee.Gender == true) // true = Male
            {
                return ServiceResult.Failure("Chỉ nhân viên nữ mới được đăng ký nghỉ thai sản.");
            }
        }

        if (leaveType.LeaveName == "Unpaid Leave")
        {
            return ServiceResult.Failure("Loại nghỉ này hiện chưa được hỗ trợ.");
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

            // 1️⃣ Đảm bảo balance tồn tại (có carry 3 năm nếu cần)
            EnsureAnnualLeaveBalance(employeeId, year);

            // 2️⃣ Lấy balance sau khi đã ensure
            var balance = _annualLeaveBalanceRepositry
                .GetByEmployeeAndYear(employeeId, year);

            if (balance == null || balance.IsExpired)
            {
                return ServiceResult.Failure("Không tìm thấy số dư phép năm hợp lệ.");
            }

            // 3️⃣ Check quota
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

        var leaveType = _leaveTypeRepository.GetById(leave.LeaveTypeId);

        if (leaveType == null)
            return ServiceResult.Failure("Loại nghỉ không tồn tại.");

        // 🔥 Tính số ngày nghỉ thực tế
        double requestedDays = CalculateLeaveDays(leave.FromDate, leave.ToDate);

        // ======================================================
        // 🔥 ANNUAL LEAVE → TRỪ QUOTA Ở ĐÂY (QUAN TRỌNG)
        // ======================================================
        if (leaveType.LeaveName == "Annual Leave")
        {
            int year = leave.FromDate.Year;

            // ensure balance tồn tại
            EnsureAnnualLeaveBalance(leave.EmployeeId, year);

            var balance = _annualLeaveBalanceRepositry
                .GetByEmployeeAndYear(leave.EmployeeId, year);

            if (balance == null || balance.IsExpired)
            {
                return ServiceResult.Failure("Không tìm thấy số dư phép năm hợp lệ.");
            }

            if (balance.RemainingDays < requestedDays)
            {
                return ServiceResult.Failure(
                    $"Không đủ ngày phép để duyệt. Còn {balance.RemainingDays} ngày.");
            }

            // 🔥 TRỪ PHÉP
            balance.UsedDays += requestedDays;
            balance.RemainingDays -= requestedDays;

            _annualLeaveBalanceRepositry.Update(balance);
        }

        // ======================================================
        // 🔥 UPDATE STATUS
        // ======================================================
        leave.Status = RequestStatus.Approved;
        leave.ApprovedBy = approverId;
        leave.ApprovedDate = GetVietnamNow();

        _leaveRequestRepo.Update(leave);

        // 🔥 SAVE 1 LẦN
        _leaveRequestRepo.Save();
        _annualLeaveBalanceRepositry.Save();

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
