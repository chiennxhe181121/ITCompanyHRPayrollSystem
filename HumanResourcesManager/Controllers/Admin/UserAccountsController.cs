using HumanResourcesManager.BLL.DTOs;
using HumanResourcesManager.BLL.DTOs.UserAccount;
using HumanResourcesManager.BLL.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace HumanResourcesManager.Controllers.Admin
{
    //[Area("Admin")]
    [Route("HumanResourcesManager/admin/UserAccounts")]
    public class UserAccountsController : Controller
    {
        private readonly IUserAccountService _service;

        public UserAccountsController(IUserAccountService service)
        {
            _service = service;
        }

        //[HttpGet("")]
        //public IActionResult Index()
        //{
        //    var accounts = _service.GetAllAccounts();
        //    //return View(accounts);
        //    return View("~/Views/Admin/UserAccounts/Index.cshtml", accounts);
        //}

        [HttpGet("")]
        public IActionResult Index(int page = 1, int pageSize = 10)
        {
            var allAccounts = _service.GetAllAccounts();

            var totalItems = allAccounts.Count;
            var totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

            var accounts = allAccounts
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;

            return View("~/Views/Admin/UserAccounts/Index.cshtml", accounts);
        }




        // ===== CREATE =====
        [HttpGet("Create")]
        public IActionResult Create()
        {
            return View("~/Views/Admin/UserAccounts/Create.cshtml",
                new UserAccountCreateDTO());
        }


        //[HttpPost("Create")]
        //public IActionResult Create(UserAccountCreateDTO dto)
        //{
        //    if (!ModelState.IsValid)
        //        return View("~/Views/Admin/UserAccounts/Create.cshtml", dto);

        //    _service.Create(dto);

        //    // 👉 TÍNH TRANG CUỐI
        //    var totalItems = _service.GetAllAccounts().Count;
        //    var pageSize = 10; // nhớ đồng bộ với Index
        //    var lastPage = (int)Math.Ceiling(totalItems / (double)pageSize);

        //    return RedirectToAction(nameof(Index), new { page = lastPage });
        //}
        [HttpPost("Create")]
        public IActionResult Create(UserAccountCreateDTO dto)
        {
            // 1️⃣ Validate DataAnnotation
            if (!ModelState.IsValid)
                return View("~/Views/Admin/UserAccounts/Create.cshtml", dto);

            try
            {
                // 2️⃣ Gọi service (có thể throw exception)
                _service.Create(dto);

                // 3️⃣ TÍNH TRANG CUỐI ĐỂ QUAY LẠI ĐÚNG CHỖ
                var totalItems = _service.GetAllAccounts().Count;
                var pageSize = 10; // ⚠️ PHẢI TRÙNG với Index
                var lastPage = (int)Math.Ceiling(totalItems / (double)pageSize);

                return RedirectToAction(nameof(Index), new { page = lastPage });
            }
            catch (Exception ex)
            {
                // 4️⃣ BẮT LỖI → HIỆN RA FORM, KHÔNG 500
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



    }

}
