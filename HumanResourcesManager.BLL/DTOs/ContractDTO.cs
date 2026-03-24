using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace HumanResourcesManager.BLL.DTOs
{
    public class ContractDTO
    {
        public int ContractId { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn nhân viên")]
        public int EmployeeId { get; set; }

        public string? EmployeeName { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn loại hợp đồng")]
        public string ContractType { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập ngày bắt đầu")]
        [DataType(DataType.Date)]
        public DateTime StartDate { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập ngày kết thúc")]
        [DataType(DataType.Date)]
        public DateTime? EndDate { get; set; }

        [Range(1000000, double.MaxValue,
            ErrorMessage = "Lương phải lớn hơn 1.000.000")]
        public decimal BasicSalary { get; set; }

        public bool IsActive { get; set; }

        // Không validate mấy cái dropdown
        [ValidateNever]
        public List<SelectListItem>? Employees { get; set; }

        
    }
}