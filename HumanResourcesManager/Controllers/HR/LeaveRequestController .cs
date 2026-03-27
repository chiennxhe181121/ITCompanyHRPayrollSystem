using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using HumanResourcesManager.BLL.Interfaces;
using HumanResourcesManager.DAL.Enum;


namespace HumanResourcesManager.Web.Controllers.HR
{
    [Authorize(Policy = "HR")]
    [Route("HR/LeaveRequest")]
    public class LeaveRequestController : Controller
    {
        private readonly ILeaveRequestService _service;

        public LeaveRequestController(ILeaveRequestService service)
        {
            _service = service;
        }

        // GET: /HR/LeaveRequest
        [HttpGet("")]
        public IActionResult Index(int page = 1, int pageSize = 10, string? search = null, RequestStatus? status = null)
        {
            var data = _service.GetAll()
    .OrderBy(x => x.Status == RequestStatus.Pending ? 0 : 1) // Pending lên đầu
    .ThenByDescending(x => x.FromDate) // mới nhất trước
    .AsQueryable();

            // 🔍 Search theo employee
            if (!string.IsNullOrWhiteSpace(search))
            {
                data = data.Where(x => x.EmployeeName.Contains(search));
            }

            // 🎯 Filter theo status nếu có chọn
            if (status.HasValue)
            {
                data = data.Where(x => x.Status == status.Value);
            }

            // 📊 Pagination
            int totalRecords = data.Count();
            var result = data
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            // ViewBag
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = (int)Math.Ceiling((double)totalRecords / pageSize);
            ViewBag.TotalRecords = totalRecords;
            ViewBag.Search = search;
            ViewBag.Status = status;

            return View("~/Views/HR/LeaveRequest/Index.cshtml", result);
        }

        // POST: Duyệt đơn
        [HttpPost("Approve")]
        [ValidateAntiForgeryToken]
        public IActionResult Approve(int leaveRequestId)
        {
            int approverId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "0");

            var result = _service.ApproveLeaveRequest(leaveRequestId, approverId);

            if (result.IsSuccess)
                TempData["Success"] = result.Message;
            else
                TempData["Error"] = result.Message;

            return RedirectToAction(nameof(Index));
        }

        // POST: Từ chối đơn
        [HttpPost("Reject")]
        [ValidateAntiForgeryToken]
        public IActionResult Reject(int leaveRequestId, string? comments)
        {
            int approverId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "0");

            // Nếu muốn lưu Reply/Comments, bạn cần sửa Approve/Reject trong Service để nhận thêm Comments
            var result = _service.RejectLeaveRequest(leaveRequestId, approverId);

            if (result.IsSuccess)
                TempData["Success"] = result.Message;
            else
                TempData["Error"] = result.Message;

            return RedirectToAction(nameof(Index));
        }
    }
}