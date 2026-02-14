using HumanResourcesManager.BLL.DTOs.Employee;
using HumanResourcesManager.BLL.Interfaces;
using HumanResourcesManager.DAL.Enum;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Text.Json;
using Volo.Abp;

[Authorize(Policy = "EMP")]
[Route("HumanResourcesManager/employee")]
public class EmployeeController : Controller
{
    private readonly IEmployeeService _employeeService;
    private readonly IAttendanceService _attendanceService;
    private readonly IUserAccountService _userAccountService;

    public EmployeeController(
        IEmployeeService employeeService,
        IAttendanceService attendanceService,
        IUserAccountService userAccountService)
    {
        _employeeService = employeeService;
        _attendanceService = attendanceService;
        _userAccountService = userAccountService;
    }

    // Lấy userId từ session
    private int CurrentUserId =>
        int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    // ===== Attendance =====
    // view attendance
    [HttpGet("attendance")]
    public IActionResult Index()
    {
        return View();
    }

    // check-in
    [HttpPost("attendance/check-in")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CheckIn(CheckInDTO dto)
    {
        Console.WriteLine(DateTime.Now);
        Console.WriteLine(DateTime.UtcNow);

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
    [HttpGet("leaves")]
    public IActionResult Leaves()
    {
        var employee = _employeeService.GetOwnProfile(CurrentUserId);
        return View("~/Views/Employee/LeavesTab.cshtml", employee);
    }

    [HttpGet("overtime")]
    public IActionResult Overtime()
    {
        var employee = _employeeService.GetOwnProfile(CurrentUserId);
        return View("~/Views/Employee/OvertimeTab.cshtml", employee);
    }

    [HttpGet("payroll")]
    public IActionResult Payroll()
    {
        var employee = _employeeService.GetOwnProfile(CurrentUserId);
        return View("~/Views/Employee/PayrollTab.cshtml", employee);
    }
}
