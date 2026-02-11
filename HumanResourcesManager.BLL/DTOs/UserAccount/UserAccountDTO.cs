using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


// @HoangDH 
namespace HumanResourcesManager.BLL.DTOs.UserAccount
{
    public  class UserAccountDTO
    {
        public int UserId { get; set; }
        public string Username { get; set; } = null!;
        public string? FullName { get; set; }
        public string? Email { get; set; }

        //public int? EmployeeId { get; set; } // 👈 THÊM DÒNG NÀY
        //public string? Avatar { get; set; } // URL or path to avatar image

        public string RoleCode { get; set; } = null!;
        public string RoleName { get; set; } = null!;
        public int Status { get; set; }
        public bool HasEmployee { get; set; }
    }
}
