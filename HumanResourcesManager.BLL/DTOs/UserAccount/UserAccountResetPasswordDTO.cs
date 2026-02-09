using System.ComponentModel.DataAnnotations;

namespace HumanResourcesManager.BLL.DTOs.UserAccount
{
    public class UserAccountResetPasswordDTO
    {
        public int UserId { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập mật khẩu mới")]
        [MinLength(8, ErrorMessage = "Mật khẩu phải có ít nhất 8 ký tự")]
        public string NewPassword { get; set; }
    }
}
