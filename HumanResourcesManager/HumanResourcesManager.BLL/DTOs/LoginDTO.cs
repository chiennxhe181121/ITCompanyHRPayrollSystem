using System.ComponentModel.DataAnnotations;

namespace HumanResourcesManager.BLL.DTOs
{
    public class LoginDTO
    {
        [Display(Name = "Username or Email")]
        [Required(ErrorMessage = "Username or Email is required")]
        public string LoginKey { get; set; } = null!;

        [Required(ErrorMessage = "Password is required")]
        public string Password { get; set; } = null!;
    }
}
