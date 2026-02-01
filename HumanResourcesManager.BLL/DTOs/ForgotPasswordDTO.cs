using System.ComponentModel.DataAnnotations;

namespace HumanResourcesManager.BLL.DTOs
{
    public class ForgotPasswordDTO
    {
        [Required]
        [EmailAddress(ErrorMessage = "Invalid email")]
        public string Email { get; set; } = null!;
    }
}
