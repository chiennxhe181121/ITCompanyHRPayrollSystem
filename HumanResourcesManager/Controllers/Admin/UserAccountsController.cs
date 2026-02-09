using HumanResourcesManager.BLL.DTOs;
using HumanResourcesManager.BLL.DTOs.UserAccount;
using HumanResourcesManager.BLL.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;


// @HoangDH 
namespace HumanResourcesManager.Controllers.Admin
{
    //[Area("Admin")]
    [Authorize(Roles = "ADMIN")]
    [Route("HumanResourcesManager/admin/UserAccounts")]
    public class UserAccountsController : Controller
    {
        private readonly IUserAccountService _service;

        public UserAccountsController(IUserAccountService service)
        {
            _service = service;
        }


        [HttpGet("")]
        public IActionResult Index(
            string? keyword,
            string? roleCode, 
            string? status,
            int page = 1)
        {
            int pageSize = 10;

            var result = _service.SearchAccounts(keyword, roleCode, status, page, pageSize);

            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = (int)Math.Ceiling(result.TotalItems / (double)pageSize);

            return View("~/Views/Admin/UserAccounts/Index.cshtml", result.Items);
        }


        // ===== CREATE =====
        [HttpGet("Create")]
        public IActionResult Create()
        {
            return View("~/Views/Admin/UserAccounts/Create.cshtml",
                new UserAccountCreateDTO());
        }

        [HttpPost("Create")]
        public IActionResult Create(UserAccountCreateDTO dto)
        {
            if (!ModelState.IsValid)
                return View("~/Views/Admin/UserAccounts/Create.cshtml", dto);

            try
            {
                _service.Create(dto);

                var totalItems = _service.GetAllAccounts().Count;
                var pageSize = 10; 
                var lastPage = (int)Math.Ceiling(totalItems / (double)pageSize);

                return RedirectToAction(nameof(Index), new { page = lastPage });
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                return View("~/Views/Admin/UserAccounts/Create.cshtml", dto);
            }
        }


        [HttpGet("Edit/{id}")]
        public IActionResult Edit(int id)
        {
            try
            {
                int currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
                var targetUser = _service.GetById(id);
                var currentUser = _service.GetById(currentUserId);

                if (targetUser == null) return NotFound();

                if (targetUser.UserId == currentUserId)
                {
                    TempData["Error"] = "Bạn không thể tự sửa tài khoản của mình tại đây. Vui lòng vào trang Cá nhân!";
                    return RedirectToAction(nameof(Index));
                }

                if (targetUser.RoleCode == currentUser.RoleCode)
                {
                    TempData["Error"] = $"Bạn không thể chỉnh sửa đồng nghiệp cùng cấp ({targetUser.RoleCode})!";
                    return RedirectToAction(nameof(Index));
                }

                var dto = new UserAccountUpdateDTO
                {
                    UserId = targetUser.UserId,
                    RoleCode = targetUser.RoleCode
                };

                return View("~/Views/Admin/UserAccounts/Edit.cshtml", dto);
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Lỗi: " + ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }


        [HttpPost("Edit/{id}")]
        public IActionResult Edit(int id, UserAccountUpdateDTO dto)
        {
            if (id != dto.UserId)
                return BadRequest();

            if (!ModelState.IsValid)
                return View("~/Views/Admin/UserAccounts/Edit.cshtml", dto);

            try
            {
                int currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

                _service.Update(dto, currentUserId);

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);

                return View("~/Views/Admin/UserAccounts/Edit.cshtml", dto);
            }
        }



        // ===== RESET PASSWORD =====
        [HttpGet("ResetPassword/{id}")]
        public IActionResult ResetPassword(int id)
        {
            var user = _service.GetById(id);
            if (user == null) return NotFound();

            var dto = new UserAccountResetPasswordDTO
            {
                UserId = id
            };

            return View("~/Views/Admin/UserAccounts/ResetPassword.cshtml", dto);
        }

        [HttpPost("ResetPassword")]
        public IActionResult ResetPassword(UserAccountResetPasswordDTO dto)
        {
            if (!ModelState.IsValid)
                return View("~/Views/Admin/UserAccounts/ResetPassword.cshtml", dto);

            _service.ResetPassword(dto);
            return RedirectToAction(nameof(Index));
        }

        [HttpPost("Active/{id}")]
        public IActionResult Active(int id)
        {
            _service.SetActive(id);
            return RedirectToAction(nameof(Index));
        }

        [HttpPost("Inactive/{id}")]
        public IActionResult Inactive(int id)
        {
            try
            {
                
                int currentUserId = int.Parse(
                    User.FindFirst(ClaimTypes.NameIdentifier)!.Value
                );

                _service.SetInactive(id, currentUserId);
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }





    }

}
