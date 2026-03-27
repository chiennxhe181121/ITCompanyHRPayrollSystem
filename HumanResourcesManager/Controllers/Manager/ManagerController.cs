using HumanResourcesManager.BLL.DTOs;
using HumanResourcesManager.BLL.DTOs.Employee;
using HumanResourcesManager.BLL.DTOs.Manager;
using HumanResourcesManager.BLL.Interfaces;
using HumanResourcesManager.Helpers;
using HumanResourcesManager.Models.Manager;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using HumanResourcesManager.DAL.Data;
using System;
using System.Linq;
using System.Security.Claims;
using Volo.Abp;

namespace HumanResourcesManager.Controllers.Manager
{
    [Authorize(Roles = "MANAGER")]
    [Route("HumanResourcesManager/manager")]
    public class ManagerController : Controller
    {
        private readonly IEmployeeService _employeeService;
        private readonly IUserAccountService _userAccountService;
        private readonly IOTScheduleService _scheduleService;
        private readonly EmailService _emailService;
        private readonly HumanManagerContext _context;

        public ManagerController(
            IEmployeeService employeeService, 
            IUserAccountService userAccountService,
            IOTScheduleService scheduleService,
            EmailService emailService,
            HumanManagerContext context)
        {
            _employeeService = employeeService;
            _userAccountService = userAccountService;
            _scheduleService = scheduleService;
            _emailService = emailService;
            _context = context;
        }

        private int CurrentUserId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        private void LoadSidebarUserCard()
        {
            var profile = _employeeService.GetOwnProfile(CurrentUserId);
            if (profile == null)
            {
                return;
            }

            ViewBag.SidebarUserName = profile.FullName;
            ViewBag.SidebarUserPosition = profile.PositionName ?? "Quản lý";
            ViewBag.SidebarUserAvatar = profile.ImgAvatar;
            ViewBag.HasAvatar = !string.IsNullOrEmpty(profile.ImgAvatar);
            ViewBag.SidebarUserAvatarInitial = string.IsNullOrWhiteSpace(profile.FullName)
                ? "M"
                : profile.FullName[..1].ToUpper();
        }

        private void LoadManagerStats()
        {
            ViewBag.TeamSize = _employeeService.CountManagedEmployees(CurrentUserId);
            ViewBag.CompletedOT = _employeeService.CountCompletedOTSchedules(CurrentUserId);
            ViewBag.ScheduleCount = _employeeService.CountOvertimeSchedules(CurrentUserId);
        }

        [HttpGet("")]
        public IActionResult Index()
        {
            return RedirectToAction(nameof(Team));
        }

        [HttpGet("profile")]
        public IActionResult Profile()
        {
            LoadSidebarUserCard();
            var profile = _employeeService.GetOwnProfile(CurrentUserId);
            return View("~/Views/Manager/Profile.cshtml", profile);
        }

