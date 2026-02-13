using HumanResourcesManager.BLL.DTOs;
using HumanResourcesManager.BLL.Interfaces;
using HumanResourcesManager.BLL.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Text.Json;

namespace HumanResourcesManager.Controllers
{
    [AllowAnonymous]
    [Route("HumanResourcesManager")]
    public class AuthController : Controller
    {
        private readonly IAuthService _authService;
        private readonly OtpService _otpService;
        private readonly EmailService _emailService;

        public AuthController(IAuthService authService, OtpService otpService,
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
        public async Task<IActionResult> Login(LoginDTO dto)
        {
            var sessionUser = _authService.Login(dto);

            if (sessionUser == null)
            {
                ViewBag.Error = "Invalid username or password";
                return View(dto);
            }



            // ✅ 1. Lưu session (phụ)
            HttpContext.Session.SetString(
                "USER_SESSION",
                JsonSerializer.Serialize(sessionUser)
            );

            // ✅ 2. TẠO CLAIMS (CỰC QUAN TRỌNG)
            var claims = new List<Claim>
             {
        new Claim(ClaimTypes.NameIdentifier, sessionUser.UserId.ToString()),
        new Claim(ClaimTypes.Name, sessionUser.Username),
        new Claim(ClaimTypes.Role, sessionUser.RoleCode) // ADMIN / HR / EMPLOYEE
                         };

            var identity = new ClaimsIdentity(
                claims,
                CookieAuthenticationDefaults.AuthenticationScheme
            );

            var principal = new ClaimsPrincipal(identity);

            // ✅ 3. SIGN IN COOKIE
            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                principal
            );

            // ✅ 4. Redirect theo role (xịn hơn)
            return sessionUser.RoleCode switch
            {
                "ADMIN" => RedirectToAction("Index", "Admin"),
                "HR" => RedirectToAction("Index", "HR"),
                "EMP" => RedirectToAction("Index", "Employee"),
                "MANAGER" => RedirectToAction("Index", "Manager"),
                _ => RedirectToAction("Index", "Home")
            };
        }


        // GET: /HumanResourcesManager/logout
        //[HttpGet("logout")]
        //public IActionResult Logout()
        //{
        //    HttpContext.Session.Clear();
        //    return RedirectToAction("Login");
        //}
        [HttpGet("logout")]
        public async Task<IActionResult> Logout()
        {
            HttpContext.Session.Clear();

            await HttpContext.SignOutAsync(
                CookieAuthenticationDefaults.AuthenticationScheme
            );

            return RedirectToAction("Login");
        }


        [HttpGet("forgot-password")]
        public IActionResult ForgotPassword()
        {
            return View();
        }

        [HttpPost("forgot-password")]
        public IActionResult ForgotPassword(ForgotPasswordDTO dto)
        {
            if (!ModelState.IsValid)
            {
                if (IsAjaxRequest())
                {
                    return Json(new
                    {
                        success = false,
                        error = "Email không hợp lệ."
                    });
                }

                return View(dto);
            }

            try
            {
                var otp = _otpService.GenerateOtp();

                HttpContext.Session.SetString("RESET_EMAIL", dto.Email);
                HttpContext.Session.SetString("RESET_OTP", otp);
                HttpContext.Session.SetString(
                    "RESET_EXPIRE",
                    DateTime.Now.AddMinutes(5).ToString()
                );

                // gửi mail thật
                _emailService.SendOtp(dto.Email, otp);

                if (IsAjaxRequest())
                {
                    return Json(new { success = true });
                }

                return RedirectToAction("VerifyOtp");
            }
            catch (Exception ex)
            {
                if (IsAjaxRequest())
                {
                    return Json(new { success = false, error = ex.Message });
                }

                ViewBag.Error = ex.Message;
                return View(dto);
            }
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
                if (IsAjaxRequest())
                {
                    return Json(new
                    {
                        success = false,
                        error = "OTP đã hết hạn. Vui lòng thực hiện lại bước quên mật khẩu."
                    });
                }

                ViewBag.Error = "OTP expired";
                return View(dto);
            }

            if (DateTime.Parse(expireStr) < DateTime.Now)
            {
                if (IsAjaxRequest())
                {
                    return Json(new
                    {
                        success = false,
                        error = "OTP đã hết hạn. Vui lòng thực hiện lại bước quên mật khẩu."
                    });
                }

                ViewBag.Error = "OTP expired";
                return View(dto);
            }

            if (dto.Otp != sessionOtp)
            {
                if (IsAjaxRequest())
                {
                    return Json(new
                    {
                        success = false,
                        error = "Mã OTP không chính xác."
                    });
                }

                ViewBag.Error = "Invalid OTP";
                return View(dto);
            }

