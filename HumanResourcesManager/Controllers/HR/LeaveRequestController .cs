using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

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
    public IActionResult Index()
    {
        var data = _service.GetAll();
        return View("~/Views/HR/LeaveRequest/Index.cshtml", data);
    }

    // POST: /HR/LeaveRequest/Edit
    [HttpPost("Edit")]
    [ValidateAntiForgeryToken]
    public IActionResult Edit(LeaveRequestDTO model)
    {
        _service.UpdateStatus(model.LeaveRequestId, model.Status);
        return RedirectToAction(nameof(Index));
    }
}
