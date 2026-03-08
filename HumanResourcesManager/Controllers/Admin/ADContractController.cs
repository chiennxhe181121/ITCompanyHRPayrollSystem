using HumanResourcesManager.BLL.DTOs;
using HumanResourcesManager.BLL.Interfaces;
using HumanResourcesManager.BLL.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace HumanResourcesManager.Controllers.Admin
{
    [Authorize(Roles = "ADMIN")]
    [Route("HumanResourcesManager/admin/contracts")] 
    public class ADContractController : Controller
    {
        private readonly IContractService _contractService;
        private readonly IADEmployeeService _employeeService; 

        public ADContractController(IContractService contractService, IADEmployeeService employeeService)
        {
            _contractService = contractService;
            _employeeService = employeeService;
        }

        // ==========================================
        // 1. INDEX (DÙNG CHUNG CHO CẢ TỔNG & CHI TIẾT NHÂN VIÊN)
        // ==========================================
        [HttpGet("")]
        public IActionResult Index(
            int? employeeId, 
            bool isActive = true,
            int page = 1,
            string? search = null,
            string? contractType = null)
        {
            var query = _contractService.GetAll().Where(x => x.IsActive == isActive).AsQueryable();

            if (employeeId.HasValue && employeeId.Value > 0)
            {
                query = query.Where(c => c.EmployeeId == employeeId.Value);
                ViewBag.EmployeeId = employeeId; 

                var empName = query.FirstOrDefault()?.EmployeeName;
                if (!string.IsNullOrEmpty(empName)) ViewBag.EmployeeName = empName;
            }

            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.Trim().ToLower();
                query = query.Where(x => x.EmployeeName != null && x.EmployeeName.ToLower().Contains(search));
            }

            if (!string.IsNullOrEmpty(contractType))
            {
                query = query.Where(x => x.ContractType == contractType);
            }

            query = query.OrderByDescending(x => x.StartDate);

            int pageSize = 10;
            var totalRecords = query.Count();
            var totalPages = (int)Math.Ceiling((double)totalRecords / pageSize);

            if (page > totalPages && totalPages > 0) page = totalPages;
            if (page < 1) page = 1;

            var data = query.Skip((page - 1) * pageSize).Take(pageSize).ToList();

            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;
            ViewBag.TotalRecords = totalRecords;
            ViewBag.IsActive = isActive;
            ViewBag.Search = search;
            ViewBag.ContractType = contractType;

            return View("~/Views/Admin/Contract/Index.cshtml", data);
        }

        // ==========================================
        // 2. CREATE (Hỗ trợ nhận employeeId từ URL)
        // ==========================================
        [HttpGet("create")]
        public IActionResult Create(int? employeeId)
        {
            var model = new ContractDTO
            {
                StartDate = DateTime.Now,
                IsActive = true,
                EmployeeId = employeeId ?? 0 
            };

            LoadEmployeeDropdown(model);
            return View("~/Views/Admin/Contract/Create.cshtml", model);
        }

        [HttpPost("create")]
        [ValidateAntiForgeryToken]
        public IActionResult Create(ContractDTO dto)
        {
            if (dto.EndDate.HasValue && dto.EndDate <= dto.StartDate)
            {
                ModelState.AddModelError("EndDate", "Ngày kết thúc phải lớn hơn ngày bắt đầu");
            }

            if (!ModelState.IsValid)
            {
                LoadEmployeeDropdown(dto);
                return View("~/Views/Admin/Contract/Create.cshtml", dto);
            }

            _contractService.Create(dto);
            TempData["Success"] = "Tạo hợp đồng thành công!";
            return RedirectToAction(nameof(Index));
        }

        // ==========================================
        // 3. EDIT
        // ==========================================
        [HttpGet("edit/{id}")]
        public IActionResult Edit(int id, int page = 1, string? keyword = null, int? status = null)
        {
            var dto = _contractService.GetById(id);
            if (dto == null) return NotFound();

            LoadEmployeeDropdown(dto);
            ViewBag.Page = page;
            ViewBag.Keyword = keyword;
            ViewBag.Status = status;

            return View("~/Views/Admin/Contract/Edit.cshtml", dto);
        }

        [HttpPost("edit/{id}")]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(int id, ContractDTO dto, int page = 1, string? keyword = null, int? status = null)
        {
            dto.ContractId = id;

            if (dto.EndDate.HasValue && dto.EndDate <= dto.StartDate)
            {
                ModelState.AddModelError("EndDate", "Ngày kết thúc phải lớn hơn ngày bắt đầu");
            }

            if (!ModelState.IsValid)
            {
                LoadEmployeeDropdown(dto);
                return View("~/Views/Admin/Contract/Edit.cshtml", dto);
            }

            _contractService.Update(dto);
            TempData["Success"] = "Cập nhật hợp đồng thành công!";
            return RedirectToAction(nameof(Index), new { page, keyword, status });
        }

        [HttpPost("toggle-status/{id}")]
        [ValidateAntiForgeryToken]
        public IActionResult ToggleStatus(int id, int page = 1, string? search = null, string? contractType = null, bool isActive = true, int? employeeId = null)
        {
            var contract = _contractService.GetById(id);
            if (contract != null)
            {
                contract.IsActive = !contract.IsActive;
                _contractService.Update(contract);
                TempData["Success"] = contract.IsActive ? "Đã kích hoạt hợp đồng." : "Đã khóa hợp đồng.";
            }

            return RedirectToAction(nameof(Index), new { page, search, contractType, isActive, employeeId });
        }

        private void LoadEmployeeDropdown(ContractDTO dto)
        {
            var employees = _employeeService.GetAllEmployees();
            dto.Employees = employees.Select(e => new SelectListItem
            {
                Value = e.EmployeeId.ToString(),
                Text = $"{e.EmployeeCode} - {e.FullName}"
            }).ToList();
        }
    }
}