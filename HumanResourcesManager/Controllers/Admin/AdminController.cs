using HumanResourcesManager.BLL.DTOs.Dashboard;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;


// @HoangDH 
namespace HumanResourcesManager.Controllers.Admin
{
    //[Authorize(Roles = "ADMIN")]
    //[Route("HumanResourcesManager/admin")]
    //public class AdminController : Controller
    //{
    //    [HttpGet("")]
    //    public IActionResult Index()
    //    {
    //        return View("~/Views/Admin/Dashboard/Index.cshtml");
    //        //return View("~/Views/Admin/Index.cshtml");
    //        //return View();
    //    }
    //}

    [Authorize(Roles = "ADMIN")]
    [Route("HumanResourcesManager/admin")]
    public class AdminController : Controller
    {
        [HttpGet("")]
        public IActionResult Index()
        {
            var model = new DashboardViewModel
            {
                TotalUsers = 120,
                TotalEmployees = 115,
                TotalDepartments = 5,
                ActiveUsers = 118,
                EmployeeStatsByMonth = new List<int> { 10, 12, 15, 14, 20, 25 },
                // Giả lập dữ liệu biểu đồ tròn
                DepartmentDistribution = new Dictionary<string, int>
                {
                    { "IT", 40 },
                    { "HR", 10 },
                    { "Marketing", 25 },
                    { "Sales", 30 },
                    { "Finance", 15 }
                },

                // Giả lập danh sách user mới
                RecentUsers = new List<NewUserDTO>
                {
                    new NewUserDTO { Username = "Huy_Hoang", RoleName = "Admin", CreatedDate = DateTime.Now.AddDays(-1), Status = "Active" },
                    new NewUserDTO { Username = "Thuy_Kieu", RoleName = "HR", CreatedDate = DateTime.Now.AddDays(-2), Status = "Active" },
                    new NewUserDTO { Username = "Thuy_Van", RoleName = "Employee", CreatedDate = DateTime.Now.AddDays(-5), Status = "Inactive" },
                }
            };
            return View("~/Views/Admin/Dashboard/Index.cshtml", model);
        }
    }

}
