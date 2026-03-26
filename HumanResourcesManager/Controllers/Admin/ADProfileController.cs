using HumanResourcesManager.BLL.DTOs; 
using HumanResourcesManager.BLL.DTOs.ADEmployee; 
using HumanResourcesManager.BLL.Interfaces;
using HumanResourcesManager.Helpers; 
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace HumanResourcesManager.Controllers.Admin
{
    [Authorize(Roles = "ADMIN")]
    [Route("HumanResourcesManager/admin/myprofile")]
    public class ADProfileController : Controller
    {
        private readonly IADEmployeeService _service;

        public ADProfileController(IADEmployeeService service)
        {
            _service = service;
        }

        [HttpGet("")]
        public IActionResult Index()
        {
            int userId = GetCurrentUserId();
            var profile = _service.GetProfileByUserId(userId);

            if (profile == null)
                return NotFound("Không tìm thấy hồ sơ liên kết với tài khoản này.");

            return View("~/Views/Admin/Profile/Index.cshtml", profile);
        }

        [HttpPost("")]
        [ValidateAntiForgeryToken]
        public IActionResult Index(AdminProfileDTO dto)
        {
            if (!ModelState.IsValid)
            {
                ReloadViewOnlyData(dto);
                return View("~/Views/Admin/Profile/Index.cshtml", dto);
            }

            string message;
            bool isSuccess = _service.UpdateProfile(dto, out message);

            if (isSuccess)
            {
                TempData["Success"] = "Cập nhật hồ sơ thành công!";

                // Cập nhật lại Session (để Avatar/Tên trên Sidebar đổi ngay lập tức)
                UpdateSession(dto);

                return RedirectToAction(nameof(Index));
            }
            else
            {
                TempData["Error"] = message;
                ReloadViewOnlyData(dto);
                return View("~/Views/Admin/Profile/Index.cshtml", dto);
            }
        }


        private int GetCurrentUserId()
        {
            var claimId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return int.TryParse(claimId, out int id) ? id : 0;
        }

        private void ReloadViewOnlyData(AdminProfileDTO dto)
        {
            var original = _service.GetProfileByUserId(GetCurrentUserId());

            if (original != null)
            {
                dto.EmployeeCode = original.EmployeeCode;
                dto.DepartmentName = original.DepartmentName;
                dto.PositionName = original.PositionName;
                dto.HireDate = original.HireDate;
                dto.RoleName = original.RoleName;


                if (dto.AvatarFile == null)
                {
                    dto.ImgAvatar = original.ImgAvatar;
                }
            }
        }

        private void UpdateSession(AdminProfileDTO dto)
        {
            var sessionUser = HttpContext.Session.GetObject<UserSessionDTO>("USER_SESSION");
            if (sessionUser != null)
            {
                sessionUser.FullName = dto.FullName;

                var freshProfile = _service.GetProfileByUserId(GetCurrentUserId());
                sessionUser.ImgAvatar = freshProfile?.ImgAvatar;

                HttpContext.Session.SetObject("USER_SESSION", sessionUser);
            }
        }
    }
}