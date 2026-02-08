using System.ComponentModel.DataAnnotations;


// @HoangDH 
namespace HumanResourcesManager.BLL.DTOs.UserAccount
{
    public class UserAccountUpdateDTO
    {
        public int UserId { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn vai trò")]
        public string RoleCode { get; set; } = null!; // ADMIN | HR | EMP

        [Required]
        public int Status { get; set; } // Active / Inactive
    }
}
