using HumanResourcesManager.BLL.DTOs;
using HumanResourcesManager.BLL.Interfaces;
using HumanResourcesManager.BLL.Services;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace HumanResourcesManager.Controllers
{
    [Route("HumanResourcesManager")]
    public class AuthController : Controller
    {
        private readonly IAuthService _authService;
        private readonly OtpService _otpService;
        private readonly EmailService _emailService;

        public AuthController( IAuthService authService,OtpService otpService,
            EmailService emailService)
        {
            _authService = authService;
            _otpService = otpService;
            _emailService = emailService;
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



        ////// forgot password //////
        //[HttpGet("forgot-password")]
        //public IActionResult ForgotPassword()
        //{
        //    return View();
        //}
        //[HttpPost("forgot-password")]
        //public IActionResult ForgotPassword(ForgotPasswordDTO dto)
        //{
        //    if (!ModelState.IsValid)
        //        return View(dto);

        //    var otp = _authService.GenerateOtp();

        //    HttpContext.Session.SetString("RESET_EMAIL", dto.Email);
        //    HttpContext.Session.SetString("RESET_OTP", otp);
        //    HttpContext.Session.SetString(
        //        "RESET_EXPIRE",
        //        DateTime.Now.AddMinutes(5).ToString()
        //    );

        //    // 🔥 Gửi mail (demo – log ra trước)
        //    Console.WriteLine($"OTP for {dto.Email}: {otp}");

        //    return RedirectToAction("ResetPassword");
        //}


        //[HttpGet("reset-password")]
        //public IActionResult ResetPassword()
        //{
        //    return View();
        //}

        //[HttpPost("reset-password")]
        //public IActionResult ResetPassword(ResetPasswordDTO dto)
        //{
        //    var sessionOtp = HttpContext.Session.GetString("RESET_OTP");
        //    var email = HttpContext.Session.GetString("RESET_EMAIL");
        //    var expireStr = HttpContext.Session.GetString("RESET_EXPIRE");

        //    if (sessionOtp == null || email == null)
        //    {
        //        ViewBag.Error = "Session expired";
        //        return View(dto);
        //    }

        //    if (DateTime.Parse(expireStr!) < DateTime.Now)
        //    {
        //        ViewBag.Error = "OTP expired";
        //        return View(dto);
        //    }

        //    if (dto.OtpCode != sessionOtp)
        //    {
        //        ViewBag.Error = "Invalid OTP";
        //        return View(dto);
        //    }

        //    if (dto.NewPassword != dto.ConfirmPassword)
        //    {
        //        ViewBag.Error = "Password is not match";
        //        return View(dto);
        //    }

        //    _authService.ResetPasswordByEmail(email, dto.NewPassword, dto.ConfirmPassword);

        //    HttpContext.Session.Remove("RESET_OTP");
        //    HttpContext.Session.Remove("RESET_EMAIL");

        //    return RedirectToAction("Login");
        //}


      

        [HttpGet("forgot-password")]
        public IActionResult ForgotPassword()
        {
            return View();
        }

        [HttpPost("forgot-password")]
        public IActionResult ForgotPassword(ForgotPasswordDTO dto)
        {
            if (!ModelState.IsValid)
                return View(dto);

            var otp = _otpService.GenerateOtp();

            HttpContext.Session.SetString("RESET_EMAIL", dto.Email);
            HttpContext.Session.SetString("RESET_OTP", otp);
            HttpContext.Session.SetString(
                "RESET_EXPIRE",
                DateTime.Now.AddMinutes(5).ToString()
            );

            // ✅ gửi mail thật
            _emailService.SendOtp(dto.Email, otp);

            return RedirectToAction("VerifyOtp");
        }

        [HttpGet("verify-otp")]
        public IActionResult VerifyOtp()
        {
            return View();
        }

        [HttpPost("verify-otp")]
        public IActionResult VerifyOtp(VerifyOtpDTO dto)
        {
            var sessionOtp = HttpContext.Session.GetString("RESET_OTP");
            var expireStr = HttpContext.Session.GetString("RESET_EXPIRE");

            if (sessionOtp == null || expireStr == null)
            {
                ViewBag.Error = "OTP expired";
                return View(dto);
            }

            if (DateTime.Parse(expireStr) < DateTime.Now)
            {
                ViewBag.Error = "OTP expired";
                return View(dto);
            }

            if (dto.Otp != sessionOtp)
            {
                ViewBag.Error = "Invalid OTP";
                return View(dto);
            }

            return RedirectToAction("ResetPassword");
        }


        // reset password
        [HttpGet("reset-password")]
        public IActionResult ResetPassword()
        {
            return View();
        }

        [HttpPost("reset-password")]
        public IActionResult ResetPassword(ResetPasswordDTO dto)
        {
            if (!ModelState.IsValid)
                return View(dto);

            var email = HttpContext.Session.GetString("RESET_EMAIL");

            if (email == null)
            {
                ViewBag.Error = "Session expired";
                return View(dto);
            }

            _authService.ResetPassword(email, dto.NewPassword, dto.ConfirmPassword);

            HttpContext.Session.Remove("RESET_OTP");
            HttpContext.Session.Remove("RESET_EMAIL");
            TempData["ResetSuccess"] = true;

            return RedirectToAction("Login");
        }



    }
}