            if (IsAjaxRequest())
            {
                return Json(new { success = true });
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
            {
                if (IsAjaxRequest())
                {
                    return Json(new
                    {
                        success = false,
                        error = "Dữ liệu mật khẩu không hợp lệ."
                    });
                }

                return View(dto);
            }

            var email = HttpContext.Session.GetString("RESET_EMAIL");

            if (email == null)
            {
                if (IsAjaxRequest())
                {
                    return Json(new
                    {
                        success = false,
                        error = "Phiên đặt lại mật khẩu đã hết hạn. Vui lòng thực hiện lại từ đầu."
                    });
                }

                ViewBag.Error = "Session expired";
                return View(dto);
            }

            try
            {
                _authService.ResetPassword(email, dto.NewPassword, dto.ConfirmPassword);

                HttpContext.Session.Remove("RESET_OTP");
                HttpContext.Session.Remove("RESET_EMAIL");
                TempData["ResetSuccess"] = true;

                if (IsAjaxRequest())
                {
                    return Json(new { success = true });
                }

                return RedirectToAction("Login");
            }
            catch (Exception ex)
            {
                if (IsAjaxRequest())
                {
                    return Json(new { success = false, error = ex.Message });
                }

                ViewBag.Error = ex.Message;
                return View(dto);
            }
        }


        //[HttpGet("google-login")]
        //public IActionResult GoogleLogin()
        //{
        //    var props = new AuthenticationProperties
        //    {
        //        RedirectUri = Url.Action(
        //            "GoogleResponse",
        //            "Auth",
        //            new { area = "" }
        //        )
        //    };

        //    return Challenge(props, GoogleDefaults.AuthenticationScheme);
        //}

        // KHÔNG có Challenge → Google không được quyền auth 
        [HttpGet("google-login")]
        public IActionResult GoogleLogin(string returnUrl = "/")
        {
            var props = new AuthenticationProperties
            {
                RedirectUri = Url.Action("GoogleResponse"),
                Items = { { "returnUrl", returnUrl } }
            };

            return Challenge(props, GoogleDefaults.AuthenticationScheme);
        }


        // Signup with Google
        //[HttpGet("signin-google")]
        //public async Task<IActionResult> GoogleResponse()
        //{
        //    var result = await HttpContext.AuthenticateAsync(GoogleDefaults.AuthenticationScheme);

        //    if (!result.Succeeded || result.Principal == null)
        //        return RedirectToAction("Login");

        //    var email = result.Principal.FindFirstValue(ClaimTypes.Email);
        //    var name = result.Principal.FindFirstValue(ClaimTypes.Name) ?? "Google User";

        //    if (string.IsNullOrEmpty(email))
        //        return RedirectToAction("Login");

        //    var session = _authService.LoginWithGoogle(email, name);

        //    HttpContext.Session.SetString(
        //        "USER_SESSION",
        //        JsonSerializer.Serialize(session)
        //    );

        //    return RedirectToAction("Index", "Home");
        //}

        [HttpGet("signin-google")]
        public async Task<IActionResult> GoogleResponse()
        {
            var result = await HttpContext.AuthenticateAsync(
                GoogleDefaults.AuthenticationScheme
            );

            if (!result.Succeeded || result.Principal == null)
                return RedirectToAction("Login");

            var email = result.Principal.FindFirstValue(ClaimTypes.Email);
            var name = result.Principal.FindFirstValue(ClaimTypes.Name) ?? "Google User";

            if (string.IsNullOrEmpty(email))
                return RedirectToAction("Login");

            var session = _authService.LoginWithGoogle(email, name);

            //  Lưu session
            HttpContext.Session.SetString(
                "USER_SESSION",
                JsonSerializer.Serialize(session)
            );

            //  TẠO CLAIMS
            var claims = new List<Claim>
    {
        new Claim(ClaimTypes.NameIdentifier, session.UserId.ToString()),
        new Claim(ClaimTypes.Name, session.Username),
        new Claim(ClaimTypes.Role, session.RoleCode)
    };

            var identity = new ClaimsIdentity(
                claims,
                CookieAuthenticationDefaults.AuthenticationScheme
            );

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(identity)
            );

            return session.RoleCode == "ADMIN"
                ? RedirectToAction("Index", "Admin")
                : RedirectToAction("Index", "Home");
        }

        private bool IsAjaxRequest()
        {
            return string.Equals(
                Request.Headers["X-Requested-With"],
                "XMLHttpRequest",
                StringComparison.OrdinalIgnoreCase
            );
        }

    }
}
