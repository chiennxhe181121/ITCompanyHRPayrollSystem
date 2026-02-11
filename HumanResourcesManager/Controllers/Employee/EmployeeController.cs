using HumanResourcesManager.BLL.DTOs;
using HumanResourcesManager.BLL.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Volo.Abp;

[Authorize(Policy = "EMP")]
[Route("HumanResourcesManager/employee")]
public class EmployeeController : Controller
{
    private readonly IEmployeeService _employeeService;

    public EmployeeController(IEmployeeService employeeService)
    {
        _employeeService = employeeService;
    }

    private int CurrentUserId =>
        int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    // ===== Attendance =====
    // view attendance
    [HttpGet("attendance")]
    public IActionResult Index()
    {
        var employee = _employeeService.GetOwnProfile(CurrentUserId);
        return View(employee);
    }

    // ===== Profile =====
    // view profile
    [HttpGet("profile")]
    public IActionResult Profile()
    {
        var employee = _employeeService.GetOwnProfile(CurrentUserId);
        return View("~/Views/Employee/ProfileTab.cshtml", employee);
    }

    // update profile
    [HttpPost("update-profile")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateProfile(
        EmployeeOwnerProfileDTO model,
        IFormFile? avatarFile
    )
    {
        // ===== ANNOTATION VALIDATION =====
        ModelState.Remove(nameof(EmployeeOwnerProfileDTO.EmployeeCode));
        ModelState.Remove(nameof(EmployeeOwnerProfileDTO.DepartmentName));
        ModelState.Remove(nameof(EmployeeOwnerProfileDTO.PositionName));
        ModelState.Remove(nameof(EmployeeOwnerProfileDTO.HireDate));
        if (!ModelState.IsValid)
        {
            return View("~/Views/Employee/ProfileTab.cshtml", model);
            // hoặc đúng path view mày đang dùng
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
            ModelState.AddModelError(
                string.Empty,
                ex.Details ?? ex.Message
            );
            return View("ProfileTab", model);
        }
    }

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
