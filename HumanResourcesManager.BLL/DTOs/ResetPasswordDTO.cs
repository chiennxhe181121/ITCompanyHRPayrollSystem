using System.ComponentModel.DataAnnotations;

namespace HumanResourcesManager.BLL.DTOs
{
    public class ResetPasswordDTO
    {
        //[Required]
        //public string OtpCode { get; set; } = null!;

        [Required]
        [MinLength(8)]
        public string NewPassword { get; set; } = null!;

        [Required]
        [Compare("NewPassword", ErrorMessage = "Password confirmation does not match")]
        public string ConfirmPassword { get; set; } = null!;
    }
}
