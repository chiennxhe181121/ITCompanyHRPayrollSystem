using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HumanResourcesManager.Controllers.Employee
{
    [Authorize(Policy = "EMP")]
    [Route("HumanResourcesManager/employee")]
    public class EmployeeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
