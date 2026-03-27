using HumanResourcesManager.BLL.DTOs;
using HumanResourcesManager.BLL.Interfaces;
using HumanResourcesManager.Models.Manager;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace HumanResourcesManager.Controllers.Manager
{
    [Authorize(Roles = "MANAGER")]
    [Route("HumanResourcesManager/manager/team-management")]
    public class ManagerTeamController : Controller
    {
        private readonly IEmployeeService _employeeService;

        public ManagerTeamController(IEmployeeService employeeService)
        {
            _employeeService = employeeService;
        }

        private int CurrentUserId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        [HttpGet("")]
        public IActionResult Index(string? keyword, int? status, int page = 1, int pageSize = 10)
        {
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

            return View("~/Views/Manager/TeamManagement/Index.cshtml", model);
        }

        [HttpGet("detail/{id:int}")]
        public IActionResult Detail(int id, string? keyword, int? status, int page = 1)
        {
            var member = _employeeService.GetTeamMembers(CurrentUserId).FirstOrDefault(x => x.EmployeeId == id);
            if (member == null)
            {
                TempData["Error"] = "Không tìm thấy nhân sự thuộc quyền quản lý.";
                return RedirectToAction(nameof(Index), new { keyword, status, page });
            }

            ViewBag.Keyword = keyword;
            ViewBag.Status = status;
            ViewBag.Page = page;
            ViewData["Title"] = "Chi tiết nhân viên";
            ViewData["PageTitle"] = "Chi tiết nhân viên";
            ViewData["PageSubtitle"] = "Thông tin chi tiết nhân sự thuộc nhóm của bạn";

            return View("~/Views/Manager/TeamManagement/Detail.cshtml", member);
        }

        [HttpPost("delete/{id:int}")]
        [ValidateAntiForgeryToken]
        public IActionResult Delete(int id, string? keyword, int? status, int page = 1)
        {
            var member = _employeeService.GetTeamMembers(CurrentUserId).FirstOrDefault(x => x.EmployeeId == id);
            if (member == null)
            {
                TempData["Error"] = "Không thể xóa: nhân sự không thuộc quyền quản lý.";
                return RedirectToAction(nameof(Index), new { keyword, status, page });
            }

            _employeeService.Delete(id);
            TempData["Success"] = $"Đã xóa nhân sự {member.FullName} thành công.";
            return RedirectToAction(nameof(Index), new { keyword, status, page });
        }
    }
}
