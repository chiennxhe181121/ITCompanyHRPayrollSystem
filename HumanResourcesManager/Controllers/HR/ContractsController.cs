using HumanResourcesManager.BLL.DTOs;
using HumanResourcesManager.BLL.Interfaces;
using HumanResourcesManager.BLL.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace HumanResourcesManager.Controllers
{
    [Route("HumanResourcesManager/HR/Contract")]
    public class ContractsController : Controller
    {
        private readonly IContractService _contractService;
        private readonly IEmployeeService _employeeService;

        public ContractsController(
    IContractService contractService,
    IEmployeeService employeeService)
        {
            _contractService = contractService;
            _employeeService = employeeService;
        }

        // =========================
        // INDEX RIÊNG CHO CONTRACT
        // =========================
        [HttpGet("")]
        public IActionResult Index(
     bool isActive = true,
     int page = 1,
     int pageSize = 1,
     string? search = null,
     string? contractType = null)
        {
            var query = _contractService.GetAll()
                            .Where(x => x.IsActive == isActive);

            // SEARCH theo tên nhân viên
            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.Trim().ToLower();

                query = query.Where(x =>
                    x.EmployeeName.ToLower().Contains(search));
            }

            // FILTER theo loại hợp đồng
            if (!string.IsNullOrEmpty(contractType))
            {
                query = query.Where(x => x.ContractType == contractType);
            }

            query = query.OrderByDescending(x => x.StartDate);

            var totalRecords = query.Count();
            var totalPages = (int)Math.Ceiling((double)totalRecords / pageSize);

            if (page > totalPages && totalPages > 0)
                page = totalPages;

            var data = query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;
            ViewBag.PageSize = pageSize;
            ViewBag.TotalRecords = totalRecords;
            ViewBag.IsActive = isActive;
            ViewBag.Search = search;
            ViewBag.ContractType = contractType;

            return View("~/Views/HR/Contracts/Index.cshtml", data);
        }

        // =========================
        // GET CREATE
        // =========================
        [HttpGet("create")]
        public IActionResult Create()
        {
            var allEmployees = _employeeService.GetAll();
            var contracts = _contractService.GetAll();

            var employeeHasContract = contracts
                .Where(c => c.IsActive)
                .Select(c => c.EmployeeId)
                .ToList();

            var availableEmployees = allEmployees
                .Where(e => !employeeHasContract.Contains(e.EmployeeId))
                .ToList();

            var model = new ContractDTO
            {
                StartDate = DateTime.Now,
                IsActive = true,
                Employees = availableEmployees
                    .Select(e => new SelectListItem
                    {
                        Value = e.EmployeeId.ToString(),
                        Text = e.FullName
                    }).ToList()
            };

            return View("~/Views/HR/Contracts/Create.cshtml", model);
        }

        // =========================
        // POST CREATE
        // =========================
        [HttpPost("create")]
        public IActionResult Create(ContractDTO dto)
        {
            // Validate ngày
            if (dto.EndDate <= dto.StartDate)
            {
                ModelState.AddModelError("EndDate",
                    "Ngày kết thúc phải lớn hơn ngày bắt đầu");
            }

            if (!ModelState.IsValid)
            {
                
                return View("~/Views/HR/Contracts/Create.cshtml", dto);
            }

            _contractService.Create(dto);
            return RedirectToAction("Index");
        }


        // =========================
        // DELETE (SOFT DELETE)
        // =========================
        [HttpPost("delete")]
        public IActionResult Delete(int id, bool isActive, int page, int pageSize)
        {
            _contractService.SoftDelete(id);

            return RedirectToAction("Index", new
            {
                isActive = isActive,
                page = page,
                pageSize = pageSize
            });
        }

        [HttpGet("edit/{id}")]
        public IActionResult Edit(int id)
        {
            var dto = _contractService.GetById(id);

            if (dto == null)
                return NotFound();

            return View("~/Views/HR/Contracts/Edit.cshtml", dto);
        }

        [HttpPost("edit")]
        public IActionResult Edit(ContractDTO dto)
        {
            if (dto.EndDate <= dto.StartDate)
            {
                ModelState.AddModelError("EndDate",
                    "Ngày kết thúc phải lớn hơn ngày bắt đầu");
            }

            if (!ModelState.IsValid)
                return View("~/Views/HR/Contracts/Edit.cshtml", dto);

            _contractService.Update(dto);

            return RedirectToAction("Index");
        }


    }
}