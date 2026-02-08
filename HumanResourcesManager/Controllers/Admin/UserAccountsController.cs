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

        [HttpPost("Create")]
        public IActionResult Create(UserAccountCreateDTO dto)
        {
            if (!ModelState.IsValid)
                return View("~/Views/Admin/UserAccounts/Create.cshtml", dto);

            _service.Create(dto);
            return RedirectToAction(nameof(Index));
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
        [HttpPost("ResetPassword")]
        public IActionResult ResetPassword(UserAccountResetPasswordDTO dto)
        {
            _service.ResetPassword(dto);
            return RedirectToAction(nameof(Index));
        }

        // ===== INACTIVE =====
        [HttpPost("Inactive/{id}")]
        public IActionResult Inactive(int id)
        {
            _service.SetInactive(id);
            return RedirectToAction(nameof(Index));
        }


    }

}
