//using HumanResourcesManager.BLL.DTOs;
//using HumanResourcesManager.BLL.Interfaces;
//using Microsoft.AspNetCore.Authorization;
//using Microsoft.AspNetCore.Mvc;

//namespace HumanResourcesManager.Controllers
//{
//    [Authorize(Policy = "HR")]
//    [Route("HumanResourcesManager/HR/Employee")]
//    public class HREmployeeController : Controller
//    {
//        private readonly IEmployeeService _employeeService;

//        public HREmployeeController(IEmployeeService employeeService)
//        {
//            _employeeService = employeeService;
//        }

//        // GET: /HR/Employee
//        [HttpGet("")]
//        public IActionResult Index()
//        {
//            var employees = _employeeService.GetAll();
//            return View("~/Views/HR/Employee/Index.cshtml", employees);
//        }

//        // POST: /HR/Employee/Edit
//        [HttpPost("Edit")]
//        [ValidateAntiForgeryToken]
//        public IActionResult Edit(EmployeeDTO dto)
//        {
//            if (!ModelState.IsValid)
//            {
//                // modal -> quay về index
//                return RedirectToAction(nameof(Index));
//            }

//            _employeeService.Update(dto);
//            return RedirectToAction(nameof(Index));
//        }

//        // (OPTIONAL) Create page
//        [HttpGet("Create")]
//        public IActionResult Create()
//        {
//            return View("~/Views/HR/Employee/Create.cshtml");
//        }

//        [HttpPost("Create")]
//        [ValidateAntiForgeryToken]
//        public IActionResult Create(EmployeeDTO dto)
//        {
//            if (!ModelState.IsValid)
//                return View("~/Views/HR/Employee/Create.cshtml", dto);

//            _employeeService.Create(dto);
//            return RedirectToAction(nameof(Index));
//        }
//    }
//}


