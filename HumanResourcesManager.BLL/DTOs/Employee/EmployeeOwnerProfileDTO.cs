using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HumanResourcesManager.BLL.DTOs.Employee
{
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
