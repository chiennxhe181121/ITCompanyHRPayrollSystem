using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HumanResourcesManager.Controllers
{
    [Authorize(Policy = "HR")]
    [Route("HumanResourcesManager/HR")]
    public class HRController : Controller
    {
        [HttpGet("")]
        public IActionResult Index()
        {
            return View();
        }
    }
}