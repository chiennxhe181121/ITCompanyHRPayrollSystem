using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HumanResourcesManager.Controllers.Employee
{
    //[Authorize(Policy = "EMPLOYEE")]
    [AllowAnonymous]
    [Route("HumanResourcesManager/Employee")]
    public class EmployeeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
