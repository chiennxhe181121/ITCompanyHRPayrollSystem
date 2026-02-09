using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HumanResourcesManager.BLL.DTOs.Dashboard
{
    public class DashboardViewModel
    {
        // Thống kê số lượng
        public int TotalUsers { get; set; }
        public int TotalEmployees { get; set; }
        public int TotalDepartments { get; set; }
        public int ActiveUsers { get; set; }

        // Dữ liệu cho biểu đồ (Ví dụ)
        public List<int> EmployeeStatsByMonth { get; set; } = new List<int>(); // Số nhân viên 6 tháng gần đây
        public Dictionary<string, int> DepartmentDistribution { get; set; } = new Dictionary<string, int>(); // Phân bổ phòng ban

        // Danh sách mới nhất
        public List<NewUserDTO> RecentUsers { get; set; } = new List<NewUserDTO>();
    }

    public class NewUserDTO
    {
        public string Username { get; set; }
        public string RoleName { get; set; }
        public DateTime CreatedDate { get; set; }
        public string Status { get; set; }
    }
}
