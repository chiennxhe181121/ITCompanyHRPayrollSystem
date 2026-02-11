using HumanResourcesManager.BLL.DTOs.Department;
using HumanResourcesManager.BLL.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

// HoangDH
namespace HumanResourcesManager.Controllers.Admin
{

    [Authorize(Roles = "ADMIN")]
    [Route("HumanResourcesManager/admin/departments")]
    public class DepartmentController : Controller
    {

        private readonly IDepartmentService _service;

        public DepartmentController(IDepartmentService service)
        {
            _service = service;
        }


        [HttpGet("")]
        public IActionResult Index(
            string? keyword,
            int? status,     
             int page = 1)
        {
            if (page < 1) page = 1;

            int pageSize = 10;

            var result = _service.Search(keyword, status, page, pageSize);

            ViewBag.Keyword = keyword;
            ViewBag.Status = status;
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = (int)Math.Ceiling(result.TotalItems / (double)pageSize);

            return View("~/Views/Admin/Department/Index.cshtml", result.Items);
        }


        [HttpGet("create")]
        public IActionResult Create()
        {
            return View("~/Views/Admin/Department/Create.cshtml",
                new DepartmentCreateUpdateDTO());
        }



        [HttpPost("create")]
        [ValidateAntiForgeryToken]
        public IActionResult Create(DepartmentCreateUpdateDTO dto)
        {
            if (!ModelState.IsValid)
                return View("~/Views/Admin/Department/Create.cshtml", dto);

            var success = _service.Create(dto);

            if (!success)
            {
                ModelState.AddModelError("DepartmentName", "Tên phòng ban đã tồn tại.");
                return View("~/Views/Admin/Department/Create.cshtml", dto);
            }

            int totalItems = _service.Count();
            int pageSize = 10;
            int lastPage = (int)Math.Ceiling(totalItems / (double)pageSize);

            TempData["Success"] = "Tạo phòng ban thành công.";
            return RedirectToAction(nameof(Index), new { page = lastPage });
        }


        [HttpGet("edit/{id}")]
        public IActionResult Edit(int id)
        {
            var dto = _service.GetById(id);
            if (dto == null)
                return NotFound();

            return View("~/Views/Admin/Department/Edit.cshtml", dto);
        }


        [HttpPost("edit/{id}")]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(
                              int id,
                              DepartmentCreateUpdateDTO dto,
                              int page,
                              string? keyword,
                              int? status)
        {
            if (!ModelState.IsValid)
            {
                dto.DepartmentId = id;
                return View("~/Views/Admin/Department/Edit.cshtml", dto);
            }

            if (!_service.Update(dto))
            {
                ModelState.AddModelError("DepartmentName", "Tên phòng ban đã tồn tại.");
                return View("~/Views/Admin/Department/Edit.cshtml", dto);
            }


            dto.DepartmentId = id;
            _service.Update(dto);

            TempData["Success"] = "Cập nhật phòng ban thành công.";

            return RedirectToAction(nameof(Index), new
            {
                page,
                keyword,
                status
            });
        }


        [HttpPost("inactive/{id}")]
        [ValidateAntiForgeryToken]
        public IActionResult Inactive(
            int id,
            int page,
            string? keyword,
            int? status)
        {
            var success = _service.Delete(id);

            if (!success)
            {
                TempData["Error"] = "Không thể vô hiệu hóa phòng ban vì vẫn còn nhân viên đang hoạt động.";
            }
            else
            {
                TempData["Success"] = "Phòng ban đã được vô hiệu hóa.";
            }

            return RedirectToAction(nameof(Index), new
            {
                page,
                keyword,
                status
            });
        }


        [HttpPost("active/{id}")]
        [ValidateAntiForgeryToken]
        public IActionResult Active(
            int id,
            int page,
            string? keyword,
            int? status)
        {
            _service.SetActive(id);

            TempData["Success"] = "Phòng ban đã được kích hoạt lại.";

            return RedirectToAction(nameof(Index), new
            {
                page,
                keyword,
                status
            });
        }




    }
}
