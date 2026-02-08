using HumanResourcesManager.BLL.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace HumanResourcesManager.Controllers.Employee
{
    [Authorize(Policy = "EMP")]
    [Route("HumanResourcesManager/employee")]
    public class EmployeeController : Controller
    {
        private readonly IEmployeeService _employeeService;

        public EmployeeController(IEmployeeService employeeService)
        {
            _employeeService = employeeService;
        }

        public IActionResult Index()
        {
            int userId = int.Parse(
                User.FindFirstValue(ClaimTypes.NameIdentifier)!
            );

            // dùng userId để lấy dữ liệu
            var employee = _employeeService.GetByUserId(userId);

            return View(employee);
        }
    }
}