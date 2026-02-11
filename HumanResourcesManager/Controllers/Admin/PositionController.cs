using HumanResourcesManager.BLL.DTOs.Position;
using HumanResourcesManager.BLL.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

// HoangDH
namespace HumanResourcesManager.Controllers.Admin
{
    [Authorize(Roles = "ADMIN")]
    [Route("HumanResourcesManager/admin/positions")]
    public class PositionController : Controller
    {
        private readonly IPositionService _service;

        public PositionController(IPositionService service)
        {
            _service = service;
        }

        [HttpGet("")]
        public IActionResult Index(string? keyword, int? status, int page = 1)
        {
            if (page < 1) page = 1;

            var result = _service.GetAll();
            // Lọc theo từ khóa
            if (!string.IsNullOrEmpty(keyword))
                result = result.Where(p => p.PositionName.Contains(keyword, StringComparison.OrdinalIgnoreCase));

            if (status.HasValue)
                result = result.Where(p => p.Status == status.Value);

            int pageSize = 10; 
            int totalItems = result.Count();

            int totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);
            if (page > totalPages && totalPages > 0) page = totalPages;
            // Phân trang
            var pagedData = result.Skip((page - 1) * pageSize).Take(pageSize).ToList();

            ViewBag.Keyword = keyword;
            ViewBag.Status = status;
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;

            return View("~/Views/Admin/Position/Index.cshtml", pagedData);
        }

        [HttpGet("create")]
        public IActionResult Create()
        {
            return View("~/Views/Admin/Position/Create.cshtml");
        }

        [HttpPost("create")]
        [ValidateAntiForgeryToken]
        public IActionResult Create(PositionCreateUpdateDTO dto)
        {
            if (!ModelState.IsValid)
                return View("~/Views/Admin/Position/Create.cshtml", dto);

            if (!_service.Create(dto))
            {
                ModelState.AddModelError("PositionName", "Tên chức vụ đã tồn tại.");
                return View("~/Views/Admin/Position/Create.cshtml", dto);
            }

            int totalItems = _service.Count();
            int pageSize = 10;
            int lastPage = (int)Math.Ceiling(totalItems / (double)pageSize);

            TempData["Success"] = "Tạo chức vụ thành công.";
            return RedirectToAction(nameof(Index), new { page = lastPage });
        }

        [HttpGet("edit/{id}")]
        public IActionResult Edit(int id, int page = 1, string? keyword = null, int? status = null)
        {
            var dto = _service.GetById(id);
            if (dto == null) return NotFound();

            ViewBag.Page = page;
            ViewBag.Keyword = keyword;
            ViewBag.Status = status;

            return View("~/Views/Admin/Position/Edit.cshtml", dto);
        }

        [HttpPost("edit/{id}")]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(
            int id,
            PositionCreateUpdateDTO dto,
            int page,
            string? keyword,
            [FromQuery(Name = "status")] int? filterStatus)
        {
            dto.PositionId = id;

            if (!ModelState.IsValid)
            {
                ViewBag.Page = page;
                ViewBag.Keyword = keyword;
                ViewBag.Status = filterStatus;
                return View("~/Views/Admin/Position/Edit.cshtml", dto);
            }

            if (!_service.Update(dto))
            {
                ModelState.AddModelError("PositionName", "Có lỗi xảy ra hoặc tên chức vụ đã tồn tại.");
                ViewBag.Page = page;
                ViewBag.Keyword = keyword;
                ViewBag.Status = filterStatus;
                return View("~/Views/Admin/Position/Edit.cshtml", dto);
            }

            TempData["Success"] = "Cập nhật chức vụ thành công.";

            return RedirectToAction(nameof(Index), new { page, keyword, status = filterStatus });
        }

        [HttpPost("inactive/{id}")]
        [ValidateAntiForgeryToken]
        public IActionResult Inactive(
            int id,
            int page,
            string? keyword,
            [FromQuery(Name = "status")] int? filterStatus)
        {
            var success = _service.SoftDelete(id);

            if (!success)
                TempData["Error"] = "Không thể vô hiệu hóa chức vụ này vì vẫn còn nhân viên đang nắm giữ.";
            else
                TempData["Success"] = "Đã tạm dừng chức vụ thành công.";

            return RedirectToAction(nameof(Index), new { page, keyword, status = filterStatus });
        }

        [HttpPost("active/{id}")]
        [ValidateAntiForgeryToken]
        public IActionResult Active(
            int id,
            int page,
            string? keyword,
            [FromQuery(Name = "status")] int? filterStatus)
        {
            _service.SetActive(id);
            TempData["Success"] = "Đã kích hoạt lại chức vụ.";
            return RedirectToAction(nameof(Index), new { page, keyword, status = filterStatus });
        }
    }
}