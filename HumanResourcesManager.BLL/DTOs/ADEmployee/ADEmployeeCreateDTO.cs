using System.ComponentModel.DataAnnotations;

namespace HumanResourcesManager.BLL.DTOs.ADEmployee
{
    public class ADEmployeeCreateDTO
    {
        // --- Thông tin cá nhân ---
        [Required(ErrorMessage = "Họ tên không được để trống")]
        public string FullName { get; set; } = null!;

        [Required(ErrorMessage = "Vui lòng chọn giới tính")]
        public bool Gender { get; set; } 

        [Required(ErrorMessage = "Ngày sinh là bắt buộc")]
        public DateTime DateOfBirth { get; set; }

        [Required(ErrorMessage = "Email là bắt buộc")]
        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        public string Email { get; set; } = null!;

        [Required(ErrorMessage = "Số điện thoại là bắt buộc")]
        public string Phone { get; set; } = null!;

        public string? Address { get; set; }
        public string? ImgAvatar { get; set; }

        [Required]
        public DateTime HireDate { get; set; } = DateTime.Now;

        [Required(ErrorMessage = "Vui lòng chọn phòng ban")]
        public int DepartmentId { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn chức vụ")]
        public int PositionId { get; set; }

        public bool IsCreateAccount { get; set; } = false;
        public string? Username { get; set; } 
        public string? Password { get; set; } 
    }

    public class DepartmentSelectDTO
    {
        public int DepartmentId { get; set; }
        public string DepartmentName { get; set; } = null!;
    }

    public class PositionSelectDTO
    {
        public int PositionId { get; set; }
        public string PositionName { get; set; } = null!;
        public decimal BaseSalary { get; set; }
    }
}