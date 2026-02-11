using HumanResourcesManager.DAL.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HumanResourcesManager.DAL.Models
{
    public class Department
    {
        public int DepartmentId { get; set; }
        public string DepartmentName { get; set; } = null!;
        public string? Description { get; set; }

        // Hoàng thêm để Soft delete
        public int Status { get; set; } = Constants.Active;
        // 1 – N
        public ICollection<Employee> Employees { get; set; } = new List<Employee>();
    }
}
