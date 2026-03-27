using Microsoft.AspNetCore.Mvc;
using HumanResourcesManager.BLL.Services;
using HumanResourcesManager.DAL.Models;
using HumanResourcesManager.BLL.DTOs;
using HumanResourcesManager.BLL.Interfaces;
using Rotativa.AspNetCore;
using HumanResourcesManager.BLL.Helpers;

namespace HumanResourcesManager.Controllers.HR
{
    [Route("HumanResourcesManager/HR/Payroll")]
    public class PayrollController : Controller
    {
        private readonly IPayrollService _service;

        public PayrollController(IPayrollService service)
        {
            _service = service;
        }

        // ================== LIST ==================
        [HttpGet("")]
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


        // ================== CREATE =================
        [HttpGet("create")]
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

        [HttpPost("create")]
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
        [HttpGet("edit/{id}")]
        public async Task<IActionResult> Edit(int id)
        {
            var data = await _service.GetByIdAsync(id);
            if (data == null) return NotFound();

            data.PayrollDetails ??= new List<PayrollDetailDTO>();

            // Lấy audit để hiển thị các khoản cần điều chỉnh
            var audit = await _service.AuditPayrollSimpleAsync(id);
            ViewBag.Audit = audit;

            return View("~/Views/HR/Payroll/Edit.cshtml", data);
        }

        [HttpPost("edit")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(PayrollDTO model)
        {
            if (!ModelState.IsValid)
            {
                var audit = await _service.AuditPayrollSimpleAsync(model.PayrollId.GetValueOrDefault());
                ViewBag.Audit = audit;
                return View("~/Views/HR/Payroll/Edit.cshtml", model);
            }

            var existing = await _service.GetByIdAsync(model.PayrollId.GetValueOrDefault());
            if (existing == null) return NotFound();

            // Cập nhật các trường tổng HR có thể chỉnh
            existing.BasicSalary = model.BasicSalary;
            existing.TotalOT = model.TotalOT;
            existing.TotalAllowance = model.TotalAllowance;
            existing.MissingMinutesPenalty = model.MissingMinutesPenalty ;

            // === QUAN TRỌNG: CHỈ THÊM DÒNG MỚI, KHÔNG XÓA DÒNG CŨ ===
            if (model.PayrollDetails != null && model.PayrollDetails.Any())
            {
                foreach (var detail in model.PayrollDetails)
                {
                    bool alreadyExists = existing.PayrollDetails.Any(ed =>
                        string.Equals(ed.Description?.Trim(), detail.Description?.Trim(), StringComparison.OrdinalIgnoreCase) &&
                        ed.Type == detail.Type);

                    if (!alreadyExists)
                    {
                        existing.PayrollDetails.Add(new PayrollDetailDTO
                        {
                            Description = detail.Description?.Trim(),
                            Amount = detail.Amount,
                            Type = detail.Type
                        });
                    }
                }
            }

            // Gọi service để update và tính lại NetSalary
            await _service.UpdateAsync(existing);

            return RedirectToAction("Index");
        }

        // ================== DELETE ==================
        [HttpPost("delete")]
        public async Task<IActionResult> Delete(int id)
        {
            await _service.DeleteAsync(id);
            return RedirectToAction("Index");
        }

        // ================== DETAILS ==================
        [HttpGet("details/{id}")]
        public async Task<IActionResult> Details(int id)
        {
            var data = await _service.GetByIdAsync(id);
            if (data == null) return NotFound();

            return View("~/Views/HR/Payroll/Details.cshtml", data);
        }
        [HttpGet("export-pdf")]
        public async Task<IActionResult> ExportPdf(int payrollId, int month, int year)
        {
            var model = await _service.GetByIdAsync(payrollId);

            if (model == null)
            {
                return Content("Không có dữ liệu bảng lương");
            }

            // 🔥 nếu muốn đảm bảo chắc chắn (optional)
            model.Month = month;
            model.Year = year;
            var safeName = RemoveVietNameseHelper.RemoveVietnamese(model.EmployeeName);

            return new ViewAsPdf("~/Views/HR/Payroll/PayrollPdf.cshtml", model)
            {
                FileName = $"BangLuong_{safeName}_T{month}_{year}.pdf",
                PageSize = Rotativa.AspNetCore.Options.Size.A4,
                CustomSwitches = "--enable-local-file-access"
            };
        }
    }
}