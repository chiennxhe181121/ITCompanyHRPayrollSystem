using System.ComponentModel.DataAnnotations;

namespace HumanResourcesManager.BLL.DTOs
{
    public class EmployeeDTO
    {
        public int EmployeeId { get; set; }
        public string EmployeeCode { get; set; } = null!;
        public string FullName { get; set; } = null!;
        public bool Gender { get; set; }
        public DateTime DateOfBirth { get; set; }
        public string Email { get; set; } = null!;
        public string Phone { get; set; } = null!;
        public string? Address { get; set; }
        public string? ImgAvatar { get; set; }
        public DateTime HireDate { get; set; }
        public int Status { get; set; }
        public int DepartmentId { get; set; }
        public string? DepartmentName { get; set; }
        public int PositionId { get; set; }
        public string? PositionName { get; set; }
    }
    public class EmployeeOwnerProfileDTO
    {
        // ===== EDITABLE (REQUEST) =====
        [Required]
        [StringLength(100)]
        public string FullName { get; set; } = null!;

        public bool Gender { get; set; }

        [Required]
        [DataType(DataType.Date)]
        public DateTime DateOfBirth { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; } = null!;

        [Required]
        [Phone]
        [StringLength(11)]
        public string Phone { get; set; } = null!;

        [StringLength(255)]
        public string? Address { get; set; }

        public bool RemoveAvatar { get; set; }

        // ===== READ-ONLY (RESPONSE) =====
        public string EmployeeCode { get; set; } = null!;
        public DateTime HireDate { get; set; }
        public string? DepartmentName { get; set; }
        public string? PositionName { get; set; }
        public string? ImgAvatar { get; set; }
    }
}
