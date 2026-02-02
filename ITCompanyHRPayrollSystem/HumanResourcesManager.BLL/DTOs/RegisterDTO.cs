using System.ComponentModel.DataAnnotations;

namespace HumanResourcesManager.BLL.DTOs
{
    public class RegisterDTO
    {
        [Required(ErrorMessage = "Full name is required")]
        [StringLength(150)]
        public string FullName { get; set; } = null!;

        [Required]
        [EmailAddress(ErrorMessage = "Invalid email")]
        public string Email { get; set; } = null!;

        public bool Gender { get; set; }

        [Required]
        [MinLength(4, ErrorMessage = "Username must be at least 4 characters")]
        public string Username { get; set; } = null!;

        [Required]
        [MinLength(8, ErrorMessage = "Password must be at least 8 characters")]
        public string Password { get; set; } = null!;

        [Required]
        [Compare("Password", ErrorMessage = "Password confirmation does not match")]
        public string ConfirmPassword { get; set; } = null!;
    }
}