        [HttpPost("update-profile")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateProfile(EmployeeOwnerProfileDTO model, IFormFile? avatarFile)
        {
            ModelState.Remove(nameof(EmployeeOwnerProfileDTO.EmployeeCode));
            ModelState.Remove(nameof(EmployeeOwnerProfileDTO.DepartmentName));
            ModelState.Remove(nameof(EmployeeOwnerProfileDTO.PositionName));
            ModelState.Remove(nameof(EmployeeOwnerProfileDTO.HireDate));
            ModelState.Remove(nameof(EmployeeOwnerProfileDTO.ImgAvatar));

            if (!ModelState.IsValid)
            {
                LoadSidebarUserCard();
                var currentProfile = _employeeService.GetOwnProfile(CurrentUserId);
                return View("~/Views/Manager/Profile.cshtml", currentProfile);
            }

            try
            {
                await _employeeService.UpdateOwnProfile(CurrentUserId, model, avatarFile);
                UpdateSession();
                TempData["Success"] = "Cập nhật hồ sơ thành công";
                return RedirectToAction(nameof(Profile));
            }
            catch (BusinessException ex)
            {
                LoadSidebarUserCard();
                ModelState.AddModelError(string.Empty, ex.Details ?? ex.Message);
                var currentProfile = _employeeService.GetOwnProfile(CurrentUserId);
                return View("~/Views/Manager/Profile.cshtml", currentProfile);
            }
        }

        [HttpGet("profile/change-password")]
        public IActionResult ChangePassword()
        {
            LoadSidebarUserCard();
            return View("~/Views/Manager/ChangePassword.cshtml", new ChangePasswordDTO());
        }

        [HttpPost("profile/change-password")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(ChangePasswordDTO dto)
        {
            if (!ModelState.IsValid)
            {
                LoadSidebarUserCard();
                return View("~/Views/Manager/ChangePassword.cshtml", dto);
            }

            var result = _userAccountService.ChangePassword(CurrentUserId, dto);
            if (!result.IsSuccess)
            {
                LoadSidebarUserCard();
                ModelState.AddModelError(string.Empty, result.Message);
                return View("~/Views/Manager/ChangePassword.cshtml", dto);
            }

            HttpContext.Session.Clear();
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            TempData["Success"] = "Đổi mật khẩu thành công. Vui lòng đăng nhập lại.";
            return RedirectToAction("Login", "Auth");
        }

        [HttpGet("team")]
        public IActionResult Team(string? keyword, int? status, int page = 1, int pageSize = 10)
        {
            LoadSidebarUserCard();
            LoadManagerStats();

            if (page < 1) page = 1;
            if (pageSize < 1) pageSize = 10;

            var query = _employeeService.GetTeamMembers(CurrentUserId).AsQueryable();

            if (!string.IsNullOrWhiteSpace(keyword))
            {
                var term = keyword.Trim().ToLower();
                query = query.Where(x =>
                    (!string.IsNullOrEmpty(x.EmployeeCode) && x.EmployeeCode.ToLower().Contains(term)) ||
                    (!string.IsNullOrEmpty(x.FullName) && x.FullName.ToLower().Contains(term)) ||
                    (!string.IsNullOrEmpty(x.Email) && x.Email.ToLower().Contains(term)) ||
                    (!string.IsNullOrEmpty(x.Phone) && x.Phone.ToLower().Contains(term)) ||
                    (!string.IsNullOrEmpty(x.PositionName) && x.PositionName.ToLower().Contains(term))
                );
            }

            if (status.HasValue)
            {
                query = query.Where(x => x.Status == status.Value);
            }

            var totalItems = query.Count();
            var totalPages = Math.Max(1, (int)Math.Ceiling(totalItems / (double)pageSize));
            if (page > totalPages) page = totalPages;

            var items = query
                .OrderBy(x => x.FullName)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            var model = new ManagerTeamIndexViewModel
            {
                Items = items,
                Keyword = keyword,
                Status = status,
                Page = page,
                PageSize = pageSize,
                TotalItems = totalItems,
                TotalPages = totalPages
            };

            ViewData["Title"] = "Nhân viên của tôi";
            ViewData["PageTitle"] = "Nhân viên của tôi";
            ViewData["PageSubtitle"] = "Quản lý nhân sự theo phòng ban của bạn";

            return View("TeamManagement/Index", model);
        }

        [HttpGet("team/detail/{id:int}")]
        public IActionResult TeamDetail(int id, string? keyword, int? status, int page = 1)
        {
            LoadSidebarUserCard();
            var member = _employeeService.GetTeamMembers(CurrentUserId).FirstOrDefault(x => x.EmployeeId == id);
            if (member == null)
            {
                TempData["Error"] = "Không tìm thấy nhân sự thuộc quyền quản lý.";
                return RedirectToAction(nameof(Team), new { keyword, status, page });
            }

            ViewBag.Keyword = keyword;
            ViewBag.Status = status;
            ViewBag.Page = page;
            ViewData["Title"] = "Chi tiết nhân viên";
            ViewData["PageTitle"] = "Chi tiết nhân viên";
            ViewData["PageSubtitle"] = "Thông tin chi tiết nhân sự thuộc nhóm của bạn";

            return View("TeamManagement/Detail", member);
        }

        [HttpPost("team/delete/{id:int}")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteTeamMember(int id, string? keyword, int? status, int page = 1)
        {
            var member = _employeeService.GetTeamMembers(CurrentUserId).FirstOrDefault(x => x.EmployeeId == id);
            if (member == null)
            {
                TempData["Error"] = "Không thể xóa: nhân sự không thuộc quyền quản lý.";
                return RedirectToAction(nameof(Team), new { keyword, status, page });
            }

            _employeeService.Delete(id);
            TempData["Success"] = $"Đã xóa nhân sự {member.FullName} thành công.";
            return RedirectToAction(nameof(Team), new { keyword, status, page });
        }

        [HttpPost("team/status/{id:int}")]
        [ValidateAntiForgeryToken]
        public IActionResult ChangeTeamMemberStatus(int id, int newStatus, string? keyword, int? status, int page = 1)
        {
            var ok = _employeeService.ChangeTeamMemberStatus(CurrentUserId, id, newStatus, out var message);
            TempData[ok ? "Success" : "Error"] = message;
            return RedirectToAction(nameof(Team), new { keyword, status, page });
        }



        [HttpGet("schedule")]
        public IActionResult Schedule(int page = 1, int pageSize = 10)
        {
            _scheduleService.CancelExpiredSchedules();
            LoadSidebarUserCard();
            LoadManagerStats();
            if (page < 1) page = 1;
            if (pageSize < 1) pageSize = 10;

            var query = _scheduleService.GetManagerSchedules(CurrentUserId)
                .OrderByDescending(x => x.CreatedAt)
                .AsQueryable();

            var totalItems = query.Count();
            var totalPages = Math.Max(1, (int)Math.Ceiling(totalItems / (double)pageSize));
            if (page > totalPages) page = totalPages;

            var items = query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            var model = new HumanResourcesManager.Models.Manager.ManagerScheduleIndexViewModel
            {
                Items = items,
                Page = page,
                PageSize = pageSize,
                TotalItems = totalItems,
                TotalPages = totalPages
            };

            return View("~/Views/Manager/Schedule.cshtml", model);
        }

        [HttpGet("schedule/create")]
        public IActionResult CreateSchedule()
        {
            LoadSidebarUserCard();
            LoadManagerStats();
            return View("~/Views/Manager/CreateSchedule.cshtml", new OTScheduleCreateDTO { StartDate = DateTime.Today, EndDate = DateTime.Today });
        }

        [HttpPost("schedule/create")]
        [ValidateAntiForgeryToken]
        public IActionResult CreateSchedule(OTScheduleCreateDTO model)
        {
            if (model.EndDate < model.StartDate)
            {
                ModelState.AddModelError(string.Empty,
                    $"Ngày kết thúc phải từ {model.StartDate:dd/MM/yyyy} trở đi.");
            }

            if (model.EndTime <= model.StartTime)
            {
                var start = DateTime.Today.Add(model.StartTime).ToString("h:mm tt", System.Globalization.CultureInfo.InvariantCulture);
                var end = DateTime.Today.Add(model.EndTime).ToString("h:mm tt", System.Globalization.CultureInfo.InvariantCulture);
                ModelState.AddModelError(string.Empty,
                    $"Giờ kết thúc phải sau giờ bắt đầu. Bạn đang chọn {start} → {end}.");
            }

            if (!ModelState.IsValid)
            {
                LoadSidebarUserCard();
                LoadManagerStats();
                return View("~/Views/Manager/CreateSchedule.cshtml", model);
            }

            var ok = _scheduleService.CreateOTSchedule(CurrentUserId, model, out var message);
            if (!ok)
            {
                ModelState.AddModelError(string.Empty, message);
                LoadSidebarUserCard();
                LoadManagerStats();
                return View("~/Views/Manager/CreateSchedule.cshtml", model);
            }

            TempData["Success"] = message;
            return RedirectToAction(nameof(Schedule));
        }

        [HttpGet("schedule/{id:int}/edit")]
        public IActionResult EditSchedule(int id)
        {
            LoadSidebarUserCard();
            LoadManagerStats();
            var dto = _scheduleService.GetScheduleForEdit(CurrentUserId, id);
            if (dto == null)
            {
                TempData["Error"] = "Không thể chỉnh sửa lịch OT này.";
                return RedirectToAction(nameof(Schedule));
            }
            ViewBag.ScheduleId = id;
            ViewData["Title"] = "Chỉnh Sửa Lịch Tăng Ca";
            ViewData["PageTitle"] = "Chỉnh Sửa Lịch Tăng Ca";
            ViewData["PageSubtitle"] = "Cập nhật thông tin lịch tăng ca";
            return View("~/Views/Manager/EditSchedule.cshtml", dto);
        }

        [HttpPost("schedule/{id:int}/edit")]
        [ValidateAntiForgeryToken]
        public IActionResult EditSchedule(int id, OTScheduleCreateDTO model)
        {
            if (model.EndDate < model.StartDate)
            {
                ModelState.AddModelError(string.Empty,
                    $"Ngày kết thúc phải từ {model.StartDate:dd/MM/yyyy} trở đi.");
            }

            if (model.EndTime <= model.StartTime)
            {
                var start = DateTime.Today.Add(model.StartTime).ToString("h:mm tt", System.Globalization.CultureInfo.InvariantCulture);
                var end = DateTime.Today.Add(model.EndTime).ToString("h:mm tt", System.Globalization.CultureInfo.InvariantCulture);
                ModelState.AddModelError(string.Empty,
                    $"Giờ kết thúc phải sau giờ bắt đầu. Bạn đang chọn {start} → {end}.");
            }

            if (!ModelState.IsValid)
            {
                LoadSidebarUserCard();
                LoadManagerStats();
                ViewBag.ScheduleId = id;
                ViewData["Title"] = "Chỉnh Sửa Lịch Tăng Ca";
                ViewData["PageTitle"] = "Chỉnh Sửa Lịch Tăng Ca";
                ViewData["PageSubtitle"] = "Cập nhật thông tin lịch tăng ca";
                return View("~/Views/Manager/EditSchedule.cshtml", model);
            }

            var ok = _scheduleService.UpdateOTSchedule(CurrentUserId, id, model, out var message, out var notifyList);
            if (!ok)
            {
                ModelState.AddModelError(string.Empty, message);
                LoadSidebarUserCard();
                LoadManagerStats();
                ViewBag.ScheduleId = id;
                ViewData["Title"] = "Chỉnh Sửa Lịch Tăng Ca";
                ViewData["PageTitle"] = "Chỉnh Sửa Lịch Tăng Ca";
                ViewData["PageSubtitle"] = "Cập nhật thông tin lịch tăng ca";
                return View("~/Views/Manager/EditSchedule.cshtml", model);
            }

            // Gửi mail thông báo cho nhân viên đang trong lịch OT (fire-and-forget, bỏ qua lỗi mail)
            foreach (var (email, name) in notifyList)
            {
                try
                {
                    _emailService.SendOTScheduleUpdated(
                        email, name,
                        model.Name,
                        model.StartDate, model.StartTime,
                        model.EndDate, model.EndTime,
                        model.Quantity,
                        model.Description);
                }
                catch { /* bỏ qua nếu gửi mail thất bại */ }
            }

            TempData["Success"] = message;
            return RedirectToAction(nameof(Schedule));
        }

        [HttpGet("schedule/{id:int}")]
        public IActionResult ScheduleDetails(int id)
        {
            LoadSidebarUserCard();
            LoadManagerStats();
            var detail = _scheduleService.GetOTScheduleDetails(CurrentUserId, id);
            if (detail == null)
            {
                TempData["Error"] = "Không tìm thấy thông tin Lịch OT hoặc bạn không có quyền xem.";
                return RedirectToAction(nameof(Schedule));
            }
            return View("~/Views/Manager/ScheduleDetails.cshtml", detail);
        }

        [HttpPost("schedule/cancel/{id:int}")]
        [ValidateAntiForgeryToken]
        public IActionResult CancelSchedule(int id)
        {
            // Lấy thông tin lịch trước khi hủy để gửi mail
            var detail = _scheduleService.GetOTScheduleDetails(CurrentUserId, id);

            var ok = _scheduleService.CancelOTSchedule(CurrentUserId, id, out var message, out var notifyList);
            TempData[ok ? "Success" : "Error"] = message;

            if (ok && detail != null && notifyList.Any())
            {
                var sent = 0;
                var failed = 0;
                foreach (var (email, name) in notifyList)
                {
                    try
                    {
                        _emailService.SendOTScheduleCancelled(
                            email, name,
                            detail.Name,
                            detail.StartDate, detail.StartTime,
                            detail.EndDate, detail.EndTime);
                        sent++;
                    }
                    catch
                    {
                        failed++;
                    }
                }

                TempData["Success"] = $"{message} (Email: {sent}/{notifyList.Count} gửi thành công{(failed > 0 ? $", {failed} thất bại" : "")}).";
            }
            else if (ok)
            {
                TempData["Success"] = $"{message} (Không có email để gửi thông báo.)";
            }

            return RedirectToAction(nameof(Schedule));
        }

        [HttpPost("schedule/reopen/{id:int}")]
        [ValidateAntiForgeryToken]
        public IActionResult ReopenSchedule(int id)
        {
            var ok = _scheduleService.ReopenOTSchedule(CurrentUserId, id, out var message);
            TempData[ok ? "Success" : "Error"] = message;
            return RedirectToAction(nameof(Schedule));
        }

        [HttpPost("schedule/complete/{id:int}")]
        [ValidateAntiForgeryToken]
        public IActionResult MarkCompleted(int id)
        {
            var ok = _scheduleService.MarkAsCompleted(CurrentUserId, id, out var message);
            TempData[ok ? "Success" : "Error"] = message;
            return RedirectToAction(nameof(Schedule));
        }

        [HttpPost("schedule/{id:int}/participants/remove/{employeeId:int}")]
        [ValidateAntiForgeryToken]
        public IActionResult RemoveParticipant(int id, int employeeId)
        {
            var detail = _scheduleService.GetOTScheduleDetails(CurrentUserId, id);
            if (detail == null)
            {
                TempData["Error"] = "Không tìm thấy thông tin Lịch OT hoặc bạn không có quyền thao tác.";
                return RedirectToAction(nameof(Schedule));
            }

            var ok = _scheduleService.RemoveEmployeeFromSchedule(CurrentUserId, id, employeeId, out var message, out var removedEmployee);
            TempData[ok ? "Success" : "Error"] = message;

            if (ok && removedEmployee.HasValue)
            {
                if (string.IsNullOrWhiteSpace(removedEmployee.Value.Email))
                {
                    TempData["Success"] = $"{message} (Nhân viên chưa có email, không thể gửi thông báo.)";
                }
                else try
                {
                    _emailService.SendOTRegistrationRemoved(
                        removedEmployee.Value.Email,
                        removedEmployee.Value.Name,
                        detail.Name,
                        detail.StartDate, detail.StartTime,
                        detail.EndDate, detail.EndTime);
                }
                catch
                {
                    // vẫn xóa thành công; chỉ thông báo gửi mail thất bại
                    TempData["Success"] = $"{message} (Gửi email thông báo thất bại.)";
                }
            }

            return RedirectToAction(nameof(ScheduleDetails), new { id });
        }

        private void UpdateSession()
        {
            var sessionUser = HttpContext.Session.GetObject<UserSessionDTO>("USER_SESSION");
            if (sessionUser == null)
            {
                return;
            }

            var freshProfile = _employeeService.GetOwnProfile(CurrentUserId);
            if (freshProfile == null)
            {
                return;
            }

            sessionUser.FullName = freshProfile.FullName;
            sessionUser.ImgAvatar = freshProfile.ImgAvatar;
            HttpContext.Session.SetObject("USER_SESSION", sessionUser);
        }

        [HttpPost("generate-test-data")]
        [ValidateAntiForgeryToken]
        public IActionResult GenerateTestData()
        {
            try
            {
                TestDataGenerator.GenerateExtraData(_context);
                TempData["Success"] = "Đã sinh thêm dữ liệu test (40 nhân viên, 15 lịch OT và hàng trăm đăng ký) thành công!";
            }
            catch (Exception ex)
            {
                var innerMsg = ex.InnerException != null ? " | Chi tiết: " + ex.InnerException.Message : "";
                TempData["Error"] = "Lỗi khi sinh dữ liệu: " + ex.Message + innerMsg;
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
