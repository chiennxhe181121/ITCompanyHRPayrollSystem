using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

// @HoangDH 
namespace HumanResourcesManager.BLL.DTOs.UserAccount
{
    public class UserAccountCreateDTO
    {
        [Required(ErrorMessage = "Username không được để trống")]
        [MinLength(4, ErrorMessage = "Username tối thiểu 4 ký tự")]
        public string Username { get; set; } = null!;

        [Required(ErrorMessage = "Password không được để trống")]
        [MinLength(6, ErrorMessage = "Password tối thiểu 6 ký tự")]
        public string Password { get; set; } = null!;

        [Required(ErrorMessage = "Vui lòng chọn vai trò")]
        public int RoleId { get; set; }

        public string? FullName { get; set; }

        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        public string? Email { get; set; }
    }


}
