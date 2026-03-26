using Microsoft.AspNetCore.Mvc;
using HumanResourcesManager.BLL.Services;
using HumanResourcesManager.DAL.Models;
using HumanResourcesManager.BLL.DTOs;
using HumanResourcesManager.BLL.Interfaces;

namespace HumanResourcesManager.Controllers.HR
{
    public class PayrollController : Controller
    {
        private readonly IPayrollService _service;

        public PayrollController(IPayrollService service)
        {
            _service = service;
        }

        // ================== LIST ==================
        public async Task<IActionResult> Index(string search = "", int page = 1, int pageSize = 10)
        {
            // 1. Lấy tất cả payroll
            var allPayrolls = await _service.GetAllAsync();

            // 2. Lọc theo search nếu có
            if (!string.IsNullOrEmpty(search))
            {
                allPayrolls = allPayrolls
                    .Where(p => p.EmployeeName.Contains(search, StringComparison.OrdinalIgnoreCase))
                    .ToList();
            }

            // 3. Tính tổng số bản ghi và tổng số trang
            int totalRecords = allPayrolls.Count;
            int totalPages = (int)Math.Ceiling((double)totalRecords / pageSize);

            // 4. Lấy dữ liệu cho trang hiện tại
            var pagedPayrolls = allPayrolls
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            // 5. Truyền thông tin phân trang và search về View
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;
            ViewBag.TotalRecords = totalRecords;
            ViewBag.Search = search;

            // 6. Trả view với danh sách đã lọc + phân trang
            return View("~/Views/HR/Payroll/Index.cshtml", pagedPayrolls);
        }

        // ================== CREATE ==================
        // ================== CREATE =================
        [HttpGet]
        public async Task<IActionResult> Create(int? employeeId, int? month, int? year)
        {
            int actualMonth = month ?? DateTime.Now.AddMonths(-1).Month;
            int actualYear = year ?? DateTime.Now.AddMonths(-1).Year;

            // khởi tạo model cơ bản
            PayrollDTO model = new PayrollDTO
            {
                Month = actualMonth,
                Year = actualYear
            };

            // load danh sách nhân viên chưa có payroll tháng này
            var employees = await _service.GetAllEmployeesWithoutPayrollAsync(actualMonth, actualYear);
            model.Employees = employees;

            if (employeeId.HasValue)
            {
                // nếu employee chưa có payroll tháng filter
                if (employees.Any(e => e.EmployeeId == employeeId.Value))
                {
                    model = await _service.GeneratePayrollForEmployeeAsync(employeeId.Value, actualMonth, actualYear);
                    model.Employees = employees; // giữ list nhân viên để dropdown
                }
                else
                {
                    // employee đã có payroll → reset chọn
                    model.EmployeeId = 0;
                }
            }

            return View("~/Views/HR/Payroll/Create.cshtml", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(PayrollDTO model)
        {
            // validate cơ bản
            if (model.EmployeeId == 0 || model.Month == 0 || model.Year == 0)
            {
                ModelState.AddModelError("", "Please select Employee, Month, and Year.");
            }

            if (!ModelState.IsValid)
            {
                // reload danh sách nhân viên chưa có payroll trong tháng
                int month = model.Month > 0 ? model.Month : DateTime.Now.AddMonths(-1).Month;
                int year = model.Year > 0 ? model.Year : DateTime.Now.AddMonths(-1).Year;

                model.Employees = await _service.GetAllEmployeesWithoutPayrollAsync(month, year);
                return View("~/Views/HR/Payroll/Create.cshtml", model);
            }

            // lưu vào DB
            await _service.CreateAsync(model.EmployeeId, model.Month, model.Year);

            return RedirectToAction("Index");
        }

        // ================== EDIT ==================
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var data = await _service.GetByIdAsync(id);
            if (data == null) return NotFound();

            return View("~/Views/HR/Payroll/Edit.cshtml", data);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(PayrollDTO model)
        {
            if (!ModelState.IsValid)
                return View("~/Views/HR/Payroll/Edit.cshtml", model);

            await _service.UpdateAsync(model);
            return RedirectToAction("Index");
        }

        // ================== DELETE ==================
        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            await _service.DeleteAsync(id);
            return RedirectToAction("Index");
        }

        // ================== DETAILS ==================
        public async Task<IActionResult> Details(int id)
        {
            var data = await _service.GetByIdAsync(id);
            if (data == null) return NotFound();

            return View("~/Views/HR/Payroll/Details.cshtml", data);
        }
    }
}