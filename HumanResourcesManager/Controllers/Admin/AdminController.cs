using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HumanResourcesManager.Controllers.Admin
{
    [Authorize(Roles = "ADMIN")]
    [Route("HumanResourcesManager/admin")]
    public class AdminController : Controller
    {
        [HttpGet("")]
        public IActionResult Index()
        {
            //return View("~/Views/Admin/Dashboard/Index.cshtml");
            //return View("~/Views/Admin/Index.cshtml");
            return View();
        }
    }

}
