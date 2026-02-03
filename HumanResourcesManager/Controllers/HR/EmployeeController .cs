using HumanResourcesManager.BLL.DTOs;
using HumanResourcesManager.BLL.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HumanResourcesManager.Controllers
{
    [Authorize(Policy = "HR")]
    [Route("HumanResourcesManager/HR/Employee")]
    public class EmployeeController : Controller
    {
        private readonly IEmployeeService _employeeService;

        public EmployeeController(IEmployeeService employeeService)
        {
            _employeeService = employeeService;
        }

        // GET: /HR/Employee
        [HttpGet("")]
        public IActionResult Index()
        {
            var employees = _employeeService.GetAll();
            return View("~/Views/HR/Employee/Index.cshtml", employees);
        }

        // POST: /HR/Employee/Edit
        [HttpPost("Edit")]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(EmployeeDTO dto)
        {
            if (!ModelState.IsValid)
            {
                // modal -> quay về index
                return RedirectToAction(nameof(Index));
            }

            _employeeService.Update(dto);
            return RedirectToAction(nameof(Index));
        }

        // (OPTIONAL) Create page
        [HttpGet("Create")]
        public IActionResult Create()
        {
            return View("~/Views/HR/Employee/Create.cshtml");
        }

        [HttpPost("Create")]
        [ValidateAntiForgeryToken]
        public IActionResult Create(EmployeeDTO dto)
        {
            if (!ModelState.IsValid)
                return View("~/Views/HR/Employee/Create.cshtml", dto);

            _employeeService.Create(dto);
            return RedirectToAction(nameof(Index));
        }
    }
}
