using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HumanResourcesManager.Controllers
{
    [Authorize(Roles = "MANAGER")]
    [Route("HumanResourcesManager/manager")]
    public class ManagerController : Controller
    {
        [HttpGet("")]
        public IActionResult Index()
        {
            return View();
        }
    }
}