using HumanResourcesManager.BLL.DTOs.ADEmployee;
using HumanResourcesManager.BLL.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace HumanResourcesManager.Controllers.HR
{
    [Authorize(Roles = "HR")]
    [Route("HumanResourcesManager/hr/employees")]
    public class HREmployeeController : Controller
    {
        private readonly IADEmployeeService _service;

        public HREmployeeController(IADEmployeeService service)
        {
            _service = service;
        }

        [HttpGet("")]
        public IActionResult Index(
            string? keyword,
            int? deptId,
            int? posId,
            int? status,
            int page = 1)
        {
            var query = _service.GetAllEmployees().AsQueryable();

            if (!string.IsNullOrEmpty(keyword))
            {
                keyword = keyword.ToLower().Trim();
                query = query.Where(e =>
                    e.FullName.ToLower().Contains(keyword) ||
                    e.EmployeeCode.ToLower().Contains(keyword) ||
                    e.Email.ToLower().Contains(keyword));
            }

            if (deptId.HasValue)
                query = query.Where(e => e.DepartmentId == deptId.Value);

            if (posId.HasValue)
                query = query.Where(e => e.PositionId == posId.Value);

            if (status.HasValue)
                query = query.Where(e => e.Status == status.Value);

            // Pagination Logic
            int pageSize = 10;
            int totalItems = query.Count();
            int totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

            if (page < 1) page = 1;
            if (page > totalPages && totalPages > 0) page = totalPages;

            var pagedData = query.Skip((page - 1) * pageSize).Take(pageSize).ToList();

            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;
            ViewBag.Keyword = keyword;
            ViewBag.DeptId = deptId;
            ViewBag.PosId = posId;
            ViewBag.Status = status;

            // Load dropdown cho thanh Filter
            LoadViewBags();

            return View("~/Views/HR/Employee/Index.cshtml", pagedData);
        }

        [HttpGet("create")]
        public IActionResult Create()
        {
            LoadViewBags();
            return View("~/Views/HR/Employee/Create.cshtml", new ADEmployeeCreateDTO());
        }

        [HttpPost("create")]
        [ValidateAntiForgeryToken]
        public IActionResult Create(ADEmployeeCreateDTO dto)
        {
            if (!ModelState.IsValid)
            {
                LoadViewBags();
                return View("~/Views/HR/Employee/Create.cshtml", dto);
            }

            string message;
            bool isSuccess = _service.CreateEmployee(dto, out message);

            if (isSuccess)
            {
                TempData["Success"] = "Thêm nhân viên thành công!";
                if (dto.IsCreateAccount)
                {
                    string passInfo = string.IsNullOrEmpty(dto.Password) ? "12345678" : dto.Password;
                    TempData["Info"] = $"Tài khoản đã tạo. Pass: {passInfo}";
                }
                return RedirectToAction(nameof(Index), new { page = 1 });
            }
            else
            {
                HandleServiceError(message);
                LoadViewBags();
                return View("~/Views/HR/Employee/Create.cshtml", dto);
            }
        }

        // --- GET: Trang Edit ---
        [HttpGet("edit/{id}")]
        public IActionResult Edit(
            int id,
            int page = 1,
            string? keyword = null,
            int? deptId = null,
            int? posId = null,
            int? status = null)
        {
            var dto = _service.GetById(id);
            if (dto == null) return NotFound();

            ViewBag.Page = page;
            ViewBag.Keyword = keyword;
            ViewBag.DeptId = deptId;
            ViewBag.PosId = posId;
            ViewBag.Status = status;

            LoadViewBags();
            return View("~/Views/HR/Employee/Edit.cshtml", dto);
        }

        [HttpPost("edit/{id}")]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(
            int id,
            ADEmployeeUpdateDTO dto,
            [FromQuery] int page = 1,
            [FromQuery] string? keyword = null,
            [FromQuery] int? deptId = null,
            [FromQuery] int? posId = null,
            [FromQuery] int? status = null)
        {
            dto.EmployeeId = id;

            if (!ModelState.IsValid)
            {
                LoadViewBags();
                return View("~/Views/HR/Employee/Edit.cshtml", dto);
            }

            string message;
            bool isSuccess = _service.UpdateEmployee(dto, out message);

            if (isSuccess)
            {
                TempData["Success"] = "Cập nhật hồ sơ thành công!";
                return RedirectToAction(nameof(Index), new { page, keyword, deptId, posId, status });
            }
            else
            {
                HandleServiceError(message);
                LoadViewBags();
                return View("~/Views/HR/Employee/Edit.cshtml", dto);
            }
        }

        [HttpPost("inactive/{id}")]
        [ValidateAntiForgeryToken]
        public IActionResult Inactive(
            int id,
            int page, string? keyword, int? deptId, int? posId, int? status)
        {
            try
            {
                var userIdString = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                int? currentUserId = null;
                if (!string.IsNullOrEmpty(userIdString))
                {
                    currentUserId = int.Parse(userIdString);
                }

                _service.SetStatus(id, 0, currentUserId);

                TempData["Success"] = "Đã vô hiệu hóa nhân viên.";
            }
            catch (InvalidOperationException ex) // Bắt lỗi logic (tự xóa mình)
            {
                TempData["Error"] = ex.Message;
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Lỗi: " + ex.Message;
            }

            return RedirectToAction(nameof(Index), new { page, keyword, deptId, posId, status });
        }

        [HttpPost("active/{id}")]
        [ValidateAntiForgeryToken]
        public IActionResult Active(
            int id,
            int page, string? keyword, int? deptId, int? posId, int? status)
        {
            try
            {
                _service.SetStatus(id, 1, null);
                TempData["Success"] = "Đã kích hoạt lại nhân viên.";
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Lỗi: " + ex.Message;
            }

            return RedirectToAction(nameof(Index), new { page, keyword, deptId, posId, status });
        }

        private void LoadViewBags()
        {
            var depts = _service.GetDepartments();
            ViewBag.Departments = new SelectList(depts, "DepartmentId", "DepartmentName");

            var positions = _service.GetPositions()
                .Select(p => new {
                    Id = p.PositionId,
                    DisplayText = $"{p.PositionName} ({p.BaseSalary:N0} đ)"
                });
            ViewBag.Positions = new SelectList(positions, "Id", "DisplayText");
        }

        private void HandleServiceError(string message)
        {
            if (message == "DUPLICATE_EMAIL")
                ModelState.AddModelError("Email", "Email này đã được sử dụng trong hệ thống.");
            else if (message == "DUPLICATE_PHONE")
                ModelState.AddModelError("Phone", "Số điện thoại này đã được sử dụng.");
            else
                TempData["Error"] = message.Replace("SYSTEM_ERROR: ", "");
        }
    }
}