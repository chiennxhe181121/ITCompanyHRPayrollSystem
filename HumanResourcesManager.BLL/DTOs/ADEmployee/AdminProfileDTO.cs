using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HumanResourcesManager.BLL.DTOs.ADEmployee
{
    public class AdminProfileDTO
    {
        public int EmployeeId { get; set; }

        [Required(ErrorMessage = "Họ tên không được để trống")]
        public string FullName { get; set; } = null!;

        [Required(ErrorMessage = "Email không được để trống")]
        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        public string Email { get; set; } = null!;

        [Required(ErrorMessage = "SĐT không được để trống")]
        public string Phone { get; set; } = null!;

        public string? Address { get; set; }
        public bool Gender { get; set; } 

        public IFormFile? AvatarFile { get; set; }

        public string? ImgAvatar { get; set; } 
        public string? EmployeeCode { get; set; }
        public string? DepartmentName { get; set; }
        public string? PositionName { get; set; }
        public DateTime HireDate { get; set; }

        public string? RoleName { get; set; }
    }
}
