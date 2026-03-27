using HumanResourcesManager.BLL.DTOs.Employee;
using HumanResourcesManager.BLL.Interfaces;
using HumanResourcesManager.BLL.Services;
using HumanResourcesManager.Helpers;
using HumanResourcesManager.Models.Employee;
using HumanResourcesManager.DAL.Enum;
using HumanResourcesManager.DAL.Interfaces;
using HumanResourcesManager.DAL.Models;
using HumanResourcesManager.DAL.Shared;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Text.Json;
using Volo.Abp;

[Authorize(Roles = "EMP,MANAGER,HR")]
[Route("HumanResourcesManager/employee")]
public class EmployeeController : Controller
{
    private readonly IEmployeeService _employeeService;
    private readonly IAttendanceService _attendanceService;
    private readonly IUserAccountService _userAccountService;
    private readonly ILeaveRequestService _leaveRequestService;
    private readonly ILeaveTypeRepository _leaveTypeRepository;
    private readonly IAnnualLeaveBalanceService _annualLeaveBalanceService;
    private readonly IOTScheduleService _scheduleService;
    private readonly IOTAttendanceService _otAttendanceService;
    private readonly IPayrollService _payrollService;

    public EmployeeController(
        IEmployeeService employeeService,
        IAttendanceService attendanceService,
        IUserAccountService userAccountService,
        ILeaveRequestService leaveRequestService,
        ILeaveTypeRepository leaveTypeRepository,
        IAnnualLeaveBalanceService annualLeaveBalanceService,
        IOTScheduleService scheduleService,
        IOTAttendanceService otAttendanceService,
        IPayrollService payrollService
        )
    {
        _employeeService = employeeService;
        _attendanceService = attendanceService;
        _userAccountService = userAccountService;
        _leaveRequestService = leaveRequestService;
        _leaveTypeRepository = leaveTypeRepository;
        _annualLeaveBalanceService = annualLeaveBalanceService;
        _scheduleService = scheduleService;
        _otAttendanceService = otAttendanceService;
        _payrollService = payrollService;
    }

    // Lấy userId từ session
    private int CurrentUserId =>
        int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    private DateTime GetVietnamNow()
    {
        return TimeZoneInfo.ConvertTimeFromUtc(
            DateTime.UtcNow,
            TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time")
        );
    }

    // ===== Stats =====
    private async Task LoadStatsAsync()
    {
        var now = GetVietnamNow();
        Console.WriteLine($"NOW = {now.Month}/{now.Year}");

        ViewBag.MonthAttendance =
            _attendanceService.CountAttendanceDays(CurrentUserId, now.Month, now.Year);

        ViewBag.RemainingLeaveDays =
            _annualLeaveBalanceService.GetRemainingDays(CurrentUserId, now.Year);

        ViewBag.CurrentSalary = _payrollService.GetCurrentSalary(CurrentUserId);

        ViewBag.MonthOvertimeHours =
            await _otAttendanceService.GetMonthActualOTHoursAsync(CurrentUserId, now.Month, now.Year);
    }

    // ===== Sidebar User =====
    private void LoadSidebarUserCard()
    {
        var fullProfile = _employeeService.GetOwnProfile(CurrentUserId);

        if (fullProfile != null)
        {
            ViewBag.SidebarUserName = fullProfile.FullName;
            ViewBag.SidebarUserPosition = fullProfile.PositionName;

            // Nếu có avatar thì dùng avatar
            if (!string.IsNullOrEmpty(fullProfile.ImgAvatar))
            {
                ViewBag.SidebarUserAvatar = fullProfile.ImgAvatar;
                ViewBag.HasAvatar = true;
            }
            else
            {
                ViewBag.SidebarUserAvatarInitial =
                    string.IsNullOrEmpty(fullProfile.FullName)
                        ? "E"
                        : fullProfile.FullName.Substring(0, 1).ToUpper();

                ViewBag.HasAvatar = false;
            }
        }
    }

    // ===== Attendance =====
    // view attendance
    [HttpGet("attendance")]
    public async Task<IActionResult> Index()
    {
        LoadSidebarUserCard();

        await LoadStatsAsync();

        var model = _attendanceService.GetTodayAttendance(CurrentUserId);

        return View(model);
    }

