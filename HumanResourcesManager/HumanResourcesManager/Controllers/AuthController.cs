using HumanResourcesManager.BLL.DTOs;
using HumanResourcesManager.BLL.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace HumanResourcesManager.Controllers
{
    [Route("HumanResourcesManager")]
    public class AuthController : Controller
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        // GET: /HumanResourcesManager/register
        [HttpGet("register")]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost("register")]
        public IActionResult Register(RegisterDTO dto)
        {
            if (!ModelState.IsValid)
                return View(dto);

            try
            {
                _authService.Register(dto);
                return RedirectToAction("Login");
            }
            catch (Exception ex)
            {
                ViewBag.Error = ex.Message;
                return View(dto);
            }
        }


        // GET: /HumanResourcesManager/login
        [HttpGet("login")]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost("login")]
        public IActionResult Login(LoginDTO dto)
        {
            var sessionUser = _authService.Login(dto);

            if (sessionUser == null)
            {
                ViewBag.Error = "Invalid username or password";
                return View(dto);
            }

            HttpContext.Session.SetString(
                "USER_SESSION",
                JsonSerializer.Serialize(sessionUser)
            );

            return RedirectToAction("Index", "Home");
        }

        // GET: /HumanResourcesManager/logout
        [HttpGet("logout")]
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }
    }
}
