using HumanResourcesManager.BLL.DTOs;
using HumanResourcesManager.BLL.DTOs.UserAccount;
using HumanResourcesManager.BLL.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

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
            // Validate DataAnnotation
            if (!ModelState.IsValid)
                return View("~/Views/Admin/UserAccounts/Create.cshtml", dto);

            try
            {
                // Gọi service (có thể throw exception)
                _service.Create(dto);

                // TÍNH TRANG CUỐI ĐỂ QUAY LẠI ĐÚNG CHỖ
                var totalItems = _service.GetAllAccounts().Count;
                var pageSize = 10; // ⚠️ PHẢI TRÙNG với Index
                var lastPage = (int)Math.Ceiling(totalItems / (double)pageSize);

                return RedirectToAction(nameof(Index), new { page = lastPage });
            }
            catch (Exception ex)
            {
                // BẮT LỖI → HIỆN RA FORM, KHÔNG 500
                ModelState.AddModelError(string.Empty, ex.Message);
                return View("~/Views/Admin/UserAccounts/Create.cshtml", dto);
            }
        }


        // ===== EDIT =====
        [HttpGet("Edit/{id}")]
        public IActionResult Edit(int id)
        {
            var account = _service.GetById(id);
            if (account == null) return NotFound();

            var dto = new UserAccountUpdateDTO
            {
                UserId = account.UserId,
                RoleCode = account.RoleCode,
                Status = account.Status
            };

            return View("~/Views/Admin/UserAccounts/Edit.cshtml", dto);
        }



        [HttpPost("Edit/{id}")]
        public IActionResult Edit(int id, UserAccountUpdateDTO dto)
        {
            if (id != dto.UserId)
                return BadRequest();

            if (!ModelState.IsValid)
                return View("~/Views/Admin/UserAccounts/Edit.cshtml", dto);

            _service.Update(dto);
            return RedirectToAction(nameof(Index));
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
            _service.SetInactive(id);
            return RedirectToAction(nameof(Index));
        }



    }

}
