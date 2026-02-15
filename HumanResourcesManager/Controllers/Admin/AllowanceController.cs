using HumanResourcesManager.BLL.DTOs.Allowance;
using HumanResourcesManager.BLL.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;


// HoangDH
namespace HumanResourcesManager.Controllers.Admin
{
    [Authorize(Roles = "ADMIN")]
    [Route("HumanResourcesManager/admin/allowances")]
    public class AllowanceController : Controller
    {
        private readonly IAllowanceService _service;

        public AllowanceController(IAllowanceService service)
        {
            _service = service;
        }


        [HttpGet("")]
        public IActionResult Index(string? keyword, int? status, int page = 1)
        {
            var query = _service.GetAll().AsQueryable();

            if (!string.IsNullOrEmpty(keyword))
            {
                keyword = keyword.ToLower().Trim();
                query = query.Where(a => a.AllowanceName.ToLower().Contains(keyword));
            }

            if (status.HasValue)
            {
                query = query.Where(a => a.Status == status.Value);
            }

            int pageSize = 10; 
            int totalItems = query.Count();
            int totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

            if (page < 1) page = 1;
            if (page > totalPages && totalPages > 0) page = totalPages;

            var pagedData = query.Skip((page - 1) * pageSize).Take(pageSize).ToList();

            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;
            ViewBag.Keyword = keyword;
            ViewBag.Status = status;

            return View("~/Views/Admin/Allowance/Index.cshtml", pagedData);
        }


        [HttpGet("create")]
        public IActionResult Create()
        {
            return View("~/Views/Admin/Allowance/Create.cshtml", new AllowanceDTO());
        }

        [HttpPost("create")]
        [ValidateAntiForgeryToken]
        public IActionResult Create(AllowanceDTO dto)
        {
            if (!ModelState.IsValid)
            {
                return View("~/Views/Admin/Allowance/Create.cshtml", dto);
            }

            if (_service.Create(dto))
            {
                TempData["Success"] = "Thêm phụ cấp thành công!";

                return RedirectToAction(nameof(Index), new { page = 1 });
            }

            TempData["Error"] = "Lỗi hệ thống khi thêm phụ cấp.";
            return View("~/Views/Admin/Allowance/Create.cshtml", dto);
        }


        [HttpGet("edit/{id}")]
        public IActionResult Edit(
            int id,
            int page = 1,
            string? keyword = null,
            int? status = null)
        {
            var dto = _service.GetById(id);
            if (dto == null) return NotFound();

            ViewBag.Page = page;
            ViewBag.Keyword = keyword;
            ViewBag.Status = status;

            return View("~/Views/Admin/Allowance/Edit.cshtml", dto);
        }

        [HttpPost("edit/{id}")]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(
            int id, 
            AllowanceDTO dto,
            [FromQuery] int page = 1,
            [FromQuery] string? keyword = null,
            [FromQuery] int? status = null)
        {
            dto.AllowanceId = id;
            if (!ModelState.IsValid)
            {
                return View("~/Views/Admin/Allowance/Edit.cshtml", dto);
            }

            if (_service.Update(dto))
            {
                TempData["Success"] = "Cập nhật thành công!";
                // Redirect kèm params để giữ trang
                return RedirectToAction(nameof(Index), new { page, keyword, status });
            }

            TempData["Error"] = "Lỗi hệ thống.";
            return View("~/Views/Admin/Allowance/Edit.cshtml", dto);
        }


        [HttpPost("toggle-status/{id}")]
        [ValidateAntiForgeryToken]
        public IActionResult ToggleStatus(
            int id,
            int page = 1,
            string? keyword = null,
            int? status = null)
        {
            if (_service.ToggleStatus(id))
            {
                TempData["Success"] = "Đã thay đổi trạng thái.";
            }
            else
            {
                TempData["Error"] = "Không tìm thấy phụ cấp.";
            }
            
            return RedirectToAction(nameof(Index), new { page, keyword, status });
        }
    }
}