    // check-in
    [HttpPost("attendance/check-in")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CheckIn(CheckInDTO dto)
    {
        var result = await _attendanceService.CheckIn(CurrentUserId, dto);

        TempData[result.IsSuccess ? "Success" : "Error"] = result.Message;

        return RedirectToAction("Index");
    }

    // check-out
    [HttpPost("attendance/check-out")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CheckOut(CheckOutDTO dto)
    {
        var result = await _attendanceService.CheckOut(CurrentUserId, dto);

        TempData[result.IsSuccess ? "Success" : "Error"] = result.Message;

        return RedirectToAction("Index");
    }

    // view attendance history
    [HttpGet("attendance/history")]
    public IActionResult History(
        int page = 1,
        int pageSize = 5,
        int? month = null,
        int? year = null,
        AttendanceStatus? status = null)
    {
        LoadSidebarUserCard();

        var model = _attendanceService.GetEmployeeAttendance(
            CurrentUserId,
            page,
            pageSize,
            month,
            year,
            status
        );

        return View("~/Views/Employee/AttendanceHistory.cshtml", model);
    }

    // ===== Profile =====
    // view profile
    [HttpGet("profile")]
    public IActionResult Profile()
    {
        LoadSidebarUserCard();

        var employee = _employeeService.GetOwnProfile(CurrentUserId);

        ViewData["EmployeeJson"] = JsonSerializer.Serialize(employee);

        return View("~/Views/Employee/ProfileTab.cshtml", employee);
    }

    [HttpPost("update-profile")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateProfile(
        EmployeeOwnerProfileDTO model,
        IFormFile? avatarFile
    )
    {
        ModelState.Remove(nameof(EmployeeOwnerProfileDTO.EmployeeCode));
        ModelState.Remove(nameof(EmployeeOwnerProfileDTO.DepartmentName));
        ModelState.Remove(nameof(EmployeeOwnerProfileDTO.PositionName));
        ModelState.Remove(nameof(EmployeeOwnerProfileDTO.HireDate));

        if (!ModelState.IsValid)
        {
            var errors = ModelState.Values
                .SelectMany(v => v.Errors)
                .Select(e => e.ErrorMessage)
                .ToList();

            var fullProfile = _employeeService.GetOwnProfile(CurrentUserId);

            ModelState.Clear();

            foreach (var error in errors)
            {
                ModelState.AddModelError(string.Empty, error);
            }

            return View("ProfileTab", fullProfile);
        }

        try
        {
            await _employeeService.UpdateOwnProfile(
                CurrentUserId,
                model,
                avatarFile
            );

            TempData["Success"] = "Cập nhật thành công";
            return RedirectToAction("Profile");
        }
        catch (BusinessException ex)
        {
            var errorMessage = ex.Details ?? ex.Message;

            var fullProfile = _employeeService.GetOwnProfile(CurrentUserId);

            ModelState.Clear(); // clear trước

            ModelState.AddModelError(string.Empty, errorMessage); // add lại lỗi

            return View("ProfileTab", fullProfile);
        }
    }

    // change password
    [HttpGet("profile/change-password")]
    public IActionResult ChangePassword()
    {
        LoadSidebarUserCard();

        return View("~/Views/Employee/ChangePassword.cshtml");
    }

    [HttpPost("profile/change-password")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ChangePassword(ChangePasswordDTO dto)
    {
        if (!ModelState.IsValid)
            return View(dto);

        var result = _userAccountService.ChangePassword(CurrentUserId, dto);

        if (!result.IsSuccess)
        {
            ModelState.AddModelError("", result.Message);
            return View(dto);
        }

        HttpContext.Session.Clear();

        await HttpContext.SignOutAsync(
            CookieAuthenticationDefaults.AuthenticationScheme
        );

        TempData["Success"] = "Đổi mật khẩu thành công. Vui lòng đăng nhập lại.";

        return RedirectToAction("Login", "Auth");
    }

    // ===== Leaves =====
    // view leaves
    [HttpGet("leaves")]
    public async Task<IActionResult> Leaves(
    int page = 1,
    int pageSize = 5,
    int? year = null,
    RequestStatus? status = null)
    {
        LoadSidebarUserCard();

        await LoadStatsAsync();

        var model = _leaveRequestService.GetEmployeeLeaves(
            CurrentUserId,
            page,
            pageSize,
            year,
            status
        );
        return View("~/Views/Employee/LeavesTab.cshtml", model);
    }

    // cancel leaves
    [HttpPost]
    public IActionResult CancelLeave(int id)
    {
        _leaveRequestService.CancelLeave(id, CurrentUserId);
        return RedirectToAction("Leaves");
    }

    // GET: hiển thị form
    [HttpGet("leaves/request")]
    public IActionResult CreateLeave()
    {
        LoadSidebarUserCard();

        LoadLeaveTypes();

        return View("~/Views/Employee/CreateLeave.cshtml");
    }

    private void LoadLeaveTypes()
    {
        ViewBag.LeaveTypes = _leaveTypeRepository.GetAll();
    }

    // POST: submit form
    [HttpPost("leaves/request")]
    [ValidateAntiForgeryToken]
    public IActionResult CreateLeave(CreateLeaveRequestDTO dto, bool agreeRule)
    {
        if (!agreeRule)
        {
            ModelState.AddModelError("", "Bạn phải đồng ý với quy định nghỉ phép.");
        }

        if (!ModelState.IsValid)
        {
            LoadLeaveTypes();
            return View(dto);
        }

        var result = _leaveRequestService.CreateLeaveRequest(CurrentUserId, dto);

        if (!result.IsSuccess)
        {
            ModelState.AddModelError(string.Empty, result.Message);
            LoadLeaveTypes();
            return View(dto);
        }

        TempData["Success"] = result.Message;
        return RedirectToAction("Leaves");
    }


    [HttpGet("overtime")]
    public async Task<IActionResult> Overtime(int historyPage = 1, int historyPageSize = 5, int? historyMonth = null, int? historyYear = null)
    {
        _scheduleService.CancelExpiredSchedules();
        LoadSidebarUserCard();
        await LoadStatsAsync();
        
        // Pass Open Schedules to view
        var employee = _employeeService.GetOwnProfile(CurrentUserId);
        var schedules = _scheduleService.GetDepartmentOpenSchedules(CurrentUserId);
        
        ViewBag.EmployeeInfo = employee;

        var history = await _otAttendanceService.GetHistory(CurrentUserId, historyPage, historyPageSize, historyMonth, historyYear);

        var vm = new EmployeeOvertimeIndexViewModel
        {
            AvailableSchedules = schedules,
            History = history
        };

        return View("~/Views/Employee/OvertimeTab.cshtml", vm);
    }

    [HttpGet("overtime/ot-attendance")]
    public async Task<IActionResult> OTAttendance()
    {
        LoadSidebarUserCard();
        await LoadStatsAsync();

        var list = await _otAttendanceService.GetTodayOTs(CurrentUserId);
        return View("~/Views/Employee/OTAttendance.cshtml", list);
    }

    [HttpGet("overtime/ot-attendance/today")]
    public async Task<IActionResult> OTAttendanceToday(int id)
    {
        LoadSidebarUserCard();
        await LoadStatsAsync();

        var model = await _otAttendanceService.GetTodayOTAttendance(CurrentUserId, id);
        if (model == null)
        {
            TempData["Error"] = "Không tìm thấy OT hôm nay để chấm công.";
            return RedirectToAction(nameof(OTAttendance));
        }

        return View("~/Views/Employee/OTAttendanceToday.cshtml", model);
    }

    [HttpGet("overtime/ot-attendance/history")]
    public async Task<IActionResult> OTAttendanceHistory(int page = 1, int pageSize = 5, int? month = null, int? year = null)
    {
        LoadSidebarUserCard();
        await LoadStatsAsync();

        var model = await _otAttendanceService.GetHistory(CurrentUserId, page, pageSize, month, year);
        return View("~/Views/Employee/OTAttendanceHistory.cshtml", model);
    }

        // (removed) overtime/history-json: history is rendered server-side in Overtime view

    [HttpPost("overtime/ot-attendance/check-in")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> OTCheckIn(HumanResourcesManager.BLL.DTOs.Employee.OTCheckInDTO dto)
    {
        var result = await _otAttendanceService.CheckIn(CurrentUserId, dto);
        TempData[result.IsSuccess ? "Success" : "Error"] = result.Message;
        return RedirectToAction(nameof(OTAttendance));
    }

    [HttpPost("overtime/ot-attendance/check-out")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> OTCheckOut(HumanResourcesManager.BLL.DTOs.Employee.OTCheckOutDTO dto)
    {
        var result = await _otAttendanceService.CheckOut(CurrentUserId, dto);
        TempData[result.IsSuccess ? "Success" : "Error"] = result.Message;
        return RedirectToAction(nameof(OTAttendance));
    }

    [HttpPost("overtime/register/{id:int}")]
    [ValidateAntiForgeryToken]
    public IActionResult RegisterOT(int id)
    {
        var ok = _scheduleService.RegisterOT(CurrentUserId, id, out string message);
        TempData[ok ? "Success" : "Error"] = message;
        return RedirectToAction(nameof(Overtime));
    }

    [HttpPost("overtime/cancel/{id:int}")]
    [ValidateAntiForgeryToken]
    public IActionResult CancelOT(int id)
    {
        var ok = _scheduleService.CancelRegistration(CurrentUserId, id, out string message);
        TempData[ok ? "Success" : "Error"] = message;
        return RedirectToAction(nameof(Overtime));
    }

    [HttpGet("payroll")]
    public async Task<IActionResult> Payroll()
    {
        LoadSidebarUserCard();

        await LoadStatsAsync();
        var model = _payrollService.GetPayrolls(CurrentUserId, page: 1, pageSize: 5, month: null, year: null);
        return View("~/Views/Employee/PayrollTab.cshtml", model);
    }

    [HttpGet("payroll/detail")]
    public async Task<IActionResult> PayrollDetail(int id)
    {
        var model = _payrollService.GetPayrollDetail(id, CurrentUserId);
        if (model == null)
        {
            TempData["Error"] = "Không tìm thấy bảng lương";
            return RedirectToAction("Payroll");
        }

        LoadSidebarUserCard();
        await LoadStatsAsync();
        return View("~/Views/Employee/PayrollDetail.cshtml", model);
    }
}
