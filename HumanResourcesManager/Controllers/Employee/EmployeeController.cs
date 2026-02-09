using HumanResourcesManager.BLL.DTOs;
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

        [HttpPost("update-profile")]
        [ValidateAntiForgeryToken]
        public IActionResult UpdateProfile(EmployeeRequestDTO dto)
        {
            int userId = int.Parse(
                User.FindFirstValue(ClaimTypes.NameIdentifier)!
            );

            var result = _employeeService.UpdateOwnProfile(userId, dto);

            if (result == null)
                return BadRequest();

            return Ok();
        }

        public IActionResult Index()
        {
            int userId = int.Parse(
                User.FindFirstValue(ClaimTypes.NameIdentifier)!
            );

            // dùng userId để lấy dữ liệu
            var employee = _employeeService.GetOwnProfile(userId);

            return View(employee);
        }
    }
}